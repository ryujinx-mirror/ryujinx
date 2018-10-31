using System;

namespace ChocolArm64.Events
{
    public class InstExceptionEventArgs : EventArgs
    {
        public long Position { get; private set; }
        public int  Id       { get; private set; }

        public InstExceptionEventArgs(long position, int id)
        {
            Position = position;
            Id       = id;
        }
    }
}