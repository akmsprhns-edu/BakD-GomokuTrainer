using GomokuLib;
using Microsoft.ML.OnnxRuntime;
using NLog;
using OnnxEstimatorLib;
using OnnxEstimatorLib.Models;
using System;
using System.IO;
using System.Linq;
namespace SelfPlayTestSimulation
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly bool _LOG = false;
        private static bool UseGpu = false;
        private static int RunCount = 30;
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

                for (int i = 0; i < RunCount; i++)
                {
                    RunGameSession(model);
                }
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

        public static void RunGameSession(OnnxModel model)
        {
            var player = CreatePlayer(model);
            var gameState = GameState.NewGame();


            while (true)
            {

                var move = player.TreeSearch.FindBestMove(gameState);
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
    }
}
