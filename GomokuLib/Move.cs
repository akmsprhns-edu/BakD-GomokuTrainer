using System;
using System.Collections.Generic;

namespace GomokuLib
{
    public class Move : IEquatable<Move>
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Move);
        }

        public bool Equals(Move other)
        {
            return other != null &&
                   Row == other.Row &&
                   Column == other.Column;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column);
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
