using System;

namespace Ryujinx.HLE.Exceptions
{
    public class TamperCompilationException : Exception
    {
        public TamperCompilationException(string message) : base(message) { }
    }
}
