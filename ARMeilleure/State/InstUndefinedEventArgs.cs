using System;

namespace ARMeilleure.State
{
    public class InstUndefinedEventArgs : EventArgs
    {
        public ulong Address { get; }
        public int   OpCode  { get; }

        public InstUndefinedEventArgs(ulong address, int opCode)
        {
            Address = address;
            OpCode  = opCode;
        }
    }
}