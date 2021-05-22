import tensorflow as tf
from tensorflow import keras
from tensorflow.keras import layers

input_shape = (15,15,1)
input_len = input_shape[0] * input_shape[1] * input_shape[2]

# Ieejas slānis A
inputA = keras.Input(shape=input_len, name="input_board")
# Ieejas slānis B
inputB = keras.Input(shape=1, name="input_turn")
# Transformācijas slānis 1D -> 2D
a = layers.Reshape(input_shape)(inputA)
# 1.Konvolūciju slānis
a = layers.Conv2D(32, kernel_size=(2, 2), kernel_initializer=keras.initializers.HeNormal())(a)
# Aktivizācijas slānis
a = layers.LeakyReLU(alpha=0.2)(a)
# 2. Konvolūciju slānis
a = layers.Conv2D(64, kernel_size=(2, 2), kernel_initializer=keras.initializers.HeNormal())(a)
# Aktivizācijas slānis
a = layers.LeakyReLU(alpha=0.2)(a)
# 3. Konvolūciju slānis
a = layers.Conv2D(64, kernel_size=(2, 2), kernel_initializer=keras.initializers.HeNormal())(a)
# Aktivizācijas slānis
a = layers.LeakyReLU(alpha=0.2)(a)
# Transformācijas slānis 2D -> 1D
a = layers.Flatten()(a)
# 1. Pilnsaistes slānis
a = layers.Dense(64, kernel_initializer=keras.initializers.HeNormal())(a)
# Aktivizācijas slānis
a = layers.LeakyReLU(alpha=0.2)(a)
# Konkatenācijas slānis
a = layers.Concatenate()([a, inputB])
# 2. Pilnsaistes slānis
a = layers.Dense(32, kernel_initializer=keras.initializers.HeNormal())(a)
# Aktivizācijas slānis
a = layers.LeakyReLU(alpha=0.2)(a)
# Izejas slānis ar aktivizācijas funkciju ‘tanh’
a = layers.Dense(1, activation='tanh', name="output")(a)

model = keras.Model(inputs=[inputA,inputB], outputs=a)

