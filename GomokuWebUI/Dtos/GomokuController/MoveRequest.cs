
namespace GomokuWebUI.Dtos.GomokuController
{
    public class MoveRequest
    {
        public string GameSessionGuid { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }
}
