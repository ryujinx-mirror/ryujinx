using ChocolArm64.Exceptions;

namespace ChocolArm64.Memory
{
    public class AMemoryAlloc
    {
        private long PhysPos;

        public long Alloc(long Size)
        {
            long Position = PhysPos;

            Size = AMemoryHelper.PageRoundUp(Size);

            PhysPos += Size;

            if (PhysPos > AMemoryMgr.RamSize || PhysPos < 0)
            {
                throw new VmmOutOfMemoryException(Size);
            }

            return Position;
        }

        public void Free(long Position)
        {
            //TODO
        }

        public long GetFreeMem()
        {
            return AMemoryMgr.RamSize - PhysPos;
        }
    }
}