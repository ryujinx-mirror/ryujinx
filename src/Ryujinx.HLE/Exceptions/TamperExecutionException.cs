using System;

namespace Ryujinx.HLE.Exceptions
{
    public class TamperExecutionException : Exception
    {
        public TamperExecutionException(string message) : base(message) { }
    }
}
