using System;

namespace Ryujinx.Memory
{
    public class MemoryNotContiguousException : Exception
    {
        public MemoryNotContiguousException() : base("The specified memory region is not contiguous.")
        {
        }

        public MemoryNotContiguousException(string message) : base(message)
        {
        }

        public MemoryNotContiguousException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
