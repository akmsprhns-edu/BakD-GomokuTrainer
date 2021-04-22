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
        }

        protected abstract float EvaluateState(GameState gameState);
        protected abstract List<float> EvaluateStates(IEnumerable<GameState> gameStates);
        protected float MinMaxSearch(int depth, GameState sourceState)
        {
            var Maximize = sourceState.PlayerTurn == PlayerColor.White ? true : false;
            var gameResult = sourceState.IsGameOver();
            if (gameResult == GameResult.WhiteWon)
            {
                if (sourceState.PlayerTurn == PlayerColor.White)
                    throw new Exception("Something went wrong, white won, but its whites turn");

                return 1;
            }
            else if (gameResult == GameResult.BlackWon)
            {
                if (sourceState.PlayerTurn == PlayerColor.Black)
                    throw new Exception("Something went wrong, black won, but its blacks turn");

                return 0;
            }
            else if (gameResult == GameResult.Tie || depth == 0)
            {
                return EvaluateState(sourceState);
            }
            else
            {
                var childEvals = new List<float>();
                for (var row = 0; row < GameState.BoardSize; row++)
                {
                    for (var col = 0; col < GameState.BoardSize; col++)
                    {
                        if (!sourceState.IsValidMove(row,col)) {
                            continue;
                        }
                        else
                        {
                            var newState = sourceState.MakeMove(row, col);
                            childEvals.Add(MinMaxSearch(depth - 1, newState));
                        }
                    }
                }
                if (childEvals.Count == 0)
                {
                    throw new Exception("No moves found");
                }
                if (Maximize)
                {
                    return childEvals.Max();
                } else
                {
                    return childEvals.Min();
                }
            }
        }

        public Move FindBestMoveMinMax(GameState gameState, int depth)
        {
            var Maximize = gameState.PlayerTurn == PlayerColor.White ? true : false;
            var searchResults = new List<SearchResult>();
            for (var row = 0; row < GameState.BoardSize; row++)
            {
                for (var col = 0; col < GameState.BoardSize; col++)
                {
                    if (!gameState.IsValidMove(row,col))
                    {
                        continue;
                    }
                    else
                    {
                        var newState = gameState.MakeMove(row, col);
                        searchResults.Add(new SearchResult() {
                            Evaluation = MinMaxSearch(depth - 1, newState),
                            Move = new Move()
                            {
                                Row = row,
                                Column = col
                            }
                        });
                    }
                }
            }
            if (Maximize)
            {
                return searchResults.Aggregate((acc, x) => x.Evaluation > acc.Evaluation ? x : acc).Move;
            }
            else
            {
                return searchResults.Aggregate((acc, x) => x.Evaluation < acc.Evaluation ? x : acc).Move;
            }
        }

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

        public Dictionary<Move,GameTreeNode> ExpandNode(GameTreeNode gameTreeNode, bool generateStates, int depth = 1, bool onlyPriorityMoves = true)
        {
            if (depth < 1)
            {
                return null;
            }
            return GetMoves(gameTreeNode.GameState, priority: onlyPriorityMoves).Select(m => 
            {
                var node = new GameTreeNode()
                {
                    GameState = generateStates ? gameTreeNode.GameState.MakeMove(m) : null,
                    Move = m
                };
                if (depth > 0)
                    node.Children = ExpandNode(node, generateStates, depth - 1);
                return node;
            }).ToDictionary(x => x.Move);
        }

        public GameTree EvaluateTree(GameTree gameTree)
        {
            var endNodes = gameTree.GetEndNodes().ToList();
            var items = EvaluateStates(endNodes.Select(x => x.GameState)).Zip(endNodes, (eval, node) => (Eval: eval, Node: node));
            foreach (var item in items)
            {
                item.Node.Evaluation = item.Eval;
            }
            return gameTree;
        }

        public IEnumerable<Move> GetMoves(GameState gameState, bool priority = true)
        {
            for (var row = 0; row < GameState.BoardSize; row++)
            {
                for (var col = 0; col < GameState.BoardSize; col++)
                {
                    if (!priority && !gameState.IsValidMove(row, col))
                    {
                        continue;
                    } 
                    else if(priority && !gameState.IsValidPriorityMove(row, col))
                    {
                        continue;
                    }
                    else
                    {
                        yield return new Move()
                        {
                            Row = row,
                            Column = col
                        };
                    }
                }
            }
        }

        public void EvaluateNode(GameTreeNode node, bool Maximize){
            if (node.Evaluation is null)
            {
                if (node.Children is null)
                {
                    throw new Exception("End node not evaluated");
                }
                foreach (var child in node.Children.Values)
                {
                    EvaluateNode(child, !Maximize);
                }
                if (Maximize)
                {
                    node.Evaluation = node.Children.Values.Select(ch => ch.Evaluation).Max();
                }
                else
                {
                    node.Evaluation = node.Children.Values.Select(ch => ch.Evaluation).Min();
                }
            }
        }

        public IEnumerable<SearchResult> GetEvaluatedMovesMinMax(GameState gameState, bool maximize, int depth = 1)
        {
            var tree = EvaluateTree(BuildTree(gameState, true, depth));
            EvaluateNode(tree.Root, maximize);
            return tree.Root.Children.Values.Select(x => new SearchResult()
            {
                Evaluation = x.Evaluation.Value,
                Move = x.Move,
                GameState = x.GameState
            });
        }

        public IEnumerable<SearchResult> GetEvaluatedMovesSequencial(GameState gameState, bool onlyPriorityMoves = true)
        {
            var moves = GetMoves(gameState, priority: onlyPriorityMoves).ToList();
            var states = moves.Select(m => gameState.MakeMove(m)).ToList();
            return states.Select(s => EvaluateState(s)).ZipThree(states, moves, (eval, state, move) => new SearchResult()
            {
                Evaluation = eval,
                Move = move,
                GameState = state
            });
        }
        public IEnumerable<SearchResult> GetEvaluatedMovesBatch(GameState gameState, bool onlyPriorityMoves = true)
        {
            var moves = GetMoves(gameState, priority: onlyPriorityMoves).ToList();
            var states = moves.Select(m => gameState.MakeMove(m)).ToList();
            return EvaluateStates(states).ZipThree(states, moves, (eval, state, move) => new SearchResult()
            {
                Evaluation = eval,
                Move = move,
                GameState = state
            });
        }

        public virtual void MoveCurrentTreeNode(Move move)
        {

        }

        public virtual void PrintCurrentStateMoveInfo()
        {

        }

        public virtual Move FindBestMove(GameState gameState, bool batch = true, int depth = 1)
        {
            var Maximize = gameState.PlayerTurn == PlayerColor.White ? true : false;

            List<SearchResult> searchResults;
            if (depth > 1)
                searchResults = GetEvaluatedMovesMinMax(gameState, Maximize, depth).ToList();
            else if (batch)
                searchResults = GetEvaluatedMovesBatch(gameState).ToList();
            else
                searchResults = GetEvaluatedMovesSequencial(gameState).ToList();

            if (Maximize)
            {
                var maxEvaluation = searchResults.Select(x => x.Evaluation).Max();
                var maxMoves = searchResults.Where(x => x.Evaluation == maxEvaluation);
                if (maxMoves.Count() == 1)
                {
                    return maxMoves.First().Move;
                }
                else
                {
                    return maxMoves.Skip(Random.Next(maxMoves.Count())).First().Move;
                }
            }
            else
            {
                var minEvaluation = searchResults.Select(x => x.Evaluation).Min();
                var minMoves = searchResults.Where(x => x.Evaluation == minEvaluation);
                if (minMoves.Count() == 1)
                {
                    return minMoves.First().Move;
                }
                else
                {
                    return minMoves.Skip(Random.Next(minMoves.Count())).First().Move;
                }
            }
        }
    }
}
