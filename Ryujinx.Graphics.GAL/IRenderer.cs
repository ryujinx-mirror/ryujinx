using Ryujinx.Common.Configuration;
using Ryujinx.Graphics.Shader;
using System;

namespace Ryujinx.Graphics.GAL
{
    public interface IRenderer : IDisposable
    {
        IPipeline Pipeline { get; }

        IWindow Window { get; }

        IShader CompileShader(ShaderProgram shader);

        BufferHandle CreateBuffer(int size);

        IProgram CreateProgram(IShader[] shaders, TransformFeedbackDescriptor[] transformFeedbackDescriptors);

        ISampler CreateSampler(SamplerCreateInfo info);
        ITexture CreateTexture(TextureCreateInfo info, float scale);

        void DeleteBuffer(BufferHandle buffer);

        byte[] GetBufferData(BufferHandle buffer, int offset, int size);

        Capabilities GetCapabilities();

        void SetBufferData(BufferHandle buffer, int offset, ReadOnlySpan<byte> data);

        void UpdateCounters();

        void PreFrame();

        ICounterEvent ReportCounter(CounterType type, EventHandler<ulong> resultHandler);

        void ResetCounter(CounterType type);

        void Initialize(GraphicsDebugLevel logLevel);
    }
}
