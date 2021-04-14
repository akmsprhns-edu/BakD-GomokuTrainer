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
        public Dictionary<Move,GameTreeNode> Children { get; set; }
        public Move Move { get; set; }

        ///// <summary>
        ///// Node vists by MCTS
        ///// </summary>
        //public int N { get; set; }

        /// <summary>
        /// Evaluations by MCTS
        /// </summary>
        public List<double> Evals { get; }

        public float? Evaluation { get; set; }
        public GameTreeNode()
        {
            Evals = new List<double>();
        }
        public IEnumerable<GameTreeNode> GetEndNodes()
        {
            if (Children is null)
            {
                yield return this;
            }
            else
            {
                foreach (var child in Children.Values)
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

        public IEnumerable<GameTreeNode> Flatten()
        {
            yield return this;
            if (Children != null)
            {
                foreach (var child in Children.Values)
                {
                    foreach (var item in child.Flatten())
                    {
                        yield return item;
                    }
                }
            }
        }
    }
}
