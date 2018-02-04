using System;

namespace ChocolArm64.State
{
    public class SvcEventArgs : EventArgs
    {
        public int Id { get; private set; }

        public SvcEventArgs(int Id)
        {
            this.Id = Id;
        }
    }
}