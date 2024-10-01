using OpenTK.Graphics.OpenGL;

namespace Ryujinx.Graphics.OpenGL
{
    readonly struct FormatInfo
    {
        public int Components { get; }
        public bool Normalized { get; }
        public bool Scaled { get; }

        public PixelInternalFormat PixelInternalFormat { get; }
        public PixelFormat PixelFormat { get; }
        public PixelType PixelType { get; }

        public bool IsCompressed { get; }

        public FormatInfo(
            int components,
            bool normalized,
            bool scaled,
            All pixelInternalFormat,
            PixelFormat pixelFormat,
            PixelType pixelType)
        {
            Components = components;
            Normalized = normalized;
            Scaled = scaled;
            PixelInternalFormat = (PixelInternalFormat)pixelInternalFormat;
            PixelFormat = pixelFormat;
            PixelType = pixelType;
            IsCompressed = false;
        }

        public FormatInfo(int components, bool normalized, bool scaled, All pixelFormat)
        {
            Components = components;
            Normalized = normalized;
            Scaled = scaled;
            PixelInternalFormat = 0;
            PixelFormat = (PixelFormat)pixelFormat;
            PixelType = 0;
            IsCompressed = true;
        }
    }
}
