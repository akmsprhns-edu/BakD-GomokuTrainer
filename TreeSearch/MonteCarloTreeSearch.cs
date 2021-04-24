using GomokuLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeSearchLib
{
    public class MonteCarloTreeSearch : TreeSearch
    {
        public static readonly int PLAYOUT_DEPTH = 40;
        public static readonly int ITERATIONS = 10000;

        protected override float EvaluateState(GameState gameState)
        {
            throw new NotImplementedException();
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
            return avgEval + Math.Sqrt(Math.Log(parentN) / childN);
            //return avgEval;
        }
        //private static GameTreeNode MCTSSelect(GameTreeNode node)
        //{
        //    if(node.Children is null)
        //    {
        //        return node;
        //    }
        //    double bestUCB = double.MinValue;
        //    GameTreeNode bestChild = null;
        //    foreach (var child in node.Children)
        //    {
        //        var ucb = UCB(child.Evals.Average(), node.Evals.Count(), child.Evals.Count());
        //        if (ucb > bestUCB)
        //        {
        //            bestUCB = ucb;
        //            bestChild = child;
        //        }
        //        if(bestUCB == double.MaxValue)
        //            break; //Stop if maximal value found
        //    }
        //    return MCTSSelect(bestChild);
        //}
        //private double MCTSPlayout(GameState sourceGameState, int maxDepth)
        //{
        //    var gameState = sourceGameState.Copy();
        //    for (var depth = 0; depth < maxDepth; ++depth)
        //    {
        //        var gameOver = gameState.IsGameOver();
        //        if (gameOver != null)
        //        {
        //            if (gameOver.Value == GameResult.WhiteWon)
        //            {
        //                //Console.WriteLine(gameState.DrawBoard());
        //                return 1;
        //            }
        //            else if (gameOver.Value == GameResult.BlackWon)
        //                return 0;
        //            else
        //                return 0.5;
        //        }

        //        //make random move
        //        var moves = GetMoves(gameState).ToList();
        //        var randomMove = moves.ElementAt(Random.Next(moves.Count()));
        //        gameState.MakeMoveInPlace(randomMove);
        //    }
        //    //return EvaluateState(gameState);
        //    return 0.5;

        //}

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
            //return EvaluateState(gameState);
            return gameState;

        }
        private static int turn = 0;
        private GameResult? MCTSRun(GameTreeNode node)
        {
            GameResult? playoutGameResult;
            if (node.GameState is null)
                throw new ArgumentNullException(nameof(node.GameState));

            if (node.Evals.Count == 0) // new node found
            {
                //evaluation = EvaluateState(node.GameState);
                //evaluation = MCTSPlayout(node.GameState, PLAYOUT_DEPTH); //New evaluation
                //if (!maximize)
                //    evaluation = 1 - evaluation;
                var playoutGameState = MCTSPlayout(node.GameState, PLAYOUT_DEPTH);
                playoutGameResult = playoutGameState.IsGameOver();
                //if (turn > 7)
                    //Console.WriteLine(playoutGameState.DrawBoard());
            } 
            else
            {
                var isGameOverResult = node.GameState.IsGameOver();
                if (isGameOverResult != null)
                {
                    playoutGameResult = isGameOverResult;
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
                        playoutGameResult = GameResult.Tie;
                    }
                    else
                    {
                        double bestUCB = double.MinValue;
                        GameTreeNode bestChild = null;
                        foreach (var child in node.Children)
                        {
                            var ucb = UCB(child.Value.Evals.DefaultIfEmpty().Average(), node.Evals.Count(), child.Value.Evals.Count());
                            if (ucb >= bestUCB)
                            {
                                bestUCB = ucb;
                                bestChild = child.Value;
                            }
                            if (bestUCB == double.MaxValue)
                                break; //Stop if maximal value found
                        }
                        //var bestChild = node.Children.ToList()[Random.Next(node.Children.Count())];
                        if (bestChild.GameState is null)
                        {
                            bestChild.GameState = node.GameState.MakeMove(bestChild.Move);
                        }

                        playoutGameResult = MCTSRun(bestChild); // Evaluation backpropagation
                    }
                }
            }
            //else
            //{ 
            //    if (node.Children is null)
            //    {
            //        node.Children = ExpandNode(node);
            //    }
            //    if (!node.Children.Any())
            //    {
            //        //no more moves, tie
            //        evaluation = 0.5;
            //    }
            //    else
            //    {
            //        double bestUCB = double.MinValue;
            //        GameTreeNode bestChild = null;
            //        foreach (var child in node.Children)
            //        {
            //            var ucb = UCB(child.Evals.DefaultIfEmpty().Average(), node.Evals.Count(), child.Evals.Count());
            //            if (ucb >= bestUCB)
            //            {
            //                bestUCB = ucb;
            //                bestChild = child;
            //            }
            //            if (bestUCB == double.MaxValue)
            //                break; //Stop if maximal value found
            //        }

            //        evaluation = EvaluateState(node.GameState);
            //        //evaluation = MCTSRun(bestChild, reverseEvaluation); // Evaluation backpropagation
            //    }
            //}

            double evaluation;

            if (playoutGameResult != null)
            {
                if (node.GameState.PlayerTurn == PlayerColor.Black)
                {
                    evaluation = playoutGameResult.Value switch
                    {
                        GameResult.WhiteWon => 1,
                        GameResult.BlackWon => -1,
                        GameResult.Tie => 0,
                        _ => throw new NotImplementedException()
                    };

                }
                else if (node.GameState.PlayerTurn == PlayerColor.White)
                {
                    evaluation = playoutGameResult.Value switch
                    {
                        GameResult.BlackWon => 1,
                        GameResult.WhiteWon => -1,
                        GameResult.Tie => 0,
                        _ => throw new NotImplementedException()
                    };
                } else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                evaluation = 0;
            }

            node.Evals.Add(evaluation);
            return playoutGameResult;
        }
        public GameTreeNode currentTreeNode = null;

        public override void MoveCurrentTreeNode(Move move)
        {
            if (currentTreeNode != null)
            {
                if (currentTreeNode.Children.TryGetValue(move, out var newNode) && newNode.GameState != null)
                {
                    Console.WriteLine("Current node changed." +
                        $"Previous node eval. for {(currentTreeNode.GameState.PlayerTurn == PlayerColor.White ? PlayerColor.Black : PlayerColor.White)}: {currentTreeNode.Evals.DefaultIfEmpty().Average()}." + 
                        $"New node eval. for {(newNode.GameState.PlayerTurn == PlayerColor.White ? PlayerColor.Black : PlayerColor.White)}: {newNode.Evals.DefaultIfEmpty().Average()}");
                    currentTreeNode = newNode;
                    
                    Console.WriteLine(currentTreeNode.GameState.DrawBoard());
                    Console.WriteLine($"MCTS evaluated moves for {currentTreeNode.GameState.PlayerTurn} : \n" + PrintMoveInfo(currentTreeNode.Children));
                }
                else
                {
                    //Console.WriteLine("Unable to coninue current tree");
                    currentTreeNode = null;
                }
            }
        }

        public override Move FindBestMove(GameState gameState, bool batch = true, int depth = 1)
        {
            turn++;
            var Maximize = gameState.PlayerTurn == PlayerColor.White ? true : false;
            if (currentTreeNode == null)
            {
                var gameTree = BuildTree(gameState, false, onlyPriorityMoves: true);
                currentTreeNode = gameTree.Root;
            }

            //Console.WriteLine("Current MCTS node: \n" + currentTreeNode.GameState.DrawBoard());

            for (int i = 0; i < ITERATIONS; i++)
            {
                MCTSRun(currentTreeNode);
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

            
                
            var maxN = currentTreeNode.Children.Values.Select(x => x.Evals.Count).Max();
            var bestNode = currentTreeNode.Children.First(x => x.Value.Evals.Count == maxN);
            
            Console.WriteLine("MCTS evaluated moves: \n" + PrintMoveInfo(currentTreeNode.Children));
            Console.WriteLine($"maxN={maxN}");
            Console.WriteLine($"BestNode count={bestNode.Value.Evals.Count()}, move={MoveToStr(bestNode.Value.Move)} or row {bestNode.Value.Move.Row}, col {bestNode.Value.Move.Column}");
            Console.WriteLine($"Best Node UCB = {UCB(bestNode.Value.Evals.DefaultIfEmpty().Average(), currentTreeNode.Evals.Count(), bestNode.Value.Evals.Count())}");
            Console.WriteLine($"Best node average evaluation {bestNode.Value.Evals.DefaultIfEmpty().Average()}");
            return bestNode.Value.Move;
        }
        public override string PrintCurrentStateMoveInfo()
        {
            if(currentTreeNode != null)
            {
                return PrintMoveInfo(currentTreeNode.Children);
            }
            return "";
        }
        public static string PrintMoveInfo(Dictionary<Move, GameTreeNode> dict)
        {
            if(dict == null || !dict.Any())
            {
                return "No childrens...";
            }
            var stringBuilder = new StringBuilder();
            var move = "";
            var count = "";
            var avgEval = "";
            var i = 0;
            foreach(var item in dict.ToList().OrderBy(x => x.Key.Column).ThenByDescending(x => x.Key.Row))
            {
                move += $"{MoveToStr(item.Key), -6}|";
                count += $"{item.Value.Evals.Count,-6}|";
                avgEval += $"{item.Value.Evals.DefaultIfEmpty().Average(),-6:.0000}|";
                if(i > 15)
                {
                    stringBuilder.AppendLine(move);
                    stringBuilder.AppendLine(count);
                    stringBuilder.AppendLine(avgEval);
                    move = "";
                    count = "";
                    avgEval = "";
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
            }
            return stringBuilder.ToString();
        }

        public static string MoveToStr(Move move)
        {
            return $"{(char)(move.Column + 65)}{15 - move.Row}";
        }
    }
}
