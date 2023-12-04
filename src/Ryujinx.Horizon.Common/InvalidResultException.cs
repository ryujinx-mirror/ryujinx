using System;

namespace Ryujinx.Horizon.Common
{
    public class InvalidResultException : Exception
    {
        public InvalidResultException()
        {
        }

        public InvalidResultException(Result result) : base($"Unexpected result code {result} returned.")
        {
        }

        public InvalidResultException(string message) : base(message)
        {
        }

        public InvalidResultException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
