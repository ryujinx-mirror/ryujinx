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

        public bool TryCoalesce(int index, int count)
        {
            long mask = ((1L << count) - 1) << (index & 63);

            index /= 64;

            if (count >= 64)
            {
                int remaining = count;
                int tempIdx = index;

                do
                {
                    if (Masks[MaxLevel - 1][tempIdx++] != -1L)
                    {
                        return false;
                    }

                    remaining -= 64;
                }
                while (remaining != 0);

                remaining = count;
                tempIdx = index;

                do
                {
                    Masks[MaxLevel - 1][tempIdx] = 0;

                    ClearMaskBit(MaxLevel - 2, tempIdx++);

                    remaining -= 64;
                }
                while (remaining != 0);
            }
            else
            {
                long value = Masks[MaxLevel - 1][index];

                if ((mask & ~value) != 0)
                {
                    return false;
                }

                value &= ~mask;

                Masks[MaxLevel - 1][index] = value;

                if (value == 0)
                {
                    ClearMaskBit(MaxLevel - 2, index);
                }
            }

            FreeCount -= (ulong)count;

            return true;
        }

        public void ClearMaskBit(int startLevel, int index)
        {
            for (int level = startLevel; level >= 0; level--, index /= 64)
            {
                Masks[level][index / 64] &= ~(1L << (index & 63));

                if (Masks[level][index / 64] != 0)
                {
                    break;
                }
            }
        }
    }
}