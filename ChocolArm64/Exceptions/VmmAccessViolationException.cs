using ChocolArm64.Memory;
using System;

namespace ChocolArm64.Exceptions
{
    public class VmmAccessViolationException : Exception
    {
        private const string ExMsg = "Address 0x{0:x16} does not have \"{1}\" permission!";

        public VmmAccessViolationException() { }

        public VmmAccessViolationException(long Position, AMemoryPerm Perm) : base(string.Format(ExMsg, Position, Perm)) { }
    }
}