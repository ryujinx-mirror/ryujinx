using System;

namespace Ryujinx.HLE.Exceptions
{
    public class CodeRegionTamperedException : TamperExecutionException
    {
        public CodeRegionTamperedException(string message) : base(message) { }
    }
}
