
using GomokuLib;

namespace GomokuWebUI.Dtos.GomokuController
{
    public class GameSessionStateResponse
    {
        public string Guid { get; set; }
        public StoneColor[,] Board { get; set; }
        public GameResult? GameResult { get; set; }
    }
}
