using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.Gpu.Engine
{
    class GraphicsShader
    {
        public IProgram Interface { get; set; }

        public ShaderProgram[] Shader { get; }

        public GraphicsShader()
        {
            Shader = new ShaderProgram[5];
        }
    }
}