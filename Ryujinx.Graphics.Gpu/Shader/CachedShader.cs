using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.Gpu.Shader
{
    class CachedShader
    {
        public ShaderProgram Program { get; }
        public IShader       Shader  { get; set; }

        public int[] Code { get; }

        public CachedShader(ShaderProgram program, int[] code)
        {
            Program  = program;
            Code     = code;
        }
    }
}