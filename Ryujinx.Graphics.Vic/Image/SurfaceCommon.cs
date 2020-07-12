using Ryujinx.Common;
using Ryujinx.Graphics.Texture;

namespace Ryujinx.Graphics.Vic.Image
{
    static class SurfaceCommon
    {
        public static int GetPitch(int width, int bytesPerPixel)
        {
            return BitUtils.AlignUp(width * bytesPerPixel, 256);
        }

        public static int GetBlockLinearSize(int width, int height, int bytesPerPixel, int gobBlocksInY)
        {
            return SizeCalculator.GetBlockLinearTextureSize(width, height, 1, 1, 1, 1, 1, bytesPerPixel, gobBlocksInY, 1, 1).TotalSize;
        }

        public static ulong ExtendOffset(uint offset)
        {
            return (ulong)offset << 8;
        }

        public static ushort Upsample(byte value)
        {
            return (ushort)(value << 2);
        }

        public static byte Downsample(ushort value)
        {
            return (byte)(value >> 2);
        }
    }
}
