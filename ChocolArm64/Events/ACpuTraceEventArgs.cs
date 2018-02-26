using System;

namespace ChocolArm64.Events
{
    public class ACpuTraceEventArgs : EventArgs
    {
        public long Position { get; private set; }

        public string SubName { get; private set; }

        public ACpuTraceEventArgs(long Position, string SubName)
        {
            this.Position = Position;
            this.SubName  = SubName;
        }
    }
}