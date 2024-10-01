using OpenTK.Graphics.OpenGL;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.OpenGL.Image;
using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Graphics.OpenGL
{
    class Framebuffer : IDisposable
    {
        public int Handle { get; private set; }
        private int _clearFbHandle;
        private bool _clearFbInitialized;

        private FramebufferAttachment _lastDsAttachment;

        private readonly TextureView[] _colors;
        private TextureView _depthStencil;

        private int _colorsCount;
        private bool _dualSourceBlend;

        public Framebuffer()
        {
            Handle = GL.GenFramebuffer();
            _clearFbHandle = GL.GenFramebuffer();

            _colors = new TextureView[8];
        }

        public int Bind()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, Handle);
            return Handle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AttachColor(int index, TextureView color)
        {
            if (_colors[index] == color)
            {
                return;
            }

            FramebufferAttachment attachment = FramebufferAttachment.ColorAttachment0 + index;

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, attachment, color?.Handle ?? 0, 0);

            _colors[index] = color;
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
                FramebufferAttachment attachment = GetAttachment(depthStencil.Format);

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

            _depthStencil = depthStencil;
        }

        public void SetDualSourceBlend(bool enable)
        {
            bool oldEnable = _dualSourceBlend;

            _dualSourceBlend = enable;

            // When dual source blend is used,
            // we can only have one draw buffer.
            if (enable)
            {
                GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            }
            else if (oldEnable)
            {
                SetDrawBuffersImpl(_colorsCount);
            }
        }

        public void SetDrawBuffers(int colorsCount)
        {
            if (_colorsCount != colorsCount && !_dualSourceBlend)
            {
                SetDrawBuffersImpl(colorsCount);
            }

            _colorsCount = colorsCount;
        }

        private static void SetDrawBuffersImpl(int colorsCount)
        {
            DrawBuffersEnum[] drawBuffers = new DrawBuffersEnum[colorsCount];

            for (int index = 0; index < colorsCount; index++)
            {
                drawBuffers[index] = DrawBuffersEnum.ColorAttachment0 + index;
            }

            GL.DrawBuffers(colorsCount, drawBuffers);
        }

        private static FramebufferAttachment GetAttachment(Format format)
        {
            if (FormatTable.IsPackedDepthStencil(format))
            {
                return FramebufferAttachment.DepthStencilAttachment;
            }
            else if (FormatTable.IsDepthOnly(format))
            {
                return FramebufferAttachment.DepthAttachment;
            }
            else
            {
                return FramebufferAttachment.StencilAttachment;
            }
        }

        public int GetColorLayerCount(int index)
        {
            return _colors[index]?.Info.GetDepthOrLayers() ?? 0;
        }

        public int GetDepthStencilLayerCount()
        {
            return _depthStencil?.Info.GetDepthOrLayers() ?? 0;
        }

        public void AttachColorLayerForClear(int index, int layer)
        {
            TextureView color = _colors[index];

            if (!IsLayered(color))
            {
                return;
            }

            BindClearFb();
            GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + index, color.Handle, 0, layer);
        }

        public void DetachColorLayerForClear(int index)
        {
            TextureView color = _colors[index];

            if (!IsLayered(color))
            {
                return;
            }

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0 + index, 0, 0);
            Bind();
        }

        public void AttachDepthStencilLayerForClear(int layer)
        {
            TextureView depthStencil = _depthStencil;

            if (!IsLayered(depthStencil))
            {
                return;
            }

            BindClearFb();
            GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, GetAttachment(depthStencil.Format), depthStencil.Handle, 0, layer);
        }

        public void DetachDepthStencilLayerForClear()
        {
            TextureView depthStencil = _depthStencil;

            if (!IsLayered(depthStencil))
            {
                return;
            }

            GL.FramebufferTexture(FramebufferTarget.Framebuffer, GetAttachment(depthStencil.Format), 0, 0);
            Bind();
        }

        private void BindClearFb()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _clearFbHandle);

            if (!_clearFbInitialized)
            {
                SetDrawBuffersImpl(Constants.MaxRenderTargets);
                _clearFbInitialized = true;
            }
        }

        private static bool IsLayered(TextureView view)
        {
            return view != null &&
                   view.Target != Target.Texture1D &&
                   view.Target != Target.Texture2D &&
                   view.Target != Target.Texture2DMultisample &&
                   view.Target != Target.TextureBuffer;
        }

        public void Dispose()
        {
            if (Handle != 0)
            {
                GL.DeleteFramebuffer(Handle);

                Handle = 0;
            }

            if (_clearFbHandle != 0)
            {
                GL.DeleteFramebuffer(_clearFbHandle);

                _clearFbHandle = 0;
            }
        }
    }
}
