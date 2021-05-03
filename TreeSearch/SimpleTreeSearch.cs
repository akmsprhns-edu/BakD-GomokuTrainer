using GomokuLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TreeSearchLib.Extensions;

namespace TreeSearchLib
{
    public class SimpleTreeSearch : TreeSearch
    {
        
        public IEnumerable<SearchResult> GetEvaluatedMovesSequencial(GameState gameState, bool onlyPriorityMoves = true)
        {
            var moves = GetMoves(gameState, priority: onlyPriorityMoves).ToList();
            var states = moves.Select(m => gameState.MakeMove(m)).ToList();
            return states.Select(s => EvaluateState(s)).ZipThree(states, moves, (eval, state, move) => new SearchResult()
            {
                Evaluation = eval,
                Move = move,
                GameState = state
            });
        }
        public IEnumerable<SearchResult> GetEvaluatedMovesBatch(GameState gameState, bool onlyPriorityMoves = true)
        {
            var moves = GetMoves(gameState, priority: onlyPriorityMoves).ToList();
            var states = moves.Select(m => gameState.MakeMove(m)).ToList();
            return EvaluateStates(states).ZipThree(states, moves, (eval, state, move) => new SearchResult()
            {
                Evaluation = eval,
                Move = move,
                GameState = state
            });
        }

        public override PlayerMove FindBestMove(GameState gameState, bool batch = true)
        {
            var Maximize = gameState.PlayerTurn == PlayerColor.First ? true : false;

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
