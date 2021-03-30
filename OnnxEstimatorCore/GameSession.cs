using GomokuLib;
using System;

namespace OnnxEstimatorLib
{
    public class GameSession
    {
        public Player PlayerWhite { get; }
        public Player PlayerBlack { get; }

        public GameState GameState { get; private set; }

        public GameSession(Player playerWhite, Player playerBlack, GameState gameState)
        {
            PlayerWhite = playerWhite;
            PlayerBlack = playerBlack;
            GameState = gameState;
        }
        public GameResult Run(bool log = false)
        {
            if (log)
            {
                Console.WriteLine($"Player {PlayerWhite.Name} play as white");
                Console.WriteLine($"Player {PlayerBlack.Name} play as black");
            }
            Player currentPlayer;
            while (true)
            {
                currentPlayer = GameState.PlayerTurn switch
                {
                    PlayerColor.White => PlayerWhite,
                    PlayerColor.Black => PlayerBlack,
                    _ => throw new Exception("Unsupported player color")
                };

                var playerMove = currentPlayer.TreeSearch.FindBestMoveV1(GameState);
                GameState = GameState.MakeMove(playerMove.Row, playerMove.Column);
                if (log)
                {
                    Console.WriteLine($"{currentPlayer.Name, - 15} made move {playerMove.Row}, {playerMove.Column} (row, column)");
                    Console.WriteLine(GameState.DrawBoard());
                }
                var gameResult = GameState.IsGameOver();
                if(gameResult.HasValue)
                {
                    return gameResult.Value;
                }
            }
        }
    }
}
