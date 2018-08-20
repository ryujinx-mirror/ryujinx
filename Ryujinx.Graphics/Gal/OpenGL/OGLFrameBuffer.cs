using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OGLFrameBuffer : IGalFrameBuffer
    {
        private struct Rect
        {
            public int X      { get; private set; }
            public int Y      { get; private set; }
            public int Width  { get; private set; }
            public int Height { get; private set; }

            public Rect(int X, int Y, int Width, int Height)
            {
                this.X = X;
                this.Y = Y;
                this.Width = Width;
                this.Height = Height;
            }
        }

        private static readonly DrawBuffersEnum[] DrawBuffers = new DrawBuffersEnum[]
        {
            DrawBuffersEnum.ColorAttachment0,
            DrawBuffersEnum.ColorAttachment1,
            DrawBuffersEnum.ColorAttachment2,
            DrawBuffersEnum.ColorAttachment3,
            DrawBuffersEnum.ColorAttachment4,
            DrawBuffersEnum.ColorAttachment5,
            DrawBuffersEnum.ColorAttachment6,
            DrawBuffersEnum.ColorAttachment7,
        };

        private const int NativeWidth  = 1280;
        private const int NativeHeight = 720;

        private const GalImageFormat RawFormat = GalImageFormat.A8B8G8R8_UNORM_PACK32;

        private OGLTexture Texture;

        private ImageHandler RawTex;
        private ImageHandler ReadTex;

        private Rect Viewport;
        private Rect Window;

        private bool FlipX;
        private bool FlipY;

        private int CropTop;
        private int CropLeft;
        private int CropRight;
        private int CropBottom;

        //This framebuffer is used to attach guest rendertargets,
        //think of it as a dummy OpenGL VAO
        private int DummyFrameBuffer;

        //These framebuffers are used to blit images
        private int SrcFb;
        private int DstFb;

        //Holds current attachments, used to avoid unnecesary calls to OpenGL
        private int[] ColorAttachments;

        private int DepthAttachment;
        private int StencilAttachment;

        public OGLFrameBuffer(OGLTexture Texture)
        {
            ColorAttachments = new int[8];

            this.Texture = Texture;
        }

        public void BindColor(long Key, int Attachment)
        {
            if (Texture.TryGetImage(Key, out ImageHandler Tex))
            {
                EnsureFrameBuffer();

                Attach(ref ColorAttachments[Attachment], Tex.Handle, FramebufferAttachment.ColorAttachment0 + Attachment);
            }
            else
            {
                UnbindColor(Attachment);
            }
        }

        public void UnbindColor(int Attachment)
        {
            EnsureFrameBuffer();

            Attach(ref ColorAttachments[Attachment], 0, FramebufferAttachment.ColorAttachment0 + Attachment);
        }
        
        public void BindZeta(long Key)
        {
            if (Texture.TryGetImage(Key, out ImageHandler Tex))
            {
                EnsureFrameBuffer();

                if (Tex.HasDepth && Tex.HasStencil)
                {
                    if (DepthAttachment   != Tex.Handle ||
                        StencilAttachment != Tex.Handle)
                    {
                        GL.FramebufferTexture(
                            FramebufferTarget.DrawFramebuffer,
                            FramebufferAttachment.DepthStencilAttachment,
                            Tex.Handle,
                            0);

                        DepthAttachment = Tex.Handle;

                        StencilAttachment = Tex.Handle;
                    }
                }
                else if (Tex.HasDepth)
                {
                    Attach(ref DepthAttachment, Tex.Handle, FramebufferAttachment.DepthAttachment);

                    Attach(ref StencilAttachment, 0, FramebufferAttachment.StencilAttachment);
                }
                else if (Tex.HasStencil)
                {
                    Attach(ref DepthAttachment, 0, FramebufferAttachment.DepthAttachment);

                    Attach(ref StencilAttachment, Tex.Handle, FramebufferAttachment.StencilAttachment);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                UnbindZeta();
            }
        }

        public void UnbindZeta()
        {
            EnsureFrameBuffer();

            if (DepthAttachment   != 0 ||
                StencilAttachment != 0)
            {
                GL.FramebufferTexture(
                    FramebufferTarget.DrawFramebuffer,
                    FramebufferAttachment.DepthStencilAttachment,
                    0,
                    0);

                DepthAttachment = 0;

                StencilAttachment = 0;
            }
        }

        public void BindTexture(long Key, int Index)
        {
            if (Texture.TryGetImage(Key, out ImageHandler Tex))
            {
                GL.ActiveTexture(TextureUnit.Texture0 + Index);

                GL.BindTexture(TextureTarget.Texture2D, Tex.Handle);
            }
        }

        public void Set(long Key)
        {
            if (Texture.TryGetImage(Key, out ImageHandler Tex))
            {
                ReadTex = Tex;
            }
        }

        public void Set(byte[] Data, int Width, int Height)
        {
            if (RawTex == null)
            {
                RawTex = new ImageHandler();
            }

            RawTex.EnsureSetup(new GalImage(Width, Height, RawFormat));

            GL.BindTexture(TextureTarget.Texture2D, RawTex.Handle);

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, Width, Height, RawTex.PixelFormat, RawTex.PixelType, Data);

            ReadTex = RawTex;
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
            if (ReadTex == null)
            {
                return;
            }

            int SrcX0, SrcX1, SrcY0, SrcY1;

            if (CropLeft == 0 && CropRight == 0)
            {
                SrcX0 = 0;
                SrcX1 = ReadTex.Width;
            }
            else
            {
                SrcX0 = CropLeft;
                SrcX1 = CropRight;
            }

            if (CropTop == 0 && CropBottom == 0)
            {
                SrcY0 = 0;
                SrcY1 = ReadTex.Height;
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

            if (SrcFb == 0) SrcFb = GL.GenFramebuffer();

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

            GL.Viewport(0, 0, Window.Width, Window.Height);

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, SrcFb);

            GL.FramebufferTexture(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0, ReadTex.Handle, 0);

            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BlitFramebuffer(
                SrcX0, SrcY0, SrcX1, SrcY1,
                DstX0, DstY0, DstX1, DstY1,
                ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Linear);

            EnsureFrameBuffer();
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
            if (Texture.TryGetImage(SrcKey, out ImageHandler SrcTex) &&
                Texture.TryGetImage(DstKey, out ImageHandler DstTex))
            {
                if (SrcTex.HasColor != DstTex.HasColor ||
                    SrcTex.HasDepth != DstTex.HasDepth ||
                    SrcTex.HasStencil != DstTex.HasStencil)
                {
                    throw new NotImplementedException();
                }

                if (SrcTex.HasColor)
                {
                    CopyTextures(
                        SrcX0, SrcY0, SrcX1, SrcY1,
                        DstX0, DstY0, DstX1, DstY1,
                        SrcTex.Handle,
                        DstTex.Handle,
                        FramebufferAttachment.ColorAttachment0,
                        ClearBufferMask.ColorBufferBit,
                        true);
                }
                else if (SrcTex.HasDepth && SrcTex.HasStencil)
                {
                    CopyTextures(
                        SrcX0, SrcY0, SrcX1, SrcY1,
                        DstX0, DstY0, DstX1, DstY1,
                        SrcTex.Handle,
                        DstTex.Handle,
                        FramebufferAttachment.DepthStencilAttachment,
                        ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit,
                        false);
                }
                else if (SrcTex.HasDepth)
                {
                    CopyTextures(
                        SrcX0, SrcY0, SrcX1, SrcY1,
                        DstX0, DstY0, DstX1, DstY1,
                        SrcTex.Handle,
                        DstTex.Handle,
                        FramebufferAttachment.DepthAttachment,
                        ClearBufferMask.DepthBufferBit,
                        false);
                }
                else if (SrcTex.HasStencil)
                {
                    CopyTextures(
                        SrcX0, SrcY0, SrcX1, SrcY1,
                        DstX0, DstY0, DstX1, DstY1,
                        SrcTex.Handle,
                        DstTex.Handle,
                        FramebufferAttachment.StencilAttachment,
                        ClearBufferMask.StencilBufferBit,
                        false);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        public void GetBufferData(long Key, Action<byte[]> Callback)
        {
            if (Texture.TryGetImage(Key, out ImageHandler Tex))
            {
                byte[] Data = new byte[Tex.Width * Tex.Height * ImageHandler.MaxBpp];

                GL.BindTexture(TextureTarget.Texture2D, Tex.Handle);

                GL.GetTexImage(
                    TextureTarget.Texture2D,
                    0,
                    Tex.PixelFormat,
                    Tex.PixelType,
                    Data);

                Callback(Data);
            }
        }

        public void SetBufferData(
            long             Key,
            int              Width,
            int              Height,
            byte[]           Buffer)
        {
            if (Texture.TryGetImage(Key, out ImageHandler Tex))
            {
                GL.BindTexture(TextureTarget.Texture2D, Tex.Handle);

                const int Level  = 0;
                const int Border = 0;

                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    Level,
                    Tex.InternalFormat,
                    Width,
                    Height,
                    Border,
                    Tex.PixelFormat,
                    Tex.PixelType,
                    Buffer);
            }
        }

        private void EnsureFrameBuffer()
        {
            if (DummyFrameBuffer == 0)
            {
                DummyFrameBuffer = GL.GenFramebuffer();
            }

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, DummyFrameBuffer);

            GL.DrawBuffers(8, DrawBuffers);
        }

        private void Attach(ref int OldHandle, int NewHandle, FramebufferAttachment FbAttachment)
        {
            if (OldHandle != NewHandle)
            {
                GL.FramebufferTexture(
                    FramebufferTarget.DrawFramebuffer,
                    FbAttachment,
                    NewHandle,
                    0);

                OldHandle = NewHandle;
            }
        }

        private void CopyTextures(
            int SrcX0,
            int SrcY0,
            int SrcX1,
            int SrcY1,
            int DstX0,
            int DstY0,
            int DstX1,
            int DstY1,
            int SrcTexture,
            int DstTexture,
            FramebufferAttachment Attachment,
            ClearBufferMask Mask,
            bool Color)
        {
            if (SrcFb == 0) SrcFb = GL.GenFramebuffer();
            if (DstFb == 0) DstFb = GL.GenFramebuffer();

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, SrcFb);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, DstFb);

            GL.FramebufferTexture(
                FramebufferTarget.ReadFramebuffer,
                Attachment,
                SrcTexture,
                0);

            GL.FramebufferTexture(
                FramebufferTarget.DrawFramebuffer,
                Attachment,
                DstTexture,
                0);

            if (Color)
            {
                GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            }

            GL.Clear(Mask);

            GL.BlitFramebuffer(
                SrcX0, SrcY0, SrcX1, SrcY1,
                DstX0, DstY0, DstX1, DstY1,
                Mask,
                Color ? BlitFramebufferFilter.Linear : BlitFramebufferFilter.Nearest);

            EnsureFrameBuffer();
        }
    }
}