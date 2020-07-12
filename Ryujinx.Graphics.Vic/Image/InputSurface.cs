using System;

namespace Ryujinx.Graphics.Vic.Image
{
    ref struct InputSurface
    {
        public ReadOnlySpan<byte> Buffer0;
        public ReadOnlySpan<byte> Buffer1;
        public ReadOnlySpan<byte> Buffer2;

        public int Width;
        public int Height;

        public int UvWidth;
        public int UvHeight;
    }
}
