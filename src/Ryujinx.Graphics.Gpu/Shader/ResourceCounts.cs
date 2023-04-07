namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Holds counts for the resources used by a shader.
    /// </summary>
    class ResourceCounts
    {
        /// <summary>
        /// Total of uniform buffers used by the shaders.
        /// </summary>
        public int UniformBuffersCount;

        /// <summary>
        /// Total of storage buffers used by the shaders.
        /// </summary>
        public int StorageBuffersCount;

        /// <summary>
        /// Total of textures used by the shaders.
        /// </summary>
        public int TexturesCount;

        /// <summary>
        /// Total of images used by the shaders.
        /// </summary>
        public int ImagesCount;

        /// <summary>
        /// Creates a new instance of the shader resource counts class.
        /// </summary>
        public ResourceCounts()
        {
            UniformBuffersCount = 1; // The first binding is reserved for the support buffer.
        }
    }
}