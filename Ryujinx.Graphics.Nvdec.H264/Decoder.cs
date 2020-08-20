using Ryujinx.Graphics.Video;
using System;

namespace Ryujinx.Graphics.Nvdec.H264
{
    public sealed class Decoder : IH264Decoder
    {
        public bool IsHardwareAccelerated => false;

        private const int WorkBufferSize = 0x200;

        private readonly byte[] _workBuffer = new byte[WorkBufferSize];

        private readonly FFmpegContext _context = new FFmpegContext();

        public ISurface CreateSurface(int width, int height)
        {
            return new Surface();
        }

        public bool Decode(ref H264PictureInfo pictureInfo, ISurface output, ReadOnlySpan<byte> bitstream)
        {
            Span<byte> bs = Prepend(bitstream, SpsAndPpsReconstruction.Reconstruct(ref pictureInfo, _workBuffer));

            return _context.DecodeFrame((Surface)output, bs) == 0;
        }

        private static byte[] Prepend(ReadOnlySpan<byte> data, ReadOnlySpan<byte> prep)
        {
            byte[] output = new byte[data.Length + prep.Length];

            prep.CopyTo(output);
            data.CopyTo(new Span<byte>(output).Slice(prep.Length));

            return output;
        }

        public void Dispose() => _context.Dispose();
    }
}
