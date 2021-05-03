using GomokuLib;
using Microsoft.ML.OnnxRuntime;
using NLog;
using OnnxEstimatorLib;
using OnnxEstimatorLib.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TreeSearchLib;

namespace TrainDatasetGenerator
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static int ThreadCount = 10;
        private static int MinEvalCount = 500;
        private static int MCTSIterationCount = 15000;
        static async Task<int> Main(string[] args)
        {
            var TerminateProgram = false;
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Output folder must be specified");
                    return -1;
                }

                string outputDir = args[0];
                outputDir = Path.GetFullPath(outputDir);
                ConfigureLogger(Path.Combine(outputDir,"logs"));
                Logger.Info($"Output directory {outputDir}");
                var random = new Random();

                var threads = new List<Thread>();
                var positions = new ConcurrentQueue<Position>();
                var threadFailed = false;

                for (int i = 0; i < ThreadCount; ++i)
                {
                    threads.Add(new Thread(() =>
                    {
                        try
                        {
                            while (!TerminateProgram)
                            {
                                foreach (var position in RunGameSession())
                                {
                                    positions.Enqueue(position);
                                    if (TerminateProgram)
                                        break;
                                }
                            }
                            Logger.Info("Finishing thread");
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                            threadFailed = true;
                        }
                    }));
                }

                foreach (var thread in threads)
                {
                    thread.Start();
                }

                //write positions to csv
                var outputFilePath = Path.GetFullPath(Path.Combine(outputDir, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv"));
                var totalLines = 0;
                using (var writer = new StreamWriter(outputFilePath))
                {
                    while (!TerminateProgram)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(0.1));
                        if (threadFailed)
                        {
                            throw new Exception("Error in thread occured");
                        }
                        while (positions.TryDequeue(out var position))
                        {
                            await writer.WriteLineAsync(position.ToCsvString());
                            Logger.Info(++totalLines);
                        }
                        await writer.FlushAsync();

                        while (Console.KeyAvailable)
                        {
                            if (Console.ReadKey(false).Key == ConsoleKey.X) {
                                TerminateProgram = true;
                            } else
                            {
                                Console.WriteLine("Press X to quit");
                            }
                        }
                    }
                    Logger.Info("Finishing writing to file");
                }

                foreach (var thread in threads)
                {
                    thread.Join();
                }

                return 0;
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "Exception occured, program terminated");
                throw e;
            }
        }

        public static Player CreatePlayer()
        {
            var treeSearch = new MonteCarloTreeSearch(iterations: MCTSIterationCount, enableLogging: false);
            return new Player("", treeSearch);
        }

        public static IEnumerable<Position> RunGameSession()
        {
            var player = CreatePlayer();
            var gameState = GameState.NewGame();

            while (true)
            {

                var move = player.TreeSearch.FindBestMove(gameState);
                foreach(var state in player.TreeSearch.CurrentTreeNode.Children.Where(x => x.Value.Evals.Count > MinEvalCount))
                {
                    yield return new Position()
                    {
                        Board = state.Value.GameState.GetBoardByteArray(),
                        PlayerTurn = state.Value.GameState.PlayerTurn,
                        EvalCount = state.Value.Evals.Count,
                        Eval = MonteCarloTreeSearch.EVAL(state.Value)
                    };
                }
                gameState = gameState.MakeMove(move.Row, move.Column);
                player.TreeSearch.MoveCurrentTreeNode(move);
                //Logger.Info(gameState.DrawBoard());
                var gameResult = gameState.IsGameOver();
                if (gameResult.HasValue)
                {
                    break;
                }
            }
            Logger.Info("Game finished");
            Logger.Info("Final game state:\n" + gameState.DrawBoard());
        }
        public static void ConfigureLogger(string logDir)
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console  yyyy-MM-dd_HH-mm-ss
            var layout = "${longdate} | ${message}";
            var fileName = Path.GetFullPath(Path.Combine(logDir, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log"));
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = fileName, Layout = layout };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole") { Layout = layout };

            // Rules for mapping loggers to targets            
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
        }
    }
}
