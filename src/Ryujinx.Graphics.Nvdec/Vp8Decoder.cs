using Ryujinx.Graphics.Nvdec.FFmpeg.Vp8;
using Ryujinx.Graphics.Nvdec.Image;
using Ryujinx.Graphics.Nvdec.Types.Vp8;
using Ryujinx.Graphics.Video;
using System;

namespace Ryujinx.Graphics.Nvdec
{
    static class Vp8Decoder
    {
        public static void Decode(NvdecDecoderContext context, ResourceManager rm, ref NvdecRegisters state)
        {
            PictureInfo pictureInfo = rm.MemoryManager.DeviceRead<PictureInfo>(state.SetDrvPicSetupOffset);
            ReadOnlySpan<byte> bitstream = rm.MemoryManager.DeviceGetSpan(state.SetInBufBaseOffset, (int)pictureInfo.VLDBufferSize);

            Decoder decoder = context.GetVp8Decoder();

            ISurface outputSurface = rm.Cache.Get(decoder, 0, 0, pictureInfo.FrameWidth, pictureInfo.FrameHeight);

            Vp8PictureInfo info = pictureInfo.Convert();

            uint lumaOffset = state.SetPictureLumaOffset[3];
            uint chromaOffset = state.SetPictureChromaOffset[3];

            if (decoder.Decode(ref info, outputSurface, bitstream))
            {
                SurfaceWriter.Write(rm.MemoryManager, outputSurface, lumaOffset, chromaOffset);
            }

            rm.Cache.Put(outputSurface);
        }
    }
}
