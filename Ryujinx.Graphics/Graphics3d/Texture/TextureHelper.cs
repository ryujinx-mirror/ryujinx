using ChocolArm64.Memory;
using Ryujinx.Common;
using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Memory;

namespace Ryujinx.Graphics.Texture
{
    static class TextureHelper
    {
        public static ISwizzle GetSwizzle(GalImage Image)
        {
            int BlockWidth    = ImageUtils.GetBlockWidth   (Image.Format);
            int BlockHeight   = ImageUtils.GetBlockHeight  (Image.Format);
            int BlockDepth    = ImageUtils.GetBlockDepth   (Image.Format);
            int BytesPerPixel = ImageUtils.GetBytesPerPixel(Image.Format);

            int Width  = BitUtils.DivRoundUp(Image.Width,  BlockWidth);
            int Height = BitUtils.DivRoundUp(Image.Height, BlockHeight);
            int Depth  = BitUtils.DivRoundUp(Image.Depth,  BlockDepth);

            if (Image.Layout == GalMemoryLayout.BlockLinear)
            {
                int AlignMask = Image.TileWidth * (64 / BytesPerPixel) - 1;

                Width = (Width + AlignMask) & ~AlignMask;

                return new BlockLinearSwizzle(
                    Width,
                    Height,
                    Depth,
                    Image.GobBlockHeight,
                    Image.GobBlockDepth,
                    BytesPerPixel);
            }
            else
            {
                return new LinearSwizzle(Image.Pitch, BytesPerPixel, Width, Height);
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
