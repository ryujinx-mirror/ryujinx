// This code was written for the OpenTK library and has been released
// to the Public Domain.
// It is provided "as is" without express or implied warranty of any kind.

using Gal;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Ryujinx.OsHle;
using System;

namespace Ryujinx
{
    public class GLScreen : GameWindow
    {
        class ScreenTexture : IDisposable
        {
            private Switch       Ns;
            private IGalRenderer Renderer;
            
            private int Width;
            private int Height;
            private int TexHandle;            

            private int[] Pixels;

            public ScreenTexture(Switch Ns, IGalRenderer Renderer, int Width, int Height)
            {
                this.Ns       = Ns;
                this.Renderer = Renderer;
                this.Width    = Width;
                this.Height   = Height;

                Pixels = new int[Width * Height];

                TexHandle = GL.GenTexture();

                GL.BindTexture(TextureTarget.Texture2D, TexHandle);
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
                    IntPtr.Zero);
            }

            public int Texture
            {
                get
                {
                    UploadBitmap();

                    return TexHandle;
                }
            }

            unsafe void UploadBitmap()
            {
                int FbSize = Width * Height * 4;

                if (Renderer.FrameBufferPtr == 0 || Renderer.FrameBufferPtr + FbSize > uint.MaxValue)
                {
                    return;
                }

                byte* SrcPtr = (byte*)Ns.Ram + (uint)Renderer.FrameBufferPtr;

                for (int Y = 0; Y < Height; Y++)
                {
                    for (int X = 0; X < Width; X++)
                    {
                        int SrcOffs = GetSwizzleOffset(X, Y, 4);

                        Pixels[X + Y * Width] = *((int*)(SrcPtr + SrcOffs));
                    }
                }

                GL.BindTexture(TextureTarget.Texture2D, TexHandle);
                GL.TexSubImage2D(TextureTarget.Texture2D,
                    0,
                    0,
                    0,
                    Width,
                    Height,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    Pixels);
            }

            private int GetSwizzleOffset(int X, int Y, int Bpp)
            {
                int Pos;

                Pos  = (Y & 0x7f) >> 4;
                Pos += (X >> 4) << 3;
                Pos += (Y >> 7) * ((Width >> 4) << 3);
                Pos *= 1024;
                Pos += ((Y & 0xf) >> 3) << 9;
                Pos += ((X & 0xf) >> 3) << 8;
                Pos += ((Y & 0x7) >> 1) << 6;
                Pos += ((X & 0x7) >> 2) << 5;
                Pos += ((Y & 0x1) >> 0) << 4;
                Pos += ((X & 0x3) >> 0) << 2;

                return Pos;
            }

            private bool disposed;

            public void Dispose()
            {
                Dispose(true);
                
                GC.SuppressFinalize(this);
            }

            void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        GL.DeleteTexture(TexHandle);
                    }

                    disposed = true;
                }
            }
        }

        private string VtxShaderSource = @"
#version 330 core

precision highp float;

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec4 in_color;
layout(location = 2) in vec2 in_tex_coord;

out vec4 color;
out vec2 tex_coord;

void main(void) { 
    color = in_color;
    tex_coord = in_tex_coord;
    gl_Position = vec4((in_position + vec3(-960, 270, 0)) / vec3(1920, 270, 1), 1);
}";

        private string FragShaderSource = @"
#version 330 core

precision highp float;

uniform sampler2D tex;

in vec4 color;
in vec2 tex_coord;
out vec4 out_frag_color;

void main(void) {
    out_frag_color = vec4(texture(tex, tex_coord).rgb, color.a);
}";

        private int VtxShaderHandle,
                    FragShaderHandle,
                    PrgShaderHandle;
        
        private int VaoHandle;
        private int VboHandle;

        private Switch Ns;

        private IGalRenderer Renderer;

        private ScreenTexture ScreenTex;

        public GLScreen(Switch Ns, IGalRenderer Renderer)
            : base(1280, 720,
            new GraphicsMode(), "Ryujinx", 0,
            DisplayDevice.Default, 3, 3,
            GraphicsContextFlags.ForwardCompatible)
        {
            this.Ns       = Ns;
            this.Renderer = Renderer;

            ScreenTex = new ScreenTexture(Ns, Renderer, 1280, 720);
        }

        protected override void OnLoad(EventArgs e)
        {
            VSync = VSyncMode.On;

            CreateShaders();
            CreateVbo();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        protected override void OnUnload(EventArgs e)
        {
            ScreenTex.Dispose();

            GL.DeleteVertexArray(VaoHandle);
            GL.DeleteBuffer(VboHandle);
        }

        private void CreateVbo()
        {
            VaoHandle = GL.GenVertexArray();
            VboHandle = GL.GenBuffer();

            uint[] Buffer = new uint[]
            {
                0xc4700000, 0x80000000, 0x00000000, 0xffffffff, 0x00000000, 0x00000000, 0x00000000,
                0x45340000, 0x80000000, 0x00000000, 0xffffffff, 0x00000000, 0x3f800000, 0x00000000,
                0xc4700000, 0xc4070000, 0x00000000, 0xffffffff, 0x00000000, 0x00000000, 0x3f800000,
                0x45340000, 0xc4070000, 0x00000000, 0xffffffff, 0x00000000, 0x3f800000, 0x3f800000
            };

            IntPtr Length = new IntPtr(Buffer.Length * 4);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, Length, Buffer, BufferUsageHint.StreamDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.BindVertexArray(VaoHandle);

            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VboHandle);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 28, 0);

            GL.EnableVertexAttribArray(1);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VboHandle);

            GL.VertexAttribPointer(1, 4, VertexAttribPointerType.UnsignedByte, false, 28, 12);

            GL.EnableVertexAttribArray(2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VboHandle);

            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 28, 20);

            GL.BindVertexArray(0);
        }

        private void CreateShaders()
        {
            VtxShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            FragShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(VtxShaderHandle, VtxShaderSource);
            GL.ShaderSource(FragShaderHandle, FragShaderSource);
            GL.CompileShader(VtxShaderHandle);
            GL.CompileShader(FragShaderHandle);

            PrgShaderHandle = GL.CreateProgram();

            GL.AttachShader(PrgShaderHandle, VtxShaderHandle);
            GL.AttachShader(PrgShaderHandle, FragShaderHandle);
            GL.LinkProgram(PrgShaderHandle);
            GL.UseProgram(PrgShaderHandle);

            int TexLocation = GL.GetUniformLocation(PrgShaderHandle, "tex");

            GL.Uniform1(TexLocation, 0);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            unsafe
            {
                long HidOffset = Ns.Os.GetVirtHidOffset();

                if (HidOffset == 0 || HidOffset + Horizon.HidSize > uint.MaxValue)
                {
                    return;
                }

                byte* Ptr = (byte*)Ns.Ram + (uint)HidOffset;

                int State = 0;
                
                if (Keyboard[OpenTK.Input.Key.Up])
                {
                    State |= 0x2000;
                }

                if (Keyboard[OpenTK.Input.Key.Down])
                {
                    State |= 0x8000;
                }

                if (Keyboard[OpenTK.Input.Key.Left])
                {
                    State |= 0x1000;
                }

                if (Keyboard[OpenTK.Input.Key.Right])
                {
                    State |= 0x4000;
                }

                if (Keyboard[OpenTK.Input.Key.A])
                {
                    State |= 0x1;
                }

                if (Keyboard[OpenTK.Input.Key.S])
                {
                    State |= 0x2;
                }

                if (Keyboard[OpenTK.Input.Key.Z])
                {
                    State |= 0x4;
                }

                if (Keyboard[OpenTK.Input.Key.X])
                {
                    State |= 0x8;
                }

                if (Keyboard[OpenTK.Input.Key.Enter])
                {
                    State |= 0x400;
                }

                if (Keyboard[OpenTK.Input.Key.Tab])
                {
                    State |= 0x800;
                }

                *((int*)(Ptr + 0xae38)) = (int)State;
            }

            if (Keyboard[OpenTK.Input.Key.Escape])
            {
                this.Exit();
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Viewport(0, 0, 1280, 720);

            Title = $"Ryujinx Screen - (Vsync: {VSync} - FPS: {1f / e.Time:0})";

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            RenderFb();

            GL.UseProgram(PrgShaderHandle);
            
            Renderer.RunActions();
            Renderer.BindTexture(0);
            Renderer.Render();           

            SwapBuffers();
        }

        void RenderFb()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, ScreenTex.Texture);
            GL.BindVertexArray(VaoHandle);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }
    }
}