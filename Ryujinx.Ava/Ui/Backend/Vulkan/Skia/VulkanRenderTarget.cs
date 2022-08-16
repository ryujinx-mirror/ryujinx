using System;
using Avalonia;
using Avalonia.Skia;
using Ryujinx.Ava.Ui.Vulkan;
using Ryujinx.Ava.Ui.Vulkan.Surfaces;
using Silk.NET.Vulkan;
using SkiaSharp;

namespace Ryujinx.Ava.Ui.Backend.Vulkan
{
    internal class VulkanRenderTarget : ISkiaGpuRenderTarget
    {
        public GRContext GrContext { get; private set; }

        private readonly VulkanSurfaceRenderTarget _surface;
        private readonly VulkanPlatformInterface _vulkanPlatformInterface;
        private readonly IVulkanPlatformSurface _vulkanPlatformSurface;
        private GRVkBackendContext _grVkBackend;

        public VulkanRenderTarget(VulkanPlatformInterface vulkanPlatformInterface, IVulkanPlatformSurface vulkanPlatformSurface)
        {
            _surface = vulkanPlatformInterface.CreateRenderTarget(vulkanPlatformSurface);
            _vulkanPlatformInterface = vulkanPlatformInterface;
            _vulkanPlatformSurface = vulkanPlatformSurface;

            Initialize();
        }

        private void Initialize()
        {
            GRVkGetProcedureAddressDelegate getProc = GetVulkanProcAddress;

            _grVkBackend = new GRVkBackendContext()
            {
                VkInstance = _surface.Device.Handle,
                VkPhysicalDevice = _vulkanPlatformInterface.PhysicalDevice.Handle,
                VkDevice = _surface.Device.Handle,
                VkQueue = _surface.Device.Queue.Handle,
                GraphicsQueueIndex = _vulkanPlatformInterface.PhysicalDevice.QueueFamilyIndex,
                GetProcedureAddress = getProc
            };

            GrContext = GRContext.CreateVulkan(_grVkBackend);

            var gpu = AvaloniaLocator.Current.GetService<VulkanSkiaGpu>();

            if (gpu.MaxResourceBytes.HasValue)
            {
                GrContext.SetResourceCacheLimit(gpu.MaxResourceBytes.Value);
            }
        }

        private IntPtr GetVulkanProcAddress(string name, IntPtr instanceHandle, IntPtr deviceHandle)
        {
            IntPtr addr;

            if (deviceHandle != IntPtr.Zero)
            {
                addr = _vulkanPlatformInterface.Api.GetDeviceProcAddr(new Device(deviceHandle), name);

                if (addr != IntPtr.Zero)
                {
                    return addr;
                }

                addr = _vulkanPlatformInterface.Api.GetDeviceProcAddr(new Device(_surface.Device.Handle), name);

                if (addr != IntPtr.Zero)
                {
                    return addr;
                }
            }

            addr = _vulkanPlatformInterface.Api.GetInstanceProcAddr(new Instance(_vulkanPlatformInterface.Instance.Handle), name);

            if (addr == IntPtr.Zero)
            {
                addr = _vulkanPlatformInterface.Api.GetInstanceProcAddr(new Instance(instanceHandle), name);
            }

            return addr;
        }

        public void Dispose()
        {
            _grVkBackend.Dispose();
            GrContext.Dispose();
            _surface.Dispose();
        }

        public ISkiaGpuRenderSession BeginRenderingSession()
        {
            var session = _surface.BeginDraw(_vulkanPlatformSurface.Scaling);
            bool success = false;
            try
            {
                var disp = session.Display;
                var api = session.Api;

                var size = session.Size;
                var scaling = session.Scaling;
                if (size.Width <= 0 || size.Height <= 0 || scaling < 0)
                {
                    size = new Avalonia.PixelSize(1, 1);
                    scaling = 1;
                }

                lock (GrContext)
                {
                    GrContext.ResetContext();

                    var image = _surface.GetImage();

                    var imageInfo = new GRVkImageInfo()
                    {
                        CurrentQueueFamily = disp.QueueFamilyIndex,
                        Format = (uint)image.Format,
                        Image = image.Handle,
                        ImageLayout = (uint)image.CurrentLayout,
                        ImageTiling = (uint)image.Tiling,
                        ImageUsageFlags = _surface.UsageFlags,
                        LevelCount = _surface.MipLevels,
                        SampleCount = 1,
                        Protected = false,
                        Alloc = new GRVkAlloc()
                        {
                            Memory = image.MemoryHandle,
                            Flags = 0,
                            Offset = 0,
                            Size = _surface.MemorySize
                        }
                    };

                    var renderTarget =
                        new GRBackendRenderTarget((int)size.Width, (int)size.Height, 1,
                            imageInfo);
                    var surface = SKSurface.Create(GrContext, renderTarget,
                        GRSurfaceOrigin.TopLeft,
                        _surface.IsRgba ? SKColorType.Rgba8888 : SKColorType.Bgra8888, SKColorSpace.CreateSrgb());

                    if (surface == null)
                    {
                        throw new InvalidOperationException(
                            "Surface can't be created with the provided render target");
                    }

                    success = true;

                    return new VulkanGpuSession(GrContext, renderTarget, surface, session);
                }
            }
            finally
            {
                if (!success)
                {
                    session.Dispose();
                }
            }
        }

        public bool IsCorrupted { get; }

        internal class VulkanGpuSession : ISkiaGpuRenderSession
        {
            private readonly GRBackendRenderTarget _backendRenderTarget;
            private readonly VulkanSurfaceRenderingSession _vulkanSession;

            public VulkanGpuSession(GRContext grContext,
                GRBackendRenderTarget backendRenderTarget,
                SKSurface surface,
                VulkanSurfaceRenderingSession vulkanSession)
            {
                GrContext = grContext;
                _backendRenderTarget = backendRenderTarget;
                SkSurface = surface;
                _vulkanSession = vulkanSession;

                SurfaceOrigin = GRSurfaceOrigin.TopLeft;
            }

            public void Dispose()
            {
                lock (_vulkanSession.Display.Lock)
                {
                    SkSurface.Canvas.Flush();

                    SkSurface.Dispose();
                    _backendRenderTarget.Dispose();
                    GrContext.Flush();

                    _vulkanSession.Dispose();
                }
            }

            public GRContext GrContext { get; }
            public SKSurface SkSurface { get; }
            public double ScaleFactor => _vulkanSession.Scaling;
            public GRSurfaceOrigin SurfaceOrigin { get; }
        }
    }
}
