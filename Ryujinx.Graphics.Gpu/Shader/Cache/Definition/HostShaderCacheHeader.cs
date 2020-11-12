using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader.Cache.Definition
{
    /// <summary>
    /// The header of a shader program in the guest cache.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0x1, Size = 0x10)]
    struct HostShaderCacheHeader
    {
        /// <summary>
        /// The count of shaders defining this program.
        /// </summary>
        public byte Count;

        /// <summary>
        /// Unused/reserved.
        /// </summary>
        public byte Reserved1;

        /// <summary>
        /// Unused/reserved.
        /// </summary>
        public ushort Reserved2;

        /// <summary>
        /// Size of the shader binary.
        /// </summary>
        public int CodeSize;

        /// <summary>
        /// Create a new host shader cache header.
        /// </summary>
        /// <param name="count">The count of shaders defining this program</param>
        /// <param name="codeSize">The size of the shader binary</param>
        public HostShaderCacheHeader(byte count, int codeSize) : this()
        {
            Count    = count;
            CodeSize = codeSize;
        }
    }
}
