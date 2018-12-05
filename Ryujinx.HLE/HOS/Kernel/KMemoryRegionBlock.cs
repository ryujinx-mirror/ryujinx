namespace Ryujinx.HLE.HOS.Kernel
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

        public bool TryCoalesce(int Index, int Size)
        {
            long Mask = ((1L << Size) - 1) << (Index & 63);

            Index /= 64;

            if ((Mask & ~Masks[MaxLevel - 1][Index]) != 0)
            {
                return false;
            }

            Masks[MaxLevel - 1][Index] &= ~Mask;

            for (int Level = MaxLevel - 2; Level >= 0; Level--, Index /= 64)
            {
                Masks[Level][Index / 64] &= ~(1L << (Index & 63));

                if (Masks[Level][Index / 64] != 0)
                {
                    break;
                }
            }

            FreeCount -= (ulong)Size;

            return true;
        }
    }
}