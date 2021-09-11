using Ryujinx.Common;
using Ryujinx.Graphics.Device;
using Ryujinx.Graphics.Gpu.Engine.Threed;
using Ryujinx.Graphics.Texture;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

namespace Ryujinx.Graphics.Gpu.Engine.Dma
{
    /// <summary>
    /// Represents a DMA copy engine class.
    /// </summary>
    class DmaClass : IDeviceState
    {
        private readonly GpuContext _context;
        private readonly GpuChannel _channel;
        private readonly ThreedClass _3dEngine;
        private readonly DeviceState<DmaClassState> _state;

        /// <summary>
        /// Copy flags passed on DMA launch.
        /// </summary>
        [Flags]
        private enum CopyFlags
        {
            SrcLinear = 1 << 7,
            DstLinear = 1 << 8,
            MultiLineEnable = 1 << 9,
            RemapEnable = 1 << 10
        }

        /// <summary>
        /// Creates a new instance of the DMA copy engine class.
        /// </summary>
        /// <param name="context">GPU context</param>
        /// <param name="channel">GPU channel</param>
        /// <param name="threedEngine">3D engine</param>
        public DmaClass(GpuContext context, GpuChannel channel, ThreedClass threedEngine)
        {
            _context = context;
            _channel = channel;
            _3dEngine = threedEngine;
            _state = new DeviceState<DmaClassState>(new Dictionary<string, RwCallback>
            {
                { nameof(DmaClassState.LaunchDma), new RwCallback(LaunchDma, null) }
            });
        }

        /// <summary>
        /// Reads data from the class registers.
        /// </summary>
        /// <param name="offset">Register byte offset</param>
        /// <returns>Data at the specified offset</returns>
        public int Read(int offset) => _state.Read(offset);

        /// <summary>
        /// Writes data to the class registers.
        /// </summary>
        /// <param name="offset">Register byte offset</param>
        /// <param name="data">Data to be written</param>
        public void Write(int offset, int data) => _state.Write(offset, data);

        /// <summary>
        /// Determine if a buffer-to-texture region covers the entirety of a texture.
        /// </summary>
        /// <param name="tex">Texture to compare</param>
        /// <param name="linear">True if the texture is linear, false if block linear</param>
        /// <param name="bpp">Texture bytes per pixel</param>
        /// <param name="stride">Texture stride</param>
        /// <param name="xCount">Number of pixels to be copied</param>
        /// <param name="yCount">Number of lines to be copied</param>
        /// <returns></returns>
        private static bool IsTextureCopyComplete(DmaTexture tex, bool linear, int bpp, int stride, int xCount, int yCount)
        {
            if (linear)
            {
                // If the stride is negative, the texture has to be flipped, so
                // the fast copy is not trivial, use the slow path.
                if (stride <= 0)
                {
                    return false;
                }

                int alignWidth = Constants.StrideAlignment / bpp;
                return tex.RegionX == 0 &&
                       tex.RegionY == 0 &&
                       stride / bpp == BitUtils.AlignUp(xCount, alignWidth);
            }
            else
            {
                int alignWidth = Constants.GobAlignment / bpp;
                return tex.RegionX == 0 &&
                       tex.RegionY == 0 &&
                       tex.Width == BitUtils.AlignUp(xCount, alignWidth) &&
                       tex.Height == yCount;
            }
        }

        /// <summary>
        /// Performs a buffer to buffer, or buffer to texture copy.
        /// </summary>
        /// <param name="argument">Method call argument</param>
        private void LaunchDma(int argument)
        {
            var memoryManager = _channel.MemoryManager;

            CopyFlags copyFlags = (CopyFlags)argument;

            bool srcLinear = copyFlags.HasFlag(CopyFlags.SrcLinear);
            bool dstLinear = copyFlags.HasFlag(CopyFlags.DstLinear);
            bool copy2D = copyFlags.HasFlag(CopyFlags.MultiLineEnable);
            bool remap = copyFlags.HasFlag(CopyFlags.RemapEnable);

            uint size = _state.State.LineLengthIn;

            if (size == 0)
            {
                return;
            }

            ulong srcGpuVa = ((ulong)_state.State.OffsetInUpperUpper << 32) | _state.State.OffsetInLower;
            ulong dstGpuVa = ((ulong)_state.State.OffsetOutUpperUpper << 32) | _state.State.OffsetOutLower;

            int xCount = (int)_state.State.LineLengthIn;
            int yCount = (int)_state.State.LineCount;

            _3dEngine.FlushUboDirty();

            if (copy2D)
            {
                // Buffer to texture copy.
                int componentSize = (int)_state.State.SetRemapComponentsComponentSize + 1;
                int srcBpp = remap ? ((int)_state.State.SetRemapComponentsNumSrcComponents + 1) * componentSize : 1;
                int dstBpp = remap ? ((int)_state.State.SetRemapComponentsNumDstComponents + 1) * componentSize : 1;

                var dst = Unsafe.As<uint, DmaTexture>(ref _state.State.SetDstBlockSize);
                var src = Unsafe.As<uint, DmaTexture>(ref _state.State.SetSrcBlockSize);

                int srcStride = (int)_state.State.PitchIn;
                int dstStride = (int)_state.State.PitchOut;

                var srcCalculator = new OffsetCalculator(
                    src.Width,
                    src.Height,
                    srcStride,
                    srcLinear,
                    src.MemoryLayout.UnpackGobBlocksInY(),
                    src.MemoryLayout.UnpackGobBlocksInZ(),
                    srcBpp);

                var dstCalculator = new OffsetCalculator(
                    dst.Width,
                    dst.Height,
                    dstStride,
                    dstLinear,
                    dst.MemoryLayout.UnpackGobBlocksInY(),
                    dst.MemoryLayout.UnpackGobBlocksInZ(),
                    dstBpp);

                (int srcBaseOffset, int srcSize) = srcCalculator.GetRectangleRange(src.RegionX, src.RegionY, xCount, yCount);
                (int dstBaseOffset, int dstSize) = dstCalculator.GetRectangleRange(dst.RegionX, dst.RegionY, xCount, yCount);

                if (srcLinear && srcStride < 0)
                {
                    srcBaseOffset += srcStride * (yCount - 1);
                }

                if (dstLinear && dstStride < 0)
                {
                    dstBaseOffset += dstStride * (yCount - 1);
                }

                ReadOnlySpan<byte> srcSpan = memoryManager.GetSpan(srcGpuVa + (ulong)srcBaseOffset, srcSize, true);
                Span<byte> dstSpan = memoryManager.GetSpan(dstGpuVa + (ulong)dstBaseOffset, dstSize).ToArray();

                bool completeSource = IsTextureCopyComplete(src, srcLinear, srcBpp, srcStride, xCount, yCount);
                bool completeDest = IsTextureCopyComplete(dst, dstLinear, dstBpp, dstStride, xCount, yCount);

                if (completeSource && completeDest)
                {
                    var target = memoryManager.Physical.TextureCache.FindTexture(
                        memoryManager,
                        dst,
                        dstGpuVa,
                        dstBpp,
                        dstStride,
                        xCount,
                        yCount,
                        dstLinear);

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
                                srcStride,
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

                        memoryManager.Write(dstGpuVa + (ulong)dstBaseOffset, dstSpan);

                        return;
                    }
                }

                unsafe bool Convert<T>(Span<byte> dstSpan, ReadOnlySpan<byte> srcSpan) where T : unmanaged
                {
                    fixed (byte* dstPtr = dstSpan, srcPtr = srcSpan)
                    {
                        byte* dstBase = dstPtr - dstBaseOffset; // Layout offset is relative to the base, so we need to subtract the span's offset.
                        byte* srcBase = srcPtr - srcBaseOffset;

                        for (int y = 0; y < yCount; y++)
                        {
                            srcCalculator.SetY(src.RegionY + y);
                            dstCalculator.SetY(dst.RegionY + y);

                            for (int x = 0; x < xCount; x++)
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

                memoryManager.Write(dstGpuVa + (ulong)dstBaseOffset, dstSpan);
            }
            else
            {
                if (remap &&
                    _state.State.SetRemapComponentsDstX == SetRemapComponentsDst.ConstA &&
                    _state.State.SetRemapComponentsDstY == SetRemapComponentsDst.ConstA &&
                    _state.State.SetRemapComponentsDstZ == SetRemapComponentsDst.ConstA &&
                    _state.State.SetRemapComponentsDstW == SetRemapComponentsDst.ConstA &&
                    _state.State.SetRemapComponentsNumSrcComponents == SetRemapComponentsNumComponents.One &&
                    _state.State.SetRemapComponentsNumDstComponents == SetRemapComponentsNumComponents.One &&
                    _state.State.SetRemapComponentsComponentSize == SetRemapComponentsComponentSize.Four)
                {
                    // Fast path for clears when remap is enabled.
                    memoryManager.Physical.BufferCache.ClearBuffer(memoryManager, dstGpuVa, size * 4, _state.State.SetRemapConstA);
                }
                else
                {
                    // TODO: Implement remap functionality.
                    // Buffer to buffer copy.
                    memoryManager.Physical.BufferCache.CopyBuffer(memoryManager, srcGpuVa, dstGpuVa, size);
                }
            }
        }
    }
}
