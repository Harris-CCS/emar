using System;

namespace PulseCheck.Utilities
{
    public class Exceptions
    {
        public class NonFatalException : Exception
        {
            public NonFatalException(string message) : base(message)
            {
            }

            public NonFatalException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}
