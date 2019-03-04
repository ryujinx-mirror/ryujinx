using System;

namespace Ryujinx.Graphics.Gal
{
    public interface IGalRasterizer
    {
        void LockCaches();
        void UnlockCaches();

        void ClearBuffers(
            GalClearBufferFlags flags,
            int attachment,
            float red,
            float green,
            float blue,
            float alpha,
            float depth,
            int stencil);

        bool IsVboCached(long key, long dataSize);

        bool IsIboCached(long key, long dataSize);

        void CreateVbo(long key, int dataSize, IntPtr hostAddress);
        void CreateVbo(long key, byte[] data);

        void CreateIbo(long key, int dataSize, IntPtr hostAddress);
        void CreateIbo(long key, int dataSize, byte[] buffer);

        void SetIndexArray(int size, GalIndexFormat format);

        void DrawArrays(int first, int count, GalPrimitiveType primType);

        void DrawElements(long iboKey, int first, int vertexBase, GalPrimitiveType primType);
    }
}