using System;

namespace ChocolArm64.Events
{
    public class AInstExceptionEventArgs : EventArgs
    {
        public int Id { get; private set; }

        public AInstExceptionEventArgs(int Id)
        {
            this.Id = Id;
        }
    }
}