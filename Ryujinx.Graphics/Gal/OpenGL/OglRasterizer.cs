using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OglRasterizer : IGalRasterizer
    {
        private const long MaxVertexBufferCacheSize = 128 * 1024 * 1024;
        private const long MaxIndexBufferCacheSize  = 64  * 1024 * 1024;

        private int[] _vertexBuffers;

        private OglCachedResource<int> _vboCache;
        private OglCachedResource<int> _iboCache;

        private struct IbInfo
        {
            public int Count;
            public int ElemSizeLog2;

            public DrawElementsType Type;
        }

        private IbInfo _indexBuffer;

        public OglRasterizer()
        {
            _vertexBuffers = new int[32];

            _vboCache = new OglCachedResource<int>(GL.DeleteBuffer, MaxVertexBufferCacheSize);
            _iboCache = new OglCachedResource<int>(GL.DeleteBuffer, MaxIndexBufferCacheSize);

            _indexBuffer = new IbInfo();
        }

        public void LockCaches()
        {
            _vboCache.Lock();
            _iboCache.Lock();
        }

        public void UnlockCaches()
        {
            _vboCache.Unlock();
            _iboCache.Unlock();
        }

        public void ClearBuffers(
            GalClearBufferFlags flags,
            int attachment,
            float red,
            float green,
            float blue,
            float alpha,
            float depth,
            int stencil)
        {
            GL.ColorMask(
                attachment,
                flags.HasFlag(GalClearBufferFlags.ColorRed),
                flags.HasFlag(GalClearBufferFlags.ColorGreen),
                flags.HasFlag(GalClearBufferFlags.ColorBlue),
                flags.HasFlag(GalClearBufferFlags.ColorAlpha));

            GL.ClearBuffer(ClearBuffer.Color, attachment, new float[] { red, green, blue, alpha });

            GL.ColorMask(attachment, true, true, true, true);
            GL.DepthMask(true);

            if (flags.HasFlag(GalClearBufferFlags.Depth))
            {
                GL.ClearBuffer(ClearBuffer.Depth, 0, ref depth);
            }

            if (flags.HasFlag(GalClearBufferFlags.Stencil))
            {
                GL.ClearBuffer(ClearBuffer.Stencil, 0, ref stencil);
            }
        }

        public bool IsVboCached(long key, long dataSize)
        {
            return _vboCache.TryGetSize(key, out long size) && size == dataSize;
        }

        public bool IsIboCached(long key, long dataSize)
        {
            return _iboCache.TryGetSize(key, out long size) && size == dataSize;
        }

        public void CreateVbo(long key, int dataSize, IntPtr hostAddress)
        {
            int handle = GL.GenBuffer();

            _vboCache.AddOrUpdate(key, handle, dataSize);

            IntPtr length = new IntPtr(dataSize);

            GL.BindBuffer(BufferTarget.ArrayBuffer, handle);
            GL.BufferData(BufferTarget.ArrayBuffer, length, hostAddress, BufferUsageHint.StreamDraw);
        }

        public void CreateVbo(long key, byte[] data)
        {
            int handle = GL.GenBuffer();

            _vboCache.AddOrUpdate(key, handle, data.Length);

            IntPtr length = new IntPtr(data.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, handle);
            GL.BufferData(BufferTarget.ArrayBuffer, length, data, BufferUsageHint.StreamDraw);
        }

        public void CreateIbo(long key, int dataSize, IntPtr hostAddress)
        {
            int handle = GL.GenBuffer();

            _iboCache.AddOrUpdate(key, handle, (uint)dataSize);

            IntPtr length = new IntPtr(dataSize);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, length, hostAddress, BufferUsageHint.StreamDraw);
        }

        public void CreateIbo(long key, int dataSize, byte[] buffer)
        {
            int handle = GL.GenBuffer();

            _iboCache.AddOrUpdate(key, handle, dataSize);

            IntPtr length = new IntPtr(buffer.Length);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, length, buffer, BufferUsageHint.StreamDraw);
        }

        public void SetIndexArray(int size, GalIndexFormat format)
        {
            _indexBuffer.Type = OglEnumConverter.GetDrawElementsType(format);

            _indexBuffer.Count = size >> (int)format;

            _indexBuffer.ElemSizeLog2 = (int)format;
        }

        public void DrawArrays(int first, int count, GalPrimitiveType primType)
        {
            if (count == 0)
            {
                return;
            }

            if (primType == GalPrimitiveType.Quads)
            {
                for (int offset = 0; offset < count; offset += 4)
                {
                    GL.DrawArrays(PrimitiveType.TriangleFan, first + offset, 4);
                }
            }
            else if (primType == GalPrimitiveType.QuadStrip)
            {
                GL.DrawArrays(PrimitiveType.TriangleFan, first, 4);

                for (int offset = 2; offset < count; offset += 2)
                {
                    GL.DrawArrays(PrimitiveType.TriangleFan, first + offset, 4);
                }
            }
            else
            {
                GL.DrawArrays(OglEnumConverter.GetPrimitiveType(primType), first, count);
            }
        }

        public void DrawElements(long iboKey, int first, int vertexBase, GalPrimitiveType primType)
        {
            if (!_iboCache.TryGetValue(iboKey, out int iboHandle))
            {
                return;
            }

            PrimitiveType mode = OglEnumConverter.GetPrimitiveType(primType);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, iboHandle);

            first <<= _indexBuffer.ElemSizeLog2;

            if (vertexBase != 0)
            {
                IntPtr indices = new IntPtr(first);

                GL.DrawElementsBaseVertex(mode, _indexBuffer.Count, _indexBuffer.Type, indices, vertexBase);
            }
            else
            {
                GL.DrawElements(mode, _indexBuffer.Count, _indexBuffer.Type, first);
            }
        }

        public bool TryGetVbo(long vboKey, out int vboHandle)
        {
            return _vboCache.TryGetValue(vboKey, out vboHandle);
        }
    }
}