﻿using GomokuLib;
using Microsoft.ML;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.Linq;
using TreeSearchLib;

namespace OnnxEstimatorLib
{
    public class OnnxEstimatorTreeSearch : MonteCarloTreeSearch
    {
        private readonly InferenceSession _inferenceSession;
        public OnnxEstimatorTreeSearch(InferenceSession inferenceSession, int iterations = 15000, int playoutDepth = 2, bool enableLogging = true)
            : base(iterations: iterations, playoutDepth: playoutDepth, enableLogging: enableLogging)
        {
            _inferenceSession = inferenceSession;
        }
        protected override float EvaluateState(GameState gameState)
        {
            var boardData = gameState.GetBoardFloatArray();
            var turnData = gameState.PlayerTurn == PlayerColor.First ? new float[] { 1 } : new float[] { -1 };
            var inputs = new List<NamedOnnxValue>()
                {
                    NamedOnnxValue.CreateFromTensor(Consts.NNBoardInputName, new DenseTensor<float>(boardData, Consts.NNBoardInputShape)),
                    NamedOnnxValue.CreateFromTensor(Consts.NNTurnInputName, new DenseTensor<float>(turnData, Consts.NNTurnInputShape))
                };
            var outputs = new List<string>()
                {
                    Consts.NNOutputName
                };

            using var results = _inferenceSession.Run(inputs, outputs);
            return results.First().AsEnumerable<float>().First();
        }

        protected override List<float> EvaluateStates(IEnumerable<GameState> gameStates)
        {
            var inputShape = (int[])Consts.NNBoardInputShape.Clone();
            inputShape[0] = gameStates.Count();
            var inputData = gameStates.SelectMany(x => x.GetBoardFloatArray()).ToArray();
            var inputs = new List<NamedOnnxValue>()
                {
                    NamedOnnxValue.CreateFromTensor(Consts.NNBoardInputName, new DenseTensor<float>(inputData, inputShape))
                };
            var outputs = new List<string>()
                {
                    Consts.NNOutputName
                };
            //throw new System.NotImplementedException();
            using var results = _inferenceSession.Run(inputs, outputs);
            return results.First().AsEnumerable<float>().ToList();
        }

        //protected override List<float> EvaluateStates(IEnumerable<GameState> gameStates)
        //{
        //    var rand = new System.Random();
        //    return Enumerable.Range(0, gameStates.Count()).Select(x => (float)rand.NextDouble()).ToList();
        //}

    }
}
