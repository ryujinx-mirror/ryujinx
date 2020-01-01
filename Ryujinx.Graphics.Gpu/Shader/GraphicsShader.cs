using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Cached graphics shader code for all stages.
    /// </summary>
    class GraphicsShader
    {
        /// <summary>
        /// Host shader program object.
        /// </summary>
        public IProgram HostProgram { get; set; }

        /// <summary>
        /// Compiled shader for each shader stage.
        /// </summary>
        public CachedShader[] Shaders { get; }

        /// <summary>
        /// Creates a new instance of cached graphics shader.
        /// </summary>
        public GraphicsShader()
        {
            Shaders = new CachedShader[Constants.ShaderStages];
        }
    }
}