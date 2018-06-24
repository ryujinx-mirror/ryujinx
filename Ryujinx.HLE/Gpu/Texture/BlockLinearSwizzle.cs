using System;

namespace Ryujinx.HLE.Gpu.Texture
{
    class BlockLinearSwizzle : ISwizzle
    {
        private int BhShift;
        private int BppShift;
        private int BhMask;

        private int XShift;
        private int GobStride;

        public BlockLinearSwizzle(int Width, int Bpp, int BlockHeight = 16)
        {
            BhMask = (BlockHeight * 8) - 1;

            BhShift  = CountLsbZeros(BlockHeight * 8);
            BppShift = CountLsbZeros(Bpp);

            int WidthInGobs = (int)MathF.Ceiling(Width * Bpp / 64f);

            GobStride = 512 * BlockHeight * WidthInGobs;

            XShift = CountLsbZeros(512 * BlockHeight);
        }

        private int CountLsbZeros(int Value)
        {
            int Count = 0;

            while (((Value >> Count) & 1) == 0)
            {
                Count++;
            }

            return Count;
        }

        public int GetSwizzleOffset(int X, int Y)
        {
            X <<= BppShift;

            int Position = (Y >> BhShift) * GobStride;

            Position += (X >> 6) << XShift;

            Position += ((Y & BhMask) >> 3) << 9;

            Position += ((X & 0x3f) >> 5) << 8;
            Position += ((Y & 0x07) >> 1) << 6;
            Position += ((X & 0x1f) >> 4) << 5;
            Position += ((Y & 0x01) >> 0) << 4;
            Position += ((X & 0x0f) >> 0) << 0;

            return Position;
        }
    }
}