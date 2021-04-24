using GomokuLib;
using Microsoft.ML;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Linq;
using TreeSearchLib;

namespace OnnxEstimatorLib
{
    public class RealPlayerTreeSearch : TreeSearch
    {
        private readonly InferenceSession _inferenceSession;
        public RealPlayerTreeSearch()
            : base()
        {
        }
        protected override float EvaluateState(GameState gameState)
        {
            throw new NotImplementedException();
        }

        protected override List<float> EvaluateStates(IEnumerable<GameState> gameStates)
        {
            throw new NotImplementedException();
        }

        public override Move FindBestMove(GameState gameState, bool batch = true, int depth = 1)
        {
            Console.Write("Enter move: ");
            string userInput = Console.ReadLine();
            var userInputSplit = userInput.Split();
            int col = userInputSplit[0].ToUpper()[0] - 65;
            int row = 15 - int.Parse(userInputSplit[1]);
            return new Move(row , col, gameState.PlayerTurn);
        }

        //protected override List<float> EvaluateStates(IEnumerable<GameState> gameStates)
        //{
        //    var rand = new System.Random();
        //    return Enumerable.Range(0, gameStates.Count()).Select(x => (float)rand.NextDouble()).ToList();
        //}
    }
}
