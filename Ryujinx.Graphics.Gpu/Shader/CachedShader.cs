using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Cached shader code for a single shader stage.
    /// </summary>
    class CachedShader
    {
        /// <summary>
        /// Shader program containing translated code.
        /// </summary>
        public ShaderProgram Program { get; }

        /// <summary>
        /// Host shader object.
        /// </summary>
        public IShader Shader { get; set; }

        /// <summary>
        /// Maxwell binary shader code.
        /// </summary>
        public int[] Code { get; }

        /// <summary>
        /// Creates a new instace of the cached shader.
        /// </summary>
        /// <param name="program">Shader program</param>
        /// <param name="code">Maxwell binary shader code</param>
        public CachedShader(ShaderProgram program, int[] code)
        {
            Program  = program;
            Code     = code;
        }
    }
}