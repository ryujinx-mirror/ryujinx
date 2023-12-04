using System;

namespace Ryujinx.HLE.Exceptions
{
    class InternalServiceException : Exception
    {
        public InternalServiceException(string message) : base(message) { }
    }
}
