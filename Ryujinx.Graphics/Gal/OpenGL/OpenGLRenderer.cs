using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    public class OpenGLRenderer : IGalRenderer
    {
        private struct VertexBuffer
        {
            public int VaoHandle;
            public int VboHandle;

            public int PrimCount;
        }

        private struct Texture
        {
            public int Handle;
        }

        private List<VertexBuffer> VertexBuffers;

        private Texture[] Textures;

        private ConcurrentQueue<Action> ActionsQueue;

        private FrameBuffer FbRenderer;

        public OpenGLRenderer()
        {
            VertexBuffers = new List<VertexBuffer>();

            Textures = new Texture[8];

            ActionsQueue = new ConcurrentQueue<Action>();
        }

        public void InitializeFrameBuffer()
        {
            FbRenderer = new FrameBuffer(1280, 720);
        }

        public void QueueAction(Action ActionMthd)
        {
            ActionsQueue.Enqueue(ActionMthd);
        }

        public void RunActions()
        {
            int Count = ActionsQueue.Count;

            while (Count-- > 0 && ActionsQueue.TryDequeue(out Action RenderAction))
            {
                RenderAction();
            }
        }

        public void Render()
        {
            FbRenderer.Render();

            for (int Index = 0; Index < VertexBuffers.Count; Index++)
            {
                VertexBuffer Vb = VertexBuffers[Index];

                if (Vb.VaoHandle != 0 &&
                    Vb.PrimCount != 0)
                {
                    GL.BindVertexArray(Vb.VaoHandle);
                    GL.DrawArrays(PrimitiveType.TriangleStrip, 0, Vb.PrimCount);
                }
            }
        }

        public void SetWindowSize(int Width, int Height)
        {
            FbRenderer.WindowWidth  = Width;
            FbRenderer.WindowHeight = Height;
        }

        public unsafe void SetFrameBuffer(
            byte* Fb,
            int   Width,
            int   Height,
            float ScaleX,
            float ScaleY,
            float OffsX,
            float OffsY,
            float Rotate)
        {
            Matrix2 Transform;

            Transform  = Matrix2.CreateScale(ScaleX, ScaleY);
            Transform *= Matrix2.CreateRotation(Rotate);

            Vector2 Offs = new Vector2(OffsX, OffsY);

            FbRenderer.Set(Fb, Width, Height, Transform, Offs);
        }

        public void SendVertexBuffer(int Index, byte[] Buffer, int Stride, GalVertexAttrib[] Attribs)
        {
            if (Index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Index));
            }

            if (Buffer.Length == 0 || Stride == 0)
            {
                return;
            }

            EnsureVbInitialized(Index);

            VertexBuffer Vb = VertexBuffers[Index];

            Vb.PrimCount = Buffer.Length / Stride;

            VertexBuffers[Index] = Vb;

            IntPtr Length = new IntPtr(Buffer.Length);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Vb.VboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, Length, Buffer, BufferUsageHint.StreamDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(Vb.VaoHandle);

            for (int Attr = 0; Attr < 16; Attr++)
            {
                GL.DisableVertexAttribArray(Attr);
            }

            foreach (GalVertexAttrib Attrib in Attribs)
            {
                if (Attrib.Index >= 3) break;

                GL.EnableVertexAttribArray(Attrib.Index);

                GL.BindBuffer(BufferTarget.ArrayBuffer, Vb.VboHandle);

                int Size = 0;

                switch (Attrib.Size)
                {
                    case GalVertexAttribSize._8:
                    case GalVertexAttribSize._16:
                    case GalVertexAttribSize._32:
                        Size = 1;
                        break;
                    case GalVertexAttribSize._8_8:
                    case GalVertexAttribSize._16_16:
                    case GalVertexAttribSize._32_32:
                        Size = 2;
                        break;
                    case GalVertexAttribSize._8_8_8:
                    case GalVertexAttribSize._11_11_10:
                    case GalVertexAttribSize._16_16_16:
                    case GalVertexAttribSize._32_32_32:
                        Size = 3;
                        break;
                    case GalVertexAttribSize._8_8_8_8:
                    case GalVertexAttribSize._10_10_10_2:
                    case GalVertexAttribSize._16_16_16_16:
                    case GalVertexAttribSize._32_32_32_32:
                        Size = 4;
                        break;
                }

                bool Signed =
                    Attrib.Type == GalVertexAttribType.Snorm ||
                    Attrib.Type == GalVertexAttribType.Sint  ||
                    Attrib.Type == GalVertexAttribType.Sscaled;

                bool Normalize =
                    Attrib.Type == GalVertexAttribType.Snorm ||
                    Attrib.Type == GalVertexAttribType.Unorm;

                VertexAttribPointerType Type = 0;

                switch (Attrib.Type)
                {
                    case GalVertexAttribType.Snorm:
                    case GalVertexAttribType.Unorm:
                    case GalVertexAttribType.Sint:
                    case GalVertexAttribType.Uint:
                    case GalVertexAttribType.Uscaled:
                    case GalVertexAttribType.Sscaled:                    
                    {
                        switch (Attrib.Size)
                        {
                            case GalVertexAttribSize._8:
                            case GalVertexAttribSize._8_8:
                            case GalVertexAttribSize._8_8_8:
                            case GalVertexAttribSize._8_8_8_8:
                            {
                                Type = Signed
                                    ? VertexAttribPointerType.Byte
                                    : VertexAttribPointerType.UnsignedByte;

                                break;
                            }

                            case GalVertexAttribSize._16:
                            case GalVertexAttribSize._16_16:
                            case GalVertexAttribSize._16_16_16:
                            case GalVertexAttribSize._16_16_16_16:
                            {
                                Type = Signed
                                    ? VertexAttribPointerType.Short
                                    : VertexAttribPointerType.UnsignedShort;

                                break;
                            }

                            case GalVertexAttribSize._10_10_10_2:
                            case GalVertexAttribSize._11_11_10:
                            case GalVertexAttribSize._32:
                            case GalVertexAttribSize._32_32:
                            case GalVertexAttribSize._32_32_32:
                            case GalVertexAttribSize._32_32_32_32:
                            {
                                Type = Signed
                                    ? VertexAttribPointerType.Int
                                    : VertexAttribPointerType.UnsignedInt;

                                break;
                            }
                        }

                        break;
                    }

                    case GalVertexAttribType.Float:
                    {
                        Type = VertexAttribPointerType.Float;

                        break;
                    }
                }

                GL.VertexAttribPointer(
                    Attrib.Index,
                    Size,
                    Type,
                    Normalize,
                    Stride,
                    Attrib.Offset);
            }

            GL.BindVertexArray(0);
        }

        public void SendR8G8B8A8Texture(int Index, byte[] Buffer, int Width, int Height)
        {
            EnsureTexInitialized(Index);

            GL.BindTexture(TextureTarget.Texture2D, Textures[Index].Handle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                Width,
                Height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                Buffer);
        }

        public void BindTexture(int Index)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + Index);

            GL.BindTexture(TextureTarget.Texture2D, Textures[Index].Handle);            
        }

        private void EnsureVbInitialized(int VbIndex)
        {
            while (VbIndex >= VertexBuffers.Count)
            {
                VertexBuffers.Add(new VertexBuffer());
            }

            VertexBuffer Vb = VertexBuffers[VbIndex];

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

        private void EnsureTexInitialized(int TexIndex)
        {
            Texture Tex = Textures[TexIndex];

            if (Tex.Handle == 0)
            {
                Tex.Handle = GL.GenTexture();
            }

            Textures[TexIndex] = Tex;
        }
    }
}