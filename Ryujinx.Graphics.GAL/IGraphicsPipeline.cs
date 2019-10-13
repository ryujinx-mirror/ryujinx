using Ryujinx.Graphics.GAL.Blend;
using Ryujinx.Graphics.GAL.Color;
using Ryujinx.Graphics.GAL.DepthStencil;
using Ryujinx.Graphics.GAL.InputAssembler;
using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.GAL
{
    public interface IGraphicsPipeline
    {
        void BindBlendState(int index, BlendDescriptor blend);

        void BindIndexBuffer(BufferRange buffer, IndexType type);

        void BindProgram(IProgram program);

        void BindSampler(int index, ShaderStage stage, ISampler sampler);
        void BindTexture(int index, ShaderStage stage, ITexture texture);

        void BindStorageBuffers(int index, ShaderStage stage, BufferRange[] buffers);
        void BindUniformBuffers(int index, ShaderStage stage, BufferRange[] buffers);

        void BindVertexAttribs(VertexAttribDescriptor[] vertexAttribs);
        void BindVertexBuffers(VertexBufferDescriptor[] vertexBuffers);

        void ClearRenderTargetColor(int index, uint componentMask, ColorF color);
        void ClearRenderTargetColor(int index, uint componentMask, ColorSI color);
        void ClearRenderTargetColor(int index, uint componentMask, ColorUI color);

        void ClearRenderTargetDepthStencil(
            float depthValue,
            bool  depthMask,
            int   stencilValue,
            int   stencilMask);

        void Draw(int vertexCount, int instanceCount, int firstVertex, int firstInstance);
        void DrawIndexed(
            int indexCount,
            int instanceCount,
            int firstIndex,
            int firstVertex,
            int firstInstance);
        void DrawIndirect       (BufferRange buffer, ulong offset, int drawCount, int stride);
        void DrawIndexedIndirect(BufferRange buffer, ulong offset, int drawCount, int stride);

        void SetBlendColor(ColorF color);

        void SetDepthBias(PolygonModeMask enables, float factor, float units, float clamp);

        void SetDepthTest(DepthTestDescriptor depthTest);

        void SetFaceCulling(bool enable, Face face);

        void SetFrontFace(FrontFace frontFace);

        void SetPrimitiveRestart(bool enable, int index);

        void SetPrimitiveTopology(PrimitiveTopology topology);

        void SetRenderTargetColorMasks(uint[] componentMask);

        void SetRenderTargets(ITexture color3D, ITexture depthStencil);
        void SetRenderTargets(ITexture[] colors, ITexture depthStencil);

        void SetStencilTest(StencilTestDescriptor stencilTest);

        void SetViewports(int first, Viewport[] viewports);
    }
}
