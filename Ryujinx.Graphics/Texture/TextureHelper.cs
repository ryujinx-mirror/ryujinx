using ChocolArm64.Memory;
using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Memory;

namespace Ryujinx.Graphics.Texture
{
    static class TextureHelper
    {
        public static ISwizzle GetSwizzle(GalImage Image)
        {
            int BlockWidth    = ImageUtils.GetBlockWidth   (Image.Format);
            int BytesPerPixel = ImageUtils.GetBytesPerPixel(Image.Format);

            int Width = (Image.Width + (BlockWidth - 1)) / BlockWidth;

            if (Image.Layout == GalMemoryLayout.BlockLinear)
            {
                int AlignMask = Image.TileWidth * (64 / BytesPerPixel) - 1;

                Width = (Width + AlignMask) & ~AlignMask;

                return new BlockLinearSwizzle(Width, BytesPerPixel, Image.GobBlockHeight);
            }
            else
            {
                return new LinearSwizzle(Image.Pitch, BytesPerPixel);
            }
        }

        public static (MemoryManager Memory, long Position) GetMemoryAndPosition(
            IMemory Memory,
            long    Position)
        {
            if (Memory is NvGpuVmm Vmm)
            {
                return (Vmm.Memory, Vmm.GetPhysicalAddress(Position));
            }

            return ((MemoryManager)Memory, Position);
        }
    }
}
