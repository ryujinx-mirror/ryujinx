using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// Cached shader code for a single shader stage.
    /// </summary>
    class ShaderCodeHolder
    {
        /// <summary>
        /// Shader program containing translated code.
        /// </summary>
        public ShaderProgram Program { get; }

        /// <summary>
        /// Shader program information.
        /// </summary>
        public ShaderProgramInfo Info { get; }

        /// <summary>
        /// Host shader object.
        /// </summary>
        /// <remarks>Null if the host shader program cache is in use.</remarks>
        public IShader HostShader { get; set; }

        /// <summary>
        /// Maxwell binary shader code.
        /// </summary>
        public byte[] Code { get; }

        /// <summary>
        /// Optional maxwell binary shader code for "Vertex A" shader.
        /// </summary>
        public byte[] Code2 { get; }

        /// <summary>
        /// Creates a new instace of the shader code holder.
        /// </summary>
        /// <param name="program">Shader program</param>
        /// <param name="info">Shader program information</param>
        /// <param name="code">Maxwell binary shader code</param>
        /// <param name="code2">Optional binary shader code of the "Vertex A" shader, when combined with "Vertex B"</param>
        public ShaderCodeHolder(ShaderProgram program, ShaderProgramInfo info, byte[] code, byte[] code2 = null)
        {
            Program = program;
            Info    = info;
            Code    = code;
            Code2   = code2;
        }
    }
}