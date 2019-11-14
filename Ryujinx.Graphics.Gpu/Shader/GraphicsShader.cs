using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.Shader
{
    class GraphicsShader
    {
        public IProgram HostProgram { get; set; }

        public CachedShader[] Shader { get; }

        public GraphicsShader()
        {
            Shader = new CachedShader[5];
        }
    }
}