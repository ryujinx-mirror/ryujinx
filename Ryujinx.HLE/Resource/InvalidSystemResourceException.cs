using System;

namespace Ryujinx.HLE.Resource
{
    public class InvalidSystemResourceException : Exception
    {
        public InvalidSystemResourceException(string message)
            : base(message)
        {
        }

    }
}
