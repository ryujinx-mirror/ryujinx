using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Texture;
using System;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        /// <summary>
        /// Performs a buffer to buffer, or buffer to texture copy.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void CopyBuffer(GpuState state, int argument)
        {
            var cbp = state.Get<CopyBufferParams>(MethodOffset.CopyBufferParams);

            var swizzle = state.Get<CopyBufferSwizzle>(MethodOffset.CopyBufferSwizzle);

            bool srcLinear = (argument & (1 << 7)) != 0;
            bool dstLinear = (argument & (1 << 8)) != 0;
            bool copy2D    = (argument & (1 << 9)) != 0;

            int size = cbp.XCount;

            if (size == 0)
            {
                return;
            }

            if (copy2D)
            {
                // Buffer to texture copy.
                int srcBpp = swizzle.UnpackSrcComponentsCount() * swizzle.UnpackComponentSize();
                int dstBpp = swizzle.UnpackDstComponentsCount() * swizzle.UnpackComponentSize();

                var dst = state.Get<CopyBufferTexture>(MethodOffset.CopyBufferDstTexture);
                var src = state.Get<CopyBufferTexture>(MethodOffset.CopyBufferSrcTexture);

                var srcCalculator = new OffsetCalculator(
                    src.Width,
                    src.Height,
                    cbp.SrcStride,
                    srcLinear,
                    src.MemoryLayout.UnpackGobBlocksInY(),
                    srcBpp);

                var dstCalculator = new OffsetCalculator(
                    dst.Width,
                    dst.Height,
                    cbp.DstStride,
                    dstLinear,
                    dst.MemoryLayout.UnpackGobBlocksInY(),
                    dstBpp);

                ulong srcBaseAddress = _context.MemoryManager.Translate(cbp.SrcAddress.Pack());
                ulong dstBaseAddress = _context.MemoryManager.Translate(cbp.DstAddress.Pack());

                for (int y = 0; y < cbp.YCount; y++)
                for (int x = 0; x < cbp.XCount; x++)
                {
                    int srcOffset = srcCalculator.GetOffset(src.RegionX + x, src.RegionY + y);
                    int dstOffset = dstCalculator.GetOffset(dst.RegionX + x, dst.RegionY + y);

                    ulong srcAddress = srcBaseAddress + (ulong)srcOffset;
                    ulong dstAddress = dstBaseAddress + (ulong)dstOffset;

                    ReadOnlySpan<byte> pixel = _context.PhysicalMemory.GetSpan(srcAddress, (ulong)srcBpp);

                    _context.PhysicalMemory.Write(dstAddress, pixel);
                }
            }
            else
            {
                // Buffer to buffer copy.
                BufferManager.CopyBuffer(cbp.SrcAddress, cbp.DstAddress, (uint)size);
            }
        }
    }
}