using GomokuLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeSearchLib
{
    public class MinimaxTreeSearch : TreeSearch
    {
        protected readonly int _depth;
        public MinimaxTreeSearch(int depth)
        {
            _depth = depth;
        }

        public void EvaluateNode(GameTreeNode node, bool Maximize)
        {
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

        public IEnumerable<SearchResult> GetEvaluatedMovesMinMax(GameState gameState, bool maximize, int depth = 1)
        {
            var tree = EvaluateTree(BuildTree(gameState, true, depth));
            EvaluateNode(tree.Root, maximize);
            return tree.Root.Children.Values.Select(x => new SearchResult()
            {
                Evaluation = x.Evaluation.Value,
                GameState = x.GameState
            });
        }

        public override PlayerMove FindBestMove(GameState gameState, bool batch = true)
        {
            var Maximize = gameState.PlayerTurn == PlayerColor.First ? true : false;

            List<SearchResult> searchResults;
            searchResults = GetEvaluatedMovesMinMax(gameState, Maximize, _depth).ToList();

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

        //public PlayerMove FindBestMoveMinMax(GameState gameState, int depth)
        //{
        //    var Maximize = gameState.PlayerTurn == PlayerColor.First ? true : false;
        //    var searchResults = new List<SearchResult>();
        //    for (var row = 0; row < GameState.BoardSize; row++)
        //    {
        //        for (var col = 0; col < GameState.BoardSize; col++)
        //        {
        //            if (!gameState.IsValidMove(row, col))
        //            {
        //                continue;
        //            }
        //            else
        //            {
        //                var newState = gameState.MakeMove(row, col);
        //                searchResults.Add(new SearchResult()
        //                {
        //                    Evaluation = MinMaxSearch(depth - 1, newState),
        //                    Move = new PlayerMove(row, col, gameState.PlayerTurn)
        //                });
        //            }
        //        }
        //    }
        //    if (Maximize)
        //    {
        //        return searchResults.Aggregate((acc, x) => x.Evaluation > acc.Evaluation ? x : acc).Move;
        //    }
        //    else
        //    {
        //        return searchResults.Aggregate((acc, x) => x.Evaluation < acc.Evaluation ? x : acc).Move;
        //    }
        //}

        //protected float MinMaxSearch(int depth, GameState sourceState)
        //{
        //    var Maximize = sourceState.PlayerTurn == PlayerColor.First ? true : false;
        //    var gameResult = sourceState.IsGameOver();
        //    if (gameResult == GameResult.FirstPlayerWon)
        //    {
        //        if (sourceState.PlayerTurn == PlayerColor.First)
        //            throw new Exception("Something went wrong, first player won, but its NOT second player's turn");

        //        return 1;
        //    }
        //    else if (gameResult == GameResult.SecondPlayerWon)
        //    {
        //        if (sourceState.PlayerTurn == PlayerColor.Second)
        //            throw new Exception("Something went wrong, second player won, but its NOT first player's turn");

        //        return 0;
        //    }
        //    else if (gameResult == GameResult.Tie || depth == 0)
        //    {
        //        return EvaluateState(sourceState);
        //    }
        //    else
        //    {
        //        var childEvals = new List<float>();
        //        for (var row = 0; row < GameState.BoardSize; row++)
        //        {
        //            for (var col = 0; col < GameState.BoardSize; col++)
        //            {
        //                if (!sourceState.IsValidMove(row, col))
        //                {
        //                    continue;
        //                }
        //                else
        //                {
        //                    var newState = sourceState.MakeMove(row, col);
        //                    childEvals.Add(MinMaxSearch(depth - 1, newState));
        //                }
        //            }
        //        }
        //        if (childEvals.Count == 0)
        //        {
        //            throw new Exception("No moves found");
        //        }
        //        if (Maximize)
        //        {
        //            return childEvals.Max();
        //        }
        //        else
        //        {
        //            return childEvals.Min();
        //        }
        //    }
        //}
    }
}
