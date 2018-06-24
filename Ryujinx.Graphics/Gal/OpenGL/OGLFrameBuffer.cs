using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    public class OGLFrameBuffer : IGalFrameBuffer
    {
        private struct Rect
        {
            public int X      { get; private set; }
            public int Y      { get; private set; }
            public int Width  { get; private set; }
            public int Height { get; private set; }

            public Rect(int X, int Y, int Width, int Height)
            {
                this.X      = X;
                this.Y      = Y;
                this.Width  = Width;
                this.Height = Height;
            }
        }

        private class FrameBuffer
        {
            public int Width  { get; set; }
            public int Height { get; set; }

            public int Handle    { get; private set; }
            public int RbHandle  { get; private set; }
            public int TexHandle { get; private set; }

            public FrameBuffer(int Width, int Height)
            {
                this.Width  = Width;
                this.Height = Height;

                Handle    = GL.GenFramebuffer();
                RbHandle  = GL.GenRenderbuffer();
                TexHandle = GL.GenTexture();
            }
        }

        private struct ShaderProgram
        {
            public int Handle;
            public int VpHandle;
            public int FpHandle;
        }

        private Dictionary<long, FrameBuffer> Fbs;

        private ShaderProgram Shader;

        private Rect Viewport;
        private Rect Window;

        private bool IsInitialized;

        private int RawFbTexWidth;
        private int RawFbTexHeight;
        private int RawFbTexHandle;

        private int CurrFbHandle;
        private int CurrTexHandle;

        private int VaoHandle;
        private int VboHandle;

        public OGLFrameBuffer()
        {
            Fbs = new Dictionary<long, FrameBuffer>();

            Shader = new ShaderProgram();
        }

        public void Create(long Key, int Width, int Height)
        {
            //TODO: We should either use the original frame buffer size,
            //or just remove the Width/Height arguments.
            Width  = Window.Width;
            Height = Window.Height;

            if (Fbs.TryGetValue(Key, out FrameBuffer Fb))
            {
                if (Fb.Width  != Width ||
                    Fb.Height != Height)
                {
                    SetupTexture(Fb.TexHandle, Width, Height);

                    Fb.Width  = Width;
                    Fb.Height = Height;
                }

                return;
            }

            Fb = new FrameBuffer(Width, Height);

            SetupTexture(Fb.TexHandle, Width, Height);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Fb.Handle);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Fb.RbHandle);

            GL.RenderbufferStorage(
                RenderbufferTarget.Renderbuffer,
                RenderbufferStorage.Depth24Stencil8,
                Width,
                Height);

            GL.FramebufferRenderbuffer(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.DepthStencilAttachment,
                RenderbufferTarget.Renderbuffer,
                Fb.RbHandle);

            GL.FramebufferTexture(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                Fb.TexHandle,
                0);

            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            GL.Viewport(0, 0, Width, Height);

            Fbs.Add(Key, Fb);
        }

        public void Bind(long Key)
        {
            if (Fbs.TryGetValue(Key, out FrameBuffer Fb))
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, Fb.Handle);

                CurrFbHandle = Fb.Handle;
            }
        }

        public void BindTexture(long Key, int Index)
        {
            if (Fbs.TryGetValue(Key, out FrameBuffer Fb))
            {
                GL.ActiveTexture(TextureUnit.Texture0 + Index);

                GL.BindTexture(TextureTarget.Texture2D, Fb.TexHandle);
            }
        }

        public void Set(long Key)
        {
            if (Fbs.TryGetValue(Key, out FrameBuffer Fb))
            {
                CurrTexHandle = Fb.TexHandle;
            }
        }

        public void Set(byte[] Data, int Width, int Height)
        {
            if (RawFbTexHandle == 0)
            {
                RawFbTexHandle = GL.GenTexture();
            }

            if (RawFbTexWidth  != Width ||
                RawFbTexHeight != Height)
            {
                SetupTexture(RawFbTexHandle, Width, Height);

                RawFbTexWidth  = Width;
                RawFbTexHeight = Height;
            }

            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindTexture(TextureTarget.Texture2D, RawFbTexHandle);

            (PixelFormat Format, PixelType Type) = OGLEnumConverter.GetTextureFormat(GalTextureFormat.A8B8G8R8);

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, Width, Height, Format, Type, Data);

            CurrTexHandle = RawFbTexHandle;
        }

        public void SetTransform(float SX, float SY, float Rotate, float TX, float TY)
        {
            EnsureInitialized();

            Matrix2 Transform;

            Transform  = Matrix2.CreateScale(SX, SY);
            Transform *= Matrix2.CreateRotation(Rotate);

            Vector2 Offs = new Vector2(TX, TY);

            int CurrentProgram = GL.GetInteger(GetPName.CurrentProgram);

            GL.UseProgram(Shader.Handle);

            int TransformUniformLocation = GL.GetUniformLocation(Shader.Handle, "transform");

            GL.UniformMatrix2(TransformUniformLocation, false, ref Transform);

            int OffsetUniformLocation = GL.GetUniformLocation(Shader.Handle, "offset");

            GL.Uniform2(OffsetUniformLocation, ref Offs);

            GL.UseProgram(CurrentProgram);
        }

        public void SetWindowSize(int Width, int Height)
        {
            int CurrentProgram = GL.GetInteger(GetPName.CurrentProgram);

            GL.UseProgram(Shader.Handle);

            int WindowSizeUniformLocation = GL.GetUniformLocation(Shader.Handle, "window_size");

            GL.Uniform2(WindowSizeUniformLocation, new Vector2(Width, Height));

            GL.UseProgram(CurrentProgram);

            Window = new Rect(0, 0, Width, Height);
        }

        public void SetViewport(int X, int Y, int Width, int Height)
        {
            Viewport = new Rect(X, Y, Width, Height);

            //TODO
        }

        public void Render()
        {
            if (CurrTexHandle != 0)
            {
                EnsureInitialized();

                bool AlphaBlendEnable = GL.GetInteger(GetPName.Blend) != 0;

                GL.Disable(EnableCap.Blend);

                GL.ActiveTexture(TextureUnit.Texture0);

                GL.BindTexture(TextureTarget.Texture2D, CurrTexHandle);

                int CurrentProgram = GL.GetInteger(GetPName.CurrentProgram);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                SetViewport(Window);

                GL.Clear(
                    ClearBufferMask.ColorBufferBit |
                    ClearBufferMask.DepthBufferBit);

                GL.BindVertexArray(VaoHandle);

                GL.UseProgram(Shader.Handle);

                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);

                //Restore the original state.
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, CurrFbHandle);

                GL.UseProgram(CurrentProgram);

                if (AlphaBlendEnable)
                {
                    GL.Enable(EnableCap.Blend);
                }

                //GL.Viewport(0, 0, 1280, 720);
            }
        }

        public void GetBufferData(long Key, Action<byte[]> Callback)
        {
            if (Fbs.TryGetValue(Key, out FrameBuffer Fb))
            {
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, Fb.Handle);

                byte[] Data = new byte[Fb.Width * Fb.Height * 4];

                (PixelFormat Format, PixelType Type) = OGLEnumConverter.GetTextureFormat(GalTextureFormat.A8B8G8R8);

                GL.ReadPixels(
                    0,
                    0,
                    Fb.Width,
                    Fb.Height,
                    Format,
                    Type,
                    Data);

                Callback(Data);

                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, CurrFbHandle);
            }
        }

        private void SetViewport(Rect Viewport)
        {
            GL.Viewport(
                Viewport.X,
                Viewport.Y,
                Viewport.Width,
                Viewport.Height);
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                IsInitialized = true;

                SetupShader();
                SetupVertex();
            }
        }

        private void SetupShader()
        {
            Shader.VpHandle = GL.CreateShader(ShaderType.VertexShader);
            Shader.FpHandle = GL.CreateShader(ShaderType.FragmentShader);

            string VpSource = EmbeddedResource.GetString("GlFbVtxShader");
            string FpSource = EmbeddedResource.GetString("GlFbFragShader");

            GL.ShaderSource(Shader.VpHandle, VpSource);
            GL.ShaderSource(Shader.FpHandle, FpSource);
            GL.CompileShader(Shader.VpHandle);
            GL.CompileShader(Shader.FpHandle);

            Shader.Handle = GL.CreateProgram();

            GL.AttachShader(Shader.Handle, Shader.VpHandle);
            GL.AttachShader(Shader.Handle, Shader.FpHandle);
            GL.LinkProgram(Shader.Handle);
            GL.UseProgram(Shader.Handle);

            Matrix2 Transform = Matrix2.Identity;

            int TexUniformLocation = GL.GetUniformLocation(Shader.Handle, "tex");

            GL.Uniform1(TexUniformLocation, 0);

            int WindowSizeUniformLocation = GL.GetUniformLocation(Shader.Handle, "window_size");

            GL.Uniform2(WindowSizeUniformLocation, new Vector2(1280.0f, 720.0f));

            int TransformUniformLocation = GL.GetUniformLocation(Shader.Handle, "transform");

            GL.UniformMatrix2(TransformUniformLocation, false, ref Transform);
        }

        private void SetupVertex()
        {
            VaoHandle = GL.GenVertexArray();
            VboHandle = GL.GenBuffer();

            float[] Buffer = new float[]
            {
                -1,  1,  0,  0,
                 1,  1,  1,  0,
                -1, -1,  0,  1,
                 1, -1,  1,  1
            };

            IntPtr Length = new IntPtr(Buffer.Length * 4);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, Length, Buffer, BufferUsageHint.StreamDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(VaoHandle);

            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VboHandle);

            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 16, 0);

            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VboHandle);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 16, 8);
        }

        private void SetupTexture(int Handle, int Width, int Height)
        {
            GL.BindTexture(TextureTarget.Texture2D, Handle);

            const int MinFilter = (int)TextureMinFilter.Linear;
            const int MagFilter = (int)TextureMagFilter.Linear;

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, MinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, MagFilter);

            (PixelFormat Format, PixelType Type) = OGLEnumConverter.GetTextureFormat(GalTextureFormat.A8B8G8R8);

            const PixelInternalFormat InternalFmt = PixelInternalFormat.Rgba;

            const int Level  = 0;
            const int Border = 0;

            GL.TexImage2D(
                TextureTarget.Texture2D,
                Level,
                InternalFmt,
                Width,
                Height,
                Border,
                Format,
                Type,
                IntPtr.Zero);
        }
    }
}