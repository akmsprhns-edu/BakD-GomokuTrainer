using GomokuLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeSearchLib;

namespace GomokuWebUI
{
    public enum AIType
    {
        PureMonteCarlo
    }
    public class PvEGameSession
    {
        public string Guid { get; private set; }
        public GameState GameState { get; private set; }
        private TreeSearch AITreeSearch { get; set; }
        private PlayerColor PColor { get; set; }
        private PlayerColor AIColor { get => PColor == PlayerColor.White ? PlayerColor.Black : PlayerColor.White; }
        public PvEGameSession(string guid, AIType aiType)
        {
            Guid = guid;
            GameState = GameState.NewGame();
            PColor = PlayerColor.White;
            if(aiType == AIType.PureMonteCarlo)
            {
                AITreeSearch = new MonteCarloTreeSearch();
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

            MakeMove(new Move()
            {
                Row = row,
                Column = col
            });
        }

        public void MakeComputerMove()
        {
            if(GameState.PlayerTurn != AIColor)
            {
                throw new Exception("Move out of order");
            }
            var move = AITreeSearch.FindBestMove(GameState, true, 1);
            MakeMove(move);
        }

        private void MakeMove(Move move)
        {
            if(GameState.OccupiedBy(move.Row, move.Column) != StoneColor.None)
            {
                throw new Exception("Impossible move, already occupied");
            }
            AITreeSearch.MoveCurrentTreeNode(move);
            GameState = GameState.MakeMove(move);
        }
    }
}
