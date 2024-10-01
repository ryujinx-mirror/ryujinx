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
        /// Total of extra sets used by the shaders.
        /// </summary>
        public int SetsCount;
    }
}
