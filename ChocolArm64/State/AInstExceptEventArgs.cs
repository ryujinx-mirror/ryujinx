using System;

namespace ChocolArm64.State
{
    public class AInstExceptEventArgs : EventArgs
    {
        public int Id { get; private set; }

        public AInstExceptEventArgs(int Id)
        {
            this.Id = Id;
        }
    }
}