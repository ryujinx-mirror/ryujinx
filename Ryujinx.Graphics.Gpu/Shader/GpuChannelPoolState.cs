namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// State used by the <see cref="GpuAccessor"/>.
    /// </summary>
    struct GpuChannelPoolState
    {
        /// <summary>
        /// GPU virtual address of the texture pool.
        /// </summary>
        public readonly ulong TexturePoolGpuVa;

        /// <summary>
        /// Maximum ID of the texture pool.
        /// </summary>
        public readonly int TexturePoolMaximumId;

        /// <summary>
        /// Constant buffer slot where the texture handles are located.
        /// </summary>
        public readonly int TextureBufferIndex;

        /// <summary>
        /// Creates a new GPU texture pool state.
        /// </summary>
        /// <param name="texturePoolGpuVa">GPU virtual address of the texture pool</param>
        /// <param name="texturePoolMaximumId">Maximum ID of the texture pool</param>
        /// <param name="textureBufferIndex">Constant buffer slot where the texture handles are located</param>
        public GpuChannelPoolState(ulong texturePoolGpuVa, int texturePoolMaximumId, int textureBufferIndex)
        {
            TexturePoolGpuVa = texturePoolGpuVa;
            TexturePoolMaximumId = texturePoolMaximumId;
            TextureBufferIndex = textureBufferIndex;
        }
    }
}