using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    static class HwCapabilities
    {
        private static readonly Lazy<bool> _supportsAstcCompression           = new Lazy<bool>(() => HasExtension("GL_KHR_texture_compression_astc_ldr"));
        private static readonly Lazy<bool> _supportsImageLoadFormatted        = new Lazy<bool>(() => HasExtension("GL_EXT_shader_image_load_formatted"));
        private static readonly Lazy<bool> _supportsPolygonOffsetClamp        = new Lazy<bool>(() => HasExtension("GL_EXT_polygon_offset_clamp"));
        private static readonly Lazy<bool> _supportsViewportSwizzle           = new Lazy<bool>(() => HasExtension("GL_NV_viewport_swizzle"));
        private static readonly Lazy<bool> _supportsSeamlessCubemapPerTexture = new Lazy<bool>(() => HasExtension("GL_ARB_seamless_cubemap_per_texture"));
        private static readonly Lazy<bool> _supportsParallelShaderCompile     = new Lazy<bool>(() => HasExtension("GL_ARB_parallel_shader_compile"));

        private static readonly Lazy<int> _maximumComputeSharedMemorySize = new Lazy<int>(() => GetLimit(All.MaxComputeSharedMemorySize));
        private static readonly Lazy<int> _storageBufferOffsetAlignment   = new Lazy<int>(() => GetLimit(All.ShaderStorageBufferOffsetAlignment));

        public enum GpuVendor
        {
            Unknown,
            Amd,
            IntelWindows,
            IntelUnix,
            Nvidia
        }

        private static readonly Lazy<GpuVendor> _gpuVendor = new Lazy<GpuVendor>(GetGpuVendor);

        public static GpuVendor Vendor => _gpuVendor.Value;

        private static Lazy<float> _maxSupportedAnisotropy = new Lazy<float>(GL.GetFloat((GetPName)All.MaxTextureMaxAnisotropy));

        public static bool SupportsAstcCompression           => _supportsAstcCompression.Value;
        public static bool SupportsImageLoadFormatted        => _supportsImageLoadFormatted.Value;
        public static bool SupportsPolygonOffsetClamp        => _supportsPolygonOffsetClamp.Value;
        public static bool SupportsViewportSwizzle           => _supportsViewportSwizzle.Value;
        public static bool SupportsSeamlessCubemapPerTexture => _supportsSeamlessCubemapPerTexture.Value;
        public static bool SupportsParallelShaderCompile     => _supportsParallelShaderCompile.Value;
        public static bool SupportsNonConstantTextureOffset  => _gpuVendor.Value == GpuVendor.Nvidia;
        public static bool RequiresSyncFlush                 => _gpuVendor.Value == GpuVendor.Amd || _gpuVendor.Value == GpuVendor.IntelWindows || _gpuVendor.Value == GpuVendor.IntelUnix;

        public static int MaximumComputeSharedMemorySize => _maximumComputeSharedMemorySize.Value;
        public static int StorageBufferOffsetAlignment   => _storageBufferOffsetAlignment.Value;

        public static float MaximumSupportedAnisotropy => _maxSupportedAnisotropy.Value;

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

        private static GpuVendor GetGpuVendor()
        {
            string vendor = GL.GetString(StringName.Vendor).ToLower();

            if (vendor == "nvidia corporation")
            {
                return GpuVendor.Nvidia;
            }
            else if (vendor == "intel")
            {
                string renderer = GL.GetString(StringName.Renderer).ToLower();
                
                return renderer.Contains("mesa") ? GpuVendor.IntelUnix : GpuVendor.IntelWindows;
            }
            else if (vendor == "ati technologies inc." || vendor == "advanced micro devices, inc.")
            {
                return GpuVendor.Amd;
            }
            else
            {
                return GpuVendor.Unknown;
            }
        }
    }
}