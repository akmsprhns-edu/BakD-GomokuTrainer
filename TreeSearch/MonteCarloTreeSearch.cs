using GomokuLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeSearchLib
{
    public abstract class MonteCarloTreeSearch : TreeSearch
    {
        public static readonly int PLAYOUT_DEPTH = 1;
        public static readonly int ITERATIONS = 150;
        public static double UCB(double avgEval, int parentN, int childN)
        {
            return avgEval;
            if (childN == 0)
            {
                return double.MaxValue;
            }
            return avgEval + Math.Sqrt(2 * Math.Log(parentN) / childN);
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
        private double MCTSPlayout(GameState gameState, int depth)
        {
            if(depth == 0)
                return EvaluateState(gameState);
            var gameOver = gameState.IsGameOver();
            if(gameOver != null)
            {
                if (gameOver.Value == GameResult.WhiteWon)
                    return 1;
                else if (gameOver.Value == GameResult.BlackWon)
                    return 0;
                else
                    return 0.5;
            }

            //make random move
            return MCTSPlayout(gameState.MakeRandomMove(), depth - 1);
        }
        private double MCTSRun(GameTreeNode node, bool reverseEvaluation)
        {
            double evaluation = 0;
            //var child = 
            //evaluation = EvaluateState(child.GameState);
            if (node.GameState is null)
                throw new ArgumentNullException(nameof(node.GameState));

            if (node.Evals.Count == 0) // new node found
            {
                evaluation = EvaluateState(node.GameState);
                //evaluation = MCTSPlayout(node.GameState, PLAYOUT_DEPTH); //New evaluation
                if (reverseEvaluation)
                    evaluation = 1 - evaluation;
            } else
            {
                if (node.Children is null)
                {
                    node.Children = ExpandNode(node, false);
                }
                if (!node.Children.Any())
                {
                    //no more moves, tie
                    evaluation = 0.5;
                } 
                else
                {
                    //double bestUCB = double.MinValue;
                    //GameTreeNode bestChild = null;
                    //foreach (var child in node.Children)
                    //{
                    //    var ucb = UCB(child.Evals.DefaultIfEmpty().Average(), node.Evals.Count(), child.Evals.Count());
                    //    if (ucb >= bestUCB)
                    //    {
                    //        bestUCB = ucb;
                    //        bestChild = child;
                    //    }
                    //    if (bestUCB == double.MaxValue)
                    //        break; //Stop if maximal value found
                    //}
                    var bestChild = node.Children.ToList()[Random.Next(node.Children.Count())];
                    if(bestChild.Value.GameState is null)
                    {
                        bestChild.Value.GameState = node.GameState.MakeMove(bestChild.Key);
                    }

                    evaluation = MCTSRun(bestChild.Value, reverseEvaluation); // Evaluation backpropagation
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
            node.Evals.Add(evaluation);
            return evaluation;
        }

        public override Move FindBestMove(GameState gameState, bool batch = true, int depth = 1)
        {
            var Maximize = gameState.PlayerTurn == PlayerColor.White ? true : false;
            var gameTree = BuildTree(gameState, false);

            for (int i = 0; i < ITERATIONS; i++)
            {
                MCTSRun(gameTree.Root, !Maximize);
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

            var maxN = gameTree.Root.Children.Values.Select(x => x.Evals.Count()).Max();
            var bestNode = gameTree.Root.Children.Values.First(x => x.Evals.Count() == maxN);

            return bestNode.Move;
        }
    }
}
