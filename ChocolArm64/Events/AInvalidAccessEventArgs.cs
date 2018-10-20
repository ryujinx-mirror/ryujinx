using System;

namespace ChocolArm64.Events
{
    public class AInvalidAccessEventArgs : EventArgs
    {
        public long Position { get; private set; }

        public AInvalidAccessEventArgs(long Position)
        {
            this.Position = Position;
        }
    }
}