using GomokuLib.Exceptions;
using System;

namespace GomokuLib
{
    public class GameState
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

        public void MakeMoveInPlace(Move move)
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

        public GameState MakeMove(Move move)
        {
            return MakeMove(move.Row, move.Column);
        }

        public GameState MakeMove(int row, int col)
        {
            if(!IsValidMove(row, col))
            {
                throw new GameStateException($"Impossible move, [{row},{col}] aledy occupied");
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
            return IsValidMove(row, col) && (IsInCentre(row, col) || Board.IsAnyAdjacent(row, col));
        }
        private const int halfSize = (Consts.BOARD_SIZE + 1) / 2;
        public bool IsInCentre(int row, int col)
        {
            return row >= halfSize - 1 && row <= halfSize + 1 && col >= halfSize - 1 && col <= halfSize + 1;
        }

        public StoneColor OccupiedBy(int row, int col)
        {
            return Board.OccupiedBy(row, col);
        }

        public bool[] GetBoardStateArray()
        {
            return Board.GetBoardStateArray();
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
    }
}
