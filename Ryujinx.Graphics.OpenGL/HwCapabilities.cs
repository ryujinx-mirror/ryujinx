using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class HwCapabilities
    {
        private static Lazy<bool> _astcCompression = new Lazy<bool>(SupportsAstcCompressionImpl);

        public static bool SupportsAstcCompression => _astcCompression.Value;

        private static bool SupportsAstcCompressionImpl()
        {
            // The NVIDIA driver has software decompression support for ASTC textures,
            // but the extension is not exposed, so we check the list of compressed
            // formats too, since the support is indicated there.
            return SupportsAnyAstcFormat() || HasExtension("GL_KHR_texture_compression_astc_ldr");
        }

        private static bool SupportsAnyAstcFormat()
        {
            int formatsCount = GL.GetInteger(GetPName.NumCompressedTextureFormats);

            int[] formats = new int[formatsCount];

            GL.GetInteger(GetPName.CompressedTextureFormats, formats);

            foreach (int format in formats)
            {
                if (IsAstcFormat(format))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsAstcFormat(int format)
        {
            switch ((All)format)
            {
                case All.CompressedRgbaAstc4X4Khr:
                case All.CompressedRgbaAstc5X4Khr:
                case All.CompressedRgbaAstc5X5Khr:
                case All.CompressedRgbaAstc6X5Khr:
                case All.CompressedRgbaAstc6X6Khr:
                case All.CompressedRgbaAstc8X5Khr:
                case All.CompressedRgbaAstc8X6Khr:
                case All.CompressedRgbaAstc8X8Khr:
                case All.CompressedRgbaAstc10X5Khr:
                case All.CompressedRgbaAstc10X6Khr:
                case All.CompressedRgbaAstc10X8Khr:
                case All.CompressedRgbaAstc10X10Khr:
                case All.CompressedRgbaAstc12X10Khr:
                case All.CompressedRgbaAstc12X12Khr:
                case All.CompressedSrgb8Alpha8Astc4X4Khr:
                case All.CompressedSrgb8Alpha8Astc5X4Khr:
                case All.CompressedSrgb8Alpha8Astc5X5Khr:
                case All.CompressedSrgb8Alpha8Astc6X5Khr:
                case All.CompressedSrgb8Alpha8Astc6X6Khr:
                case All.CompressedSrgb8Alpha8Astc8X5Khr:
                case All.CompressedSrgb8Alpha8Astc8X6Khr:
                case All.CompressedSrgb8Alpha8Astc8X8Khr:
                case All.CompressedSrgb8Alpha8Astc10X5Khr:
                case All.CompressedSrgb8Alpha8Astc10X6Khr:
                case All.CompressedSrgb8Alpha8Astc10X8Khr:
                case All.CompressedSrgb8Alpha8Astc10X10Khr:
                case All.CompressedSrgb8Alpha8Astc12X10Khr:
                case All.CompressedSrgb8Alpha8Astc12X12Khr:
                    return true;
            }

            return false;
        }

        private static bool HasExtension(string name)
        {
            int numExtensions = GL.GetInteger(GetPName.NumExtensions);

            for (int extension = 0; extension < numExtensions; extension++)
            {
                if (GL.GetString(StringNameIndexed.Extensions, extension) == name)
                {
                    return true;
                }
            }

            return false;
        }
    }
}