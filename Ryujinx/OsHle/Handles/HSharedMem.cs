namespace Ryujinx.OsHle.Handles
{
    class HSharedMem
    {
        public long PhysPos { get; private set; }

        public HSharedMem(long PhysPos)
        {
            this.PhysPos = PhysPos;
        }
    }
}