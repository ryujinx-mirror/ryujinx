namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Represents a GPU state and memory accessor.
    /// </summary>
    class GpuAccessorBase
    {
        private readonly GpuContext _context;

        /// <summary>
        /// Creates a new instance of the GPU state accessor.
        /// </summary>
        /// <param name="context">GPU context</param>
        public GpuAccessorBase(GpuContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Queries host about the presence of the FrontFacing built-in variable bug.
        /// </summary>
        /// <returns>True if the bug is present on the host device used, false otherwise</returns>
        public bool QueryHostHasFrontFacingBug() => _context.Capabilities.HasFrontFacingBug;

        /// <summary>
        /// Queries host about the presence of the vector indexing bug.
        /// </summary>
        /// <returns>True if the bug is present on the host device used, false otherwise</returns>
        public bool QueryHostHasVectorIndexingBug() => _context.Capabilities.HasVectorIndexingBug;

        /// <summary>
        /// Queries host storage buffer alignment required.
        /// </summary>
        /// <returns>Host storage buffer alignment in bytes</returns>
        public int QueryHostStorageBufferOffsetAlignment() => _context.Capabilities.StorageBufferOffsetAlignment;

        /// <summary>
        /// Queries host support for readable images without a explicit format declaration on the shader.
        /// </summary>
        /// <returns>True if formatted image load is supported, false otherwise</returns>
        public bool QueryHostSupportsImageLoadFormatted() => _context.Capabilities.SupportsImageLoadFormatted;

        /// <summary>
        /// Queries host GPU non-constant texture offset support.
        /// </summary>
        /// <returns>True if the GPU and driver supports non-constant texture offsets, false otherwise</returns>
        public bool QueryHostSupportsNonConstantTextureOffset() => _context.Capabilities.SupportsNonConstantTextureOffset;

        /// <summary>
        /// Queries host GPU texture shadow LOD support.
        /// </summary>
        /// <returns>True if the GPU and driver supports texture shadow LOD, false otherwise</returns>
        public bool QueryHostSupportsTextureShadowLod() => _context.Capabilities.SupportsTextureShadowLod;
    }
}
