using System;

namespace ChocolArm64.Exceptions
{
    public class VmmPageFaultException : Exception
    {
        private const string ExMsg = "Tried to access unmapped address 0x{0:x16}!";

        public VmmPageFaultException() { }

        public VmmPageFaultException(long position) : base(string.Format(ExMsg, position)) { }
    }
}