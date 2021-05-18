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

namespace ComparisonSimulation
{
    public enum AIType
    {
        PureMCTS,
        NeuralMCTS
    }
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static int ThreadCount = 9;
        private static int MCTSIterationCount = 1500;
        private static string OnnxModelPath = "";
        private static bool _LOG = false;
        static async Task<int> Main(string[] args)
        {
            var TerminateProgram = false;
            try
            {
                if (args.Length < 2)
                {
                    Console.WriteLine("Two parameters required, onnx model path and output dir");
                    return -1;
                }

                OnnxModelPath = Path.GetFullPath(args[0]);
                string outputDir = Path.GetFullPath(args[1]);
                ConfigureLogger(Path.Combine(outputDir, "logs"));
                Logger.Info($"Onnx model path {OnnxModelPath}");
                Logger.Info($"Output directory {outputDir}");

                if (!File.Exists(OnnxModelPath))
                {
                    throw new FileNotFoundException("file not found", OnnxModelPath);
                }

                var random = new Random();

                var threads = new List<Thread>();
                var results = new ConcurrentQueue<Result>();
                var threadFailed = false;

                for (int i = 0; i < ThreadCount; ++i)
                {
                    threads.Add(new Thread(() =>
                    {
                        try
                        {
                            while (!TerminateProgram)
                            {

                                results.Enqueue(RunGameSession(AIType.NeuralMCTS, AIType.PureMCTS));
                                if (TerminateProgram)
                                    break;
                                results.Enqueue(RunGameSession(AIType.PureMCTS, AIType.NeuralMCTS));
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
                var outputFilePath = Path.GetFullPath(Path.Combine(outputDir, $"SimResults_{DateTime.Now:MM-dd_HH-mm-ss}.csv"));
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
                        while (results.TryDequeue(out var position))
                        {
                            await writer.WriteLineAsync(position.ToCsvString());
                            Logger.Info(++totalLines);
                        }
                        await writer.FlushAsync();

                        while (Console.KeyAvailable)
                        {
                            if (Console.ReadKey(false).Key == ConsoleKey.X)
                            {
                                TerminateProgram = true;
                            }
                            else
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

        public static Player CreatePlayer(AIType aiType)
        {
            if (aiType == AIType.PureMCTS)
            {
                var treeSearch = new MonteCarloTreeSearch(iterations: MCTSIterationCount, enableLogging: false);
                return new Player("PureMCTS", treeSearch);
            }
            else if (aiType == AIType.NeuralMCTS)
            {
                var modelPath = OnnxModelPath;
                var playerName = "NeuralMCTS";

                var inferenceSession = new InferenceSession(modelPath);

                var treeSearch = new OnnxEstimatorTreeSearch(inferenceSession, iterations: MCTSIterationCount, enableLogging: false);
                return new Player(playerName, treeSearch);
            } else
            {
                throw new Exception("Unsupported player type");
            }
        }

        public static Result RunGameSession(AIType firstPlayerType, AIType secondPlayerType)
        {
            var playerOne = CreatePlayer(firstPlayerType);
            var playerTwo = CreatePlayer(secondPlayerType);
            var gameState = GameState.NewGame();
            var gameSession = new GameSession(playerOne, playerTwo, gameState);
            var start = DateTime.Now;
            Logger.Info($"Starting game session ({firstPlayerType} vs {secondPlayerType})");
            var gameResult = gameSession.Run(log: _LOG);
            var duration = DateTime.Now - start;
            Logger.Info($"Game session ({firstPlayerType} vs {secondPlayerType}) ended in {duration} s. Game result: {gameResult}.\n" + gameSession.GameState.DrawBoard());

            return new Result()
            {
                FirstPlayer = firstPlayerType,
                SecondPlayer = secondPlayerType,
                GameResult = gameResult
            };
            
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
