using Ryujinx.Graphics.Shader;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader.Cache.Definition
{
    /// <summary>
    /// Header of a cached guest gpu accessor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 0x20, Pack = 1)]
    struct GuestGpuAccessorHeader
    {
        /// <summary>
        /// The count of texture descriptors.
        /// </summary>
        public int TextureDescriptorCount;

        /// <summary>
        /// Local Size X for compute shaders.
        /// </summary>
        public int ComputeLocalSizeX;

        /// <summary>
        /// Local Size Y for compute shaders.
        /// </summary>
        public int ComputeLocalSizeY;

        /// <summary>
        /// Local Size Z for compute shaders.
        /// </summary>
        public int ComputeLocalSizeZ;

        /// <summary>
        /// Local Memory size in bytes for compute shaders.
        /// </summary>
        public int ComputeLocalMemorySize;

        /// <summary>
        /// Shared Memory size in bytes for compute shaders.
        /// </summary>
        public int ComputeSharedMemorySize;

        /// <summary>
        /// Unused/reserved.
        /// </summary>
        public int Reserved1;

        /// <summary>
        /// Current primitive topology for geometry shaders.
        /// </summary>
        public InputTopology PrimitiveTopology;

        /// <summary>
        /// Unused/reserved.
        /// </summary>
        public ushort Reserved2;

        /// <summary>
        /// GPU boolean state that can influence shader compilation.
        /// </summary>
        public GuestGpuStateFlags StateFlags;
    }
}
