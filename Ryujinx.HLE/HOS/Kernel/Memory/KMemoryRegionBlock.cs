namespace Ryujinx.HLE.HOS.Kernel.Memory
{
    class KMemoryRegionBlock
    {
        public long[][] Masks;

        public ulong FreeCount;
        public int   MaxLevel;
        public ulong StartAligned;
        public ulong SizeInBlocksTruncated;
        public ulong SizeInBlocksRounded;
        public int   Order;
        public int   NextOrder;

        public bool TryCoalesce(int index, int size)
        {
            long mask = ((1L << size) - 1) << (index & 63);

            index /= 64;

            if ((mask & ~Masks[MaxLevel - 1][index]) != 0)
            {
                return false;
            }

            Masks[MaxLevel - 1][index] &= ~mask;

            for (int level = MaxLevel - 2; level >= 0; level--, index /= 64)
            {
                Masks[level][index / 64] &= ~(1L << (index & 63));

                if (Masks[level][index / 64] != 0)
                {
                    break;
                }
            }

            FreeCount -= (ulong)size;

            return true;
        }
    }
}