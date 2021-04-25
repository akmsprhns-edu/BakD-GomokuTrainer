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
        public Dictionary<PlayerMove, GameTreeNode> Children { get; set; }
        //public Move Move { get; set; }
        public HashSet<PlayerMove> Moves { get; set; }

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
            Moves = new HashSet<PlayerMove>();
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

        public override bool Equals(object obj)
        {
            return Equals(obj as GameTreeNode);
        }

        public bool Equals(GameTreeNode other)
        {
            return other != null && Moves.SetEquals(other.Moves);
        }

        public override int GetHashCode()
        {
            int hc = 0;
            foreach (var item in Moves)
                hc ^= item.GetHashCode();
            return hc;
        }
    }
}
