using System;

namespace ChocolArm64.Exceptions
{
    public class VmmAccessException : Exception
    {
        private const string ExMsg = "Memory region at 0x{0} with size 0x{1} is not contiguous!";

        public VmmAccessException() { }

        public VmmAccessException(long Position, long Size) : base(string.Format(ExMsg, Position, Size)) { }
    }
}