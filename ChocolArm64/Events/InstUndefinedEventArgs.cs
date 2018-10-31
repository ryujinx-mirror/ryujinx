using System;

namespace ChocolArm64.Events
{
    public class InstUndefinedEventArgs : EventArgs
    {
        public long Position  { get; private set; }
        public int  RawOpCode { get; private set; }

        public InstUndefinedEventArgs(long position, int rawOpCode)
        {
            Position  = position;
            RawOpCode = rawOpCode;
        }
    }
}