using Avalonia;
using Avalonia.OpenGL;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using OpenTK.Graphics.OpenGL;
using Ryujinx.Common.Configuration;
using SkiaSharp;
using SPB.Graphics;
using SPB.Graphics.OpenGL;
using SPB.Platform;
using SPB.Windowing;
using System;

namespace Ryujinx.Ava.Ui.Controls
{
    internal class OpenGLRendererControl : RendererControl
    {
        public int Major { get; }
        public int Minor { get; }
        public OpenGLContextBase GameContext { get; set; }

        public static OpenGLContextBase PrimaryContext =>
            AvaloniaLocator.Current.GetService<IPlatformOpenGlInterface>()
                .PrimaryContext.AsOpenGLContextBase();

        private SwappableNativeWindowBase _gameBackgroundWindow;

        private IntPtr _fence;

        public OpenGLRendererControl(int major, int minor, GraphicsDebugLevel graphicsDebugLevel) : base(graphicsDebugLevel)
        {
            Major = major;
            Minor = minor;
        }

        public override void DestroyBackgroundContext()
        {
            Image = null;

            if (_fence != IntPtr.Zero)
            {
                DrawOperation.Dispose();
                GL.DeleteSync(_fence);
            }

            GlDrawOperation.DeleteFramebuffer();

            GameContext?.Dispose();

            _gameBackgroundWindow?.Dispose();
        }

        internal override void Present(object image)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Image = (int)image;

                InvalidateVisual();
            }).Wait();

            if (_fence != IntPtr.Zero)
            {
                GL.DeleteSync(_fence);
            }

            _fence = GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);

            InvalidateVisual();

            _gameBackgroundWindow.SwapBuffers();
        }

        internal override void MakeCurrent()
        {
            GameContext.MakeCurrent(_gameBackgroundWindow);
        }

        internal override void MakeCurrent(SwappableNativeWindowBase window)
        {
            GameContext.MakeCurrent(window);
        }

        protected override void CreateWindow()
        {
            var flags = OpenGLContextFlags.Compat;
            if (DebugLevel != GraphicsDebugLevel.None)
            {
                flags |= OpenGLContextFlags.Debug;
            }
            _gameBackgroundWindow = PlatformHelper.CreateOpenGLWindow(FramebufferFormat.Default, 0, 0, 100, 100);
            _gameBackgroundWindow.Hide();

            GameContext = PlatformHelper.CreateOpenGLContext(FramebufferFormat.Default, Major, Minor, flags, shareContext: PrimaryContext);
            GameContext.Initialize(_gameBackgroundWindow);
        }

        protected override ICustomDrawOperation CreateDrawOperation()
        {
            return new GlDrawOperation(this);
        }

        private class GlDrawOperation : ICustomDrawOperation
        {
            private static int _framebuffer;

            public Rect Bounds { get; }

            private readonly OpenGLRendererControl _control;

            public GlDrawOperation(OpenGLRendererControl control)
            {
                _control = control;
                Bounds = _control.Bounds;
            }

            public void Dispose() { }

            public static void DeleteFramebuffer()
            {
                if (_framebuffer == 0)
                {
                    GL.DeleteFramebuffer(_framebuffer);
                }

                _framebuffer = 0;
            }

            public bool Equals(ICustomDrawOperation other)
            {
                return other is GlDrawOperation operation && Equals(this, operation) && operation.Bounds == Bounds;
            }

            public bool HitTest(Point p)
            {
                return Bounds.Contains(p);
            }

            private void CreateRenderTarget()
            {
                _framebuffer = GL.GenFramebuffer();
            }

            public void Render(IDrawingContextImpl context)
            {
                if (_control.Image == null)
                {
                    return;
                }

                if (_framebuffer == 0)
                {
                    CreateRenderTarget();
                }

                int currentFramebuffer = GL.GetInteger(GetPName.FramebufferBinding);

                var image = _control.Image;
                var fence = _control._fence;

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, (int)image, 0);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, currentFramebuffer);

                if (context is not ISkiaDrawingContextImpl skiaDrawingContextImpl)
                {
                    return;
                }

                var imageInfo = new SKImageInfo((int)_control.RenderSize.Width, (int)_control.RenderSize.Height, SKColorType.Rgba8888);
                var glInfo = new GRGlFramebufferInfo((uint)_framebuffer, SKColorType.Rgba8888.ToGlSizedFormat());

                GL.WaitSync(fence, WaitSyncFlags.None, ulong.MaxValue);

                using var backendTexture = new GRBackendRenderTarget(imageInfo.Width, imageInfo.Height, 1, 0, glInfo);
                using var surface = SKSurface.Create(skiaDrawingContextImpl.GrContext, backendTexture, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);

                if (surface == null)
                {
                    return;
                }

                var rect = new Rect(new Point(), _control.RenderSize);

                using var snapshot = surface.Snapshot();
                skiaDrawingContextImpl.SkCanvas.DrawImage(snapshot, rect.ToSKRect(), _control.Bounds.ToSKRect(), new SKPaint());
            }
        }
    }
}
