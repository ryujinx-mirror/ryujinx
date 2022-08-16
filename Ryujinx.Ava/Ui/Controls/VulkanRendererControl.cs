using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using Ryujinx.Ava.Ui.Backend.Vulkan;
using Ryujinx.Ava.Ui.Vulkan;
using Ryujinx.Common.Configuration;
using Ryujinx.Graphics.Vulkan;
using Silk.NET.Vulkan;
using SkiaSharp;
using SPB.Windowing;
using System;
using System.Collections.Concurrent;

namespace Ryujinx.Ava.Ui.Controls
{
    internal class VulkanRendererControl : RendererControl
    {
        private const int MaxImagesInFlight = 3;

        private VulkanPlatformInterface _platformInterface;
        private ConcurrentQueue<PresentImageInfo> _imagesInFlight;
        private PresentImageInfo _currentImage;

        public VulkanRendererControl(GraphicsDebugLevel graphicsDebugLevel) : base(graphicsDebugLevel)
        {
            _platformInterface = AvaloniaLocator.Current.GetService<VulkanPlatformInterface>();

            _imagesInFlight = new ConcurrentQueue<PresentImageInfo>();
        }

        public override void DestroyBackgroundContext()
        {

        }

        protected override ICustomDrawOperation CreateDrawOperation()
        {
            return new VulkanDrawOperation(this);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            _imagesInFlight.Clear();

            if (_platformInterface.MainSurface.Display != null)
            {
                _platformInterface.MainSurface.Display.Presented -= Window_Presented;
            }
            
            _currentImage?.Put();
            _currentImage = null;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);

            _platformInterface.MainSurface.Display.Presented += Window_Presented;
        }

        private void Window_Presented(object sender, EventArgs e)
        {
            _platformInterface.MainSurface.Device.QueueWaitIdle();
            _currentImage?.Put();
            _currentImage = null;
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);
        }

        protected override void CreateWindow()
        {
        }

        internal override void MakeCurrent()
        {
        }

        internal override void MakeCurrent(SwappableNativeWindowBase window)
        {
        }

        internal override void Present(object image)
        {
            Image = image;

            _imagesInFlight.Enqueue((PresentImageInfo)image);

            if (_imagesInFlight.Count > MaxImagesInFlight)
            {
                _imagesInFlight.TryDequeue(out _);
            }

            Dispatcher.UIThread.Post(InvalidateVisual);
        }

        private PresentImageInfo GetImage()
        {
            lock (_imagesInFlight)
            {
                if (!_imagesInFlight.TryDequeue(out _currentImage))
                {
                    _currentImage = (PresentImageInfo)Image;
                }

                return _currentImage;
            }
        }

        private class VulkanDrawOperation : ICustomDrawOperation
        {
            public Rect Bounds { get; }

            private readonly VulkanRendererControl _control;
            private bool _isDestroyed;

            public VulkanDrawOperation(VulkanRendererControl control)
            {
                _control = control;
                Bounds = _control.Bounds;
            }

            public void Dispose()
            {
                if (_isDestroyed)
                {
                    return;
                }

                _isDestroyed = true;
            }

            public bool Equals(ICustomDrawOperation other)
            {
                return other is VulkanDrawOperation operation && Equals(this, operation) && operation.Bounds == Bounds;
            }

            public bool HitTest(Point p)
            {
                return Bounds.Contains(p);
            }

            public unsafe void Render(IDrawingContextImpl context)
            {
                if (_isDestroyed || _control.Image == null || _control.RenderSize.Width == 0 || _control.RenderSize.Height == 0 ||
                    context is not ISkiaDrawingContextImpl skiaDrawingContextImpl)
                {
                    return;
                }

                var image = _control.GetImage();

                if (!image.State.IsValid)
                {
                    _control._currentImage = null;

                    return;
                }

                var gpu = AvaloniaLocator.Current.GetService<VulkanSkiaGpu>();

                image.Get();

                var imageInfo = new GRVkImageInfo()
                {
                    CurrentQueueFamily = _control._platformInterface.PhysicalDevice.QueueFamilyIndex,
                    Format = (uint)Format.R8G8B8A8Unorm,
                    Image = image.Image.Handle,
                    ImageLayout = (uint)ImageLayout.TransferSrcOptimal,
                    ImageTiling = (uint)ImageTiling.Optimal,
                    ImageUsageFlags = (uint)(ImageUsageFlags.ImageUsageColorAttachmentBit
                                             | ImageUsageFlags.ImageUsageTransferSrcBit
                                             | ImageUsageFlags.ImageUsageTransferDstBit),
                    LevelCount = 1,
                    SampleCount = 1,
                    Protected = false,
                    Alloc = new GRVkAlloc()
                    {
                        Memory = image.Memory.Handle,
                        Flags = 0,
                        Offset = image.MemoryOffset,
                        Size = image.MemorySize
                    }
                };

                using var backendTexture = new GRBackendRenderTarget(
                    (int)image.Extent.Width,
                    (int)image.Extent.Height,
                    1,
                    imageInfo);
                
                var vulkan = AvaloniaLocator.Current.GetService<VulkanPlatformInterface>();

                using var surface = SKSurface.Create(
                    skiaDrawingContextImpl.GrContext,
                    backendTexture,
                    GRSurfaceOrigin.TopLeft,
                    SKColorType.Rgba8888);

                if (surface == null)
                {
                    return;
                }

                var rect = new Rect(new Point(), new Size(image.Extent.Width, image.Extent.Height));

                using var snapshot = surface.Snapshot();
                skiaDrawingContextImpl.SkCanvas.DrawImage(snapshot, rect.ToSKRect(), _control.Bounds.ToSKRect(),
                    new SKPaint());
            }
        }
    }
}
