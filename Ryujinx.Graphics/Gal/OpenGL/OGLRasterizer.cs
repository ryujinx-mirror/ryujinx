using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OGLRasterizer
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

        private struct VbInfo
        {
            public int VaoHandle;
            public int VboHandle;

            public int PrimCount;
        }

        private struct IbInfo
        {
            public int IboHandle;
            public int Count;

            public DrawElementsType Type;
        }

        private VbInfo[] VertexBuffers;

        private IbInfo IndexBuffer;

        public OGLRasterizer()
        {
            VertexBuffers = new VbInfo[32];

            IndexBuffer = new IbInfo();
        }

        public void ClearBuffers(int RtIndex, GalClearBufferFlags Flags)
        {
            ClearBufferMask Mask = 0;

            //OpenGL doesn't support clearing just a single color channel,
            //so we can't just clear all channels...
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

        public void SetVertexArray(int VbIndex, int Stride, byte[] Buffer, GalVertexAttrib[] Attribs)
        {
            EnsureVbInitialized(VbIndex);

            VertexBuffers[VbIndex].PrimCount = Buffer.Length / Stride;

            VbInfo Vb = VertexBuffers[VbIndex];

            IntPtr Length = new IntPtr(Buffer.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Vb.VboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, Length, Buffer, BufferUsageHint.StreamDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(Vb.VaoHandle);

            for (int Attr = 0; Attr < 16; Attr++)
            {
                GL.DisableVertexAttribArray(Attr);
            }

            for (int Index = 0; Index < Attribs.Length; Index++)
            {
                GalVertexAttrib Attrib = Attribs[Index];

                GL.EnableVertexAttribArray(Index);

                GL.BindBuffer(BufferTarget.ArrayBuffer, Vb.VboHandle);

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

                GL.VertexAttribPointer(Index, Size, Type, Normalize, Stride, Offset);
            }

            GL.BindVertexArray(0);
        }

        public void SetIndexArray(byte[] Buffer, GalIndexFormat Format)
        {
            EnsureIbInitialized();

            IndexBuffer.Type = OGLEnumConverter.GetDrawElementsType(Format);

            IndexBuffer.Count = Buffer.Length >> (int)Format;

            IntPtr Length = new IntPtr(Buffer.Length);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer.IboHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Length, Buffer, BufferUsageHint.StreamDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void DrawArrays(int VbIndex, GalPrimitiveType PrimType)
        {
            VbInfo Vb = VertexBuffers[VbIndex];

            if (Vb.PrimCount == 0)
            {
                return;
            }

            GL.BindVertexArray(Vb.VaoHandle);

            GL.DrawArrays(OGLEnumConverter.GetPrimitiveType(PrimType), 0, Vb.PrimCount);
        }

        public void DrawElements(int VbIndex, int First, GalPrimitiveType PrimType)
        {
            VbInfo Vb = VertexBuffers[VbIndex];

            if (Vb.PrimCount == 0)
            {
                return;
            }

            PrimitiveType Mode = OGLEnumConverter.GetPrimitiveType(PrimType);

            GL.BindVertexArray(Vb.VaoHandle);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndexBuffer.IboHandle);

            GL.DrawElements(Mode, IndexBuffer.Count, IndexBuffer.Type, First);
        }

        private void EnsureVbInitialized(int VbIndex)
        {
            VbInfo Vb = VertexBuffers[VbIndex];

            if (Vb.VaoHandle == 0)
            {
                Vb.VaoHandle = GL.GenVertexArray();
            }

            if (Vb.VboHandle == 0)
            {
                Vb.VboHandle = GL.GenBuffer();
            }

            VertexBuffers[VbIndex] = Vb;
        }

        private void EnsureIbInitialized()
        {
            if (IndexBuffer.IboHandle == 0)
            {
                IndexBuffer.IboHandle = GL.GenBuffer();
            }
        }
    }
}