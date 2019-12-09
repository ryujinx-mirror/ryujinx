using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.GAL.Sampler;
using Ryujinx.Graphics.GAL.Texture;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.OpenGL
{
    public class Renderer : IRenderer
    {
        public IPipeline Pipeline { get; }

        private Counters _counters;

        private Window _window;

        public IWindow Window => _window;

        internal TextureCopy TextureCopy { get; }

        public Renderer()
        {
            Pipeline = new Pipeline();

            _counters = new Counters();

            _window = new Window();

            TextureCopy = new TextureCopy();
        }

        public IShader CompileShader(ShaderProgram shader)
        {
            return new Shader(shader);
        }

        public IBuffer CreateBuffer(int size)
        {
            return new Buffer(size);
        }

        public IProgram CreateProgram(IShader[] shaders)
        {
            return new Program(shaders);
        }

        public ISampler CreateSampler(SamplerCreateInfo info)
        {
            return new Sampler(info);
        }

        public ITexture CreateTexture(TextureCreateInfo info)
        {
            return new TextureStorage(this, info).CreateDefaultView();
        }

        public void FlushPipelines()
        {
            GL.Finish();
        }

        public Capabilities GetCapabilities()
        {
            return new Capabilities(
                HwCapabilities.SupportsAstcCompression,
                HwCapabilities.MaximumViewportDimensions,
                HwCapabilities.MaximumComputeSharedMemorySize,
                HwCapabilities.StorageBufferOffsetAlignment);
        }

        public ulong GetCounter(CounterType type)
        {
            return _counters.GetCounter(type);
        }

        public void InitializeCounters()
        {
            _counters.Initialize();
        }

        public void ResetCounter(CounterType type)
        {
            _counters.ResetCounter(type);
        }
    }
}
