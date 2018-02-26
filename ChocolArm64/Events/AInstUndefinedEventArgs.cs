using System;

namespace ChocolArm64.Events
{
    public class AInstUndefinedEventArgs : EventArgs
    {
        public long Position  { get; private set; }
        public int  RawOpCode { get; private set; }

        public AInstUndefinedEventArgs(long Position, int RawOpCode)
        {
            this.Position  = Position;
            this.RawOpCode = RawOpCode;
        }
    }
}