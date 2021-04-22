from typing import Callable, List
import numpy as np
import json
import subprocess
import sys
import pickle
from pathlib import Path
from os import path, _exit
from tensorflow import keras
from tensorflow.keras import layers
import onnx
import keras2onnx
from tensorflow.python.keras.engine.sequential import Sequential
from tensorflow.python.keras.layers.advanced_activations import LeakyReLU
from es import OpenES
from shutil import rmtree
import time

class Evaluator:
  def __init__(self, create_model_function: Callable[[],Sequential], estimator_exe_path: str):
    self.estimator_exe_path = estimator_exe_path

    self.model = create_model_function()
    self.model.compile()
    ### unwrap layers weights
    self.layer_count = len(self.model.get_weights())
    self.layer_shapes = [None] * self.layer_count
    self.layer_sizes = [None] * self.layer_count
    #self.flat_layers = [None] * self.layer_count
    layer_n = 0
    for layer in self.model.get_weights():
      np_layer = np.array(layer)
      self.layer_shapes[layer_n] = np_layer.shape
      self.layer_sizes[layer_n] = np_layer.size
      #flat_layers[layer_n] = np_layer.reshape(-1)
      layer_n += 1

    #flat_weights = np.concatenate(flat_layers)
  def unwrap_weighs(self, model_weights):
    ### unwrap layers weights
    layer_count = len(model_weights)
    flat_layers = [None] * layer_count
    layer_n = 0
    for layer in model_weights:
      flat_layers[layer_n] = np.array(layer).reshape(-1)
      layer_n += 1
    flat_weights = np.concatenate(flat_layers)
    return flat_weights

  def wrap_weights(self, flat_weights: List[float]):
    #### wrap layer weights back
    split_indexes = [l for l in self.layer_sizes]
    for i in range(1, len(split_indexes)):
      split_indexes[i] += split_indexes[i-1]
    
    flat_layers = np.split(flat_weights, split_indexes)
    if(len(flat_layers[-1]) == 0):
      flat_layers.pop() # remove last layer, because it's always 0
    else:
      raise Exception("Something went wrong, last layer size not 0 but " + str(len(flat_layers[-1])) )

    layer_n = 0
    model_weights = [None] * self.layer_count
    for layer in flat_layers:
      model_weights[layer_n] = layer.reshape(self.layer_shapes[layer_n])
      layer_n += 1
    return model_weights

  def set_model_weights(self, flat_weights: List[float]):
    wrapped_weights = self.wrap_weights(flat_weights)
    self.model.set_weights(wrapped_weights)

  def generate_file(self, model_number: int, models_output_dir: str):
    onnx.save_model(keras2onnx.convert_keras(self.model, self.model.name), path.abspath(path.join(models_output_dir , "model." + str(model_number) + ".onnx")))

  def save_keras_model(self, models_output_dir: str, model_number: int):
    self.model.save(path.abspath(path.join(models_output_dir, 'keras-model-' + str(model_number))))

  def get_results_from_json(self, models_output_dir: str) -> List[float]:
    with open(path.abspath(path.join(models_output_dir , "result.json"))) as json_file:
      return json.load(json_file)

  def evaluate(self, list_of_flat_weights: List[List[float]], models_output_dir:str, keep_files: bool, save_keras_model: bool):
    Path(models_output_dir).mkdir(parents=True, exist_ok=True)
    model_n = 1
    for flat_weights in list_of_flat_weights:
      self.set_model_weights(flat_weights)
      self.generate_file(model_n, models_output_dir)
      if save_keras_model:
        print("saving keras model")
        self.save_keras_model(models_output_dir, model_n)
      model_n += 1
    print("starting evaluator exe")
    command = [path.abspath(self.estimator_exe_path), path.abspath(models_output_dir), "--gpu"]
    print(command)
    while(True):
      run_result = subprocess.run(command)
      if(run_result.returncode != 0):
        print(f"!!!!! Evaluator.exe returned {run_result.returncode} !!!!")
        time.sleep(5)
      else:
        print("evaluator exe finished successfuly")
        break
    
    results = self.get_results_from_json(models_output_dir)
    if(not keep_files):
      rmtree(path.abspath(models_output_dir))
    return results

input_shape = (15,15,2)
input_len = input_shape[0] * input_shape[1] * input_shape[2]

def create_model():
  return keras.Sequential(
    [
      keras.Input(shape=input_len, name="input"),
      layers.Reshape(input_shape),
      layers.Conv2D(32, kernel_size=(3, 3), kernel_initializer=keras.initializers.HeNormal()),
      layers.LeakyReLU(alpha=0.2),
      layers.MaxPooling2D((2, 2)),
      layers.Conv2D(32, kernel_size=(3, 3), kernel_initializer=keras.initializers.HeNormal()),
      layers.LeakyReLU(alpha=0.2),
      layers.Flatten(),
      layers.Dense(32, kernel_initializer=keras.initializers.HeNormal()),
      layers.LeakyReLU(alpha=0.2),
      layers.Dense(1, activation='sigmoid', name="output")
    ]
  )
def save_solver(solver, dir: str):
  with open(path.join(dir,'solver.pkl'), 'wb') as output:
    pickle.dump(solver, output, pickle.HIGHEST_PROTOCOL)

def save_solver_params(solver, iteration: int, dir: str):
  params_file_path = path.join(dir,'solver_params.csv')
  if(not path.exists(params_file_path)):
    with open(params_file_path, 'w') as output:
      output.write(
        ",".join(["iteration", "sigma", "learning_rate", "optimizer_stepsize"]) + '\n'
      )
  with open(params_file_path, 'a') as output:
    output.write(
      ",".join([str(iteration), str(solver.sigma), str(solver.learning_rate), str(solver.optimizer.stepsize)]) + '\n'
    )


def load_solver(dir: str):
  solver_file_path = path.join(dir,'solver.pkl')
  if(not path.exists(solver_file_path)):
    return None
  with open(solver_file_path, 'rb') as input:
     return pickle.load(input)

def get_iteration_dir(result_dir: str, iteration: int) -> str :
  return path.join(result_dir, f"iteration{iteration:05d}")

def run_solver(solver, iteration_count: int, result_dir: str, estimator_exe_path: str):
  evaluator = Evaluator(create_model, estimator_exe_path)
  history = []
  if(not hasattr(solver,'iteration')):
    solver.iteration = -1
    # first iteration, set initial mu
    initial_weights = evaluator.unwrap_weighs(create_model().get_weights())
    if len(initial_weights) != len(solver.mu):
      raise Exception(f"len(initial_weights) != len(solver.mu) => {len(initial_weights)} != {len(solver.mu)}")
    solver.mu = initial_weights

  for iteration in range(solver.iteration + 1, solver.iteration + iteration_count + 1):
    list_of_flat_weights = solver.ask()
    solver.iteration = iteration
    print("Evaluating iteration " + str(iteration))
    eval_result = evaluator.evaluate(
      list_of_flat_weights, 
      get_iteration_dir(result_dir, iteration), 
      keep_files= iteration % 5 == 0,
      save_keras_model= iteration % 100 == 0
    )
    solver.tell(eval_result)
    save_solver(solver, result_dir)
    save_solver_params(solver, iteration, result_dir)
    result = solver.result() # first element is the best solution, second element is the best fitness
    history.append(result[1])
  return history

def get_param_count():
  return sum([np.array(layer).size for layer in create_model().get_weights()])

def main(argv):
  if len(sys.argv) < 4:
    print("Insufficient arguments")
    return -1
  
  iteration_count = int(argv[1])
  result_dir = path.abspath(argv[2])
  estimator_exe_path = path.abspath(argv[3])
  NPARAMS = get_param_count()
  NPOPULATION = 128

  print("iteration count = " + str(iteration_count))
  print("result dir = " + result_dir)
  print("estimator exe path = " + estimator_exe_path) 
  print("param count = " + str(NPARAMS))
  print("population = " + str(NPOPULATION))

  oes = load_solver(result_dir)
  if (oes is None):
    oes = OpenES(NPARAMS,         # number of model parameters
        sigma_init=0.1,                 # initial standard deviation
        sigma_decay=0.9995,             # don't anneal standard deviation
        sigma_limit=0.01,
        learning_rate=0.01,             # learning rate for standard deviation
        learning_rate_decay = 0.999,    # annealing the learning rate
        learning_rate_limit = 0.0005,
        popsize=NPOPULATION,            # population size
        antithetic=False,               # whether to use antithetic sampling
        weight_decay=0.00,              # weight decay coefficient
        rank_fitness=True,              # use rank rather than fitness numbers
        forget_best=True)

  run_solver(oes, iteration_count, result_dir, estimator_exe_path)
  return 0

if __name__ == '__main__':
  exitcode = main(sys.argv)
  sys.stdout.flush()
  _exit(exitcode)