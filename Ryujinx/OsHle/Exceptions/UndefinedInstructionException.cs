using System;

namespace Ryujinx.OsHle.Exceptions
{
    public class UndefinedInstructionException : Exception
    {
        private const string ExMsg = "The instruction at 0x{0:x16} (opcode 0x{1:x8}) is undefined!";

        public UndefinedInstructionException() : base() { }

        public UndefinedInstructionException(long Position, int OpCode) : base(string.Format(ExMsg, Position, OpCode)) { }
    }
}