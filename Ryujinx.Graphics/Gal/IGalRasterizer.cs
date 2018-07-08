namespace Ryujinx.Graphics.Gal
{
    public interface IGalRasterizer
    {
        void ClearBuffers(GalClearBufferFlags Flags);

        bool IsVboCached(long Key, long DataSize);

        bool IsIboCached(long Key, long DataSize);

        void SetFrontFace(GalFrontFace FrontFace);

        void EnableCullFace();

        void DisableCullFace();

        void SetCullFace(GalCullFace CullFace);

        void EnableDepthTest();

        void DisableDepthTest();

        void SetDepthFunction(GalComparisonOp Func);

        void SetClearDepth(float Depth);

        void EnableStencilTest();

        void DisableStencilTest();

        void SetStencilFunction(bool IsFrontFace, GalComparisonOp Func, int Ref, int Mask);

        void SetStencilOp(bool IsFrontFace, GalStencilOp Fail, GalStencilOp ZFail, GalStencilOp ZPass);

        void SetStencilMask(bool IsFrontFace, int Mask);

        void SetClearStencil(int Stencil);

        void EnablePrimitiveRestart();

        void DisablePrimitiveRestart();

        void SetPrimitiveRestartIndex(uint Index);

        void CreateVbo(long Key, byte[] Buffer);

        void CreateIbo(long Key, byte[] Buffer);

        void SetVertexArray(int VbIndex, int Stride, long VboKey, GalVertexAttrib[] Attribs);

        void SetIndexArray(long Key, int Size, GalIndexFormat Format);

        void DrawArrays(int First, int PrimCount, GalPrimitiveType PrimType);

        void DrawElements(long IboKey, int First, int VertexBase, GalPrimitiveType PrimType);
    }
}