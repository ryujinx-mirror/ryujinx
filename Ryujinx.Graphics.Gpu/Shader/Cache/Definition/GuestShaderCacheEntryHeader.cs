using Ryujinx.Graphics.Shader;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader.Cache.Definition
{
    /// <summary>
    /// The header of a guest shader entry in a guest shader program.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0x1, Size = 0x30)]
    struct GuestShaderCacheEntryHeader
    {
        /// <summary>
        /// The stage of this shader.
        /// </summary>
        public ShaderStage Stage;

        /// <summary>
        /// Unused/reserved.
        /// </summary>
        public byte Reserved1;

        /// <summary>
        /// Unused/reserved.
        /// </summary>
        public byte Reserved2;

        /// <summary>
        /// Unused/reserved.
        /// </summary>
        public byte Reserved3;

        /// <summary>
        /// The size of the code section.
        /// </summary>
        public int Size;

        /// <summary>
        /// The size of the code2 section if present. (Vertex A)
        /// </summary>
        public int SizeA;

        /// <summary>
        /// Unused/reserved.
        /// </summary>
        public int Reserved4;

        /// <summary>
        /// The header of the cached gpu accessor.
        /// </summary>
        public GuestGpuAccessorHeader GpuAccessorHeader;

        /// <summary>
        /// Create a new guest shader entry header.
        /// </summary>
        /// <param name="stage">The stage of this shader</param>
        /// <param name="size">The size of the code section</param>
        /// <param name="sizeA">The size of the code2 section if present (Vertex A)</param>
        /// <param name="gpuAccessorHeader">The header of the cached gpu accessor</param>
        public GuestShaderCacheEntryHeader(ShaderStage stage, int size, int sizeA, GuestGpuAccessorHeader gpuAccessorHeader) : this()
        {
            Stage = stage;
            Size  = size;
            SizeA = sizeA;
            GpuAccessorHeader = gpuAccessorHeader;
        }
    }
}
