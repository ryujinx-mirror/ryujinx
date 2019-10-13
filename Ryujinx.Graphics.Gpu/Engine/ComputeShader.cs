using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.Gpu.Engine
{
    class ComputeShader
    {
        public IProgram Interface { get; set; }

        public ShaderProgram Shader { get; }

        public ComputeShader(IProgram program, ShaderProgram shader)
        {
            Interface = program;
            Shader    = shader;
        }
    }
}