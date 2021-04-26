
using GomokuLib;
using System.Collections.Generic;

namespace GomokuWebUI.Dtos.GomokuController
{
    public class GameSessionStateResponse
    {
        public string Guid { get; set; }
        public StoneColor[,] Board { get; set; }
        public GameResult? GameResult { get; set; }
        public Dictionary<string, int> Moves { get; set; }
    }
}
