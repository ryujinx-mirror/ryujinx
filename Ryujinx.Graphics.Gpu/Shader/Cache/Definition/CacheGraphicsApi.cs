namespace Ryujinx.Graphics.Gpu.Shader.Cache.Definition
{
    /// <summary>
    /// Graphics API type accepted by the shader cache.
    /// </summary>
    enum CacheGraphicsApi : byte
    {
        /// <summary>
        /// OpenGL Core
        /// </summary>
        OpenGL,

        /// <summary>
        /// OpenGL ES
        /// </summary>
        OpenGLES,

        /// <summary>
        /// Vulkan
        /// </summary>
        Vulkan,

        /// <summary>
        /// DirectX
        /// </summary>
        DirectX,

        /// <summary>
        /// Metal
        /// </summary>
        Metal,

        /// <summary>
        /// Guest, used to cache games raw shader programs.
        /// </summary>
        Guest
    }
}
