using GomokuLib;
using Microsoft.ML.OnnxRuntime;
using NLog;
using OnnxEstimatorLib;
using OnnxEstimatorLib.Models;
using System;
using System.IO;
using System.Linq;
using TreeSearchLib;

namespace OnnxEstimatorTestPlay
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

                var model = AllModels.First();

                RunGameSession(model);

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

        public static Player CreateRealPlayer()
        {
            var treeSearch = new RealPlayerTreeSearch();
            return new Player("RealPlayer", treeSearch);
        }

        public static void RunGameSession(OnnxModel modelOne)
        {
            var rand = new Random();
            Player playerFirst;
            Player playerSecond;
            if (rand.NextDouble() > 0.5)
            {
                playerFirst = CreatePlayer(modelOne);
                playerSecond = CreateRealPlayer();
            } else
            {
                playerFirst = CreateRealPlayer();
                playerSecond = CreatePlayer(modelOne);
            }
            var gameState = GameState.NewGame();

            Logger.Info($"Starting game session ({playerFirst.Name} vs {playerSecond.Name})");

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

                //var searchResults = currentPlayer.TreeSearch.GetEvaluatedMovesBatch(gameState);
                //foreach (var searchResult in searchResults)
                //{
                //    Logger.Info($"#######\n Evaluation: {searchResult.Evaluation} \n" + searchResult.GameState.DrawBoard());
                //    if (Console.ReadKey().Key == ConsoleKey.Enter)
                //    {
                //        break;
                //    }
                //}
                Console.WriteLine("Suggested moves:");
                playerFirst.TreeSearch.PrintCurrentStateMoveInfo();
                playerSecond.TreeSearch.PrintCurrentStateMoveInfo();
                var playerMove = currentPlayer.TreeSearch.FindBestMove(gameState);
                gameState = gameState.MakeMove(playerMove.Row, playerMove.Column);
                playerFirst.TreeSearch.MoveCurrentTreeNode(playerMove);
                playerSecond.TreeSearch.MoveCurrentTreeNode(playerMove);
                //if (log)
                //{
                Console.WriteLine($"{currentPlayer.Name,-15} made move {playerMove.Row}, {playerMove.Column} (row, column)");
                Console.WriteLine(gameState.DrawBoard());
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
