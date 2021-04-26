using GomokuLib;
using System;

namespace OnnxEstimatorLib
{
    public class GameSession
    {
        public Player PlayerFirst { get; }
        public Player PlayerSecond { get; }

        public GameState GameState { get; private set; }

        public GameSession(Player playerFirst, Player playerSecond, GameState gameState)
        {
            PlayerFirst = playerFirst;
            PlayerSecond = playerSecond;
            GameState = gameState;
        }
        public GameResult Run(bool log = false)
        {
            if (log)
            {
                Console.WriteLine($"Player {PlayerFirst.Name} plays first");
                Console.WriteLine($"Player {PlayerSecond.Name} plays second");
            }
            Player currentPlayer;
            while (true)
            {
                currentPlayer = GameState.PlayerTurn switch
                {
                    PlayerColor.First => PlayerFirst,
                    PlayerColor.Second => PlayerSecond,
                    _ => throw new Exception("Unsupported player color")
                };

                var playerMove = currentPlayer.TreeSearch.FindBestMove(GameState, batch: false, depth: 2);
                GameState = GameState.MakeMove(playerMove.Row, playerMove.Column);
                PlayerFirst.TreeSearch.MoveCurrentTreeNode(playerMove);
                PlayerSecond.TreeSearch.MoveCurrentTreeNode(playerMove);
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
