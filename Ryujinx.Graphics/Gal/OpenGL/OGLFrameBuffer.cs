using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OGLFrameBuffer
    {
        private struct FrameBuffer
        {
            public int FbHandle;
            public int RbHandle;
            public int TexHandle;
        }

        private struct ShaderProgram
        {
            public int Handle;
            public int VpHandle;
            public int FpHandle;
        }

        private FrameBuffer[] Fbs;

        private ShaderProgram Shader;

        private bool IsInitialized;

        private int VaoHandle;
        private int VboHandle;

        public OGLFrameBuffer()
        {
            Fbs = new FrameBuffer[16];

            Shader = new ShaderProgram();
        }

        public void Set(int Index, int Width, int Height)
        {
            if (Fbs[Index].FbHandle != 0)
            {
                return;
            }

            Fbs[Index].FbHandle  = GL.GenFramebuffer();
            Fbs[Index].RbHandle  = GL.GenRenderbuffer();
            Fbs[Index].TexHandle = GL.GenTexture();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Fbs[Index].FbHandle);

            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, Fbs[Index].RbHandle);

            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, 1280, 720);

            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, Fbs[Index].RbHandle);

            GL.BindTexture(TextureTarget.Texture2D, Fbs[Index].TexHandle);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 1280, 720, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, Fbs[Index].TexHandle, 0);

            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
        }

        public void Bind(int Index)
        {
            if (Fbs[Index].FbHandle == 0)
            {
                return;
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Fbs[Index].FbHandle);
        }

        public void Draw(int Index)
        {
            if (Fbs[Index].FbHandle == 0)
            {
                return;
            }

            EnsureInitialized();

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            GL.BindTexture(TextureTarget.Texture2D, Fbs[Index].TexHandle);

            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindVertexArray(VaoHandle);

            GL.UseProgram(Shader.Handle);

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
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

            Matrix2 Transform = Matrix2.CreateScale(1, -1);

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
    }
}