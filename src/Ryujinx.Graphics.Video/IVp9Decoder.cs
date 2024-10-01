using System;

namespace Ryujinx.Graphics.Video
{
    public interface IVp9Decoder : IDecoder
    {
        bool Decode(
            ref Vp9PictureInfo pictureInfo,
            ISurface output,
            ReadOnlySpan<byte> bitstream,
            ReadOnlySpan<Vp9MvRef> mvsIn,
            Span<Vp9MvRef> mvsOut);
    }
}
