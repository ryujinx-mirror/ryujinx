using Avalonia;
using Ryujinx.Ava.Ui.Vulkan.Surfaces;
using Ryujinx.Graphics.Vulkan;
using Silk.NET.Vulkan;
using System;

namespace Ryujinx.Ava.Ui.Vulkan
{
    internal class VulkanPlatformInterface : IDisposable
    {
        private static VulkanOptions _options;

        private VulkanPlatformInterface(VulkanInstance instance)
        {
            Instance = instance;
            Api = instance.Api;
        }

        public VulkanPhysicalDevice PhysicalDevice { get; private set; }
        public VulkanInstance Instance { get; }
        public VulkanDevice Device { get; set; }
        public Vk Api { get; private set; }
        public VulkanSurfaceRenderTarget MainSurface { get; set; }

        public void Dispose()
        {
            Device?.Dispose();
            Instance?.Dispose();
            Api?.Dispose();
        }

        private static VulkanPlatformInterface TryCreate()
        {
            _options = AvaloniaLocator.Current.GetService<VulkanOptions>() ?? new VulkanOptions();

            var instance = VulkanInstance.Create(_options);

            return new VulkanPlatformInterface(instance);
        }

        public static bool TryInitialize()
        {
            var feature = TryCreate();
            if (feature != null)
            {
                AvaloniaLocator.CurrentMutable.Bind<VulkanPlatformInterface>().ToConstant(feature);
                return true;
            }

            return false;
        }

        public VulkanSurfaceRenderTarget CreateRenderTarget(IVulkanPlatformSurface platformSurface)
        {
            var surface = VulkanSurface.CreateSurface(Instance, platformSurface);

            if (Device == null)
            {
                PhysicalDevice = VulkanPhysicalDevice.FindSuitablePhysicalDevice(Instance, surface, _options.PreferDiscreteGpu, _options.PreferredDevice);
                var device = VulkanInitialization.CreateDevice(Instance.Api,
                                                               PhysicalDevice.InternalHandle,
                                                               PhysicalDevice.QueueFamilyIndex,
                                                               VulkanInitialization.GetSupportedExtensions(Instance.Api, PhysicalDevice.InternalHandle),
                                                               PhysicalDevice.QueueCount);

                Device = new VulkanDevice(device, PhysicalDevice, Instance.Api);
            }

            var renderTarget = new VulkanSurfaceRenderTarget(this, surface);

            if (MainSurface == null && surface != null)
            {
                MainSurface = renderTarget;
                MainSurface.Display.ChangeVSyncMode(false);
            }

            return renderTarget;
        }
    }
}
