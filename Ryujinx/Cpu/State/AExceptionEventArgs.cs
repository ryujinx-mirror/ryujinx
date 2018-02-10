using System;

namespace ChocolArm64.State
{
    public class AExceptionEventArgs : EventArgs
    {
        public int Id { get; private set; }

        public AExceptionEventArgs(int Id)
        {
            this.Id = Id;
        }
    }
}