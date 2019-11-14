using Ryujinx.Graphics.GAL;

namespace Ryujinx.Graphics.Gpu.Shader
{
    class ComputeShader
    {
        public IProgram HostProgram { get; set; }

        public CachedShader Shader { get; }

        public ComputeShader(IProgram hostProgram, CachedShader shader)
        {
            HostProgram = hostProgram;
            Shader      = shader;
        }
    }
}