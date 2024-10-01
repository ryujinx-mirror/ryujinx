using Ryujinx.Graphics.Nvdec.FFmpeg.Native;
using Ryujinx.Graphics.Video;
using System;

namespace Ryujinx.Graphics.Nvdec.FFmpeg.Vp8
{
    public sealed class Decoder : IDecoder
    {
        public bool IsHardwareAccelerated => false;

        private readonly FFmpegContext _context = new(AVCodecID.AV_CODEC_ID_VP8);

        public ISurface CreateSurface(int width, int height)
        {
            return new Surface(width, height);
        }

        public bool Decode(ref Vp8PictureInfo pictureInfo, ISurface output, ReadOnlySpan<byte> bitstream)
        {
            Surface outSurf = (Surface)output;

            int uncompHeaderSize = pictureInfo.KeyFrame ? 10 : 3;

            byte[] frame = new byte[bitstream.Length + uncompHeaderSize];

            uint firstPartSizeShifted = pictureInfo.FirstPartSize << 5;

            frame[0] = (byte)(pictureInfo.KeyFrame ? 0 : 1);
            frame[0] |= (byte)((pictureInfo.Version & 7) << 1);
            frame[0] |= 1 << 4;
            frame[0] |= (byte)firstPartSizeShifted;
            frame[1] |= (byte)(firstPartSizeShifted >> 8);
            frame[2] |= (byte)(firstPartSizeShifted >> 16);

            if (pictureInfo.KeyFrame)
            {
                frame[3] = 0x9d;
                frame[4] = 0x01;
                frame[5] = 0x2a;
                frame[6] = (byte)pictureInfo.FrameWidth;
                frame[7] = (byte)((pictureInfo.FrameWidth >> 8) & 0x3F);
                frame[8] = (byte)pictureInfo.FrameHeight;
                frame[9] = (byte)((pictureInfo.FrameHeight >> 8) & 0x3F);
            }

            bitstream.CopyTo(new Span<byte>(frame)[uncompHeaderSize..]);

            return _context.DecodeFrame(outSurf, frame) == 0;
        }

        public void Dispose() => _context.Dispose();
    }
}
