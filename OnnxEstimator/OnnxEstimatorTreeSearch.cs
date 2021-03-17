using GomokuLib;
using Microsoft.ML;
using OnnxEstimator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeSearchLib;

namespace OnnxEstimator
{
    public class OnnxEstimatorTreeSearch : TreeSearch
    {
        private readonly PredictionEngine<InputData, Prediction> _predictionEngine;
        //private readonly Random _random;
        public OnnxEstimatorTreeSearch(PredictionEngine<InputData,Prediction> predictionEngine)
            : base()
        {
            _predictionEngine = predictionEngine;
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
    }
}
