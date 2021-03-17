using System;
using System.Collections.Generic;
using System.Text;

namespace GomokuLib.Exceptions
{
    public class BoardStateException : GameStateException
    {
        public BoardStateException()
        {

        }
        public BoardStateException(string message)
            : base(message)
        {

        }
        public BoardStateException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
