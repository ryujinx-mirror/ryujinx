using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    unsafe class FrameBuffer
    {
        public int WindowWidth  { get; set; }
        public int WindowHeight { get; set; }

        private int VtxShaderHandle;
        private int FragShaderHandle;
        private int PrgShaderHandle;

        private int TexHandle;
        private int TexWidth;
        private int TexHeight;

        private int VaoHandle;
        private int VboHandle;

        private int[] Pixels;

        private byte* FbPtr;

        public FrameBuffer(int Width, int Height)
        {
            if (Width < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Width));
            }

            if (Height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Height));
            }

            TexWidth  = Width;
            TexHeight = Height;

            WindowWidth  = Width;
            WindowHeight = Height;

            SetupShaders();
            SetupTexture();
            SetupVertex();
        }

        private void SetupShaders()
        {
            VtxShaderHandle  = GL.CreateShader(ShaderType.VertexShader);
            FragShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

            string VtxShaderSource  = EmbeddedResource.GetString("GlFbVtxShader");
            string FragShaderSource = EmbeddedResource.GetString("GlFbFragShader");

            GL.ShaderSource(VtxShaderHandle, VtxShaderSource);
            GL.ShaderSource(FragShaderHandle, FragShaderSource);
            GL.CompileShader(VtxShaderHandle);
            GL.CompileShader(FragShaderHandle);

            PrgShaderHandle = GL.CreateProgram();

            GL.AttachShader(PrgShaderHandle, VtxShaderHandle);
            GL.AttachShader(PrgShaderHandle, FragShaderHandle);
            GL.LinkProgram(PrgShaderHandle);
            GL.UseProgram(PrgShaderHandle);

            int TexUniformLocation = GL.GetUniformLocation(PrgShaderHandle, "tex");

            GL.Uniform1(TexUniformLocation, 0);

            int WindowSizeUniformLocation = GL.GetUniformLocation(PrgShaderHandle, "window_size");

            GL.Uniform2(WindowSizeUniformLocation, new Vector2(1280.0f, 720.0f));
        }

        private void SetupTexture()
        {
            Pixels = new int[TexWidth * TexHeight];

            if (TexHandle == 0)
            {
                TexHandle = GL.GenTexture();
            }

            GL.BindTexture(TextureTarget.Texture2D, TexHandle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Rgba,
                TexWidth,
                TexHeight,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                IntPtr.Zero);
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

            GL.BindVertexArray(0);
        }

        public unsafe void Set(byte* Fb, int Width, int Height, Matrix2 Transform)
        {
            if (Fb == null)
            {
                throw new ArgumentNullException(nameof(Fb));
            }

            if (Width < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Width));
            }

            if (Height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Height));
            }

            FbPtr = Fb;

            if (Width  != TexWidth ||
                Height != TexHeight)
            {
                TexWidth  = Width;
                TexHeight = Height;

                SetupTexture();
            }

            GL.UseProgram(PrgShaderHandle);

            int TransformUniformLocation = GL.GetUniformLocation(PrgShaderHandle, "transform");

            GL.UniformMatrix2(TransformUniformLocation, false, ref Transform);

            int WindowSizeUniformLocation = GL.GetUniformLocation(PrgShaderHandle, "window_size");

            GL.Uniform2(WindowSizeUniformLocation, new Vector2(WindowWidth, WindowHeight));
        }

        public void Render()
        {
            if (FbPtr == null)
            {
                return;
            }

            for (int Y = 0; Y < TexHeight; Y++)
            for (int X = 0; X < TexWidth;  X++)
            {
                Pixels[X + Y * TexWidth] = *((int*)(FbPtr + GetSwizzleOffset(X, Y)));
            }

            GL.BindTexture(TextureTarget.Texture2D, TexHandle);
            GL.TexSubImage2D(TextureTarget.Texture2D,
                0,
                0,
                0,
                TexWidth,
                TexHeight,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                Pixels);
            
            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindVertexArray(VaoHandle);

            GL.UseProgram(PrgShaderHandle);

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }

        private int GetSwizzleOffset(int X, int Y)
        {
            int Pos;

            Pos  = (Y & 0x7f) >> 4;
            Pos += (X >> 4) << 3;
            Pos += (Y >> 7) * ((TexWidth >> 4) << 3);
            Pos *= 1024;
            Pos += ((Y & 0xf) >> 3) << 9;
            Pos += ((X & 0xf) >> 3) << 8;
            Pos += ((Y & 0x7) >> 1) << 6;
            Pos += ((X & 0x7) >> 2) << 5;
            Pos += ((Y & 0x1) >> 0) << 4;
            Pos += ((X & 0x3) >> 0) << 2;

            return Pos;
        }
    }
}