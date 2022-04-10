using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.OpenGL.Image;
using Ryujinx.Graphics.OpenGL.Queries;
using Ryujinx.Graphics.Shader.Translation;
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

        public event EventHandler<ScreenCaptureImageInfo> ScreenCaptured;

        internal PersistentBuffers PersistentBuffers { get; }

        internal ResourcePool ResourcePool { get; }

        internal int BufferCount { get; private set; }

        public string GpuVendor { get; private set; }
        public string GpuRenderer { get; private set; }
        public string GpuVersion { get; private set; }

        public bool PreferThreading => true;

        public Renderer()
        {
            _pipeline = new Pipeline();
            _counters = new Counters();
            _window = new Window(this);
            _textureCopy = new TextureCopy(this);
            _backgroundTextureCopy = new TextureCopy(this);
            _sync = new Sync();
            PersistentBuffers = new PersistentBuffers();
            ResourcePool = new ResourcePool();
        }

        public BufferHandle CreateBuffer(int size)
        {
            BufferCount++;

            return Buffer.Create(size);
        }

        public IProgram CreateProgram(ShaderSource[] shaders, ShaderInfo info)
        {
            return new Program(shaders, info.FragmentOutputMap);
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

        public ReadOnlySpan<byte> GetBufferData(BufferHandle buffer, int offset, int size)
        {
            return Buffer.GetData(this, buffer, offset, size);
        }

        public Capabilities GetCapabilities()
        {
            return new Capabilities(
                api: TargetApi.OpenGL,
                vendorName: GpuVendor,
                hasFrontFacingBug: HwCapabilities.Vendor == HwCapabilities.GpuVendor.IntelWindows,
                hasVectorIndexingBug: HwCapabilities.Vendor == HwCapabilities.GpuVendor.AmdWindows,
                supportsAstcCompression: HwCapabilities.SupportsAstcCompression,
                supports3DTextureCompression: false,
                supportsBgraFormat: false,
                supportsR4G4Format: false,
                supportsFragmentShaderInterlock: HwCapabilities.SupportsFragmentShaderInterlock,
                supportsFragmentShaderOrderingIntel: HwCapabilities.SupportsFragmentShaderOrdering,
                supportsImageLoadFormatted: HwCapabilities.SupportsImageLoadFormatted,
                supportsMismatchingViewFormat: HwCapabilities.SupportsMismatchingViewFormat,
                supportsNonConstantTextureOffset: HwCapabilities.SupportsNonConstantTextureOffset,
                supportsShaderBallot: HwCapabilities.SupportsShaderBallot,
                supportsTextureShadowLod: HwCapabilities.SupportsTextureShadowLod,
                supportsViewportSwizzle: HwCapabilities.SupportsViewportSwizzle,
                supportsIndirectParameters: HwCapabilities.SupportsIndirectParameters,
                maximumComputeSharedMemorySize: HwCapabilities.MaximumComputeSharedMemorySize,
                maximumSupportedAnisotropy: HwCapabilities.MaximumSupportedAnisotropy,
                storageBufferOffsetAlignment: HwCapabilities.StorageBufferOffsetAlignment);
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

        public ICounterEvent ReportCounter(CounterType type, EventHandler<ulong> resultHandler, bool hostReserved)
        {
            return _counters.QueueReport(type, resultHandler, _pipeline.DrawCount, hostReserved);
        }

        public void Initialize(GraphicsDebugLevel glLogLevel)
        {
            Debugger.Initialize(glLogLevel);

            PrintGpuInformation();

            if (HwCapabilities.SupportsParallelShaderCompile)
            {
                GL.Arb.MaxShaderCompilerThreads(Math.Min(Environment.ProcessorCount, 8));
            }

            _pipeline.Initialize(this);
            _counters.Initialize();

            // This is required to disable [0, 1] clamping for SNorm outputs on compatibility profiles.
            // This call is expected to fail if we're running with a core profile,
            // as this clamp target was deprecated, but that's fine as a core profile
            // should already have the desired behaviour were outputs are not clamped.
            GL.ClampColor(ClampColorTarget.ClampFragmentColor, ClampColorMode.False);
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

        public void BackgroundContextAction(Action action, bool alwaysBackground = false)
        {
            // alwaysBackground is ignored, since we cannot switch from the current context.

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
            PersistentBuffers.Dispose();
            ResourcePool.Dispose();
            _pipeline.Dispose();
            _window.Dispose();
            _counters.Dispose();
            _sync.Dispose();
        }

        public IProgram LoadProgramBinary(byte[] programBinary, bool hasFragmentShader, ShaderInfo info)
        {
            return new Program(programBinary, hasFragmentShader, info.FragmentOutputMap);
        }

        public void CreateSync(ulong id)
        {
            _sync.Create(id);
        }

        public void WaitSync(ulong id)
        {
            _sync.Wait(id);
        }

        public void Screenshot()
        {
            _window.ScreenCaptureRequested = true;
        }

        public void OnScreenCaptured(ScreenCaptureImageInfo bitmap)
        {
            ScreenCaptured?.Invoke(this, bitmap);
        }
    }
}
