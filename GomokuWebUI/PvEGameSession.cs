using GomokuLib;
using Microsoft.ML.OnnxRuntime;
using OnnxEstimatorLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeSearchLib;

namespace GomokuWebUI
{
    public enum AIType
    {
        PureMonteCarlo,
        MCTSAndNeuralNet
    }
    public class PvEGameSession
    {
        public string Guid { get; private set; }
        public GameState GameState { get; private set; }
        public Dictionary<string, int> Moves { get; private set; }
        private TreeSearch AITreeSearch { get; set; }
        private PlayerColor PColor { get; set; }
        private PlayerColor AIColor { get => PColor == PlayerColor.First ? PlayerColor.Second : PlayerColor.First; }
        public PvEGameSession(string guid, AIType aiType)
        {
            Guid = guid;
            GameState = GameState.NewGame();
            Moves = new Dictionary<string, int>();
            PColor = PlayerColor.First;
            if(aiType == AIType.PureMonteCarlo)
            {
                AITreeSearch = new MonteCarloTreeSearch(enableLogging: true);
            }
            else if (aiType == AIType.MCTSAndNeuralNet)
            {
                AITreeSearch = new OnnxEstimatorTreeSearch(new InferenceSession(@"C:\Projects\OnnxEstimator\ModelsWebUI\model.onnx"));
            }
            else
            {
                throw new NotSupportedException($"AI type {aiType} not supported");
            }
                
        }

        public void MakePlayerMove(int row, int col)
        {
            if (GameState.PlayerTurn != PColor)
            {
                throw new Exception("Move out of order");
            }

            MakeMove(new PlayerMove(row, col, PColor));
        }

        public void MakeComputerMove()
        {
            if(GameState.PlayerTurn != AIColor)
            {
                throw new Exception("Move out of order");
            }
            var move = AITreeSearch.FindBestMove(GameState, true);
            MakeMove(move);
        }

        private void MakeMove(PlayerMove move)
        {
            if(GameState.OccupiedBy(move.Row, move.Column) != StoneColor.None)
            {
                throw new Exception("Impossible move, already occupied");
            }
            AITreeSearch.MoveCurrentTreeNode(move);
            GameState = GameState.MakeMove(move);
            Moves[$"{move.Row}-{move.Column}"] = Moves.Count + 1;
        }
    }
}
