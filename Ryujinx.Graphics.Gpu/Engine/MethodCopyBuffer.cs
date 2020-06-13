using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Texture;
using System;
using System.Runtime.Intrinsics;

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

                (int srcBaseOffset, int srcSize) = srcCalculator.GetRectangleRange(src.RegionX, src.RegionY, cbp.XCount, cbp.YCount);
                (int dstBaseOffset, int dstSize) = dstCalculator.GetRectangleRange(dst.RegionX, dst.RegionY, cbp.XCount, cbp.YCount);

                ReadOnlySpan<byte> srcSpan = _context.PhysicalMemory.GetSpan(srcBaseAddress + (ulong)srcBaseOffset, srcSize);
                Span<byte> dstSpan = _context.PhysicalMemory.GetSpan(dstBaseAddress + (ulong)dstBaseOffset, dstSize).ToArray();

                bool completeSource = src.RegionX == 0 && src.RegionY == 0 && src.Width == cbp.XCount && src.Height == cbp.YCount;
                bool completeDest = dst.RegionX == 0 && dst.RegionY == 0 && dst.Width == cbp.XCount && dst.Height == cbp.YCount;

                if (completeSource && completeDest && srcCalculator.LayoutMatches(dstCalculator))
                {
                    srcSpan.CopyTo(dstSpan); // No layout conversion has to be performed, just copy the data entirely.
                }
                else 
                {
                    unsafe bool Convert<T>(Span<byte> dstSpan, ReadOnlySpan<byte> srcSpan) where T : unmanaged
                    {
                        fixed (byte* dstPtr = dstSpan, srcPtr = srcSpan)
                        {
                            byte* dstBase = dstPtr - dstBaseOffset; // Layout offset is relative to the base, so we need to subtract the span's offset.
                            byte* srcBase = srcPtr - srcBaseOffset;

                            for (int y = 0; y < cbp.YCount; y++)
                            {
                                srcCalculator.SetY(src.RegionY + y);
                                dstCalculator.SetY(dst.RegionY + y);

                                for (int x = 0; x < cbp.XCount; x++)
                                {
                                    int srcOffset = srcCalculator.GetOffset(src.RegionX + x);
                                    int dstOffset = dstCalculator.GetOffset(dst.RegionX + x);

                                    *(T*)(dstBase + dstOffset) = *(T*)(srcBase + srcOffset);
                                }
                            }
                        }
                        return true;
                    }

                    bool _ = srcBpp switch
                    {
                        1 => Convert<byte>(dstSpan, srcSpan),
                        2 => Convert<ushort>(dstSpan, srcSpan),
                        4 => Convert<uint>(dstSpan, srcSpan),
                        8 => Convert<ulong>(dstSpan, srcSpan),
                        12 => Convert<Bpp12Pixel>(dstSpan, srcSpan),
                        16 => Convert<Vector128<byte>>(dstSpan, srcSpan),
                        _ => throw new NotSupportedException($"Unable to copy ${srcBpp} bpp pixel format.")
                    };
                }

                _context.PhysicalMemory.Write(dstBaseAddress + (ulong)dstBaseOffset, dstSpan);
            }
            else
            {
                // Buffer to buffer copy.
                BufferManager.CopyBuffer(cbp.SrcAddress, cbp.DstAddress, (uint)size);
            }
        }
    }
}