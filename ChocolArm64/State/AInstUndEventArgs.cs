using System;

namespace ChocolArm64.State
{
    public class AInstUndEventArgs : EventArgs
    {
        public long Position  { get; private set; }
        public int  RawOpCode { get; private set; }

        public AInstUndEventArgs(long Position, int RawOpCode)
        {
            this.Position  = Position;
            this.RawOpCode = RawOpCode;
        }
    }
}