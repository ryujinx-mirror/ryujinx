namespace Ryujinx.Graphics.Nvdec.Vp9.Types
{
    internal enum BlockSize
    {
        Block4x4 = 0,
        Block4x8 = 1,
        Block8x4 = 2,
        Block8x8 = 3,
        Block8x16 = 4,
        Block16x8 = 5,
        Block16x16 = 6,
        Block16x32 = 7,
        Block32x16 = 8,
        Block32x32 = 9,
        Block32x64 = 10,
        Block64x32 = 11,
        Block64x64 = 12,
        BlockSizes = 13,
        BlockInvalid = BlockSizes
    }
}
