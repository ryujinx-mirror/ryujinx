using System;

namespace ChocolArm64.Events
{
    public class InvalidAccessEventArgs : EventArgs
    {
        public long Position { get; private set; }

        public InvalidAccessEventArgs(long position)
        {
            Position = position;
        }
    }
}