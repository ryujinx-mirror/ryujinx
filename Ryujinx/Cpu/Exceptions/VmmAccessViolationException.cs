using ChocolArm64.Memory;
using System;

namespace ChocolArm64.Exceptions
{
    public class VmmAccessViolationException : Exception
    {
        private const string ExMsg = "Value at address 0x{0:x16} could not be \"{1}\"!";

        public VmmAccessViolationException() { }

        public VmmAccessViolationException(long Position, AMemoryPerm Perm) : base(string.Format(ExMsg, Position, Perm)) { }
    }
}