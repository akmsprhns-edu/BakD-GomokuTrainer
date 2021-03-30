using System;
using System.Collections.Generic;
using System.Text;
using TreeSearchLib;

namespace OnnxEstimatorLib
{
    public class Player
    {
        public string Name { get; }
        public TreeSearch TreeSearch { get; }

        public Player(string name, TreeSearch treeSearch)
        {
            Name = name;
            TreeSearch = treeSearch;
        }
    }
}
