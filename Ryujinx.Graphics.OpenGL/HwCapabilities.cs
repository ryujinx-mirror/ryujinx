using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class HwCapabilities
    {
        private static Lazy<bool> _supportsAstcCompression = new Lazy<bool>(() => HasExtension("GL_KHR_texture_compression_astc_ldr"));

        private static Lazy<int> _maximumViewportDimensions    = new Lazy<int>(() => GetLimit(All.MaxViewportDims));
        private static Lazy<int> _storageBufferOffsetAlignment = new Lazy<int>(() => GetLimit(All.ShaderStorageBufferOffsetAlignment));

        public static bool SupportsAstcCompression => _supportsAstcCompression.Value;

        public static int MaximumViewportDimensions    => _maximumViewportDimensions.Value;
        public static int StorageBufferOffsetAlignment => _storageBufferOffsetAlignment.Value;

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

        private static int GetLimit(All name)
        {
            return GL.GetInteger((GetPName)name);
        }
    }
}