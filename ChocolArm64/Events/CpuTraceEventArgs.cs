using System;

namespace ChocolArm64.Events
{
    public class CpuTraceEventArgs : EventArgs
    {
        public long Position { get; private set; }

        public CpuTraceEventArgs(long position)
        {
            Position = position;
        }
    }
}