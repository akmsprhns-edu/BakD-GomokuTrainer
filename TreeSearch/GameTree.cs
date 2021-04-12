using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeSearchLib
{
    public class GameTree
    {
        public GameTreeNode Root { get; set; }

        public IEnumerable<GameTreeNode> GetEndNodes()
        {
            return Root.GetEndNodes();
        }
    }
}
