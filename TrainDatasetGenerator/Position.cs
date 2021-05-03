using GomokuLib;

namespace TrainDatasetGenerator
{
    public class Position
    {
        public byte[] Board { get; set; }
        public PlayerColor PlayerTurn { get; set; }
        public int EvalCount { get; set; }
        public double Eval { get; set; }
        
        public string ToCsvString(char separator = ';')
        {
            return string.Join(separator, string.Join(separator, Board),(int)PlayerTurn, EvalCount, Eval);
        }
    }
}
