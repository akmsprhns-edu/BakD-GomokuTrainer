using System;
using System.Collections.Generic;

namespace GomokuLib
{
    public class Move : IEquatable<Move>
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public PlayerColor ByPlayer { get; set; }

        public Move(int row, int column, PlayerColor player)
        {
            Row = row;
            Column = column;
            ByPlayer = player;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Move);
        }

        public bool Equals(Move other)
        {
            return other != null &&
                   Row == other.Row &&
                   Column == other.Column &&
                   ByPlayer == other.ByPlayer;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column, ByPlayer);
        }

        public static bool operator ==(Move left, Move right)
        {
            return EqualityComparer<Move>.Default.Equals(left, right);
        }

        public static bool operator !=(Move left, Move right)
        {
            return !(left == right);
        }
    }
}
