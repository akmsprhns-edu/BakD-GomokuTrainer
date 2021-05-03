using GomokuLib;
using Microsoft.ML.OnnxRuntime;
using NLog;
using OnnxEstimatorLib;
using OnnxEstimatorLib.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        private static int ThreadCount = 16;
        static int Main(string[] args)
        {
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
                            while (true)
                            {
                                RunGameSession();
                            }
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

                while (true)
                {
                    Task.Delay(TimeSpan.FromSeconds(1));
                    if (threadFailed)
                    {
                        throw new Exception("Error in thread occured");
                    }


                }
                
                return 0;
            }
            catch (Exception e)
            {
                Logger.Fatal(e, "Exception occured, program terminated");
                throw e;
            }
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

        public static Player CreatePlayer()
        {

            var treeSearch = new MonteCarloTreeSearch(enableLogging: false);
            return new Player("e", treeSearch);
        }

        public static IEnumerable<Position> RunGameSession()
        {
            var player = CreatePlayer();
            var gameState = GameState.NewGame();

            while (true)
            {

                var move = player.TreeSearch.FindBestMove(gameState);
                //foreach(var state in player.TreeSearch.)
                gameState = gameState.MakeMove(move.Row, move.Column);
                player.TreeSearch.MoveCurrentTreeNode(move);
                Logger.Info(gameState.DrawBoard());
                var gameResult = gameState.IsGameOver();
                if (gameResult.HasValue)
                {
                    break;
                }
            }
            throw new NotImplementedException();
            //Logger.Info("Game finished");
            //Logger.Info("Final game state:\n" + gameState.DrawBoard());
        }
    }
}
