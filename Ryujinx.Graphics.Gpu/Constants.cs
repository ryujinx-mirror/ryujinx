namespace Ryujinx.Graphics.Gpu
{
    /// <summary>
    /// Common Maxwell GPU constants.
    /// </summary>
    static class Constants
    {
        /// <summary>
        /// Maximum number of compute uniform buffers.
        /// </summary>
        public const int TotalCpUniformBuffers = 8;

        /// <summary>
        /// Maximum number of compute storage buffers (this is an API limitation).
        /// </summary>
        public const int TotalCpStorageBuffers = 16;

        /// <summary>
        /// Maximum number of graphics uniform buffers.
        /// </summary>
        public const int TotalGpUniformBuffers = 18;

        /// <summary>
        /// Maximum number of graphics storage buffers (this is an API limitation).
        /// </summary>
        public const int TotalGpStorageBuffers = 16;

        /// <summary>
        /// Maximum number of render target color buffers.
        /// </summary>
        public const int TotalRenderTargets = 8;

        /// <summary>
        /// Number of shader stages.
        /// </summary>
        public const int ShaderStages = 5;

        /// <summary>
        /// Maximum number of vertex buffers.
        /// </summary>
        public const int TotalVertexBuffers = 16;

        /// <summary>
        /// Maximum number of viewports.
        /// </summary>
        public const int TotalViewports = 8;
    }
}