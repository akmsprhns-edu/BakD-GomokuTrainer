using System.Linq;

namespace OnnxEstimatorLib
{
    public static class Consts
    {
        public static readonly int[] NNBoardInputShape = new int[] { 1, 225};
        public static int NNBoardInputLen { get => NNBoardInputShape.Aggregate(1, (a, b) => a * b); }

        public static readonly int[] NNTurnInputShape = new int[] { 1, 1 };
        public static int NNTurnInputLen { get => NNBoardInputShape.Aggregate(1, (a, b) => a * b); }

        public static readonly string NNBoardInputName = "input_board";
        public static readonly string NNTurnInputName = "input_turn";
        public static readonly string NNOutputName = "output";
    }
}
