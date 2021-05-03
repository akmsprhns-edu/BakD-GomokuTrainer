using GomokuLib.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GomokuLib
{
    public class BoardState : IEquatable<BoardState>
    {
        private bool[] _data;
        private bool[] _adjacentData;
        private const int _dataSize = Consts.BOARD_SIZE * Consts.BOARD_SIZE * 2;
        private const int _adjacentDataSize = Consts.BOARD_SIZE * Consts.BOARD_SIZE;
        private (int row, int column, int color) lastMove = (-1, -1, -1);
        private static int[][] _patterns { get {
                return new int[][] {
                new int[] { 0, 2, 4, 6, 8},
                new int[] { 0, 30, 60, 90, 120 },
                new int[] { 0, 32, 64, 96, 128 },
                new int[] { 8, 36, 64, 92, 120 }
                };
            }
        }
        
        private int[] _matchedIndexes = new int[0];
        private const int _firstPlayerColor = 0;
        private const int _secondPlayerColor = 1;
        private int _moveCount;

        public PlayerColor PlayerTurn { get => _moveCount % 2 == 0 ? PlayerColor.First : PlayerColor.Second; }

        public BoardState()
        {
            _data = new bool[_dataSize];
            _adjacentData = new bool[_adjacentDataSize];
            _moveCount = 0;
        }

        private BoardState(bool[] data, bool[] adjacentData, int moveNumber)
        {
            if (data.Length != _dataSize)
                throw new BoardStateException($"Provided data length ({data.Length}) doesn't match required array size ({_dataSize})");

            _data = data;
            _adjacentData = adjacentData;
            _moveCount = moveNumber;
        }

        private static int Index(int row, int column, int color)
        {
            return (column + row * Consts.BOARD_SIZE) * Consts.PLAYER_COUNT + color;
        }
        private static int Index(int row, int column)
        {
            return row * Consts.BOARD_SIZE + column;
        }

        private static (int row, int column) Position(int index)
        {
            return (index / Consts.BOARD_SIZE, index % Consts.BOARD_SIZE);
        }

        public BoardState Copy()
        {
            return new BoardState(_data.Clone() as bool[], _adjacentData.Clone() as bool[], _moveCount);
        }

        private void SetInPlace(int row, int column, int color)
        {
            _data[Index(row, column, color)] = true;
            SetAdjecentDataInPlace(row, column);
            lastMove = (row: row, column: column, color: color);
            _moveCount += 1;
        }

        public static readonly int[] adjacentDelta = new int[] { -1, 0, 1 };
        private void SetAdjecentDataInPlace(int row, int col)
        {
            foreach (int rowDelta in adjacentDelta)
                foreach (int colDelta in adjacentDelta)
                {
                    var r = row + rowDelta;
                    var c = col + colDelta;
                    if (r > Consts.BOARD_SIZE - 1 || r < 0 || c > Consts.BOARD_SIZE - 1 || c < 0) continue;
                    _adjacentData[Index(r,c)] = true;
                }
        }

        private BoardState Set(int row, int column, int color)
        {
            var newBoard = new BoardState(_data.Clone() as bool[], _adjacentData.Clone() as bool[], _moveCount);
            newBoard.SetInPlace(row, column, color);
            return newBoard;
        }

        public void MakeMoveInPlace(int x, int y)
        {
            if (PlayerTurn == PlayerColor.First)
                SetInPlace(x, y, _firstPlayerColor);
            else
                SetInPlace(x, y, _secondPlayerColor);
        }

        public BoardState MakeMove(int x, int y)
        {
            if (PlayerTurn == PlayerColor.First)
                return Set(x, y, _firstPlayerColor);
            else
                return Set(x, y, _secondPlayerColor);
        }

        private bool Get(int row, int column, int color)
        {
            return _data[Index(row, column, color)];
        }

        public StoneColor OccupiedBy(int row, int col)
        {
            if (Get(row, col, _secondPlayerColor))
            {
                return StoneColor.Second;
            }
            else if (Get(row, col, _firstPlayerColor))
            {
                return StoneColor.First;
            }
            return StoneColor.None;
        }

        public IEnumerable<(int row, int colmun)> GetUnoccupiedPositions()
        {
            for (int i = 0; i < Consts.BOARD_SIZE * Consts.BOARD_SIZE; i++)
            {
                if (!_data[i * 2] && !_data[i * 2 + 1]) yield return Position(i);
            }
        }

        public bool IsAnyAdjacent(int row, int col)
        {
            return _adjacentData[Index(row, col)];
        }

        public bool[] GetBoardStateArray()
        {
            return _data;
        }

        //public bool[] GetReversedBoardStateArray()
        //{
        //    var reversedData = new bool[_data.Length];
        //    for (var i = 0; i < _data.Length; i += 2)
        //    {
        //        reversedData[i] = _data[i + 1];
        //        reversedData[i + 1] = _data[i];
        //    }
        //    return reversedData;
        //}

        public GameResult? IsGameOver()
        {
            if(_moveCount == Consts.BOARD_SIZE * Consts.BOARD_SIZE)
            {
                return GameResult.Tie;
            }

            foreach(var pattern in _patterns)
            {
                var searchResult = SearchLocalPattern(pattern);
                switch (searchResult)
                {
                    case PlayerColor.Second:
                        return GameResult.SecondPlayerWon;
                    case PlayerColor.First:
                        return GameResult.FirstPlayerWon;
                }
            }
            return null;
        }

        public PlayerColor? SearchPattern(int[] pattern)
        {
            var patternLen = pattern.Max();
            var patternMaxCols = pattern.Max(x => x % Consts.BOARD_SIZE) + 1;

            for (var dataIndex = 0; dataIndex < _data.Length - patternLen; dataIndex++)
            {
                if ((dataIndex % (Consts.BOARD_SIZE * 2)) > (Consts.BOARD_SIZE * 2) - patternMaxCols)
                    continue;

                var matchFound = true;
                foreach (var patternIndex in pattern)
                {
                    if (!_data[dataIndex + patternIndex])
                    {
                        matchFound = false;
                        break;
                    }
                }
                if (matchFound)
                {
                    _matchedIndexes = pattern.Select(x => x + dataIndex).ToArray();
                    var playerWon = dataIndex % 2 == _firstPlayerColor ? PlayerColor.First : PlayerColor.Second;
                    return playerWon;
                }
            }
            return null;
        }

        public PlayerColor? SearchLocalPattern(int[] pattern)
        {
            var lastMoveIndex = Index(lastMove.row, lastMove.column, lastMove.color);
            var patternMaxCols = pattern.Max(x => x % Consts.BOARD_SIZE) + 1;
            var patternLen = pattern.Max();
            foreach (var patternOffset in pattern)
            {
                var dataIndex = lastMoveIndex - patternOffset;
                if (dataIndex < 0 || dataIndex >= _data.Length - patternLen || (dataIndex % (Consts.BOARD_SIZE * 2)) > (Consts.BOARD_SIZE * 2) - patternMaxCols)
                    continue;

                var matchFound = true;
                foreach (var patternIndex in pattern)
                {
                    if (!_data[dataIndex + patternIndex])
                    {
                        matchFound = false;
                        break;
                    }
                }
                if (matchFound)
                {
                    _matchedIndexes = pattern.Select(x => x + dataIndex).ToArray();
                    var playerWon = lastMove.color == _firstPlayerColor ? PlayerColor.First : PlayerColor.Second;
                    return playerWon;
                }
            }
            return null;
        }

        public string DrawBoard()
        {
            StringBuilder sb = new StringBuilder();
            for (int row = 0; row < Consts.BOARD_SIZE; row++)
            {
                if(Consts.BOARD_SIZE - row < 10)
                        sb.Append(' ');
                sb.Append(Consts.BOARD_SIZE - row);
                for (int column = 0; column < Consts.BOARD_SIZE; column++)
                {
                    var highlight = false;
                    if (_matchedIndexes.Contains(Index(row, column, _firstPlayerColor)) || _matchedIndexes.Contains(Index(row, column, _secondPlayerColor)))
                        highlight = true;

                    switch (OccupiedBy(row, column))
                    {
                        case StoneColor.First:
                            sb.Append(highlight ? 'X' : 'x');
                            break;
                        case StoneColor.Second:
                            sb.Append(highlight ? 'O' : 'o');
                            break;
                        case StoneColor.None:
                            if(IsAnyAdjacent(row, column))
                                sb.Append('.');
                            else
                                sb.Append(' ');
                            break;
                        default:
                            throw new BoardStateException("Unsupported board state");
                    }
                }
                sb.Append('\n');
            }
            sb.Append("  ");
            for (int column = 0; column < Consts.BOARD_SIZE; column++)
            {
                sb.Append((char)(65 + column));
            }

            return sb.ToString();
        }

        public StoneColor[,] Get2DArrary()
        {
            var array = new StoneColor[Consts.BOARD_SIZE, Consts.BOARD_SIZE];
            for (int row = 0; row < Consts.BOARD_SIZE; row++)
            {
                for (int column = 0; column < Consts.BOARD_SIZE; column++)
                {
                    //var highlight = false;
                    //if (_matchedIndexes.Contains(Index(row, column, _firstPlayerColor)) || _matchedIndexes.Contains(Index(row, column, _secondPlayerColor)))
                    //    highlight = true;

                    switch (OccupiedBy(row, column))
                    {
                        case StoneColor.First:
                            array[row, column] = StoneColor.First;
                            break;
                        case StoneColor.Second:
                            array[row, column] = StoneColor.Second;
                            break;
                        case StoneColor.None:
                            array[row, column] = StoneColor.None;
                            break;
                        default:
                            throw new BoardStateException("Unsupported board state");
                    }
                }
            }

            return array;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BoardState);
        }

        public bool Equals(BoardState other)
        {
            return other != null &&
                   _moveCount == other._moveCount && 
                   Enumerable.SequenceEqual(_data, other._data);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_moveCount);
        }
    }
}
