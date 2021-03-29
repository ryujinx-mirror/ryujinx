using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.OpenGL.Image;
using Ryujinx.Graphics.OpenGL.Queries;
using Ryujinx.Graphics.Shader;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    public sealed class Renderer : IRenderer
    {
        private readonly Pipeline _pipeline;

        public IPipeline Pipeline => _pipeline;

        private readonly Counters _counters;

        private readonly Window _window;

        public IWindow Window => _window;

        private TextureCopy _textureCopy;
        private TextureCopy _backgroundTextureCopy;
        internal TextureCopy TextureCopy => BackgroundContextWorker.InBackground ? _backgroundTextureCopy : _textureCopy;

        private Sync _sync;

        internal ResourcePool ResourcePool { get; }

        internal int BufferCount { get; private set; }

        public string GpuVendor { get; private set; }
        public string GpuRenderer { get; private set; }
        public string GpuVersion { get; private set; }

        public Renderer()
        {
            _pipeline = new Pipeline();
            _counters = new Counters();
            _window = new Window(this);
            _textureCopy = new TextureCopy(this);
            _backgroundTextureCopy = new TextureCopy(this);
            _sync = new Sync();
            ResourcePool = new ResourcePool();
        }

        public IShader CompileShader(ShaderStage stage, string code)
        {
            return new Shader(stage, code);
        }

        public BufferHandle CreateBuffer(int size)
        {
            BufferCount++;

            return Buffer.Create(size);
        }

        public IProgram CreateProgram(IShader[] shaders, TransformFeedbackDescriptor[] transformFeedbackDescriptors)
        {
            return new Program(shaders, transformFeedbackDescriptors);
        }

        public ISampler CreateSampler(SamplerCreateInfo info)
        {
            return new Sampler(info);
        }

        public ITexture CreateTexture(TextureCreateInfo info, float scaleFactor)
        {
            if (info.Target == Target.TextureBuffer)
            {
                return new TextureBuffer(this, info);
            }
            else
            {
                return ResourcePool.GetTextureOrNull(info, scaleFactor) ?? new TextureStorage(this, info, scaleFactor).CreateDefaultView();
            }
        }

        public void DeleteBuffer(BufferHandle buffer)
        {
            Buffer.Delete(buffer);
        }

        public byte[] GetBufferData(BufferHandle buffer, int offset, int size)
        {
            return Buffer.GetData(buffer, offset, size);
        }

        public Capabilities GetCapabilities()
        {
            return new Capabilities(
                HwCapabilities.SupportsAstcCompression,
                HwCapabilities.SupportsImageLoadFormatted,
                HwCapabilities.SupportsNonConstantTextureOffset,
                HwCapabilities.SupportsViewportSwizzle,
                HwCapabilities.MaximumComputeSharedMemorySize,
                HwCapabilities.MaximumSupportedAnisotropy,
                HwCapabilities.StorageBufferOffsetAlignment);
        }

        public void SetBufferData(BufferHandle buffer, int offset, ReadOnlySpan<byte> data)
        {
            Buffer.SetData(buffer, offset, data);
        }

        public void UpdateCounters()
        {
            _counters.Update();
        }

        public void PreFrame()
        {
            _sync.Cleanup();
            ResourcePool.Tick();
        }

        public ICounterEvent ReportCounter(CounterType type, EventHandler<ulong> resultHandler)
        {
            return _counters.QueueReport(type, resultHandler, _pipeline.DrawCount);
        }

        public void Initialize(GraphicsDebugLevel glLogLevel)
        {
            Debugger.Initialize(glLogLevel);

            PrintGpuInformation();

            if (HwCapabilities.SupportsParallelShaderCompile)
            {
                GL.Arb.MaxShaderCompilerThreads(Math.Min(Environment.ProcessorCount, 8));
            }

            _counters.Initialize();
        }

        private void PrintGpuInformation()
        {
            GpuVendor   = GL.GetString(StringName.Vendor);
            GpuRenderer = GL.GetString(StringName.Renderer);
            GpuVersion  = GL.GetString(StringName.Version);

            Logger.Notice.Print(LogClass.Gpu, $"{GpuVendor} {GpuRenderer} ({GpuVersion})");
        }

        public void ResetCounter(CounterType type)
        {
            _counters.QueueReset(type);
        }

        public void BackgroundContextAction(Action action)
        {
            if (IOpenGLContext.HasContext())
            {
                action(); // We have a context already - use that (assuming it is the main one).
            }
            else
            {
                _window.BackgroundContext.Invoke(action);
            }
        }

        public void InitializeBackgroundContext(IOpenGLContext baseContext)
        {
            _window.InitializeBackgroundContext(baseContext);
        }

        public void Dispose()
        {
            _textureCopy.Dispose();
            _backgroundTextureCopy.Dispose();
            ResourcePool.Dispose();
            _pipeline.Dispose();
            _window.Dispose();
            _counters.Dispose();
            _sync.Dispose();
        }

        public IProgram LoadProgramBinary(byte[] programBinary)
        {
            return new Program(programBinary);
        }

        public void CreateSync(ulong id)
        {
            _sync.Create(id);
        }

        public void WaitSync(ulong id)
        {
            _sync.Wait(id);
        }
    }
}
