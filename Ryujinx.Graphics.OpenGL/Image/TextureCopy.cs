using Ryujinx.Graphics.GAL;
using OpenTK.Graphics.OpenGL;
using System;

namespace Ryujinx.Graphics.OpenGL.Image
{
    class TextureCopy : IDisposable
    {
        private readonly Renderer _renderer;

        private int _srcFramebuffer;
        private int _dstFramebuffer;

        private int _copyPboHandle;
        private int _copyPboSize;

        public TextureCopy(Renderer renderer)
        {
            _renderer = renderer;
        }

        public void Copy(
            TextureView src,
            TextureView dst,
            Extents2D   srcRegion,
            Extents2D   dstRegion,
            bool        linearFilter)
        {
            TextureView srcConverted = src.Format.IsBgra8() != dst.Format.IsBgra8() ? BgraSwap(src) : src;

            (int oldDrawFramebufferHandle, int oldReadFramebufferHandle) = ((Pipeline)_renderer.Pipeline).GetBoundFramebuffers();

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, GetSrcFramebufferLazy());
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, GetDstFramebufferLazy());

            Attach(FramebufferTarget.ReadFramebuffer, src.Format, srcConverted.Handle);
            Attach(FramebufferTarget.DrawFramebuffer, dst.Format, dst.Handle);

            ClearBufferMask mask = GetMask(src.Format);

            if ((mask & (ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit)) != 0 || src.Format.IsInteger())
            {
                linearFilter = false;
            }

            BlitFramebufferFilter filter = linearFilter
                ? BlitFramebufferFilter.Linear
                : BlitFramebufferFilter.Nearest;

            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            GL.Disable(EnableCap.RasterizerDiscard);
            GL.Disable(IndexedEnableCap.ScissorTest, 0);

            GL.BlitFramebuffer(
                srcRegion.X1,
                srcRegion.Y1,
                srcRegion.X2,
                srcRegion.Y2,
                dstRegion.X1,
                dstRegion.Y1,
                dstRegion.X2,
                dstRegion.Y2,
                mask,
                filter);

            Attach(FramebufferTarget.ReadFramebuffer, src.Format, 0);
            Attach(FramebufferTarget.DrawFramebuffer, dst.Format, 0);

            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, oldReadFramebufferHandle);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, oldDrawFramebufferHandle);

            ((Pipeline)_renderer.Pipeline).RestoreScissor0Enable();
            ((Pipeline)_renderer.Pipeline).RestoreRasterizerDiscard();

            if (srcConverted != src)
            {
                srcConverted.Dispose();
            }
        }

        private static void Attach(FramebufferTarget target, Format format, int handle)
        {
            if (format == Format.D24UnormS8Uint || format == Format.D32FloatS8Uint)
            {
                GL.FramebufferTexture(target, FramebufferAttachment.DepthStencilAttachment, handle, 0);
            }
            else if (IsDepthOnly(format))
            {
                GL.FramebufferTexture(target, FramebufferAttachment.DepthAttachment, handle, 0);
            }
            else if (format == Format.S8Uint)
            {
                GL.FramebufferTexture(target, FramebufferAttachment.StencilAttachment, handle, 0);
            }
            else
            {
                GL.FramebufferTexture(target, FramebufferAttachment.ColorAttachment0, handle, 0);
            }
        }

        private static ClearBufferMask GetMask(Format format)
        {
            if (format == Format.D24UnormS8Uint || format == Format.D32FloatS8Uint)
            {
                return ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit;
            }
            else if (IsDepthOnly(format))
            {
                return ClearBufferMask.DepthBufferBit;
            }
            else if (format == Format.S8Uint)
            {
                return ClearBufferMask.StencilBufferBit;
            }
            else
            {
                return ClearBufferMask.ColorBufferBit;
            }
        }

        private static bool IsDepthOnly(Format format)
        {
            return format == Format.D16Unorm   ||
                   format == Format.D24X8Unorm ||
                   format == Format.D32Float;
        }

        public TextureView BgraSwap(TextureView from)
        {
            TextureView to = (TextureView)_renderer.CreateTexture(from.Info, from.ScaleFactor);

            EnsurePbo(from);

            GL.BindBuffer(BufferTarget.PixelPackBuffer, _copyPboHandle);

            from.WriteToPbo(0, forceBgra: true);

            GL.BindBuffer(BufferTarget.PixelPackBuffer, 0);
            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, _copyPboHandle);

            to.ReadFromPbo(0, _copyPboSize);

            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);

            return to;
        }

        private void EnsurePbo(TextureView view)
        {
            int requiredSize = 0;

            for (int level = 0; level < view.Info.Levels; level++)
            {
                requiredSize += view.Info.GetMipSize(level);
            }

            if (_copyPboSize < requiredSize && _copyPboHandle != 0)
            {
                GL.DeleteBuffer(_copyPboHandle);

                _copyPboHandle = 0;
            }

            if (_copyPboHandle == 0)
            {
                _copyPboHandle = GL.GenBuffer();
                _copyPboSize = requiredSize;

                GL.BindBuffer(BufferTarget.PixelPackBuffer, _copyPboHandle);
                GL.BufferData(BufferTarget.PixelPackBuffer, requiredSize, IntPtr.Zero, BufferUsageHint.DynamicCopy);
            }
        }

        private int GetSrcFramebufferLazy()
        {
            if (_srcFramebuffer == 0)
            {
                _srcFramebuffer = GL.GenFramebuffer();
            }

            return _srcFramebuffer;
        }

        private int GetDstFramebufferLazy()
        {
            if (_dstFramebuffer == 0)
            {
                _dstFramebuffer = GL.GenFramebuffer();
            }

            return _dstFramebuffer;
        }

        public void Dispose()
        {
            if (_srcFramebuffer != 0)
            {
                GL.DeleteFramebuffer(_srcFramebuffer);

                _srcFramebuffer = 0;
            }

            if (_dstFramebuffer != 0)
            {
                GL.DeleteFramebuffer(_dstFramebuffer);

                _dstFramebuffer = 0;
            }

            if (_copyPboHandle != 0)
            {
                GL.DeleteBuffer(_copyPboHandle);

                _copyPboHandle = 0;
            }
        }
    }
}
