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

        public GameState MakeMove(int row, int col)
        {
            if(Board.OccupiedBy(row,col) != StoneColor.None)
            {
                throw new GameStateException($"Impossible move, [{row},{col}] aledy occupied");
            }

            var newBoardState = Board.MakeMove(row, col);
            return new GameState(newBoardState);

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
                if(gameState.OccupiedBy(row, col) == StoneColor.None)
                {
                    gameState = gameState.MakeMove(row, col);
                }
            }
            return gameState;
        }
        public string DrawBoard()
        {
            return Board.DrawBoard();
        }
    }
}
