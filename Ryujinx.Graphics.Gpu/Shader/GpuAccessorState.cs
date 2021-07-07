namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// State used by the <see cref="GpuAccessor"/>.
    /// </summary>
    struct GpuAccessorState
    {
        /// <summary>
        /// GPU virtual address of the texture pool.
        /// </summary>
        public ulong TexturePoolGpuVa { get; }

        /// <summary>
        /// Maximum ID of the texture pool.
        /// </summary>
        public int TexturePoolMaximumId { get; }

        /// <summary>
        /// Constant buffer slot where the texture handles are located.
        /// </summary>
        public int TextureBufferIndex { get; }

        /// <summary>
        /// Early Z force enable.
        /// </summary>
        public bool EarlyZForce { get; }

        /// <summary>
        /// Creates a new instance of the GPU accessor state.
        /// </summary>
        /// <param name="texturePoolGpuVa">GPU virtual address of the texture pool</param>
        /// <param name="texturePoolMaximumId">Maximum ID of the texture pool</param>
        /// <param name="textureBufferIndex">Constant buffer slot where the texture handles are located</param>
        /// <param name="earlyZForce">Early Z force enable</param>
        public GpuAccessorState(ulong texturePoolGpuVa, int texturePoolMaximumId, int textureBufferIndex, bool earlyZForce)
        {
            TexturePoolGpuVa = texturePoolGpuVa;
            TexturePoolMaximumId = texturePoolMaximumId;
            TextureBufferIndex = textureBufferIndex;
            EarlyZForce = earlyZForce;
        }
    }
}