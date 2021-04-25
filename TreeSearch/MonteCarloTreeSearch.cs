using GomokuLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeSearchLib
{
    public class MonteCarloTreeSearch : TreeSearch
    {
        public readonly int PLAYOUT_DEPTH;
        public readonly int ITERATIONS;


        public MonteCarloTreeSearch()
        {
            PLAYOUT_DEPTH = 999;
            ITERATIONS = 15000;
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
                playoutGameState = MCTSPlayout(node.GameState, PLAYOUT_DEPTH);
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
                        GameResult.WhiteWon => 1,
                        GameResult.BlackWon => -1,
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

            if (node.GameState.PlayerTurn == PlayerColor.Black)
            {
                node.Evals.Add(evaluation.Value);

            }
            else if (node.GameState.PlayerTurn == PlayerColor.White)
            {
                node.Evals.Add(-evaluation.Value);
            }
            else
            {
                throw new NotImplementedException();
            }
            return evaluation.Value;
        }
        public GameTreeNode currentTreeNode = null;

        public override void MoveCurrentTreeNode(PlayerMove move)
        {
            if (currentTreeNode != null)
            {
                if (currentTreeNode.Children.TryGetValue(move, out var newNode) && newNode.GameState != null)
                {
                    Console.WriteLine("Current node changed." +
                        $"Previous node eval.: {(currentTreeNode.GameState.PlayerTurn == PlayerColor.White ? -AVG(currentTreeNode.Evals) : AVG(currentTreeNode.Evals))};" + 
                        $"New node eval.: {(newNode.GameState.PlayerTurn == PlayerColor.White ? -AVG(newNode.Evals) : AVG(newNode.Evals))}.");
                    currentTreeNode = newNode;
                    
                    Console.WriteLine(currentTreeNode.GameState.DrawBoard());
                    Console.WriteLine($"MCTS evaluated moves for {currentTreeNode.GameState.PlayerTurn} : \n" + PrintMoveInfo(currentTreeNode.Children));

                    AllNodes.RemoveWhere(x => x.Moves.Count < currentTreeNode.Moves.Count);
                }
                else
                {
                    //Console.WriteLine("Unable to coninue current tree");
                    currentTreeNode = null;
                }
            }
        }

        public override PlayerMove FindBestMove(GameState gameState, bool batch = true, int depth = 1)
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
            Console.WriteLine($"BestNode count={bestNode.Value.Evals.Count()}, move={MoveToStr(bestNode.Key)} or row {bestNode.Key.Row}, col {bestNode.Key.Column}");
            Console.WriteLine($"Best Node UCB = {UCB(AVG(bestNode.Value.Evals), currentTreeNode.Evals.Count(), bestNode.Value.Evals.Count())}");
            Console.WriteLine($"Best node average evaluation {AVG(bestNode.Value.Evals)}");
            return bestNode.Key;
        }
        public override string PrintCurrentStateMoveInfo()
        {
            if(currentTreeNode != null)
            {
                return PrintMoveInfo(currentTreeNode.Children);
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
            var i = 0;
            foreach(var item in dict.ToList().OrderBy(x => x.Key.Column).ThenByDescending(x => x.Key.Row))
            {
                move += $"{MoveToStr(item.Key), -6}|";
                count += $"{item.Value.Evals.Count,-6}|";
                avgEval += $"{AVG(item.Value.Evals),-6:.0000}|";
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

        public static string MoveToStr(PlayerMove move)
        {
            return $"{(char)(move.Column + 65)}{15 - move.Row}";
        }
    }
}
