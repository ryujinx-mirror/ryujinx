using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;

namespace Ryujinx.Ava.Ui.Vulkan
{
    public class VulkanInstance : IDisposable
    {
        private const string EngineName = "Avalonia Vulkan";

        private VulkanInstance(Instance apiHandle, Vk api)
        {
            InternalHandle = apiHandle;
            Api = api;
        }

        public IntPtr Handle => InternalHandle.Handle;

        internal Instance InternalHandle { get; }
        public Vk Api { get; }

        internal static IEnumerable<string> RequiredInstanceExtensions
        {
            get
            {
                yield return "VK_KHR_surface";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    yield return "VK_KHR_xlib_surface";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    yield return "VK_KHR_win32_surface";
                }
            }
        }

        public void Dispose()
        {
            Api?.DestroyInstance(InternalHandle, Span<AllocationCallbacks>.Empty);
            Api?.Dispose();
        }

        internal static unsafe VulkanInstance Create(VulkanOptions options)
        {
            var api = Vk.GetApi();
            var applicationName = Marshal.StringToHGlobalAnsi(options.ApplicationName);
            var engineName = Marshal.StringToHGlobalAnsi(EngineName);
            var enabledExtensions = new List<string>(options.InstanceExtensions);

            enabledExtensions.AddRange(RequiredInstanceExtensions);

            var applicationInfo = new ApplicationInfo
            {
                PApplicationName = (byte*)applicationName,
                ApiVersion = new Version32((uint)options.VulkanVersion.Major, (uint)options.VulkanVersion.Minor,
                    (uint)options.VulkanVersion.Build),
                PEngineName = (byte*)engineName,
                EngineVersion = new Version32(1, 0, 0),
                ApplicationVersion = new Version32(1, 0, 0)
            };

            var enabledLayers = new HashSet<string>();

            if (options.UseDebug)
            {
                enabledExtensions.Add(ExtDebugUtils.ExtensionName);
                enabledExtensions.Add(ExtDebugReport.ExtensionName);
                if (IsLayerAvailable(api, "VK_LAYER_KHRONOS_validation"))
                    enabledLayers.Add("VK_LAYER_KHRONOS_validation");
            }

            foreach (var layer in options.EnabledLayers)
                enabledLayers.Add(layer);

            var ppEnabledExtensions = stackalloc IntPtr[enabledExtensions.Count];
            var ppEnabledLayers = stackalloc IntPtr[enabledLayers.Count];

            for (var i = 0; i < enabledExtensions.Count; i++)
                ppEnabledExtensions[i] = Marshal.StringToHGlobalAnsi(enabledExtensions[i]);

            var layers = enabledLayers.ToList();

            for (var i = 0; i < enabledLayers.Count; i++)
                ppEnabledLayers[i] = Marshal.StringToHGlobalAnsi(layers[i]);

            var instanceCreateInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &applicationInfo,
                PpEnabledExtensionNames = (byte**)ppEnabledExtensions,
                PpEnabledLayerNames = (byte**)ppEnabledLayers,
                EnabledExtensionCount = (uint)enabledExtensions.Count,
                EnabledLayerCount = (uint)enabledLayers.Count
            };

            api.CreateInstance(in instanceCreateInfo, null, out var instance).ThrowOnError();

            Marshal.FreeHGlobal(applicationName);
            Marshal.FreeHGlobal(engineName);

            for (var i = 0; i < enabledExtensions.Count; i++) Marshal.FreeHGlobal(ppEnabledExtensions[i]);

            for (var i = 0; i < enabledLayers.Count; i++) Marshal.FreeHGlobal(ppEnabledLayers[i]);

            return new VulkanInstance(instance, api);
        }

        private static unsafe bool IsLayerAvailable(Vk api, string layerName)
        {
            uint layerPropertiesCount;

            api.EnumerateInstanceLayerProperties(&layerPropertiesCount, null).ThrowOnError();

            var layerProperties = new LayerProperties[layerPropertiesCount];

            fixed (LayerProperties* pLayerProperties = layerProperties)
            {
                api.EnumerateInstanceLayerProperties(&layerPropertiesCount, layerProperties).ThrowOnError();

                for (var i = 0; i < layerPropertiesCount; i++)
                {
                    var currentLayerName = Marshal.PtrToStringAnsi((IntPtr)pLayerProperties[i].LayerName);

                    if (currentLayerName == layerName) return true;
                }
            }

            return false;
        }
    }
}
