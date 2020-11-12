using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Gpu.Shader.Cache.Definition
{
    /// <summary>
    /// Header for transform feedback.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    struct GuestShaderCacheTransformFeedbackHeader
    {
        /// <summary>
        /// The buffer index of the transform feedback.
        /// </summary>
        public int BufferIndex;

        /// <summary>
        /// The stride of the transform feedback.
        /// </summary>
        public int Stride;

        /// <summary>
        /// The length of the varying location buffer of the transform feedback.
        /// </summary>
        public int VaryingLocationsLength;

        /// <summary>
        /// Reserved/unused.
        /// </summary>
        public int Reserved1;

        public GuestShaderCacheTransformFeedbackHeader(int bufferIndex, int stride, int varyingLocationsLength) : this()
        {
            BufferIndex = bufferIndex;
            Stride = stride;
            VaryingLocationsLength = varyingLocationsLength;
        }
    }
}
