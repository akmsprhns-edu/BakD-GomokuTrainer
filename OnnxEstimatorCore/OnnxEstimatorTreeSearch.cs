using GomokuLib;
using Microsoft.ML;
using Microsoft.ML.Transforms.Onnx;
using OnnxEstimatorLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeSearchLib;

namespace OnnxEstimatorLib
{
    public class OnnxEstimatorTreeSearch : TreeSearch
    {
        private readonly PredictionEngine<InputData, Prediction> _predictionEngine;
        private readonly OnnxTransformer _transformer;
        private readonly MLContext _mlContext;
        //private readonly Random _random;
        public OnnxEstimatorTreeSearch(PredictionEngine<InputData,Prediction> predictionEngine, OnnxTransformer transformer)
            : base()
        {
            _predictionEngine = predictionEngine;
            _transformer = transformer;
            _mlContext = new MLContext();
            //_random = new Random(1);

        }
        protected override float EvaluateState(GameState gameState)
        {
            var inputData = new InputData()
            {
                Input = gameState.GetBoardStateArray().Select(x => x ? 1f : 0f).ToArray()
            };
            //return (float)_random.NextDouble();
            return _predictionEngine.Predict(inputData).Evaluation;

        }

        protected override List<float> EvaluateStates(IEnumerable<GameState> gameStates)
        {
            var data = _mlContext.Data.LoadFromEnumerable(gameStates.Select(x => new InputData { 
                Input = x.GetBoardStateArray().Select(x => x ? 1f : 0f).ToArray() 
            }));
            return _mlContext.Data.CreateEnumerable<Prediction>(
                _transformer.Transform(data), 
                reuseRowObject: false
            ).Select(x => x.Evaluation).ToList();
        }
    }
}
