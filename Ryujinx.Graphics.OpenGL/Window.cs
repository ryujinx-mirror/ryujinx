using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.OpenGL
{
    class Window : IWindow
    {
        private const int NativeWidth  = 1280;
        private const int NativeHeight = 720;

        private int _width = 1280;
        private int _height = 720;

        private int _blitFramebufferHandle;
        private int _copyFramebufferHandle;

        private int _screenTextureHandle;

        private TextureReleaseCallback _release;

        private struct PresentationTexture
        {
            public TextureView Texture { get; }

            public ImageCrop Crop { get; }

            public object Context { get; }

            public PresentationTexture(TextureView texture, ImageCrop crop, object context)
            {
                Texture = texture;
                Crop    = crop;
                Context = context;
            }
        }

        private Queue<PresentationTexture> _textures;

        public Window()
        {
            _textures = new Queue<PresentationTexture>();
        }

        public void Present()
        {
            GL.Disable(EnableCap.FramebufferSrgb);

            CopyTextureFromQueue();

            int oldReadFramebufferHandle = GL.GetInteger(GetPName.ReadFramebufferBinding);
            int oldDrawFramebufferHandle = GL.GetInteger(GetPName.DrawFramebufferBinding);

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, GetCopyFramebufferHandleLazy());

            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.BlitFramebuffer(
                0,
                0,
                1280,
                720,
                0,
                0,
                1280,
                720,
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Linear);

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, oldReadFramebufferHandle);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, oldDrawFramebufferHandle);

            GL.Enable(EnableCap.FramebufferSrgb);
        }

        private void CopyTextureFromQueue()
        {
            if (!_textures.TryDequeue(out PresentationTexture presentationTexture))
            {
                return;
            }

            TextureView texture = presentationTexture.Texture;
            ImageCrop   crop    = presentationTexture.Crop;
            object      context = presentationTexture.Context;

            int oldReadFramebufferHandle = GL.GetInteger(GetPName.ReadFramebufferBinding);
            int oldDrawFramebufferHandle = GL.GetInteger(GetPName.DrawFramebufferBinding);

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, GetCopyFramebufferHandleLazy());
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, GetBlitFramebufferHandleLazy());

            GL.FramebufferTexture(
                FramebufferTarget.ReadFramebuffer,
                FramebufferAttachment.ColorAttachment0,
                texture.Handle,
                0);

            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            int srcX0, srcX1, srcY0, srcY1;

            if (crop.Left == 0 && crop.Right == 0)
            {
                srcX0 = 0;
                srcX1 = texture.Width;
            }
            else
            {
                srcX0 = crop.Left;
                srcX1 = crop.Right;
            }

            if (crop.Top == 0 && crop.Bottom == 0)
            {
                srcY0 = 0;
                srcY1 = texture.Height;
            }
            else
            {
                srcY0 = crop.Top;
                srcY1 = crop.Bottom;
            }

            float ratioX = MathF.Min(1f, (_height * (float)NativeWidth)  / ((float)NativeHeight * _width));
            float ratioY = MathF.Min(1f, (_width  * (float)NativeHeight) / ((float)NativeWidth  * _height));

            int dstWidth  = (int)(_width  * ratioX);
            int dstHeight = (int)(_height * ratioY);

            int dstPaddingX = (_width  - dstWidth)  / 2;
            int dstPaddingY = (_height - dstHeight) / 2;

            int dstX0 = crop.FlipX ? _width - dstPaddingX : dstPaddingX;
            int dstX1 = crop.FlipX ? dstPaddingX : _width - dstPaddingX;

            int dstY0 = crop.FlipY ? dstPaddingY : _height - dstPaddingY;
            int dstY1 = crop.FlipY ? _height - dstPaddingY : dstPaddingY;

            GL.BlitFramebuffer(
                srcX0,
                srcY0,
                srcX1,
                srcY1,
                dstX0,
                dstY0,
                dstX1,
                dstY1,
                ClearBufferMask.ColorBufferBit,
                BlitFramebufferFilter.Linear);

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, oldReadFramebufferHandle);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, oldDrawFramebufferHandle);

            texture.Release();

            Release(context);
        }

        public void QueueTexture(ITexture texture, ImageCrop crop, object context)
        {
            if (texture == null)
            {
                Release(context);

                return;
            }

            TextureView textureView = (TextureView)texture;

            textureView.Acquire();

            _textures.Enqueue(new PresentationTexture(textureView, crop, context));
        }

        public void RegisterTextureReleaseCallback(TextureReleaseCallback callback)
        {
            _release = callback;
        }

        private void Release(object context)
        {
            if (_release != null)
            {
                _release(context);
            }
        }

        private int GetBlitFramebufferHandleLazy()
        {
            int handle = _blitFramebufferHandle;

            if (handle == 0)
            {
                handle = GL.GenFramebuffer();

                _blitFramebufferHandle = handle;
            }

            return handle;
        }

        private int GetCopyFramebufferHandleLazy()
        {
            int handle = _copyFramebufferHandle;

            if (handle == 0)
            {
                int textureHandle = GL.GenTexture();

                GL.BindTexture(TextureTarget.Texture2D, textureHandle);

                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.Rgba8,
                    1280,
                    720,
                    0,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    IntPtr.Zero);

                handle = GL.GenFramebuffer();

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, handle);

                GL.FramebufferTexture(
                    FramebufferTarget.Framebuffer,
                    FramebufferAttachment.ColorAttachment0,
                    textureHandle,
                    0);

                _screenTextureHandle = textureHandle;

                _copyFramebufferHandle = handle;
            }

            return handle;
        }
    }
}
