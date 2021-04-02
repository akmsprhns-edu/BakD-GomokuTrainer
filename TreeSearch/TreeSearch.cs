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
        private readonly Random Random;
        public TreeSearch()
        {
            Random = new Random();
        }

        protected abstract float EvaluateState(GameState gameState);
        protected abstract List<float> EvaluateStates(IEnumerable<GameState> gameStates);
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
        public Move FindBestMoveMinMax(GameState gameState, int depth)
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
        public IEnumerable<(Move Move, GameState NewState)> GetMoves(GameState gameState)
        {
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
                        yield return (
                            Move: new Move()
                            {
                                Row = row,
                                Column = col
                            },
                            NewState: newState
                        );
                    }
                }
            }
        }

        public IEnumerable<SearchResult> GetEvaluatedMovesSequencial(GameState gameState)
        {
            return GetMoves(gameState).Select(x => new SearchResult()
            {
                Evaluation = EvaluateState(x.NewState),
                Move = x.Move,
                GameState = x.NewState
            });
        }
        public IEnumerable<SearchResult> GetEvaluatedMovesBatch(GameState gameState)
        {
            var moves = GetMoves(gameState).ToList();
            return EvaluateStates(moves.Select(x => x.NewState)).Zip(moves, (eval, move) => new SearchResult()
            {
                Evaluation = eval,
                Move = move.Move,
                GameState = move.NewState
            });
        }

        public Move FindBestMove(GameState gameState, bool batch = true)
        {
            var Maximize = gameState.PlayerTurn == PlayerColor.White ? true : false;

            List<SearchResult> searchResults;
            if (batch) 
                searchResults = GetEvaluatedMovesBatch(gameState).ToList();
            else
                searchResults = GetEvaluatedMovesSequencial(gameState).ToList();

            if (Maximize)
            {
                var maxEvaluation = searchResults.Select(x => x.Evaluation).Max();
                var maxMoves = searchResults.Where(x => x.Evaluation == maxEvaluation);
                if (maxMoves.Count() == 1)
                {
                    return maxMoves.First().Move;
                }
                else
                {
                    return maxMoves.Skip(Random.Next(maxMoves.Count())).First().Move;
                }
            }
            else
            {
                var minEvaluation = searchResults.Select(x => x.Evaluation).Min();
                var minMoves = searchResults.Where(x => x.Evaluation == minEvaluation);
                if (minMoves.Count() == 1)
                {
                    return minMoves.First().Move;
                }
                else
                {
                    return minMoves.Skip(Random.Next(minMoves.Count())).First().Move;
                }
            }
        }
    }
}
