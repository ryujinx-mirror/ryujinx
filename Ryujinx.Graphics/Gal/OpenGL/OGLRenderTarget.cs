using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.Texture;
using System;

namespace Ryujinx.Graphics.Gal.OpenGL
{
    class OGLRenderTarget : IGalRenderTarget
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

        private class FrameBufferAttachments
        {
            public long[] Colors;
            public long Zeta;

            public int MapCount;
            public DrawBuffersEnum[] Map;

            public FrameBufferAttachments()
            {
                Colors = new long[RenderTargetsCount];

                Map = new DrawBuffersEnum[RenderTargetsCount];
            }

            public void SetAndClear(FrameBufferAttachments Source)
            {
                Zeta     = Source.Zeta;
                MapCount = Source.MapCount;

                Source.Zeta     = 0;
                Source.MapCount = 0;

                for (int i = 0; i < RenderTargetsCount; i++)
                {
                    Colors[i] = Source.Colors[i];
                    Map[i]    = Source.Map[i];

                    Source.Colors[i] = 0;
                    Source.Map[i]    = 0;
                }
            }
        }

        private const int NativeWidth  = 1280;
        private const int NativeHeight = 720;

        private const int RenderTargetsCount = 8;

        private const GalImageFormat RawFormat = GalImageFormat.A8B8G8R8 | GalImageFormat.Unorm;

        private OGLTexture Texture;

        private ImageHandler ReadTex;

        private float[] Viewports;
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

        private FrameBufferAttachments Attachments;
        private FrameBufferAttachments OldAttachments;

        private int CopyPBO;

        public OGLRenderTarget(OGLTexture Texture)
        {
            Attachments = new FrameBufferAttachments();

            OldAttachments = new FrameBufferAttachments();

            Viewports = new float[RenderTargetsCount * 4];

            this.Texture = Texture;
        }

        public void Bind()
        {
            if (DummyFrameBuffer == 0)
            {
                DummyFrameBuffer = GL.GenFramebuffer();
            }

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, DummyFrameBuffer);

            ImageHandler CachedImage;

            for (int Attachment = 0; Attachment < RenderTargetsCount; Attachment++)
            {
                if (Attachments.Colors[Attachment] == OldAttachments.Colors[Attachment])
                {
                    continue;
                }

                int Handle = 0;

                if (Attachments.Colors[Attachment] != 0 &&
                    Texture.TryGetImageHandler(Attachments.Colors[Attachment], out CachedImage))
                {
                    Handle = CachedImage.Handle;
                }

                GL.FramebufferTexture(
                    FramebufferTarget.DrawFramebuffer,
                    FramebufferAttachment.ColorAttachment0 + Attachment,
                    Handle,
                    0);
            }

            if (Attachments.Zeta != OldAttachments.Zeta)
            {
                if (Attachments.Zeta != 0 && Texture.TryGetImageHandler(Attachments.Zeta, out CachedImage))
                {
                    if (CachedImage.HasDepth && CachedImage.HasStencil)
                    {
                        GL.FramebufferTexture(
                            FramebufferTarget.DrawFramebuffer,
                            FramebufferAttachment.DepthStencilAttachment,
                            CachedImage.Handle,
                            0);
                    }
                    else if (CachedImage.HasDepth)
                    {
                        GL.FramebufferTexture(
                            FramebufferTarget.DrawFramebuffer,
                            FramebufferAttachment.DepthAttachment,
                            CachedImage.Handle,
                            0);

                        GL.FramebufferTexture(
                            FramebufferTarget.DrawFramebuffer,
                            FramebufferAttachment.StencilAttachment,
                            0,
                            0);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    GL.FramebufferTexture(
                        FramebufferTarget.DrawFramebuffer,
                        FramebufferAttachment.DepthStencilAttachment,
                        0,
                        0);
                }
            }

            if (OGLExtension.ViewportArray)
            {
                GL.ViewportArray(0, 8, Viewports);
            }
            else
            {
                GL.Viewport(
                    (int)Viewports[0],
                    (int)Viewports[1],
                    (int)Viewports[2],
                    (int)Viewports[3]);
            }

            if (Attachments.MapCount > 1)
            {
                GL.DrawBuffers(Attachments.MapCount, Attachments.Map);
            }
            else if (Attachments.MapCount == 1)
            {
                GL.DrawBuffer((DrawBufferMode)Attachments.Map[0]);
            }
            else
            {
                GL.DrawBuffer(DrawBufferMode.None);
            }

            OldAttachments.SetAndClear(Attachments);
        }

        public void BindColor(long Key, int Attachment)
        {
            Attachments.Colors[Attachment] = Key;
        }

        public void UnbindColor(int Attachment)
        {
            Attachments.Colors[Attachment] = 0;
        }

        public void BindZeta(long Key)
        {
            Attachments.Zeta = Key;
        }
        
        public void UnbindZeta()
        {
            Attachments.Zeta = 0;
        }

        public void Present(long Key)
        {
            Texture.TryGetImageHandler(Key, out ReadTex);
        }

        public void SetMap(int[] Map)
        {
            if (Map != null)
            {
                Attachments.MapCount = Map.Length;

                for (int Attachment = 0; Attachment < Attachments.MapCount; Attachment++)
                {
                    Attachments.Map[Attachment] = DrawBuffersEnum.ColorAttachment0 + Map[Attachment];
                }
            }
            else
            {
                Attachments.MapCount = 0;
            }
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

        public void SetViewport(int Attachment, int X, int Y, int Width, int Height)
        {
            int Offset = Attachment * 4;

            Viewports[Offset + 0] = X;
            Viewports[Offset + 1] = Y;
            Viewports[Offset + 2] = Width;
            Viewports[Offset + 3] = Height;
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

            GL.Viewport(0, 0, Window.Width, Window.Height);

            if (SrcFb == 0)
            {
                SrcFb = GL.GenFramebuffer();
            }

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, SrcFb);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

            GL.FramebufferTexture(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0, ReadTex.Handle, 0);

            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BlitFramebuffer(
                SrcX0, SrcY0, SrcX1, SrcY1,
                DstX0, DstY0, DstX1, DstY1,
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Linear);
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
            if (Texture.TryGetImageHandler(SrcKey, out ImageHandler SrcTex) &&
                Texture.TryGetImageHandler(DstKey, out ImageHandler DstTex))
            {
                if (SrcTex.HasColor   != DstTex.HasColor ||
                    SrcTex.HasDepth   != DstTex.HasDepth ||
                    SrcTex.HasStencil != DstTex.HasStencil)
                {
                    throw new NotImplementedException();
                }

                if (SrcFb == 0)
                {
                    SrcFb = GL.GenFramebuffer();
                }

                if (DstFb == 0)
                {
                    DstFb = GL.GenFramebuffer();
                }

                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, SrcFb);
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, DstFb);

                FramebufferAttachment Attachment = GetAttachment(SrcTex);

                GL.FramebufferTexture(FramebufferTarget.ReadFramebuffer, Attachment, SrcTex.Handle, 0);
                GL.FramebufferTexture(FramebufferTarget.DrawFramebuffer, Attachment, DstTex.Handle, 0);

                BlitFramebufferFilter Filter = BlitFramebufferFilter.Nearest;

                if (SrcTex.HasColor)
                {
                    GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

                    Filter = BlitFramebufferFilter.Linear;
                }

                ClearBufferMask Mask = GetClearMask(SrcTex);

                GL.Clear(Mask);

                GL.BlitFramebuffer(SrcX0, SrcY0, SrcX1, SrcY1, DstX0, DstY0, DstX1, DstY1, Mask, Filter);
            }
        }

        public void Reinterpret(long Key, GalImage NewImage)
        {
            if (!Texture.TryGetImage(Key, out GalImage OldImage))
            {
                return;
            }

            if (NewImage.Format == OldImage.Format)
            {
                return;
            }

            if (CopyPBO == 0)
            {
                CopyPBO = GL.GenBuffer();
            }

            GL.BindBuffer(BufferTarget.PixelPackBuffer, CopyPBO);

            GL.BufferData(BufferTarget.PixelPackBuffer, Math.Max(ImageUtils.GetSize(OldImage), ImageUtils.GetSize(NewImage)), IntPtr.Zero, BufferUsageHint.StreamCopy);

            if (!Texture.TryGetImageHandler(Key, out ImageHandler CachedImage))
            {
                throw new InvalidOperationException();
            }

            (_, PixelFormat Format, PixelType Type) = OGLEnumConverter.GetImageFormat(CachedImage.Format);

            GL.BindTexture(TextureTarget.Texture2D, CachedImage.Handle);

            GL.GetTexImage(TextureTarget.Texture2D, 0, Format, Type, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);

            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, CopyPBO);

            Texture.Create(Key, ImageUtils.GetSize(NewImage), NewImage);

            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);
        }

        private static FramebufferAttachment GetAttachment(ImageHandler CachedImage)
        {
            if (CachedImage.HasColor)
            {
                return FramebufferAttachment.ColorAttachment0;
            }
            else if (CachedImage.HasDepth && CachedImage.HasStencil)
            {
                return FramebufferAttachment.DepthStencilAttachment;
            }
            else if (CachedImage.HasDepth)
            {
                return FramebufferAttachment.DepthAttachment;
            }
            else if (CachedImage.HasStencil)
            {
                return FramebufferAttachment.StencilAttachment;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static ClearBufferMask GetClearMask(ImageHandler CachedImage)
        {
            return (CachedImage.HasColor   ? ClearBufferMask.ColorBufferBit   : 0) |
                   (CachedImage.HasDepth   ? ClearBufferMask.DepthBufferBit   : 0) |
                   (CachedImage.HasStencil ? ClearBufferMask.StencilBufferBit : 0);
        }
    }
}