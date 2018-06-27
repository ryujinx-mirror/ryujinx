using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    public class OGLRasterizer : IGalRasterizer
    {
        private static Dictionary<GalVertexAttribSize, int> AttribElements =
                   new Dictionary<GalVertexAttribSize, int>()
        {
            { GalVertexAttribSize._32_32_32_32, 4 },
            { GalVertexAttribSize._32_32_32,    3 },
            { GalVertexAttribSize._16_16_16_16, 4 },
            { GalVertexAttribSize._32_32,       2 },
            { GalVertexAttribSize._16_16_16,    3 },
            { GalVertexAttribSize._8_8_8_8,     4 },
            { GalVertexAttribSize._16_16,       2 },
            { GalVertexAttribSize._32,          1 },
            { GalVertexAttribSize._8_8_8,       3 },
            { GalVertexAttribSize._8_8,         2 },
            { GalVertexAttribSize._16,          1 },
            { GalVertexAttribSize._8,           1 },
            { GalVertexAttribSize._10_10_10_2,  4 },
            { GalVertexAttribSize._11_11_10,    3 }
        };

        private static Dictionary<GalVertexAttribSize, VertexAttribPointerType> AttribTypes =
                   new Dictionary<GalVertexAttribSize, VertexAttribPointerType>()
        {
            { GalVertexAttribSize._32_32_32_32, VertexAttribPointerType.Int   },
            { GalVertexAttribSize._32_32_32,    VertexAttribPointerType.Int   },
            { GalVertexAttribSize._16_16_16_16, VertexAttribPointerType.Short },
            { GalVertexAttribSize._32_32,       VertexAttribPointerType.Int   },
            { GalVertexAttribSize._16_16_16,    VertexAttribPointerType.Short },
            { GalVertexAttribSize._8_8_8_8,     VertexAttribPointerType.Byte  },
            { GalVertexAttribSize._16_16,       VertexAttribPointerType.Short },
            { GalVertexAttribSize._32,          VertexAttribPointerType.Int   },
            { GalVertexAttribSize._8_8_8,       VertexAttribPointerType.Byte  },
            { GalVertexAttribSize._8_8,         VertexAttribPointerType.Byte  },
            { GalVertexAttribSize._16,          VertexAttribPointerType.Short },
            { GalVertexAttribSize._8,           VertexAttribPointerType.Byte  },
            { GalVertexAttribSize._10_10_10_2,  VertexAttribPointerType.Int   }, //?
            { GalVertexAttribSize._11_11_10,    VertexAttribPointerType.Int   }  //?
        };

        private int VaoHandle;

        private int[] VertexBuffers;

        private OGLCachedResource<int> VboCache;
        private OGLCachedResource<int> IboCache;

        private struct IbInfo
        {
            public int Count;

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

        public void ClearBuffers(int RtIndex, GalClearBufferFlags Flags)
        {
            ClearBufferMask Mask = 0;

            //TODO: Use glColorMask to clear just the specified channels.
            if (Flags.HasFlag(GalClearBufferFlags.ColorRed)   &&
                Flags.HasFlag(GalClearBufferFlags.ColorGreen) &&
                Flags.HasFlag(GalClearBufferFlags.ColorBlue)  &&
                Flags.HasFlag(GalClearBufferFlags.ColorAlpha))
            {
                Mask = ClearBufferMask.ColorBufferBit;
            }

            if (Flags.HasFlag(GalClearBufferFlags.Depth))
            {
                Mask |= ClearBufferMask.DepthBufferBit;
            }

            if (Flags.HasFlag(GalClearBufferFlags.Stencil))
            {
                Mask |= ClearBufferMask.StencilBufferBit;
            }

            GL.Clear(Mask);
        }

        public bool IsVboCached(long Key, long DataSize)
        {
            return VboCache.TryGetSize(Key, out long Size) && Size == DataSize;
        }

        public bool IsIboCached(long Key, long DataSize)
        {
            return IboCache.TryGetSize(Key, out long Size) && Size == DataSize;
        }

        public void EnableCullFace()
        {
            GL.Enable(EnableCap.CullFace);
        }

        public void DisableCullFace()
        {
            GL.Disable(EnableCap.CullFace);
        }

        public void EnableDepthTest()
        {
            GL.Enable(EnableCap.DepthTest);
        }

        public void DisableDepthTest()
        {
            GL.Disable(EnableCap.DepthTest);
        }

        public void SetDepthFunction(GalComparisonOp Func)
        {
            GL.DepthFunc(OGLEnumConverter.GetDepthFunc(Func));
        }

        public void CreateVbo(long Key, byte[] Buffer)
        {
            int Handle = GL.GenBuffer();

            VboCache.AddOrUpdate(Key, Handle, (uint)Buffer.Length);

            IntPtr Length = new IntPtr(Buffer.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Handle);
            GL.BufferData(BufferTarget.ArrayBuffer, Length, Buffer, BufferUsageHint.StreamDraw);
        }

        public void CreateIbo(long Key, byte[] Buffer)
        {
            int Handle = GL.GenBuffer();

            IboCache.AddOrUpdate(Key, Handle, (uint)Buffer.Length);

            IntPtr Length = new IntPtr(Buffer.Length);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Length, Buffer, BufferUsageHint.StreamDraw);
        }

        public void SetVertexArray(int VbIndex, int Stride, long VboKey, GalVertexAttrib[] Attribs)
        {
            if (!VboCache.TryGetValue(VboKey, out int VboHandle))
            {
                return;
            }

            if (VaoHandle == 0)
            {
                VaoHandle = GL.GenVertexArray();
            }

            GL.BindVertexArray(VaoHandle);

            foreach (GalVertexAttrib Attrib in Attribs)
            {
                GL.EnableVertexAttribArray(Attrib.Index);

                GL.BindBuffer(BufferTarget.ArrayBuffer, VboHandle);

                bool Unsigned =
                    Attrib.Type == GalVertexAttribType.Unorm ||
                    Attrib.Type == GalVertexAttribType.Uint  ||
                    Attrib.Type == GalVertexAttribType.Uscaled;

                bool Normalize =
                    Attrib.Type == GalVertexAttribType.Snorm ||
                    Attrib.Type == GalVertexAttribType.Unorm;

                VertexAttribPointerType Type = 0;

                if (Attrib.Type == GalVertexAttribType.Float)
                {
                    Type = VertexAttribPointerType.Float;
                }
                else
                {
                    Type = AttribTypes[Attrib.Size] + (Unsigned ? 1 : 0);
                }

                int Size   = AttribElements[Attrib.Size];
                int Offset = Attrib.Offset;

                GL.VertexAttribPointer(Attrib.Index, Size, Type, Normalize, Stride, Offset);
            }
        }

        public void SetIndexArray(long Key, int Size, GalIndexFormat Format)
        {
            IndexBuffer.Type = OGLEnumConverter.GetDrawElementsType(Format);

            IndexBuffer.Count = Size >> (int)Format;
        }

        public void DrawArrays(int First, int PrimCount, GalPrimitiveType PrimType)
        {
            if (PrimCount == 0)
            {
                return;
            }

            GL.BindVertexArray(VaoHandle);

            GL.DrawArrays(OGLEnumConverter.GetPrimitiveType(PrimType), First, PrimCount);
        }

        public void DrawElements(long IboKey, int First, GalPrimitiveType PrimType)
        {
            if (!IboCache.TryGetValue(IboKey, out int IboHandle))
            {
                return;
            }

            PrimitiveType Mode = OGLEnumConverter.GetPrimitiveType(PrimType);

            GL.BindVertexArray(VaoHandle);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IboHandle);

            GL.DrawElements(Mode, IndexBuffer.Count, IndexBuffer.Type, First);
        }
    }
}