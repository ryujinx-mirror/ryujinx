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
            PictureInfo pictureInfo = rm.Gmm.DeviceRead<PictureInfo>(state.SetPictureInfoOffset);
            H264PictureInfo info = pictureInfo.Convert();

            ReadOnlySpan<byte> bitstream = rm.Gmm.DeviceGetSpan(state.SetBitstreamOffset, (int)pictureInfo.BitstreamSize);

            int width  = (int)pictureInfo.PicWidthInMbs * MbSizeInPixels;
            int height = (int)pictureInfo.PicHeightInMbs * MbSizeInPixels;

            int surfaceIndex = (int)pictureInfo.OutputSurfaceIndex;

            uint lumaOffset   = state.SetSurfaceLumaOffset[surfaceIndex];
            uint chromaOffset = state.SetSurfaceChromaOffset[surfaceIndex];

            Decoder decoder = context.GetH264Decoder();

            ISurface outputSurface = rm.Cache.Get(decoder, 0, 0, width, height);

            if (decoder.Decode(ref info, outputSurface, bitstream))
            {
                if (outputSurface.Field == FrameField.Progressive)
                {
                    SurfaceWriter.Write(
                        rm.Gmm,
                        outputSurface,
                        lumaOffset   + pictureInfo.LumaFrameOffset,
                        chromaOffset + pictureInfo.ChromaFrameOffset);
                }
                else
                {
                    SurfaceWriter.WriteInterlaced(
                        rm.Gmm,
                        outputSurface,
                        lumaOffset   + pictureInfo.LumaTopFieldOffset,
                        chromaOffset + pictureInfo.ChromaTopFieldOffset,
                        lumaOffset   + pictureInfo.LumaBottomFieldOffset,
                        chromaOffset + pictureInfo.ChromaBottomFieldOffset);
                }
            }

            rm.Cache.Put(outputSurface);
        }
    }
}
