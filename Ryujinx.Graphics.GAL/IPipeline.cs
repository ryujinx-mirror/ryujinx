using Ryujinx.Graphics.Shader;

namespace Ryujinx.Graphics.GAL
{
    public interface IPipeline
    {
        void Barrier();

        void ClearRenderTargetColor(int index, uint componentMask, ColorF color);

        void ClearRenderTargetDepthStencil(
            float depthValue,
            bool  depthMask,
            int   stencilValue,
            int   stencilMask);

        void DispatchCompute(int groupsX, int groupsY, int groupsZ);

        void Draw(int vertexCount, int instanceCount, int firstVertex, int firstInstance);
        void DrawIndexed(
            int indexCount,
            int instanceCount,
            int firstIndex,
            int firstVertex,
            int firstInstance);

        void SetBlendState(int index, BlendDescriptor blend);

        void SetBlendColor(ColorF color);

        void SetDepthBias(PolygonModeMask enables, float factor, float units, float clamp);

        void SetDepthMode(DepthMode mode);

        void SetDepthTest(DepthTestDescriptor depthTest);

        void SetFaceCulling(bool enable, Face face);

        void SetFrontFace(FrontFace frontFace);

        void SetIndexBuffer(BufferRange buffer, IndexType type);

        void SetImage(int index, ShaderStage stage, ITexture texture);

        void SetPrimitiveRestart(bool enable, int index);

        void SetPrimitiveTopology(PrimitiveTopology topology);

        void SetProgram(IProgram program);

        void SetRenderTargetColorMasks(uint[] componentMask);

        void SetRenderTargets(ITexture[] colors, ITexture depthStencil);

        void SetSampler(int index, ShaderStage stage, ISampler sampler);

        void SetStencilTest(StencilTestDescriptor stencilTest);

        void SetStorageBuffer(int index, ShaderStage stage, BufferRange buffer);

        void SetTexture(int index, ShaderStage stage, ITexture texture);

        void SetUniformBuffer(int index, ShaderStage stage, BufferRange buffer);

        void SetVertexAttribs(VertexAttribDescriptor[] vertexAttribs);
        void SetVertexBuffers(VertexBufferDescriptor[] vertexBuffers);

        void SetViewports(int first, Viewport[] viewports);

        void TextureBarrier();
        void TextureBarrierTiled();
    }
}
