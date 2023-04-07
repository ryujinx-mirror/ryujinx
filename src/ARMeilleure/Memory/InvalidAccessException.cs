using System;

namespace ARMeilleure.Memory
{
    class InvalidAccessException : Exception
    {
        public InvalidAccessException()
        {
        }

        public InvalidAccessException(ulong address) : base($"Invalid memory access at virtual address 0x{address:X16}.")
        {
        }

        public InvalidAccessException(string message) : base(message)
        {
        }

        public InvalidAccessException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
