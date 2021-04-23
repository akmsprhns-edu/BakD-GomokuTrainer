using System;
using System.Collections.Generic;
using System.Text;

namespace GomokuLib
{
    public enum PlayerColor
    {
        White = 2,
        Black = 1
    }
    public enum StoneColor
    {  
        None = 0,
        Black = 1,
        White = 2
    }
    public enum GameResult
    {
        Tie = 0,
        WhiteWon = 2,
        BlackWon = 1
    }
}
