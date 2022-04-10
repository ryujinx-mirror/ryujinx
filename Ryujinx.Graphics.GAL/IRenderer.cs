using Ryujinx.Common.Configuration;
using Ryujinx.Graphics.Shader;
using System;

namespace Ryujinx.Graphics.GAL
{
    public interface IRenderer : IDisposable
    {
        event EventHandler<ScreenCaptureImageInfo> ScreenCaptured;

        bool PreferThreading { get; }

        IPipeline Pipeline { get; }

        IWindow Window { get; }

        void BackgroundContextAction(Action action, bool alwaysBackground = false);

        BufferHandle CreateBuffer(int size);

        IProgram CreateProgram(ShaderSource[] shaders, ShaderInfo info);

        ISampler CreateSampler(SamplerCreateInfo info);
        ITexture CreateTexture(TextureCreateInfo info, float scale);

        void CreateSync(ulong id);

        void DeleteBuffer(BufferHandle buffer);

        ReadOnlySpan<byte> GetBufferData(BufferHandle buffer, int offset, int size);

        Capabilities GetCapabilities();

        IProgram LoadProgramBinary(byte[] programBinary, bool hasFragmentShader, ShaderInfo info);

        void SetBufferData(BufferHandle buffer, int offset, ReadOnlySpan<byte> data);

        void UpdateCounters();

        void PreFrame();

        ICounterEvent ReportCounter(CounterType type, EventHandler<ulong> resultHandler, bool hostReserved);

        void ResetCounter(CounterType type);

        void RunLoop(Action gpuLoop)
        {
            gpuLoop();
        }

        void WaitSync(ulong id);

        void Initialize(GraphicsDebugLevel logLevel);

        void Screenshot();
    }
}
