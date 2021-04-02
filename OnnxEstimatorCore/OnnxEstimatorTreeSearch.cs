using GomokuLib;
using Microsoft.ML;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.Linq;
using TreeSearchLib;

namespace OnnxEstimatorLib
{
    public class OnnxEstimatorTreeSearch : TreeSearch
    {
        private readonly InferenceSession _inferenceSession;
        public OnnxEstimatorTreeSearch(InferenceSession inferenceSession)
            : base()
        {
            _inferenceSession = inferenceSession;
        }
        protected override float EvaluateState(GameState gameState)
        {
            var inputData = gameState.GetBoardStateArray().Select(x => x ? 1f : 0f).ToArray();
            var inputs = new List<NamedOnnxValue>()
                {
                    NamedOnnxValue.CreateFromTensor(Consts.NNInputName, new DenseTensor<float>(inputData, Consts.NNInputShape))
                };
            var outputs = new List<string>()
                {
                    Consts.NNOutputName
                };

            using var results = _inferenceSession.Run(inputs, outputs);
            return  results.First().AsEnumerable<float>().First(); 
        }

        protected override List<float> EvaluateStates(IEnumerable<GameState> gameStates)
        {
            var inputShape = (int[])Consts.NNInputShape.Clone();
            inputShape[0] = gameStates.Count();
            var inputData = gameStates.SelectMany(x => x.GetBoardStateArray()).Select(x => x ? 1f : 0f).ToArray();
            var inputs = new List<NamedOnnxValue>()
                {
                    NamedOnnxValue.CreateFromTensor(Consts.NNInputName, new DenseTensor<float>(inputData, inputShape))
                };
            var outputs = new List<string>()
                {
                    Consts.NNOutputName
                };

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
