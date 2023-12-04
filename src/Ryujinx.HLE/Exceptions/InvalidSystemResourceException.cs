using System;

namespace Ryujinx.HLE.Exceptions
{
    public class InvalidSystemResourceException : Exception
    {
        public InvalidSystemResourceException(string message) : base(message) { }
    }
}
