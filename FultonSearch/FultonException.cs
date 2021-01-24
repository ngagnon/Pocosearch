using System;

namespace FultonSearch
{
    public class FultonException : Exception
    {
        public FultonException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

