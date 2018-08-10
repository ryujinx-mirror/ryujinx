using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    public class OGLRasterizer : IGalRasterizer
    {
        private int[] VertexBuffers;

        private OGLCachedResource<int> VboCache;
        private OGLCachedResource<int> IboCache;

        private struct IbInfo
        {
            public int Count;
            public int ElemSizeLog2;

            public DrawElementsType Type;
        }

        private IbInfo IndexBuffer;

        public OGLRasterizer()
        {
            VertexBuffers = new int[32];

            VboCache = new OGLCachedResource<int>(GL.DeleteBuffer);
            IboCache = new OGLCachedResource<int>(GL.DeleteBuffer);

            IndexBuffer = new IbInfo();
        }

        public void LockCaches()
        {
            VboCache.Lock();
            IboCache.Lock();
        }

        public void UnlockCaches()
        {
            VboCache.Unlock();
            IboCache.Unlock();
        }

        public void ClearBuffers(
            GalClearBufferFlags Flags,
            float Red, float Green, float Blue, float Alpha,
            float Depth,
            int Stencil)
        {
            ClearBufferMask Mask = ClearBufferMask.ColorBufferBit;

            GL.ColorMask(
                Flags.HasFlag(GalClearBufferFlags.ColorRed),
                Flags.HasFlag(GalClearBufferFlags.ColorGreen),
                Flags.HasFlag(GalClearBufferFlags.ColorBlue),
                Flags.HasFlag(GalClearBufferFlags.ColorAlpha));

            if (Flags.HasFlag(GalClearBufferFlags.Depth))
            {
                Mask |= ClearBufferMask.DepthBufferBit;
            }

            if (Flags.HasFlag(GalClearBufferFlags.Stencil))
            {
                Mask |= ClearBufferMask.StencilBufferBit;
            }

            GL.ClearColor(Red, Green, Blue, Alpha);

            GL.ClearDepth(Depth);

            GL.ClearStencil(Stencil);

            GL.Clear(Mask);

            GL.ColorMask(true, true, true, true);
        }

        public bool IsVboCached(long Key, long DataSize)
        {
            return VboCache.TryGetSize(Key, out long Size) && Size == DataSize;
        }

        public bool IsIboCached(long Key, long DataSize)
        {
            return IboCache.TryGetSize(Key, out long Size) && Size == DataSize;
        }

        public void CreateVbo(long Key, int DataSize, IntPtr HostAddress)
        {
            int Handle = GL.GenBuffer();

            VboCache.AddOrUpdate(Key, Handle, (uint)DataSize);

            IntPtr Length = new IntPtr(DataSize);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
            GL.BufferData(BufferTarget.ArrayBuffer, Length, HostAddress, BufferUsageHint.StreamDraw);
        }

        public void CreateIbo(long Key, int DataSize, IntPtr HostAddress)
        {
            int Handle = GL.GenBuffer();

            IboCache.AddOrUpdate(Key, Handle, (uint)DataSize);

            IntPtr Length = new IntPtr(DataSize);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Length, HostAddress, BufferUsageHint.StreamDraw);
        }

        public void SetIndexArray(int Size, GalIndexFormat Format)
        {
            IndexBuffer.Type = OGLEnumConverter.GetDrawElementsType(Format);

            IndexBuffer.Count = Size >> (int)Format;

            IndexBuffer.ElemSizeLog2 = (int)Format;
        }

        public void DrawArrays(int First, int PrimCount, GalPrimitiveType PrimType)
        {
            if (PrimCount == 0)
            {
                return;
            }

            GL.DrawArrays(OGLEnumConverter.GetPrimitiveType(PrimType), First, PrimCount);
        }

        public void DrawElements(long IboKey, int First, int VertexBase, GalPrimitiveType PrimType)
        {
            if (!IboCache.TryGetValue(IboKey, out int IboHandle))
            {
                return;
            }

            PrimitiveType Mode = OGLEnumConverter.GetPrimitiveType(PrimType);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IboHandle);

            First <<= IndexBuffer.ElemSizeLog2;

            if (VertexBase != 0)
            {
                IntPtr Indices = new IntPtr(First);

                GL.DrawElementsBaseVertex(Mode, IndexBuffer.Count, IndexBuffer.Type, Indices, VertexBase);
            }
            else
            {
                GL.DrawElements(Mode, IndexBuffer.Count, IndexBuffer.Type, First);
            }
        }

        public bool TryGetVbo(long VboKey, out int VboHandle)
        {
            return VboCache.TryGetValue(VboKey, out VboHandle);
        }
    }
}