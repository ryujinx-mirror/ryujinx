using Ryujinx.Graphics.Nvdec.FFmpeg.H264;
using Ryujinx.Graphics.Nvdec.Image;
using Ryujinx.Graphics.Nvdec.Types.H264;
using Ryujinx.Graphics.Video;
using System;

namespace Ryujinx.Graphics.Nvdec
{
    static class H264Decoder
    {
        private const int MbSizeInPixels = 16;

        public static void Decode(NvdecDecoderContext context, ResourceManager rm, ref NvdecRegisters state)
        {
            PictureInfo pictureInfo = rm.MemoryManager.DeviceRead<PictureInfo>(state.SetDrvPicSetupOffset);
            H264PictureInfo info = pictureInfo.Convert();

            ReadOnlySpan<byte> bitstream = rm.MemoryManager.DeviceGetSpan(state.SetInBufBaseOffset, (int)pictureInfo.BitstreamSize);

            int width = (int)pictureInfo.PicWidthInMbs * MbSizeInPixels;
            int height = (int)pictureInfo.PicHeightInMbs * MbSizeInPixels;

            int surfaceIndex = (int)pictureInfo.OutputSurfaceIndex;

            uint lumaOffset = state.SetPictureLumaOffset[surfaceIndex];
            uint chromaOffset = state.SetPictureChromaOffset[surfaceIndex];

            Decoder decoder = context.GetH264Decoder();

            ISurface outputSurface = rm.Cache.Get(decoder, 0, 0, width, height);

            if (decoder.Decode(ref info, outputSurface, bitstream))
            {
                if (outputSurface.Field == FrameField.Progressive)
                {
                    SurfaceWriter.Write(
                        rm.MemoryManager,
                        outputSurface,
                        lumaOffset + pictureInfo.LumaFrameOffset,
                        chromaOffset + pictureInfo.ChromaFrameOffset);
                }
                else
                {
                    SurfaceWriter.WriteInterlaced(
                        rm.MemoryManager,
                        outputSurface,
                        lumaOffset + pictureInfo.LumaTopFieldOffset,
                        chromaOffset + pictureInfo.ChromaTopFieldOffset,
                        lumaOffset + pictureInfo.LumaBottomFieldOffset,
                        chromaOffset + pictureInfo.ChromaBottomFieldOffset);
                }
            }

            rm.Cache.Put(outputSurface);
        }
    }
}
