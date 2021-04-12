using GomokuLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeSearchLib
{
    public class GameTreeNode
    {
        public GameState GameState { get; set; }
        public List<GameTreeNode> Children { get; set; }
        public Move Move { get; set; }
        public float? Evaluation { get; set; }

        public IEnumerable<GameTreeNode> GetEndNodes()
        {
            if (Children is null)
            {
                yield return this;
            }
            else
            {
                foreach (var child in Children)
                {
                    if (child.Children is null)
                    {
                        yield return child;
                    }

                    foreach (var item in child.GetEndNodes())
                    {
                        yield return item;
                    }
                }
            }
        }
    }
}
