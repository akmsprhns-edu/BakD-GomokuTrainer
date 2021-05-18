using GomokuLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComparisonSimulation
{
    public class Result
    {
        public AIType FirstPlayer { get; set; }
        public AIType SecondPlayer { get; set; }
        public GameResult GameResult { get; set; }

        public string ToCsvString(char separator = ';')
        {
            return string.Join(separator, FirstPlayer, SecondPlayer, GameResult);
        }
    }
}
