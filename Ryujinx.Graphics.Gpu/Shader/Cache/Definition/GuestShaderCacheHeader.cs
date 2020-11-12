using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader.Cache.Definition
{
    /// <summary>
    /// The header of a shader program in the guest cache.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0x1, Size = 0x10)]
    struct GuestShaderCacheHeader
    {
        /// <summary>
        /// The count of shaders defining this program.
        /// </summary>
        public byte Count;

        /// <summary>
        /// The count of transform feedback data used in this program.
        /// </summary>
        public byte TransformFeedbackCount;

        /// <summary>
        /// Unused/reserved.
        /// </summary>
        public ushort Reserved1;

        /// <summary>
        /// Unused/reserved.
        /// </summary>
        public ulong Reserved2;

        /// <summary>
        /// Create a new guest shader cache header.
        /// </summary>
        /// <param name="count">The count of shaders defining this program</param>
        /// <param name="transformFeedbackCount">The count of transform feedback data used in this program</param>
        public GuestShaderCacheHeader(byte count, byte transformFeedbackCount) : this()
        {
            Count = count;
            TransformFeedbackCount = transformFeedbackCount;
        }
    }
}
