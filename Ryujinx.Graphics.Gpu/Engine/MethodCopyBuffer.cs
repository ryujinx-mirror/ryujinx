using Ryujinx.Common;
using Ryujinx.Graphics.Gpu.State;
using Ryujinx.Graphics.Texture;
using System;
using System.Runtime.Intrinsics;

namespace Ryujinx.Graphics.Gpu.Engine
{
    partial class Methods
    {
        private const int StrideAlignment = 32;
        private const int GobAlignment = 64;

        enum CopyFlags
        {
            SrcLinear = 1 << 7,
            DstLinear = 1 << 8,
            MultiLineEnable = 1 << 9,
            RemapEnable = 1 << 10
        }

        /// <summary>
        /// Determine if a buffer-to-texture region covers the entirety of a texture.
        /// </summary>
        /// <param name="cbp">Copy command parameters</param>
        /// <param name="tex">Texture to compare</param>
        /// <param name="linear">True if the texture is linear, false if block linear</param>
        /// <param name="bpp">Texture bytes per pixel</param>
        /// <param name="stride">Texture stride</param>
        /// <returns></returns>
        private bool IsTextureCopyComplete(CopyBufferParams cbp, CopyBufferTexture tex, bool linear, int bpp, int stride)
        {
            if (linear)
            {
                int alignWidth = StrideAlignment / bpp;
                return tex.RegionX == 0 &&
                       tex.RegionY == 0 &&
                       stride / bpp == BitUtils.AlignUp(cbp.XCount, alignWidth);
            }
            else
            {
                int alignWidth = GobAlignment / bpp;
                return tex.RegionX == 0 &&
                       tex.RegionY == 0 &&
                       tex.Width == BitUtils.AlignUp(cbp.XCount, alignWidth) &&
                       tex.Height == cbp.YCount;
            }
        }

        /// <summary>
        /// Performs a buffer to buffer, or buffer to texture copy.
        /// </summary>
        /// <param name="state">Current GPU state</param>
        /// <param name="argument">Method call argument</param>
        private void CopyBuffer(GpuState state, int argument)
        {
            var cbp = state.Get<CopyBufferParams>(MethodOffset.CopyBufferParams);

            var swizzle = state.Get<CopyBufferSwizzle>(MethodOffset.CopyBufferSwizzle);

            CopyFlags copyFlags = (CopyFlags)argument;

            bool srcLinear = copyFlags.HasFlag(CopyFlags.SrcLinear);
            bool dstLinear = copyFlags.HasFlag(CopyFlags.DstLinear);
            bool copy2D    = copyFlags.HasFlag(CopyFlags.MultiLineEnable);
            bool remap     = copyFlags.HasFlag(CopyFlags.RemapEnable);

            int size = cbp.XCount;

            if (size == 0)
            {
                return;
            }

            if (copy2D)
            {
                // Buffer to texture copy.
                int srcBpp = remap ? swizzle.UnpackSrcComponentsCount() * swizzle.UnpackComponentSize() : 1;
                int dstBpp = remap ? swizzle.UnpackDstComponentsCount() * swizzle.UnpackComponentSize() : 1;

                var dst = state.Get<CopyBufferTexture>(MethodOffset.CopyBufferDstTexture);
                var src = state.Get<CopyBufferTexture>(MethodOffset.CopyBufferSrcTexture);

                var srcCalculator = new OffsetCalculator(
                    src.Width,
                    src.Height,
                    cbp.SrcStride,
                    srcLinear,
                    src.MemoryLayout.UnpackGobBlocksInY(),
                    src.MemoryLayout.UnpackGobBlocksInZ(),
                    srcBpp);

                var dstCalculator = new OffsetCalculator(
                    dst.Width,
                    dst.Height,
                    cbp.DstStride,
                    dstLinear,
                    dst.MemoryLayout.UnpackGobBlocksInY(),
                    dst.MemoryLayout.UnpackGobBlocksInZ(),
                    dstBpp);

                ulong srcBaseAddress = _context.MemoryManager.Translate(cbp.SrcAddress.Pack());
                ulong dstBaseAddress = _context.MemoryManager.Translate(cbp.DstAddress.Pack());

                (int srcBaseOffset, int srcSize) = srcCalculator.GetRectangleRange(src.RegionX, src.RegionY, cbp.XCount, cbp.YCount);
                (int dstBaseOffset, int dstSize) = dstCalculator.GetRectangleRange(dst.RegionX, dst.RegionY, cbp.XCount, cbp.YCount);

                ReadOnlySpan<byte> srcSpan = _context.PhysicalMemory.GetSpan(srcBaseAddress + (ulong)srcBaseOffset, srcSize, true);
                Span<byte> dstSpan         = _context.PhysicalMemory.GetSpan(dstBaseAddress + (ulong)dstBaseOffset, dstSize).ToArray();

                bool completeSource = IsTextureCopyComplete(cbp, src, srcLinear, srcBpp, cbp.SrcStride);
                bool completeDest   = IsTextureCopyComplete(cbp, dst, dstLinear, dstBpp, cbp.DstStride);

                if (completeSource && completeDest)
                {
                    Image.Texture target = TextureManager.FindTexture(dst, cbp, swizzle, dstLinear);
                    if (target != null)
                    {
                        ReadOnlySpan<byte> data;
                        if (srcLinear)
                        {
                            data = LayoutConverter.ConvertLinearStridedToLinear(
                                target.Info.Width,
                                target.Info.Height,
                                1,
                                1,
                                cbp.SrcStride,
                                target.Info.FormatInfo.BytesPerPixel,
                                srcSpan);
                        }
                        else
                        {
                            data = LayoutConverter.ConvertBlockLinearToLinear(
                                src.Width,
                                src.Height,
                                1,
                                target.Info.Levels,
                                1,
                                1,
                                1,
                                srcBpp,
                                src.MemoryLayout.UnpackGobBlocksInY(),
                                src.MemoryLayout.UnpackGobBlocksInZ(),
                                1,
                                new SizeInfo((int)target.Size),
                                srcSpan);
                        }

                        target.SetData(data);
                        target.SignalModified();

                        return;
                    }
                    else if (srcCalculator.LayoutMatches(dstCalculator))
                    {
                        srcSpan.CopyTo(dstSpan); // No layout conversion has to be performed, just copy the data entirely.

                        _context.PhysicalMemory.Write(dstBaseAddress + (ulong)dstBaseOffset, dstSpan);

                        return;
                    }
                }

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

                _context.PhysicalMemory.Write(dstBaseAddress + (ulong)dstBaseOffset, dstSpan);
            }
            else
            {
                if (remap &&
                    swizzle.UnpackDstX() == BufferSwizzleComponent.ConstA &&
                    swizzle.UnpackDstY() == BufferSwizzleComponent.ConstA &&
                    swizzle.UnpackDstZ() == BufferSwizzleComponent.ConstA &&
                    swizzle.UnpackDstW() == BufferSwizzleComponent.ConstA &&
                    swizzle.UnpackSrcComponentsCount() == 1 &&
                    swizzle.UnpackDstComponentsCount() == 1 &&
                    swizzle.UnpackComponentSize() == 4)
                {
                    // Fast path for clears when remap is enabled.
                    BufferManager.ClearBuffer(cbp.DstAddress, (uint)size * 4, state.Get<uint>(MethodOffset.CopyBufferConstA));
                }
                else
                {
                    // TODO: Implement remap functionality.
                    // Buffer to buffer copy.
                    BufferManager.CopyBuffer(cbp.SrcAddress, cbp.DstAddress, (uint)size);
                }
            }
        }
    }
}