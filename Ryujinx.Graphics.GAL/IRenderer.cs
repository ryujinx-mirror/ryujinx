using Ryujinx.Common.Configuration;
using Ryujinx.Graphics.Shader;
using System;

namespace Ryujinx.Graphics.GAL
{
    public interface IRenderer : IDisposable
    {
        IPipeline Pipeline { get; }

        IWindow Window { get; }

        void BackgroundContextAction(Action action);

        IShader CompileShader(ShaderStage stage, string code);

        BufferHandle CreateBuffer(int size);

        IProgram CreateProgram(IShader[] shaders, TransformFeedbackDescriptor[] transformFeedbackDescriptors);

        ISampler CreateSampler(SamplerCreateInfo info);
        ITexture CreateTexture(TextureCreateInfo info, float scale);

        void CreateSync(ulong id);

        void DeleteBuffer(BufferHandle buffer);

        byte[] GetBufferData(BufferHandle buffer, int offset, int size);

        Capabilities GetCapabilities();

        IProgram LoadProgramBinary(byte[] programBinary);

        void SetBufferData(BufferHandle buffer, int offset, ReadOnlySpan<byte> data);

        void UpdateCounters();

        void PreFrame();

        ICounterEvent ReportCounter(CounterType type, EventHandler<ulong> resultHandler);

        void ResetCounter(CounterType type);

        void WaitSync(ulong id);

        void Initialize(GraphicsDebugLevel logLevel);
    }
}
