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
        protected readonly bool _enableLogging;
        public MinimaxTreeSearch(int depth, bool enableLogging = false)
        {
            _depth = depth;
            _enableLogging = enableLogging;
        }

        public override void MoveCurrentTreeNode(PlayerMove move)
        {
            //if (CurrentTreeNode != null)
            //{
            //    if (CurrentTreeNode.Children.TryGetValue(move, out var newNode) && newNode.GameState != null)
            //    {
            //        if (_enableLogging)
            //        {
            //            Console.WriteLine("Current node changed." +
            //                $"Previous node eval.: {CurrentTreeNode.Evaluation}; " +
            //                $"New node eval.: {newNode.Evaluation}.");
            //        }
            //        CurrentTreeNode = newNode;
            //        if (_enableLogging)
            //        {
            //            Console.WriteLine(CurrentTreeNode.GameState.DrawBoard());
            //            Console.WriteLine($"NeuralMinimax evaluated moves for {CurrentTreeNode.GameState.PlayerTurn} : \n" + PrintMoveInfo(CurrentTreeNode.Children));
            //        }

            //        AllNodes.RemoveWhere(x => x.Moves.Count < CurrentTreeNode.Moves.Count);
            //    }
            //    else
            //    {
            //        if (_enableLogging)
            //            Console.WriteLine("Unable to coninue current tree");

            //        CurrentTreeNode = null;
            //    }
            //}
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
                Console.WriteLine($"\nboard evaluation {item.Eval}:\n {item.Node.GameState.DrawBoard()}");
            }
            return gameTree;
        }

        public IEnumerable<SearchResult> GetEvaluatedMovesMinMax(GameState gameState, bool maximize, int depth = 1)
        {
            var tree = EvaluateTree(BuildTree(gameState, true, depth));
            CurrentTreeNode = tree.Root;

            EvaluateNode(tree.Root, maximize);

            if (_enableLogging)
            {
                Console.WriteLine("NeuralMinimax evaluated moves: \n" + PrintMoveInfo(CurrentTreeNode.Children));
                Console.WriteLine("Position: \n" + gameState.DrawBoard());
            }
            return tree.Root.Children.Select(x => new SearchResult()
            {
                Evaluation = x.Value.Evaluation.Value,
                GameState = x.Value.GameState,
                Move = x.Key
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

        public override string PrintCurrentStateMoveInfo()
        {
            if (CurrentTreeNode != null)
            {
                return PrintMoveInfo(CurrentTreeNode.Children);
            }
            return "";
        }
        public static string PrintMoveInfo(Dictionary<PlayerMove, GameTreeNode> dict)
        {
            if (dict == null || !dict.Any())
            {
                return "No childrens...";
            }
            var stringBuilder = new StringBuilder();
            var move = "";
            var avgEval = "";
            var i = 0;
            foreach (var item in dict.ToList().OrderBy(x => x.Key.Column).ThenByDescending(x => x.Key.Row))
            {
                move += $"{item.Key,-7}|";
                avgEval += $"{item.Value.Evaluation,-7:.0000}|";
                if (i > 15)
                {
                    stringBuilder.AppendLine(move);
                    stringBuilder.AppendLine(avgEval);
                    move = "";
                    avgEval = "";
                    i = 0;
                }
                else
                {
                    i++;
                }
            }
            if (!string.IsNullOrWhiteSpace(move))
            {
                stringBuilder.AppendLine(move);
                stringBuilder.AppendLine(avgEval);
            }
            return stringBuilder.ToString();
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
