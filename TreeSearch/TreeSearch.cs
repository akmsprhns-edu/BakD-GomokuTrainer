using GomokuLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeSearchLib
{
    public abstract class TreeSearch
    {
        public TreeSearch()
        {

        }

        protected abstract float EvaluateState(GameState gameState);

        protected float MinMaxSearch(int depth, GameState sourceState)
        {
            var Maximize = sourceState.PlayerTurn == PlayerColor.White ? true : false;
            var gameResult = sourceState.IsGameOver();
            if (gameResult == GameResult.WhiteWon)
            {
                if (sourceState.PlayerTurn == PlayerColor.White)
                    throw new Exception("Something went wrong, white won, but its whites turn");

                return 1;
            }
            else if (gameResult == GameResult.BlackWon)
            {
                if (sourceState.PlayerTurn == PlayerColor.Black)
                    throw new Exception("Something went wrong, black won, but its blacks turn");

                return 0;
            }
            else if(gameResult == GameResult.Tie || depth == 0) 
            {
                return EvaluateState(sourceState);
            }
            else
            {
                var childEvals = new List<float>();
                for (var row = 0; row < GameState.BoardSize; row++)
                {
                    for (var col = 0; col < GameState.BoardSize; col++)
                    {
                        if (sourceState.OccupiedBy(row, col) != StoneColor.None) {
                            continue;
                        }
                        else
                        {
                            var newState = sourceState.MakeMove(row, col);
                            childEvals.Add(MinMaxSearch(depth - 1, newState));
                        }
                    }
                }
                if(childEvals.Count == 0)
                {
                    throw new Exception("No moves found");
                }
                if (Maximize)
                {
                    return childEvals.Max();
                } else
                {
                    return childEvals.Min();
                }
            }
        }

        public Move FindBestMove(GameState gameState, int depth)
        {
            var Maximize = gameState.PlayerTurn == PlayerColor.White ? true : false;
            var searchResults = new List<SearchResult>();
            for (var row = 0; row < GameState.BoardSize; row++)
            {
                for (var col = 0; col < GameState.BoardSize; col++)
                {
                    if (gameState.OccupiedBy(row, col) != StoneColor.None)
                    {
                        continue;
                    }
                    else
                    {
                        var newState = gameState.MakeMove(row, col);
                        searchResults.Add( new SearchResult(){
                            Evaluation = MinMaxSearch(depth - 1, newState),
                            Move = new Move()
                            {
                                Row = row,
                                Column = col
                            }
                        });
                    }
                }
            }
            if (Maximize)
            {
                return searchResults.Aggregate((acc, x) => x.Evaluation > acc.Evaluation ? x : acc).Move;
            }
            else
            {
                return searchResults.Aggregate((acc, x) => x.Evaluation < acc.Evaluation ? x : acc).Move;
            }
        }
    }
}
