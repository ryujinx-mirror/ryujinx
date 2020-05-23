using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.OpenGL.Image;
using System;

namespace Ryujinx.Graphics.OpenGL
{
    class Framebuffer : IDisposable
    {
        public int Handle { get; private set; }

        private FramebufferAttachment _lastDsAttachment;

        private readonly TextureView[] _colors;

        public Framebuffer()
        {
            Handle = GL.GenFramebuffer();

            _colors = new TextureView[8];
        }

        public void Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
        }

        public void AttachColor(int index, TextureView color)
        {
            FramebufferAttachment attachment = FramebufferAttachment.ColorAttachment0 + index;

            if (HwCapabilities.Vendor == HwCapabilities.GpuVendor.Amd ||
                HwCapabilities.Vendor == HwCapabilities.GpuVendor.Intel)
            {
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, attachment, color?.GetIncompatibleFormatViewHandle() ?? 0, 0);

                _colors[index] = color;
            }
            else
            {
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, attachment, color?.Handle ?? 0, 0);
            }
        }

        public void AttachDepthStencil(TextureView depthStencil)
        {
            // Detach the last depth/stencil buffer if there is any.
            if (_lastDsAttachment != 0)
            {
                GL.FramebufferTexture(FramebufferTarget.Framebuffer, _lastDsAttachment, 0, 0);
            }

            if (depthStencil != null)
            {
                FramebufferAttachment attachment;

                if (IsPackedDepthStencilFormat(depthStencil.Format))
                {
                    attachment = FramebufferAttachment.DepthStencilAttachment;
                }
                else if (IsDepthOnlyFormat(depthStencil.Format))
                {
                    attachment = FramebufferAttachment.DepthAttachment;
                }
                else
                {
                    attachment = FramebufferAttachment.StencilAttachment;
                }

                GL.FramebufferTexture(
                    FramebufferTarget.Framebuffer,
                    attachment,
                    depthStencil.Handle,
                    0);

                _lastDsAttachment = attachment;
            }
            else
            {
                _lastDsAttachment = 0;
            }
        }

        public void SignalModified()
        {
            if (HwCapabilities.Vendor == HwCapabilities.GpuVendor.Amd ||
                HwCapabilities.Vendor == HwCapabilities.GpuVendor.Intel)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (_colors[i] != null)
                    {
                        _colors[i].SignalModified();
                    }
                }
            }
        }

        public void SetDrawBuffers(int colorsCount)
        {
            DrawBuffersEnum[] drawBuffers = new DrawBuffersEnum[colorsCount];

            for (int index = 0; index < colorsCount; index++)
            {
                drawBuffers[index] = DrawBuffersEnum.ColorAttachment0 + index;
            }

            GL.DrawBuffers(colorsCount, drawBuffers);
        }

        private static bool IsPackedDepthStencilFormat(Format format)
        {
            return format == Format.D24UnormS8Uint ||
                   format == Format.D32FloatS8Uint;
        }

        private static bool IsDepthOnlyFormat(Format format)
        {
            return format == Format.D16Unorm ||
                   format == Format.D24X8Unorm ||
                   format == Format.D32Float;
        }

        public void Dispose()
        {
            if (Handle != 0)
            {
                GL.DeleteFramebuffer(Handle);

                Handle = 0;
            }
        }
    }
}
