using GomokuLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeSearchLib.Extensions;

namespace TreeSearchLib
{
    public class MonteCarloTreeSearch : TreeSearch
    {
        protected readonly int _iterations;
        protected readonly int _playoutDepth;
        protected readonly bool _enableLogging;

        public MonteCarloTreeSearch(int iterations = 10_000, int playoutDepth = 999, bool enableLogging = false)
        {
            _iterations = iterations;
            _playoutDepth = playoutDepth;
            _enableLogging = enableLogging;
        }

        protected override float EvaluateState(GameState gameState)
        {
            return 0;
        }

        protected override List<float> EvaluateStates(IEnumerable<GameState> gameStates)
        {
            throw new NotImplementedException();
        }

        public static double UCB(double avgEval, int parentN, int childN)
        {
            if (childN == 0)
            {
                return double.MaxValue;
            }
            return avgEval + Math.Sqrt(2*Math.Log(parentN) / childN);
            //return avgEval;
        }

        public static double AVG(IEnumerable<double> source)
        {
            return source.DefaultIfEmpty().Average();
            //var src = source.DefaultIfEmpty().ToList();
            //return Math.Sqrt(src.Sum(x => x) / (double)src.Count);
        }

        private GameState MCTSPlayout(GameState sourceGameState, int maxDepth)
        {
            var gameState = sourceGameState.Copy();
            for (var depth = 0; depth < maxDepth; ++depth)
            {
                var gameOver = gameState.IsGameOver();
                if (gameOver != null)
                {
                    return gameState;
                }

                //make random move
                var moves = GetMoves(gameState).ToList();
                var randomMove = moves.ElementAt(Random.Next(moves.Count()));
                gameState.MakeMoveInPlace(randomMove);
            }
            return gameState;

        }
        private static int turn = 0;
        private float MCTSRun(GameTreeNode node)
        {
            float? evaluation = null;
            GameResult? simulationResult = null;
            GameState playoutGameState = null;
            if (node.GameState is null)
                throw new ArgumentNullException(nameof(node.GameState));

            var isGameOverResult = node.GameState.IsGameOver();
            if (isGameOverResult != null)
            {
                simulationResult = isGameOverResult;
            }
            else if (node.Evals.Count == 0) // new node found
            {
                playoutGameState = MCTSPlayout(node.GameState, _playoutDepth);
                simulationResult = playoutGameState.IsGameOver();
            } 
            else
            {
                if (node.Children is null)
                {
                    node.Children = ExpandNode(node, false);
                }
                if (!node.Children.Any())
                {
                    //no more moves, tie
                    simulationResult = GameResult.Tie;
                }
                else
                {
                    double bestUCB = double.MinValue;
                    KeyValuePair<PlayerMove,GameTreeNode>? bestChild = null;
                    foreach (var child in node.Children)
                    {
                        var ucb = UCB(AVG(child.Value.Evals), node.Evals.Count(), child.Value.Evals.Count());
                        if (ucb >= bestUCB)
                        {
                            bestUCB = ucb;
                            bestChild = child;
                        }
                        if (bestUCB == double.MaxValue)
                            break; //Stop if maximal value found
                    }
                    if (bestChild.Value.Value.GameState is null)
                    {
                        bestChild.Value.Value.GameState = node.GameState.MakeMove(bestChild.Value.Key);
                    }

                    evaluation = MCTSRun(bestChild.Value.Value); // Evaluation backpropagation
                }

            }

            if (evaluation == null)
            {
                if (simulationResult != null)
                {
                    evaluation = simulationResult.Value switch
                    {
                        GameResult.FirstPlayerWon => 1,
                        GameResult.SecondPlayerWon => -1,
                        GameResult.Tie => 0,
                        _ => throw new NotImplementedException()
                    };
                }
                else if(playoutGameState != null)
                {
                    evaluation = EvaluateState(playoutGameState);
                } else
                {
                    throw new Exception("Something went wrong, unable to obtain evaluation");
                }
            }

            if (node.GameState.PlayerTurn == PlayerColor.Second)
            {
                node.Evals.Add(evaluation.Value);

            }
            else if (node.GameState.PlayerTurn == PlayerColor.First)
            {
                node.Evals.Add(-evaluation.Value);
            }
            else
            {
                throw new NotImplementedException();
            }
            return evaluation.Value;
        }

        public override void MoveCurrentTreeNode(PlayerMove move)
        {
            if (CurrentTreeNode != null)
            {
                if (CurrentTreeNode.Children.TryGetValue(move, out var newNode) && newNode.GameState != null)
                {
                    if (_enableLogging)
                    {
                        Console.WriteLine("Current node changed." +
                            $"Previous node eval.: {(CurrentTreeNode.GameState.PlayerTurn == PlayerColor.First ? -AVG(CurrentTreeNode.Evals) : AVG(CurrentTreeNode.Evals))};" +
                            $"New node eval.: {(newNode.GameState.PlayerTurn == PlayerColor.First ? -AVG(newNode.Evals) : AVG(newNode.Evals))}.");
                    }
                    CurrentTreeNode = newNode;
                    if (_enableLogging)
                    {
                        Console.WriteLine(CurrentTreeNode.GameState.DrawBoard());
                        Console.WriteLine($"MCTS evaluated moves for {CurrentTreeNode.GameState.PlayerTurn} : \n" + PrintMoveInfo(CurrentTreeNode.Children));
                    }

                    AllNodes.RemoveWhere(x => x.Moves.Count < CurrentTreeNode.Moves.Count);
                }
                else
                {
                    if (_enableLogging)
                        Console.WriteLine("Unable to coninue current tree");

                    CurrentTreeNode = null;
                }
            }
        }

        public override PlayerMove FindBestMove(GameState gameState, bool batch = true)
        {
            turn++;
            var Maximize = gameState.PlayerTurn == PlayerColor.First ? true : false;

            if (CurrentTreeNode != null && !CurrentTreeNode.GameState.Equals(gameState))
            {
                if (_enableLogging)
                    Console.WriteLine("MCTS FindBestMove: CurrentTreeNode.GameState NOT equal passed gameState. Resseting tree");
                CurrentTreeNode = null;
            }

            if (CurrentTreeNode == null)
            {
                var gameTree = BuildTree(gameState, false, onlyPriorityMoves: true);
                CurrentTreeNode = gameTree.Root;
            }

            //Console.WriteLine("Current MCTS node: \n" + currentTreeNode.GameState.DrawBoard());

            for (int i = 0; i < _iterations; i++)
            {
                MCTSRun(CurrentTreeNode);
            }
            //var positions = gameTree.Flatten().ToList();
            //Console.WriteLine($"total positions generated to evaluate move (all/with game state) {positions.Count()} / {positions.Where(x => x.GameState != null).Count()}");
            //gameTree.Root.Children = ExpandNode(gameTree.Root);
            //var count = gameTree.Root.Children.Count();
            //for (int i = 0; i < ITERATIONS; i++)
            //{
            //    var child = gameTree.Root.Children[Random.Next(count)];
            //    child.Evals.Add(EvaluateState(child.GameState));
            //}
            //gameTree.Root.Children.ForEach(x =>
            //{
            //    x.Evals.Add(EvaluateState(x.GameState));
            //});



            var maxN = CurrentTreeNode.Children.Values.Select(x => x.Evals.Count).Max();
            var bestNode = CurrentTreeNode.Children.First(x => x.Value.Evals.Count == maxN);
            //var maxEval = currentTreeNode.Children.Values.Select(x => x.Evals.Count).Max();
            //var bestNode = currentTreeNode.Children.MaxBy(child => 
            //    -AVG(child.Value.Children.MaxBy(subChilder => subChilder.Value.Evals.Count).Value.Evals)
            //);
            if (_enableLogging)
            {
                Console.WriteLine("MCTS evaluated moves: \n" + PrintMoveInfo(CurrentTreeNode.Children));
                Console.WriteLine($"maxN={maxN}");
                Console.WriteLine($"BestNode count={bestNode.Value.Evals.Count()}, move={bestNode.Key} or row {bestNode.Key.Row}, col {bestNode.Key.Column}");
                Console.WriteLine($"Best Node UCB = {UCB(AVG(bestNode.Value.Evals), CurrentTreeNode.Evals.Count(), bestNode.Value.Evals.Count())}");
                Console.WriteLine($"Best node average evaluation {AVG(bestNode.Value.Evals)}");
            }
            return bestNode.Key;
        }
        public override string PrintCurrentStateMoveInfo()
        {
            if(CurrentTreeNode != null)
            {
                return PrintMoveInfo(CurrentTreeNode.Children);
            }
            return "";
        }
        public static string PrintMoveInfo(Dictionary<PlayerMove, GameTreeNode> dict)
        {
            if(dict == null || !dict.Any())
            {
                return "No childrens...";
            }
            var stringBuilder = new StringBuilder();
            var move = "";
            var count = "";
            var avgEval = "";
            //var childMinEval = "";
            var i = 0;
            foreach(var item in dict.ToList().OrderBy(x => x.Key.Column).ThenByDescending(x => x.Key.Row))
            {
                move += $"{item.Key, -7}|";
                count += $"{item.Value.Evals.Count,-7}|";
                avgEval += $"{AVG(item.Value.Evals),-7:.0000}|";
                //childMinEval += $"{-AVG(item.Value.Children.DefaultIfEmpty().MaxBy(subChilder => subChilder.Value.Evals.Count).Value.Evals.DefaultIfEmpty()),-7:.0000}|";
                if(i > 15)
                {
                    stringBuilder.AppendLine(move);
                    stringBuilder.AppendLine(count);
                    stringBuilder.AppendLine(avgEval);
                    //stringBuilder.AppendLine(childMinEval);
                    move = "";
                    count = "";
                    avgEval = "";
                    //childMinEval = "";
                    i = 0;
                } else
                {
                    i++;
                }
            }
            if (!string.IsNullOrWhiteSpace(move))
            {
                stringBuilder.AppendLine(move);
                stringBuilder.AppendLine(count);
                stringBuilder.AppendLine(avgEval);
                //stringBuilder.AppendLine(childMinEval);
            }
            return stringBuilder.ToString();
        }
    }
}
