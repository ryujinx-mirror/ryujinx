using Ryujinx.Graphics.Vulkan;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ryujinx.Ava.Ui.Vulkan
{
    public unsafe class VulkanPhysicalDevice
    {
        private VulkanPhysicalDevice(PhysicalDevice apiHandle, Vk api, uint queueCount, uint queueFamilyIndex)
        {
            InternalHandle = apiHandle;
            Api = api;
            QueueCount = queueCount;
            QueueFamilyIndex = queueFamilyIndex;

            api.GetPhysicalDeviceProperties(apiHandle, out var properties);

            DeviceName = Marshal.PtrToStringAnsi((IntPtr)properties.DeviceName);
            DeviceId = VulkanInitialization.StringFromIdPair(properties.VendorID, properties.DeviceID);

            var version = (Version32)properties.ApiVersion;
            ApiVersion = new Version((int)version.Major, (int)version.Minor, 0, (int)version.Patch);
        }

        internal PhysicalDevice InternalHandle { get; }
        internal Vk Api { get; }
        public uint QueueCount { get; }
        public uint QueueFamilyIndex { get; }
        public IntPtr Handle => InternalHandle.Handle;

        public string DeviceName { get; }
        public string DeviceId { get; }
        public Version ApiVersion { get; }
        public static Dictionary<PhysicalDevice, PhysicalDeviceProperties> PhysicalDevices { get; private set; }
        public static IEnumerable<KeyValuePair<PhysicalDevice, PhysicalDeviceProperties>> SuitableDevices { get; private set; }

        internal static void SelectAvailableDevices(VulkanInstance instance,
            VulkanSurface surface, bool preferDiscreteGpu, string preferredDevice)
        {
            uint physicalDeviceCount;

            instance.Api.EnumeratePhysicalDevices(instance.InternalHandle, &physicalDeviceCount, null).ThrowOnError();

            var physicalDevices = new PhysicalDevice[physicalDeviceCount];

            fixed (PhysicalDevice* pPhysicalDevices = physicalDevices)
            {
                instance.Api.EnumeratePhysicalDevices(instance.InternalHandle, &physicalDeviceCount, pPhysicalDevices)
                    .ThrowOnError();
            }

            PhysicalDevices = new Dictionary<PhysicalDevice, PhysicalDeviceProperties>();

            foreach (var physicalDevice in physicalDevices)
            {
                instance.Api.GetPhysicalDeviceProperties(physicalDevice, out var properties);
                PhysicalDevices.Add(physicalDevice, properties);
            }

            SuitableDevices = PhysicalDevices.Where(x => IsSuitableDevice(
                instance.Api,
                x.Key,
                x.Value,
                surface.ApiHandle,
                out _,
                out _));
        }

        internal static VulkanPhysicalDevice FindSuitablePhysicalDevice(VulkanInstance instance,
            VulkanSurface surface, bool preferDiscreteGpu, string preferredDevice)
        {
            SelectAvailableDevices(instance, surface, preferDiscreteGpu, preferredDevice);

            uint queueFamilyIndex = 0;
            uint queueCount = 0;

            if (!string.IsNullOrWhiteSpace(preferredDevice))
            {
                var physicalDevice = SuitableDevices.FirstOrDefault(x => VulkanInitialization.StringFromIdPair(x.Value.VendorID, x.Value.DeviceID) == preferredDevice);

                queueFamilyIndex = FindSuitableQueueFamily(instance.Api, physicalDevice.Key,
                    surface.ApiHandle, out queueCount);
                if (queueFamilyIndex != int.MaxValue)
                {
                    return new VulkanPhysicalDevice(physicalDevice.Key, instance.Api, queueCount, queueFamilyIndex);
                }
            }

            if (preferDiscreteGpu)
            {
                var discreteGpus = SuitableDevices.Where(p => p.Value.DeviceType == PhysicalDeviceType.DiscreteGpu);

                foreach (var gpu in discreteGpus)
                {
                    queueFamilyIndex = FindSuitableQueueFamily(instance.Api, gpu.Key,
                    surface.ApiHandle, out queueCount);
                    if (queueFamilyIndex != int.MaxValue)
                    {
                        return new VulkanPhysicalDevice(gpu.Key, instance.Api, queueCount, queueFamilyIndex);
                    }
                }
            }

            foreach (var physicalDevice in SuitableDevices)
            {
                queueFamilyIndex = FindSuitableQueueFamily(instance.Api, physicalDevice.Key,
                     surface.ApiHandle, out queueCount);
                if (queueFamilyIndex != int.MaxValue)
                {
                    return new VulkanPhysicalDevice(physicalDevice.Key, instance.Api, queueCount, queueFamilyIndex);
                }
            }

            throw new Exception("No suitable physical device found");
        }

        private static unsafe bool IsSuitableDevice(Vk api, PhysicalDevice physicalDevice, PhysicalDeviceProperties properties, SurfaceKHR surface,
            out uint queueCount, out uint familyIndex)
        {
            queueCount = 0;
            familyIndex = 0;

            if (properties.DeviceType == PhysicalDeviceType.Cpu) return false;

            var extensionMatches = 0;
            uint propertiesCount;

            api.EnumerateDeviceExtensionProperties(physicalDevice, (byte*)null, &propertiesCount, null).ThrowOnError();

            var extensionProperties = new ExtensionProperties[propertiesCount];

            fixed (ExtensionProperties* pExtensionProperties = extensionProperties)
            {
                api.EnumerateDeviceExtensionProperties(
                    physicalDevice,
                    (byte*)null,
                    &propertiesCount,
                    pExtensionProperties).ThrowOnError();

                for (var i = 0; i < propertiesCount; i++)
                {
                    var extensionName = Marshal.PtrToStringAnsi((IntPtr)pExtensionProperties[i].ExtensionName);

                    if (VulkanInitialization.RequiredExtensions.Contains(extensionName))
                    {
                        extensionMatches++;
                    }
                }
            }

            if (extensionMatches == VulkanInitialization.RequiredExtensions.Length)
            {
                familyIndex = FindSuitableQueueFamily(api, physicalDevice, surface, out queueCount);

                return familyIndex != uint.MaxValue;
            }

            return false;
        }

        internal unsafe string[] GetSupportedExtensions()
        {
            uint propertiesCount;

            Api.EnumerateDeviceExtensionProperties(InternalHandle, (byte*)null, &propertiesCount, null).ThrowOnError();

            var extensionProperties = new ExtensionProperties[propertiesCount];

            fixed (ExtensionProperties* pExtensionProperties = extensionProperties)
            {
                Api.EnumerateDeviceExtensionProperties(InternalHandle, (byte*)null, &propertiesCount, pExtensionProperties)
                    .ThrowOnError();
            }

            return extensionProperties.Select(x => Marshal.PtrToStringAnsi((IntPtr)x.ExtensionName)).ToArray();
        }

        private static uint FindSuitableQueueFamily(Vk api, PhysicalDevice physicalDevice, SurfaceKHR surface,
            out uint queueCount)
        {
            const QueueFlags RequiredFlags = QueueFlags.QueueGraphicsBit | QueueFlags.QueueComputeBit;

            var khrSurface = new KhrSurface(api.Context);

            uint propertiesCount;

            api.GetPhysicalDeviceQueueFamilyProperties(physicalDevice, &propertiesCount, null);

            var properties = new QueueFamilyProperties[propertiesCount];

            fixed (QueueFamilyProperties* pProperties = properties)
            {
                api.GetPhysicalDeviceQueueFamilyProperties(physicalDevice, &propertiesCount, pProperties);
            }

            for (uint index = 0; index < propertiesCount; index++)
            {
                var queueFlags = properties[index].QueueFlags;

                khrSurface.GetPhysicalDeviceSurfaceSupport(physicalDevice, index, surface, out var surfaceSupported)
                    .ThrowOnError();

                if (queueFlags.HasFlag(RequiredFlags) && surfaceSupported)
                {
                    queueCount = properties[index].QueueCount;
                    return index;
                }
            }

            queueCount = 0;
            return uint.MaxValue;
        }
    }
}
