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
        /// <remarks>
        /// This does not reflect the hardware count, the API will emulate some constant buffers using
        /// global memory to make up for the low amount of compute constant buffers supported by hardware (only 8).
        /// </remarks>
        public const int TotalCpUniformBuffers = 17; // 8 hardware constant buffers + 9 emulated (14 available to the user).

        /// <summary>
        /// Maximum number of compute storage buffers.
        /// </summary>
        /// <remarks>
        /// The maximum number of storage buffers is API limited, the hardware supports a unlimited amount.
        /// </remarks>
        public const int TotalCpStorageBuffers = 16;

        /// <summary>
        /// Maximum number of graphics uniform buffers.
        /// </summary>
        public const int TotalGpUniformBuffers = 18;

        /// <summary>
        /// Maximum number of graphics storage buffers.
        /// </summary>
        /// <remarks>
        /// The maximum number of storage buffers is API limited, the hardware supports a unlimited amount.
        /// </remarks>
        public const int TotalGpStorageBuffers = 16;

        /// <summary>
        /// Maximum number of transform feedback buffers.
        /// </summary>
        public const int TotalTransformFeedbackBuffers = 4;

        /// <summary>
        /// Maximum number of render target color buffers.
        /// </summary>
        public const int TotalRenderTargets = 8;

        /// <summary>
        /// Number of shader stages.
        /// </summary>
        public const int ShaderStages = 5;

        /// <summary>
        /// Maximum number of vertex attributes.
        /// </summary>
        public const int TotalVertexAttribs = 16;

        /// <summary>
        /// Maximum number of vertex buffers.
        /// </summary>
        public const int TotalVertexBuffers = 16;

        /// <summary>
        /// Maximum number of viewports.
        /// </summary>
        public const int TotalViewports = 16;

        /// <summary>
        /// Maximum size of gl_ClipDistance array in shaders.
        /// </summary>
        public const int TotalClipDistances = 8;
    }
}