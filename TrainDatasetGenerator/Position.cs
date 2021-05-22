using GomokuLib;

namespace TrainDatasetGenerator
{
    public class Position
    {
        public float[] Board { get; set; }
        public int PlayerTurn { get; set; }
        public int EvalCount { get; set; }
        public double Eval { get; set; }
        
        public string ToCsvString(char separator = ';')
        {
            return string.Join(separator, string.Join(separator, Board), PlayerTurn, EvalCount, Eval);
        }
    }
}
