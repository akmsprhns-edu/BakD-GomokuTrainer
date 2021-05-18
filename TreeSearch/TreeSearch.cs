using GomokuLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeSearchLib.Extensions;

namespace TreeSearchLib
{
    public abstract class TreeSearch
    {
        protected readonly Random Random;
        public TreeSearch()
        {
            Random = new Random();
            AllNodes = new HashSet<GameTreeNode>();
        }
        
        public HashSet<GameTreeNode> AllNodes { get; }
        public GameTreeNode CurrentTreeNode = null;

        protected virtual float EvaluateState(GameState gameState)
        {
            var gameResult = gameState.IsGameOver();
            return gameResult switch
            {
                GameResult.FirstPlayerWon => 1,
                GameResult.SecondPlayerWon => -1,
                GameResult.Tie => 0,
                null => 0,
                _ => throw new NotImplementedException()
            };
        }

        protected virtual List<float> EvaluateStates(IEnumerable<GameState> gameStates)
        {
            return gameStates.Select(gs => EvaluateState(gs)).ToList();
        }

        public abstract PlayerMove FindBestMove(GameState gameState, bool batch = true);

        public GameTree BuildTree(GameState gameState,bool generateStates, int depth = 1, bool onlyPriorityMoves = true)
        {
            var gameTree = new GameTree()
            {
                Root = new GameTreeNode()
                {
                    GameState = gameState,
                }
            };
            gameTree.Root.Children = ExpandNode(gameTree.Root, generateStates, depth: depth, onlyPriorityMoves: onlyPriorityMoves);
            return gameTree;
        }

        public Dictionary<PlayerMove,GameTreeNode> ExpandNode(GameTreeNode gameTreeNode, bool generateStates, int depth = 1, bool onlyPriorityMoves = true)
        {
            if (depth < 1 || gameTreeNode.GameState.IsGameOver() != null)
            {
                return null;
            }
            return GetMoves(gameTreeNode.GameState, priority: onlyPriorityMoves).Select(m => 
            {
                var node = new GameTreeNode()
                {
                    Moves = new HashSet<PlayerMove>(gameTreeNode.Moves)
                };
                node.Moves.Add(m);
                //if (AllNodes.TryGetValue(node, out var existingNode))
                //{
 
                //        Console.WriteLine($"\nExisting node found");

                //    return (move: m, node: existingNode);
                //}
                //else
                //{
                    node.GameState = generateStates ? gameTreeNode.GameState.MakeMove(m) : null;
                    if (depth > 0)
                        node.Children = ExpandNode(node, generateStates, depth - 1);
                    //AllNodes.Add(node);
                    return (move: m, node: node);
                //}
                
            }).ToDictionary(x => x.move, x => x.node);
        }

        public IEnumerable<PlayerMove> GetMoves(GameState gameState, bool priority = true)
        {
            foreach (var position in gameState.GetUnoccupiedPositions())
            {

                if (priority && !gameState.IsPriorityMove(position.row, position.colmun))
                {
                    continue;
                }
                else
                {
                    yield return new PlayerMove(position.row, position.colmun, gameState.PlayerTurn);
                }
            }
        }

        /// <summary>
        /// Feedback to tree search, to tell witch move was made
        /// </summary>
        /// <param name="move"></param>
        public virtual void MoveCurrentTreeNode(PlayerMove move)
        {
            if (CurrentTreeNode != null)
            {
                if (CurrentTreeNode.Children.TryGetValue(move, out var newNode) && newNode.GameState != null)
                {
                    CurrentTreeNode = newNode;

                    AllNodes.RemoveWhere(x => x.Moves.Count < CurrentTreeNode.Moves.Count);
                }
                else
                {
                    CurrentTreeNode = null;
                }
            }
        }

        public virtual string PrintCurrentStateMoveInfo()
        {
            return "";
        }

        
    }
}
