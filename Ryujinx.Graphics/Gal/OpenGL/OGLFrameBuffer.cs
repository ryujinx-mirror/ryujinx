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

            public FrameBuffer(int Width, int Height, bool HasRenderBuffer)
            {
                this.Width  = Width;
                this.Height = Height;

                Handle    = GL.GenFramebuffer();
                TexHandle = GL.GenTexture();

                if (HasRenderBuffer)
                {
                    RbHandle = GL.GenRenderbuffer();
                }
            }
        }

        private const int NativeWidth  = 1280;
        private const int NativeHeight = 720;

        private Dictionary<long, FrameBuffer> Fbs;

        private Rect Viewport;
        private Rect Window;

        private FrameBuffer CurrFb;
        private FrameBuffer CurrReadFb;

        private FrameBuffer RawFb;

        private bool FlipX;
        private bool FlipY;

        private int CropTop;
        private int CropLeft;
        private int CropRight;
        private int CropBottom;

        public OGLFrameBuffer()
        {
            Fbs = new Dictionary<long, FrameBuffer>();
        }

        public void Create(long Key, int Width, int Height)
        {
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

            Fb = new FrameBuffer(Width, Height, true);

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

            Fbs.Add(Key, Fb);
        }

        public void Bind(long Key)
        {
            if (Fbs.TryGetValue(Key, out FrameBuffer Fb))
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, Fb.Handle);

                CurrFb = Fb;
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
                CurrReadFb = Fb;
            }
        }

        public void Set(byte[] Data, int Width, int Height)
        {
            if (RawFb == null)
            {
                CreateRawFb(Width, Height);
            }

            if (RawFb.Width  != Width ||
                RawFb.Height != Height)
            {
                SetupTexture(RawFb.TexHandle, Width, Height);

                RawFb.Width  = Width;
                RawFb.Height = Height;
            }

            GL.ActiveTexture(TextureUnit.Texture0);

            GL.BindTexture(TextureTarget.Texture2D, RawFb.TexHandle);

            (PixelFormat Format, PixelType Type) = OGLEnumConverter.GetTextureFormat(GalTextureFormat.A8B8G8R8);

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, Width, Height, Format, Type, Data);

            CurrReadFb = RawFb;
        }

        public void SetTransform(bool FlipX, bool FlipY, int Top, int Left, int Right, int Bottom)
        {
            this.FlipX = FlipX;
            this.FlipY = FlipY;

            CropTop    = Top;
            CropLeft   = Left;
            CropRight  = Right;
            CropBottom = Bottom;
        }

        public void SetWindowSize(int Width, int Height)
        {
            Window = new Rect(0, 0, Width, Height);
        }

        public void SetViewport(int X, int Y, int Width, int Height)
        {
            Viewport = new Rect(X, Y, Width, Height);

            SetViewport(Viewport);
        }

        private void SetViewport(Rect Viewport)
        {
            GL.Viewport(
                Viewport.X,
                Viewport.Y,
                Viewport.Width,
                Viewport.Height);
        }

        public void Render()
        {
            if (CurrReadFb != null)
            {
                int SrcX0, SrcX1, SrcY0, SrcY1;

                if (CropLeft == 0 && CropRight == 0)
                {
                    SrcX0 = 0;
                    SrcX1 = CurrReadFb.Width;
                }
                else
                {
                    SrcX0 = CropLeft;
                    SrcX1 = CropRight;
                }

                if (CropTop == 0 && CropBottom == 0)
                {
                    SrcY0 = 0;
                    SrcY1 = CurrReadFb.Height;
                }
                else
                {
                    SrcY0 = CropTop;
                    SrcY1 = CropBottom;
                }

                float RatioX = MathF.Min(1f, (Window.Height * (float)NativeWidth)  / ((float)NativeHeight * Window.Width));
                float RatioY = MathF.Min(1f, (Window.Width  * (float)NativeHeight) / ((float)NativeWidth  * Window.Height));

                int DstWidth  = (int)(Window.Width  * RatioX);
                int DstHeight = (int)(Window.Height * RatioY);

                int DstPaddingX = (Window.Width  - DstWidth)  / 2;
                int DstPaddingY = (Window.Height - DstHeight) / 2;

                int DstX0 = FlipX ? Window.Width - DstPaddingX : DstPaddingX;
                int DstX1 = FlipX ? DstPaddingX : Window.Width - DstPaddingX;

                int DstY0 = FlipY ? DstPaddingY : Window.Height - DstPaddingY;
                int DstY1 = FlipY ? Window.Height - DstPaddingY : DstPaddingY;

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

                GL.Viewport(0, 0, Window.Width, Window.Height);

                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, CurrReadFb.Handle);

                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.BlitFramebuffer(
                    SrcX0, SrcY0, SrcX1, SrcY1,
                    DstX0, DstY0, DstX1, DstY1,
                    ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);
            }
        }

        public void Copy(
            long SrcKey,
            long DstKey,
            int  SrcX0,
            int  SrcY0,
            int  SrcX1,
            int  SrcY1,
            int  DstX0,
            int  DstY0,
            int  DstX1,
            int  DstY1)
        {
            if (Fbs.TryGetValue(SrcKey, out FrameBuffer SrcFb) &&
                Fbs.TryGetValue(DstKey, out FrameBuffer DstFb))
            {
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, SrcFb.Handle);
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, DstFb.Handle);

                GL.Clear(ClearBufferMask.ColorBufferBit);

                GL.BlitFramebuffer(
                    SrcX0, SrcY0, SrcX1, SrcY1,
                    DstX0, DstY0, DstX1, DstY1,
                    ClearBufferMask.ColorBufferBit,
                    BlitFramebufferFilter.Linear);
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
            }
        }

        public void SetBufferData(
            long             Key,
            int              Width,
            int              Height,
            GalTextureFormat Format,
            byte[]           Buffer)
        {
            if (Fbs.TryGetValue(Key, out FrameBuffer Fb))
            {
                GL.BindTexture(TextureTarget.Texture2D, Fb.TexHandle);

                const int Level  = 0;
                const int Border = 0;

                const PixelInternalFormat InternalFmt = PixelInternalFormat.Rgba;

                (PixelFormat GlFormat, PixelType Type) = OGLEnumConverter.GetTextureFormat(Format);

                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    Level,
                    InternalFmt,
                    Width,
                    Height,
                    Border,
                    GlFormat,
                    Type,
                    Buffer);
            }
        }

        private void CreateRawFb(int Width, int Height)
        {
            if (RawFb == null)
            {
                RawFb = new FrameBuffer(Width, Height, false);

                SetupTexture(RawFb.TexHandle, Width, Height);

                RawFb.Width = Width;
                RawFb.Height = Height;

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, RawFb.Handle);

                GL.FramebufferTexture(
                    FramebufferTarget.Framebuffer,
                    FramebufferAttachment.ColorAttachment0,
                    RawFb.TexHandle,
                    0);

                GL.Viewport(0, 0, Width, Height);
            }
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