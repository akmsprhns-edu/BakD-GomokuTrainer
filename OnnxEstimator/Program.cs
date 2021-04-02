using GomokuLib;
using Microsoft.ML;
using Microsoft.ML.OnnxRuntime;
using Newtonsoft.Json;
using NLog;
using OnnxEstimatorLib;
using OnnxEstimatorLib.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace OnnxEstimator
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly bool _LOG = false;
        private static bool UseGpu = true;
        static int Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Models folder must be specified");
                    return -1;
                }
                string modelsDir = args[0];
                if (args.Contains("--gpu", StringComparer.OrdinalIgnoreCase))
                {
                    UseGpu = true;
                }

                modelsDir = Path.GetFullPath(modelsDir);
                ConfigureLogger(modelsDir);
                Logger.Info($"Searching models in directory {modelsDir}");
                var random = new Random();

                var AllModels = Directory.GetFiles(modelsDir, "*.*.onnx").Select(x => new OnnxModel() { Path = x }).ToList();

                var start = DateTime.Now;
                var models = AllModels;

                while (models.Count() > 1)
                {
                    var roundWinners = new ConcurrentBag<OnnxModel>();
                    var threads = new List<Thread>();
                    for (int i = 0; i < models.Count() - 1; i += 2)
                    {
                        var modelOneIndex = i;
                        var modelTwoIndex = i + 1;
                        threads.Add(new Thread(() =>
                        {
                            var modelOne = models[modelOneIndex];
                            var modelTwo = models[modelTwoIndex];
                            Logger.Info($" model {modelOne.Number} playing against model {modelTwo.Number}");
                            var winners = new List<OnnxModel>();
                            winners.Add(RunGameSession(modelOne, modelTwo));
                            winners.Add(RunGameSession(modelTwo, modelOne));


                            var modelOneWinCount = winners.Count(x => x == modelOne);
                            var modelTwoWinCount = winners.Count(x => x == modelTwo);

                            if (modelOneWinCount > modelTwoWinCount)
                            {
                                roundWinners.Add(modelOne);
                            }
                            else if (modelTwoWinCount > modelOneWinCount)
                            {
                                roundWinners.Add(modelTwo);
                            }
                            else
                            {
                                if (random.Next(2) == 0)
                                    roundWinners.Add(modelOne);
                                else
                                    roundWinners.Add(modelTwo);
                            }

                        }));
                    }

                    var batchSize = 32;
                    var batchNumber = 0;
                    while (true)
                    {
                        var threadBatch = threads.Skip(batchNumber * batchSize).Take(batchSize).ToList();
                        if (threadBatch.Count() == 0)
                            break;

                        foreach (var thread in threadBatch)
                            thread.Start();

                        foreach (var thread in threadBatch)
                            thread.Join();

                        ++batchNumber;
                    }

                    if (models.Count % 2 == 1)
                    {
                        roundWinners.Add(models.Last());
                    }
                    models = roundWinners.ToList();
                    models.ForEach(x => x.Score += 1);
                    Logger.Info($"Round winners ({models.Count}): {string.Join(", ", models.Select(x => x.Number))}");
                }
                var duration = DateTime.Now - start;
                Logger.Info($"###############DONE in {duration.TotalSeconds} seconds################");
                var result = new EstimatorResults();
                foreach (var model in AllModels.OrderBy(m => m.Number))
                {
                    result.Add(model.Score);
                }
                File.WriteAllText( Path.Combine(modelsDir,"result.json"), JsonConvert.SerializeObject(result));

                return 0;
            } catch (Exception e)
            {
                Logger.Fatal(e, "Exception occured, program terminated");
                throw e;
            }
        }

        public static void ConfigureLogger(string fileDir)
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console  yyyy-MM-dd_HH-mm-ss
            var layout = "${longdate} | ${message}";
            var fileName = Path.GetFullPath(Path.Combine(fileDir, $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log"));
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = fileName, Layout = layout};
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole") { Layout = layout };

            // Rules for mapping loggers to targets            
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
        }

        public static Player CreatePlayer(OnnxModel onnxModel)
        {
            var modelPath = onnxModel.Path;
            var playerName = Path.GetFileNameWithoutExtension(onnxModel.Path);

            InferenceSession inferenceSession;
            if(UseGpu)
                inferenceSession = new InferenceSession(modelPath, SessionOptions.MakeSessionOptionWithCudaProvider(0));
            else
                inferenceSession = new InferenceSession(modelPath);

            var treeSearch = new OnnxEstimatorTreeSearch(inferenceSession);
            return new Player( playerName, treeSearch);
        }

        public static OnnxModel RunGameSession(OnnxModel modelOne, OnnxModel modelTwo)
        {
            var playerOne = CreatePlayer(modelOne);
            var playerTwo = CreatePlayer(modelTwo);
            var gameState = GameState.NewGame();
            var gameSession = new GameSession(playerOne, playerTwo, gameState);
            var start = DateTime.Now;
            Logger.Info($"Starting game session ({modelOne.Number} vs {modelTwo.Number})");
            var gameResult = gameSession.Run(log: _LOG);
            var duration = DateTime.Now - start;
            string logMsg = $"Game session ({modelOne.Number} vs {modelTwo.Number}) ended in {duration} s. Game result: {gameResult}.\n" + gameSession.GameState.DrawBoard();
            if (gameResult == GameResult.WhiteWon)
            {
                logMsg += $"\nModel {gameSession.PlayerWhite.Name} won";
                Logger.Info(logMsg);
                return modelOne;
            }
            if (gameResult == GameResult.BlackWon)
            {
                logMsg += $"\nModel {gameSession.PlayerBlack.Name} won";
                Logger.Info(logMsg);
                return modelTwo;
            }
            if (gameResult == GameResult.Tie)
            {
                logMsg += "\nGame ended in tie";
                Logger.Info(logMsg);
                return null;
            }

            throw new Exception("Unsupported game result");
            

        }
    }
}
