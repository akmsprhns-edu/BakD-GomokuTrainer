using System;
using System.Collections.Generic;

namespace GomokuLib
{
    public class PlayerMove : IEquatable<PlayerMove>
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public PlayerColor ByPlayer { get; set; }

        public PlayerMove(int row, int column, PlayerColor player)
        {
            Row = row;
            Column = column;
            ByPlayer = player;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PlayerMove);
        }

        public bool Equals(PlayerMove other)
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

        public static bool operator ==(PlayerMove left, PlayerMove right)
        {
            return EqualityComparer<PlayerMove>.Default.Equals(left, right);
        }

        public static bool operator !=(PlayerMove left, PlayerMove right)
        {
            return !(left == right);
        }
    }
}
