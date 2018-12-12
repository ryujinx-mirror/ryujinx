using System;

namespace ChocolArm64.Events
{
    public class MemoryAccessEventArgs : EventArgs
    {
        public long Position { get; private set; }

        public MemoryAccessEventArgs(long position)
        {
            Position = position;
        }
    }
}