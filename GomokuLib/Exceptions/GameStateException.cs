using System;

namespace GomokuLib.Exceptions
{
    public class GameStateException : Exception
    {
        public GameStateException()
        {

        }
        public GameStateException(string message) 
            :base(message)
        {

        }
        public GameStateException(string message, Exception inner)
            :base(message, inner)
        {

        }
    }
}
