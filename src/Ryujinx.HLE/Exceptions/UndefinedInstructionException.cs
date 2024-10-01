using System;

namespace Ryujinx.HLE.Exceptions
{
    public class UndefinedInstructionException : Exception
    {
        private const string ExMsg = "The instruction at 0x{0:x16} (opcode 0x{1:x8}) is undefined!";

        public UndefinedInstructionException() : base() { }

        public UndefinedInstructionException(ulong address, int opCode) : base(string.Format(ExMsg, address, opCode)) { }
    }
}
