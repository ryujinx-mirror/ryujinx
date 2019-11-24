using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    class Window : IWindow
    {
        private const int NativeWidth  = 1280;
        private const int NativeHeight = 720;

        private int _width;
        private int _height;

        private int _copyFramebufferHandle;

        public Window()
        {
            _width  = NativeWidth;
            _height = NativeHeight;
        }

        public void Present(ITexture texture, ImageCrop crop)
        {
            TextureView view = (TextureView)texture;

            GL.Disable(EnableCap.FramebufferSrgb);

            int oldReadFramebufferHandle = GL.GetInteger(GetPName.ReadFramebufferBinding);
            int oldDrawFramebufferHandle = GL.GetInteger(GetPName.DrawFramebufferBinding);

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, GetCopyFramebufferHandleLazy());

            GL.FramebufferTexture(
                FramebufferTarget.ReadFramebuffer,
                FramebufferAttachment.ColorAttachment0,
                view.Handle,
                0);

            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);

            GL.Clear(ClearBufferMask.ColorBufferBit);

            int srcX0, srcX1, srcY0, srcY1;

            if (crop.Left == 0 && crop.Right == 0)
            {
                srcX0 = 0;
                srcX1 = view.Width;
            }
            else
            {
                srcX0 = crop.Left;
                srcX1 = crop.Right;
            }

            if (crop.Top == 0 && crop.Bottom == 0)
            {
                srcY0 = 0;
                srcY1 = view.Height;
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

            GL.Enable(EnableCap.FramebufferSrgb);
        }

        public void SetSize(int width, int height)
        {
            _width  = width;
            _height = height;
        }

        private int GetCopyFramebufferHandleLazy()
        {
            int handle = _copyFramebufferHandle;

            if (handle == 0)
            {
                handle = GL.GenFramebuffer();

                _copyFramebufferHandle = handle;
            }

            return handle;
        }
    }
}
