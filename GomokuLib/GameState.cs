using GomokuLib.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GomokuLib
{
    public class GameState : IEquatable<GameState>
    {
        private BoardState Board;
        public static int BoardSize { get => Consts.BOARD_SIZE; }
        public PlayerColor PlayerTurn { get => Board.PlayerTurn; }
        private GameState()
        {
            Board = new BoardState();
        }
        private GameState(BoardState board)
        {
            Board = board;
        }

        public void MakeMoveInPlace(PlayerMove move)
        {
            MakeMoveInPlace(move.Row, move.Column);
        }

        public void MakeMoveInPlace(int row, int col)
        {
            if (!IsValidMove(row, col))
            {
                throw new GameStateException($"Impossible move, [{row},{col}] aledy occupied");
            }

            Board.MakeMoveInPlace(row, col);
        }

        public GameState MakeMove(PlayerMove move)
        {
            return MakeMove(move.Row, move.Column);
        }

        public GameState MakeMove(int row, int col)
        {
            if(!IsValidMove(row, col))
            {
                throw new GameStateException($"Impossible move, [{row},{col}] aledy occupied:\n" + DrawBoard());
            }

            var newBoardState = Board.MakeMove(row, col);
            return new GameState(newBoardState);
        }

        public GameState Copy()
        {
            return new GameState(Board.Copy());
        }
        public bool IsValidMove(int row, int col)
        {
            return Board.OccupiedBy(row, col) == StoneColor.None;
        }

        public bool IsValidPriorityMove(int row, int col)
        {
            return (IsInCentre(row, col) || Board.IsAnyAdjacent(row, col)) && IsValidMove(row, col);
        }

        public bool IsPriorityMove(int row, int col)
        {
            return IsInCentre(row, col) || Board.IsAnyAdjacent(row, col);
        }

        private const int centerIndex = (Consts.BOARD_SIZE + 1) / 2 - 1;
        public bool IsInCentre(int row, int col)
        {
            return row >= centerIndex - 1 && row <= centerIndex + 1 && col >= centerIndex - 1 && col <= centerIndex + 1;
        }

        public StoneColor OccupiedBy(int row, int col)
        {
            return Board.OccupiedBy(row, col);
        }

        public byte[] GetBoardByteArray()
        {
            return Board.GetBoardStateArray().Select(x => Convert.ToByte(x)).ToArray();
        }

        public float[] GetBoardFloatArray()
        {
            return Board.GetBoardStateArray().Select(x => x ? 1f : 0f).ToArray();
        }

        public IEnumerable<(int row, int colmun)> GetUnoccupiedPositions()
        {
            return Board.GetUnoccupiedPositions();
        }

        public GameResult? IsGameOver()
        {
            return Board.IsGameOver();
        }

        public static GameState NewGame()
        {
            return new GameState();
        }

        public static GameState GenerateRandomGameState(int? Seed = null)
        {
            Random rnd = Seed == null ? new Random() : new Random(Seed ?? 0);
            var gameState = new GameState();
            for (var i = 0; i < BoardSize * BoardSize; i++)
            {
                var row = rnd.Next(BoardSize);
                var col = rnd.Next(BoardSize);
                if(gameState.IsValidMove(row, col))
                {
                    gameState = gameState.MakeMove(row, col);
                }
            }
            return gameState;
        }

        public GameState MakeRandomMove(int? Seed = null)
        {
            Random rnd = Seed == null ? new Random() : new Random(Seed ?? 0);
            while(true)
            {
                var row = rnd.Next(BoardSize);
                var col = rnd.Next(BoardSize);
                if (IsValidMove(row, col))
                {
                    return MakeMove(row, col);
                }
            }
        }

        public string DrawBoard()
        {
            return Board.DrawBoard();
        }

        public StoneColor[,] Get2DArrary()
        {
            return Board.Get2DArrary();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GameState);
        }

        public bool Equals(GameState other)
        {
            return other != null &&
                   Board.Equals(other.Board);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Board);
        }
    }
}
