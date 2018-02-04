using System;

namespace ChocolArm64.Exceptions
{
    public class VmmOutOfMemoryException : Exception
    {
        private const string ExMsg = "Failed to allocate {0} bytes of memory!";

        public VmmOutOfMemoryException() { }

        public VmmOutOfMemoryException(long Size) : base(string.Format(ExMsg, Size)) { }
    }
}