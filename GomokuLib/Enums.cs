using System;
using System.Collections.Generic;
using System.Text;

namespace GomokuLib
{
    public enum PlayerColor
    {
        First = 1,
        Second = 2
    }
    public enum StoneColor
    {  
        None = 0,
        First = 1,
        Second = 2
    }
    public enum GameResult
    {
        Tie = 0,
        FirstPlayerWon = 1,
        SecondPlayerWon = 2
    }
}
