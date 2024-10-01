namespace Ryujinx.Graphics.Texture.Utils
{
    readonly struct BC7ModeInfo
    {
        public readonly int SubsetCount;
        public readonly int PartitionBitCount;
        public readonly int PBits;
        public readonly int RotationBitCount;
        public readonly int IndexModeBitCount;
        public readonly int ColorIndexBitCount;
        public readonly int AlphaIndexBitCount;
        public readonly int ColorDepth;
        public readonly int AlphaDepth;

        public BC7ModeInfo(
            int subsetCount,
            int partitionBitsCount,
            int pBits,
            int rotationBitCount,
            int indexModeBitCount,
            int colorIndexBitCount,
            int alphaIndexBitCount,
            int colorDepth,
            int alphaDepth)
        {
            SubsetCount = subsetCount;
            PartitionBitCount = partitionBitsCount;
            PBits = pBits;
            RotationBitCount = rotationBitCount;
            IndexModeBitCount = indexModeBitCount;
            ColorIndexBitCount = colorIndexBitCount;
            AlphaIndexBitCount = alphaIndexBitCount;
            ColorDepth = colorDepth;
            AlphaDepth = alphaDepth;
        }
    }
}
