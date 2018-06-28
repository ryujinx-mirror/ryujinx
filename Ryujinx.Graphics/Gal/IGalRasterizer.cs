namespace Ryujinx.Graphics.Gal
{
    public interface IGalRasterizer
    {
        void ClearBuffers(GalClearBufferFlags Flags);

        bool IsVboCached(long Key, long DataSize);

        bool IsIboCached(long Key, long DataSize);

        void EnableCullFace();

        void DisableCullFace();

        void EnableDepthTest();

        void DisableDepthTest();

        void SetDepthFunction(GalComparisonOp Func);

        void CreateVbo(long Key, byte[] Buffer);

        void CreateIbo(long Key, byte[] Buffer);

        void SetVertexArray(int VbIndex, int Stride, long VboKey, GalVertexAttrib[] Attribs);

        void SetIndexArray(long Key, int Size, GalIndexFormat Format);

        void DrawArrays(int First, int PrimCount, GalPrimitiveType PrimType);

        void DrawElements(long IboKey, int First, int VertexBase, GalPrimitiveType PrimType);
    }
}