using ARMeilleure.Memory;
using Ryujinx.Common;
using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Memory;

namespace Ryujinx.Graphics.Texture
{
    static class TextureHelper
    {
        public static ISwizzle GetSwizzle(GalImage image)
        {
            int blockWidth    = ImageUtils.GetBlockWidth   (image.Format);
            int blockHeight   = ImageUtils.GetBlockHeight  (image.Format);
            int blockDepth    = ImageUtils.GetBlockDepth   (image.Format);
            int bytesPerPixel = ImageUtils.GetBytesPerPixel(image.Format);

            int width  = BitUtils.DivRoundUp(image.Width,  blockWidth);
            int height = BitUtils.DivRoundUp(image.Height, blockHeight);
            int depth  = BitUtils.DivRoundUp(image.Depth,  blockDepth);

            if (image.Layout == GalMemoryLayout.BlockLinear)
            {
                int alignMask = image.TileWidth * (64 / bytesPerPixel) - 1;

                width = (width + alignMask) & ~alignMask;

                return new BlockLinearSwizzle(
                    width,
                    height,
                    depth,
                    image.GobBlockHeight,
                    image.GobBlockDepth,
                    bytesPerPixel);
            }
            else
            {
                return new LinearSwizzle(image.Pitch, bytesPerPixel, width, height);
            }
        }

        public static (MemoryManager Memory, long Position) GetMemoryAndPosition(
            IMemory memory,
            long    position)
        {
            if (memory is NvGpuVmm vmm)
            {
                return (vmm.Memory, vmm.GetPhysicalAddress(position));
            }

            return ((MemoryManager)memory, position);
        }
    }
}
