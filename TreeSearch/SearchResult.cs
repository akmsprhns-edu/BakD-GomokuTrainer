using GomokuLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace TreeSearchLib
{
    public class SearchResult
    {
        public float Evaluation { get; set; }
        public Move Move { get; set; }
        public GameState GameState { get; set; }
    }
}
