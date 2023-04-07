using Ryujinx.Graphics.Texture;
using Ryujinx.Graphics.Video;
using System;

namespace Ryujinx.Graphics.Nvdec.Image
{
    static class SurfaceCommon
    {
        public static int GetBlockLinearSize(int width, int height, int bytesPerPixel)
        {
            return SizeCalculator.GetBlockLinearTextureSize(width, height, 1, 1, 1, 1, 1, bytesPerPixel, 2, 1, 1).TotalSize;
        }

        public static void Copy(ISurface src, ISurface dst)
        {
            src.YPlane.AsSpan().CopyTo(dst.YPlane.AsSpan());
            src.UPlane.AsSpan().CopyTo(dst.UPlane.AsSpan());
            src.VPlane.AsSpan().CopyTo(dst.VPlane.AsSpan());
        }

        public unsafe static Span<byte> AsSpan(this Plane plane)
        {
            return new Span<byte>((void*)plane.Pointer, plane.Length);
        }
    }
}
