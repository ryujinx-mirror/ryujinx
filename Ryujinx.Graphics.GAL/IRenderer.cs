using Ryujinx.Graphics.GAL.Sampler;
using Ryujinx.Graphics.GAL.Texture;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.GAL
{
    public interface IRenderer
    {
        IComputePipeline  ComputePipeline  { get; }
        IGraphicsPipeline GraphicsPipeline { get; }

        IWindow Window { get; }

        IShader CompileShader(ShaderProgram shader);

        IBuffer CreateBuffer(int size);

        IProgram CreateProgram(IShader[] shaders);

        ISampler CreateSampler(SamplerCreateInfo info);
        ITexture CreateTexture(TextureCreateInfo info);

        void FlushPipelines();

        Capabilities GetCapabilities();

        ulong GetCounter(CounterType type);

        void InitializeCounters();

        void ResetCounter(CounterType type);
    }
}
