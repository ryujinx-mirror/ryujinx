using Ryujinx.Common;
using Ryujinx.Graphics.Gpu.Memory;
using Ryujinx.Graphics.Nvdec.Image;
using Ryujinx.Graphics.Nvdec.Types.Vp9;
using Ryujinx.Graphics.Nvdec.Vp9;
using Ryujinx.Graphics.Video;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Ryujinx.Graphics.Nvdec.MemoryExtensions;

namespace Ryujinx.Graphics.Nvdec
{
    static class Vp9Decoder
    {
        private static Decoder _decoder = new Decoder();

        public unsafe static void Decode(NvdecDevice device, ResourceManager rm, ref NvdecRegisters state)
        {
            PictureInfo pictureInfo = rm.Gmm.DeviceRead<PictureInfo>(state.SetPictureInfoOffset);
            EntropyProbs entropy = rm.Gmm.DeviceRead<EntropyProbs>(state.SetVp9EntropyProbsOffset);

            ISurface Rent(uint lumaOffset, uint chromaOffset, FrameSize size)
            {
                return rm.Cache.Get(_decoder, CodecId.Vp9, lumaOffset, chromaOffset, size.Width, size.Height);
            }

            ISurface lastSurface    = Rent(state.SetSurfaceLumaOffset[0], state.SetSurfaceChromaOffset[0], pictureInfo.LastFrameSize);
            ISurface goldenSurface  = Rent(state.SetSurfaceLumaOffset[1], state.SetSurfaceChromaOffset[1], pictureInfo.GoldenFrameSize);
            ISurface altSurface     = Rent(state.SetSurfaceLumaOffset[2], state.SetSurfaceChromaOffset[2], pictureInfo.AltFrameSize);
            ISurface currentSurface = Rent(state.SetSurfaceLumaOffset[3], state.SetSurfaceChromaOffset[3], pictureInfo.CurrentFrameSize);

            Vp9PictureInfo info = pictureInfo.Convert();

            info.LastReference = lastSurface;
            info.GoldenReference = goldenSurface;
            info.AltReference = altSurface;

            entropy.Convert(ref info.Entropy);

            ReadOnlySpan<byte> bitstream = rm.Gmm.DeviceGetSpan(state.SetBitstreamOffset, (int)pictureInfo.BitstreamSize);

            ReadOnlySpan<Vp9MvRef> mvsIn = ReadOnlySpan<Vp9MvRef>.Empty;

            if (info.UsePrevInFindMvRefs)
            {
                mvsIn = GetMvsInput(rm.Gmm, pictureInfo.CurrentFrameSize, state.SetVp9LastFrameMvsOffset);
            }

            int miCols = BitUtils.DivRoundUp(pictureInfo.CurrentFrameSize.Width, 8);
            int miRows = BitUtils.DivRoundUp(pictureInfo.CurrentFrameSize.Height, 8);

            using var mvsRegion = rm.Gmm.GetWritableRegion(ExtendOffset(state.SetVp9CurrFrameMvsOffset), miRows * miCols * 16);

            Span<Vp9MvRef> mvsOut = MemoryMarshal.Cast<byte, Vp9MvRef>(mvsRegion.Memory.Span);

            uint lumaOffset   = state.SetSurfaceLumaOffset[3];
            uint chromaOffset = state.SetSurfaceChromaOffset[3];

            if (_decoder.Decode(ref info, currentSurface, bitstream, mvsIn, mvsOut))
            {
                SurfaceWriter.Write(rm.Gmm, currentSurface, lumaOffset, chromaOffset);

                device.OnFrameDecoded(CodecId.Vp9, lumaOffset, chromaOffset);
            }

            WriteBackwardUpdates(rm.Gmm, state.SetVp9BackwardUpdatesOffset, ref info.BackwardUpdateCounts);

            rm.Cache.Put(lastSurface);
            rm.Cache.Put(goldenSurface);
            rm.Cache.Put(altSurface);
            rm.Cache.Put(currentSurface);
        }

        private static ReadOnlySpan<Vp9MvRef> GetMvsInput(MemoryManager gmm, FrameSize size, uint offset)
        {
            int miCols = BitUtils.DivRoundUp(size.Width, 8);
            int miRows = BitUtils.DivRoundUp(size.Height, 8);

            return MemoryMarshal.Cast<byte, Vp9MvRef>(gmm.DeviceGetSpan(offset, miRows * miCols * 16));
        }

        private static void WriteBackwardUpdates(MemoryManager gmm, uint offset, ref Vp9BackwardUpdates counts)
        {
            using var backwardUpdatesRegion = gmm.GetWritableRegion(ExtendOffset(offset), Unsafe.SizeOf<BackwardUpdates>());

            ref var backwardUpdates = ref MemoryMarshal.Cast<byte, BackwardUpdates>(backwardUpdatesRegion.Memory.Span)[0];

            backwardUpdates = new BackwardUpdates(ref counts);
        }
    }
}
