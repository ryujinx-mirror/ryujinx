using System;

namespace Ryujinx.HLE.HOS.Kernel.SupervisorCall
{
    public class InvalidSvcException : Exception
    {
        public InvalidSvcException(string message) : base(message) { }
    }
}
