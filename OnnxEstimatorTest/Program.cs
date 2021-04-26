using GomokuLib;
using Microsoft.ML.OnnxRuntime;
using NLog;
using OnnxEstimatorLib;
using OnnxEstimatorLib.Models;
using System;
using System.IO;
using System.Linq;

namespace OnnxEstimatorTest
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly bool _LOG = false;
        private static bool UseGpu = false;
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
                modelsDir = Path.GetFullPath(modelsDir);
                ConfigureLogger(modelsDir);
                Logger.Info($"Searching models in directory {modelsDir}");
                var random = new Random();

                var AllModels = Directory.GetFiles(modelsDir, "*.*.onnx").Select(x => new OnnxModel() { Path = x }).ToList();

                var models = AllModels.Take(2).ToList();

                RunGameSession(models[0], models[1]);

                return 0;
            }
            catch (Exception e)
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
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = fileName, Layout = layout };
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
            if (UseGpu)
                inferenceSession = new InferenceSession(modelPath, SessionOptions.MakeSessionOptionWithCudaProvider(0));
            else
                inferenceSession = new InferenceSession(modelPath);

            var treeSearch = new OnnxEstimatorTreeSearch(inferenceSession);
            return new Player(playerName, treeSearch);
        }

        public static void RunGameSession(OnnxModel modelOne, OnnxModel modelTwo)
        {
            var playerFirst = CreatePlayer(modelOne);
            var playerSecond = CreatePlayer(modelTwo);
            var gameState = GameState.NewGame();

            Logger.Info($"Starting game session ({modelOne.Number} vs {modelTwo.Number})");

                Console.WriteLine($"Player {playerFirst.Name} plays first");
                Console.WriteLine($"Player {playerSecond.Name} plays second");

            Player currentPlayer;
            while (true)
            {
                currentPlayer = gameState.PlayerTurn switch
                {
                    PlayerColor.First => playerFirst,
                    PlayerColor.Second => playerSecond,
                    _ => throw new Exception("Unsupported player color")
                };

                var searchResults = currentPlayer.TreeSearch.GetEvaluatedMovesBatch(gameState);
                foreach (var searchResult in searchResults)
                {
                    Logger.Info($"#######\n Evaluation: {searchResult.Evaluation} \n" + searchResult.GameState.DrawBoard());
                    if(Console.ReadKey().Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }

                var playerMove = currentPlayer.TreeSearch.FindBestMove(gameState);
                gameState = gameState.MakeMove(playerMove.Row, playerMove.Column);
                //if (log)
                //{
                //    Console.WriteLine($"{currentPlayer.Name,-15} made move {playerMove.Row}, {playerMove.Column} (row, column)");
                //    Console.WriteLine(GameState.DrawBoard());
                //}
                var gameResult = gameState.IsGameOver();
                if (gameResult.HasValue)
                {
                    break;
                    
                }
            }

            Logger.Info("Game finished");
            
        }
    }
}
