using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Vulkan
{
    public unsafe static class VulkanInitialization
    {
        private const uint InvalidIndex = uint.MaxValue;
        private const string AppName = "Ryujinx.Graphics.Vulkan";
        private const int QueuesCount = 2;

        public static string[] DesirableExtensions { get; } = new string[]
        {
            ExtConditionalRendering.ExtensionName,
            ExtExtendedDynamicState.ExtensionName,
            ExtTransformFeedback.ExtensionName,
            KhrDrawIndirectCount.ExtensionName,
            KhrPushDescriptor.ExtensionName,
            "VK_EXT_custom_border_color",
            "VK_EXT_descriptor_indexing", // Enabling this works around an issue with disposed buffer bindings on RADV.
            "VK_EXT_fragment_shader_interlock",
            "VK_EXT_index_type_uint8",
            "VK_EXT_robustness2",
            "VK_KHR_shader_float16_int8",
            "VK_EXT_shader_subgroup_ballot",
            "VK_EXT_subgroup_size_control",
            "VK_NV_geometry_shader_passthrough"
        };

        public static string[] RequiredExtensions { get; } = new string[]
        {
            KhrSwapchain.ExtensionName
        };

        private static string[] _excludedMessages = new string[]
        {
            // NOTE: Done on purpose right now.
            "UNASSIGNED-CoreValidation-Shader-OutputNotConsumed",
            // TODO: Figure out if fixable
            "VUID-vkCmdDrawIndexed-None-04584",
            // TODO: Might be worth looking into making this happy to possibly optimize copies.
            "UNASSIGNED-CoreValidation-DrawState-InvalidImageLayout",
            // TODO: Fix this, it's causing too much noise right now.
            "VUID-VkSubpassDependency-srcSubpass-00867"
        };

        internal static Instance CreateInstance(Vk api, GraphicsDebugLevel logLevel, string[] requiredExtensions, out ExtDebugUtils debugUtils, out DebugUtilsMessengerEXT debugUtilsMessenger)
        {
            var enabledLayers = new List<string>();

            void AddAvailableLayer(string layerName)
            {
                uint layerPropertiesCount;

                api.EnumerateInstanceLayerProperties(&layerPropertiesCount, null).ThrowOnError();

                LayerProperties[] layerProperties = new LayerProperties[layerPropertiesCount];

                fixed (LayerProperties* pLayerProperties = layerProperties)
                {
                    api.EnumerateInstanceLayerProperties(&layerPropertiesCount, layerProperties).ThrowOnError();

                    for (int i = 0; i < layerPropertiesCount; i++)
                    {
                        string currentLayerName = Marshal.PtrToStringAnsi((IntPtr)pLayerProperties[i].LayerName);

                        if (currentLayerName == layerName)
                        {
                            enabledLayers.Add(layerName);
                            return;
                        }
                    }
                }

                Logger.Warning?.Print(LogClass.Gpu, $"Missing layer {layerName}");
            }

            if (logLevel != GraphicsDebugLevel.None)
            {
                AddAvailableLayer("VK_LAYER_KHRONOS_validation");
            }

            var enabledExtensions = requiredExtensions.Append(ExtDebugUtils.ExtensionName).ToArray();

            var appName = Marshal.StringToHGlobalAnsi(AppName);

            var applicationInfo = new ApplicationInfo
            {
                PApplicationName = (byte*)appName,
                ApplicationVersion = 1,
                PEngineName = (byte*)appName,
                EngineVersion = 1,
                ApiVersion = Vk.Version12.Value
            };

            IntPtr* ppEnabledExtensions = stackalloc IntPtr[enabledExtensions.Length];
            IntPtr* ppEnabledLayers = stackalloc IntPtr[enabledLayers.Count];

            for (int i = 0; i < enabledExtensions.Length; i++)
            {
                ppEnabledExtensions[i] = Marshal.StringToHGlobalAnsi(enabledExtensions[i]);
            }

            for (int i = 0; i < enabledLayers.Count; i++)
            {
                ppEnabledLayers[i] = Marshal.StringToHGlobalAnsi(enabledLayers[i]);
            }

            var instanceCreateInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &applicationInfo,
                PpEnabledExtensionNames = (byte**)ppEnabledExtensions,
                PpEnabledLayerNames = (byte**)ppEnabledLayers,
                EnabledExtensionCount = (uint)enabledExtensions.Length,
                EnabledLayerCount = (uint)enabledLayers.Count
            };

            api.CreateInstance(in instanceCreateInfo, null, out var instance).ThrowOnError();

            Marshal.FreeHGlobal(appName);

            for (int i = 0; i < enabledExtensions.Length; i++)
            {
                Marshal.FreeHGlobal(ppEnabledExtensions[i]);
            }

            for (int i = 0; i < enabledLayers.Count; i++)
            {
                Marshal.FreeHGlobal(ppEnabledLayers[i]);
            }

            CreateDebugMessenger(api, logLevel, instance, out debugUtils, out debugUtilsMessenger);

            return instance;
        }

        private unsafe static uint DebugMessenger(
            DebugUtilsMessageSeverityFlagsEXT messageSeverity,
            DebugUtilsMessageTypeFlagsEXT messageTypes,
            DebugUtilsMessengerCallbackDataEXT* pCallbackData,
            void* pUserData)
        {
            var msg = Marshal.PtrToStringAnsi((IntPtr)pCallbackData->PMessage);

            foreach (string excludedMessagePart in _excludedMessages)
            {
                if (msg.Contains(excludedMessagePart))
                {
                    return 0;
                }
            }

            if (messageSeverity.HasFlag(DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt))
            {
                Logger.Error?.Print(LogClass.Gpu, msg);
                //throw new Exception(msg);
            }
            else if (messageSeverity.HasFlag(DebugUtilsMessageSeverityFlagsEXT.WarningBitExt))
            {
                Logger.Warning?.Print(LogClass.Gpu, msg);
            }
            else if (messageSeverity.HasFlag(DebugUtilsMessageSeverityFlagsEXT.InfoBitExt))
            {
                Logger.Info?.Print(LogClass.Gpu, msg);
            }
            else // if (messageSeverity.HasFlag(DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt))
            {
                Logger.Debug?.Print(LogClass.Gpu, msg);
            }

            return 0;
        }

        internal static PhysicalDevice FindSuitablePhysicalDevice(Vk api, Instance instance, SurfaceKHR surface, string preferredGpuId)
        {
            uint physicalDeviceCount;

            api.EnumeratePhysicalDevices(instance, &physicalDeviceCount, null).ThrowOnError();

            PhysicalDevice[] physicalDevices = new PhysicalDevice[physicalDeviceCount];

            fixed (PhysicalDevice* pPhysicalDevices = physicalDevices)
            {
                api.EnumeratePhysicalDevices(instance, &physicalDeviceCount, pPhysicalDevices).ThrowOnError();
            }

            // First we try to pick the the user preferred GPU.
            for (int i = 0; i < physicalDevices.Length; i++)
            {
                if (IsPreferredAndSuitableDevice(api, physicalDevices[i], surface, preferredGpuId))
                {
                    return physicalDevices[i];
                }
            }

            // If we fail to do that, just use the first compatible GPU.
            for (int i = 0; i < physicalDevices.Length; i++)
            {
                if (IsSuitableDevice(api, physicalDevices[i], surface))
                {
                    return physicalDevices[i];
                }
            }

            throw new VulkanException("Initialization failed, none of the available GPUs meets the minimum requirements.");
        }

        internal static DeviceInfo[] GetSuitablePhysicalDevices(Vk api)
        {
            var appName = Marshal.StringToHGlobalAnsi(AppName);

            var applicationInfo = new ApplicationInfo
            {
                PApplicationName = (byte*)appName,
                ApplicationVersion = 1,
                PEngineName = (byte*)appName,
                EngineVersion = 1,
                ApiVersion = Vk.Version12.Value
            };

            var instanceCreateInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &applicationInfo,
                PpEnabledExtensionNames = null,
                PpEnabledLayerNames = null,
                EnabledExtensionCount = 0,
                EnabledLayerCount = 0
            };

            api.CreateInstance(in instanceCreateInfo, null, out var instance).ThrowOnError();

            Marshal.FreeHGlobal(appName);

            uint physicalDeviceCount;

            api.EnumeratePhysicalDevices(instance, &physicalDeviceCount, null).ThrowOnError();

            PhysicalDevice[] physicalDevices = new PhysicalDevice[physicalDeviceCount];

            fixed (PhysicalDevice* pPhysicalDevices = physicalDevices)
            {
                api.EnumeratePhysicalDevices(instance, &physicalDeviceCount, pPhysicalDevices).ThrowOnError();
            }

            DeviceInfo[] devices = new DeviceInfo[physicalDevices.Length];

            for (int i = 0; i < physicalDevices.Length; i++)
            {
                var physicalDevice = physicalDevices[i];
                api.GetPhysicalDeviceProperties(physicalDevice, out var properties);

                devices[i] = new DeviceInfo(
                    StringFromIdPair(properties.VendorID, properties.DeviceID),
                    VendorUtils.GetNameFromId(properties.VendorID),
                    Marshal.PtrToStringAnsi((IntPtr)properties.DeviceName),
                    properties.DeviceType == PhysicalDeviceType.DiscreteGpu);
            }

            api.DestroyInstance(instance, null);

            return devices;
        }

        public static string StringFromIdPair(uint vendorId, uint deviceId)
        {
            return $"0x{vendorId:X}_0x{deviceId:X}";
        }

        private static bool IsPreferredAndSuitableDevice(Vk api, PhysicalDevice physicalDevice, SurfaceKHR surface, string preferredGpuId)
        {
            api.GetPhysicalDeviceProperties(physicalDevice, out var properties);

            if (StringFromIdPair(properties.VendorID, properties.DeviceID) != preferredGpuId)
            {
                return false;
            }

            return IsSuitableDevice(api, physicalDevice, surface);
        }

        private static bool IsSuitableDevice(Vk api, PhysicalDevice physicalDevice, SurfaceKHR surface)
        {
            int extensionMatches = 0;
            uint propertiesCount;

            api.EnumerateDeviceExtensionProperties(physicalDevice, (byte*)null, &propertiesCount, null).ThrowOnError();

            ExtensionProperties[] extensionProperties = new ExtensionProperties[propertiesCount];

            fixed (ExtensionProperties* pExtensionProperties = extensionProperties)
            {
                api.EnumerateDeviceExtensionProperties(physicalDevice, (byte*)null, &propertiesCount, pExtensionProperties).ThrowOnError();

                for (int i = 0; i < propertiesCount; i++)
                {
                    string extensionName = Marshal.PtrToStringAnsi((IntPtr)pExtensionProperties[i].ExtensionName);

                    if (RequiredExtensions.Contains(extensionName))
                    {
                        extensionMatches++;
                    }
                }
            }

            return extensionMatches == RequiredExtensions.Length && FindSuitableQueueFamily(api, physicalDevice, surface, out _) != InvalidIndex;
        }

        internal static uint FindSuitableQueueFamily(Vk api, PhysicalDevice physicalDevice, SurfaceKHR surface, out uint queueCount)
        {
            const QueueFlags RequiredFlags = QueueFlags.GraphicsBit | QueueFlags.ComputeBit;

            var khrSurface = new KhrSurface(api.Context);

            uint propertiesCount;

            api.GetPhysicalDeviceQueueFamilyProperties(physicalDevice, &propertiesCount, null);

            QueueFamilyProperties[] properties = new QueueFamilyProperties[propertiesCount];

            fixed (QueueFamilyProperties* pProperties = properties)
            {
                api.GetPhysicalDeviceQueueFamilyProperties(physicalDevice, &propertiesCount, pProperties);
            }

            for (uint index = 0; index < propertiesCount; index++)
            {
                var queueFlags = properties[index].QueueFlags;

                khrSurface.GetPhysicalDeviceSurfaceSupport(physicalDevice, index, surface, out var surfaceSupported).ThrowOnError();

                if (queueFlags.HasFlag(RequiredFlags) && surfaceSupported)
                {
                    queueCount = properties[index].QueueCount;
                    return index;
                }
            }

            queueCount = 0;
            return InvalidIndex;
        }

        public static Device CreateDevice(Vk api, PhysicalDevice physicalDevice, uint queueFamilyIndex, string[] supportedExtensions, uint queueCount)
        {
            if (queueCount > QueuesCount)
            {
                queueCount = QueuesCount;
            }

            float* queuePriorities = stackalloc float[(int)queueCount];

            for (int i = 0; i < queueCount; i++)
            {
                queuePriorities[i] = 1f;
            }

            var queueCreateInfo = new DeviceQueueCreateInfo()
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = queueFamilyIndex,
                QueueCount = queueCount,
                PQueuePriorities = queuePriorities
            };

            api.GetPhysicalDeviceProperties(physicalDevice, out var properties);
            bool useRobustBufferAccess = VendorUtils.FromId(properties.VendorID) == Vendor.Nvidia;

            PhysicalDeviceFeatures2 features2 = new PhysicalDeviceFeatures2()
            {
                SType = StructureType.PhysicalDeviceFeatures2
            };

            PhysicalDeviceCustomBorderColorFeaturesEXT featuresCustomBorderColorSupported = new PhysicalDeviceCustomBorderColorFeaturesEXT()
            {
                SType = StructureType.PhysicalDeviceCustomBorderColorFeaturesExt
            };

            if (supportedExtensions.Contains("VK_EXT_custom_border_color"))
            {
                features2.PNext = &featuresCustomBorderColorSupported;
            }

            PhysicalDeviceRobustness2FeaturesEXT supportedFeaturesRobustness2 = new PhysicalDeviceRobustness2FeaturesEXT()
            {
                SType = StructureType.PhysicalDeviceRobustness2FeaturesExt
            };

            if (supportedExtensions.Contains("VK_EXT_robustness2"))
            {
                supportedFeaturesRobustness2.PNext = features2.PNext;

                features2.PNext = &supportedFeaturesRobustness2;
            }

            api.GetPhysicalDeviceFeatures2(physicalDevice, &features2);

            var supportedFeatures = features2.Features;

            var features = new PhysicalDeviceFeatures()
            {
                DepthBiasClamp = true,
                DepthClamp = true,
                DualSrcBlend = true,
                FragmentStoresAndAtomics = true,
                GeometryShader = supportedFeatures.GeometryShader,
                ImageCubeArray = true,
                IndependentBlend = true,
                LogicOp = supportedFeatures.LogicOp,
                MultiViewport = true,
                OcclusionQueryPrecise = supportedFeatures.OcclusionQueryPrecise,
                PipelineStatisticsQuery = supportedFeatures.PipelineStatisticsQuery,
                SamplerAnisotropy = true,
                ShaderClipDistance = true,
                ShaderFloat64 = supportedFeatures.ShaderFloat64,
                ShaderImageGatherExtended = true,
                ShaderStorageImageMultisample = supportedFeatures.ShaderStorageImageMultisample,
                // ShaderStorageImageReadWithoutFormat = true,
                // ShaderStorageImageWriteWithoutFormat = true,
                TessellationShader = true,
                VertexPipelineStoresAndAtomics = true,
                RobustBufferAccess = useRobustBufferAccess
            };

            void* pExtendedFeatures = null;

            var featuresTransformFeedback = new PhysicalDeviceTransformFeedbackFeaturesEXT()
            {
                SType = StructureType.PhysicalDeviceTransformFeedbackFeaturesExt,
                PNext = pExtendedFeatures,
                TransformFeedback = true
            };

            pExtendedFeatures = &featuresTransformFeedback;

            if (supportedExtensions.Contains("VK_EXT_robustness2"))
            {
                var featuresRobustness2 = new PhysicalDeviceRobustness2FeaturesEXT()
                {
                    SType = StructureType.PhysicalDeviceRobustness2FeaturesExt,
                    PNext = pExtendedFeatures,
                    NullDescriptor = supportedFeaturesRobustness2.NullDescriptor
                };

                pExtendedFeatures = &featuresRobustness2;
            }

            var featuresExtendedDynamicState = new PhysicalDeviceExtendedDynamicStateFeaturesEXT()
            {
                SType = StructureType.PhysicalDeviceExtendedDynamicStateFeaturesExt,
                PNext = pExtendedFeatures,
                ExtendedDynamicState = supportedExtensions.Contains(ExtExtendedDynamicState.ExtensionName)
            };

            pExtendedFeatures = &featuresExtendedDynamicState;

            var featuresVk11 = new PhysicalDeviceVulkan11Features()
            {
                SType = StructureType.PhysicalDeviceVulkan11Features,
                PNext = pExtendedFeatures,
                ShaderDrawParameters = true
            };

            pExtendedFeatures = &featuresVk11;

            var featuresVk12 = new PhysicalDeviceVulkan12Features()
            {
                SType = StructureType.PhysicalDeviceVulkan12Features,
                PNext = pExtendedFeatures,
                DescriptorIndexing = supportedExtensions.Contains("VK_EXT_descriptor_indexing"),
                DrawIndirectCount = supportedExtensions.Contains(KhrDrawIndirectCount.ExtensionName),
                UniformBufferStandardLayout = supportedExtensions.Contains("VK_KHR_uniform_buffer_standard_layout")
            };

            pExtendedFeatures = &featuresVk12;

            PhysicalDeviceIndexTypeUint8FeaturesEXT featuresIndexU8;

            if (supportedExtensions.Contains("VK_EXT_index_type_uint8"))
            {
                featuresIndexU8 = new PhysicalDeviceIndexTypeUint8FeaturesEXT()
                {
                    SType = StructureType.PhysicalDeviceIndexTypeUint8FeaturesExt,
                    PNext = pExtendedFeatures,
                    IndexTypeUint8 = true
                };

                pExtendedFeatures = &featuresIndexU8;
            }

            PhysicalDeviceFragmentShaderInterlockFeaturesEXT featuresFragmentShaderInterlock;

            if (supportedExtensions.Contains("VK_EXT_fragment_shader_interlock"))
            {
                featuresFragmentShaderInterlock = new PhysicalDeviceFragmentShaderInterlockFeaturesEXT()
                {
                    SType = StructureType.PhysicalDeviceFragmentShaderInterlockFeaturesExt,
                    PNext = pExtendedFeatures,
                    FragmentShaderPixelInterlock = true
                };

                pExtendedFeatures = &featuresFragmentShaderInterlock;
            }

            PhysicalDeviceSubgroupSizeControlFeaturesEXT featuresSubgroupSizeControl;

            if (supportedExtensions.Contains("VK_EXT_subgroup_size_control"))
            {
                featuresSubgroupSizeControl = new PhysicalDeviceSubgroupSizeControlFeaturesEXT()
                {
                    SType = StructureType.PhysicalDeviceSubgroupSizeControlFeaturesExt,
                    PNext = pExtendedFeatures,
                    SubgroupSizeControl = true
                };

                pExtendedFeatures = &featuresSubgroupSizeControl;
            }

            PhysicalDeviceCustomBorderColorFeaturesEXT featuresCustomBorderColor;

            if (supportedExtensions.Contains("VK_EXT_custom_border_color") &&
                featuresCustomBorderColorSupported.CustomBorderColors &&
                featuresCustomBorderColorSupported.CustomBorderColorWithoutFormat)
            {
                featuresCustomBorderColor = new PhysicalDeviceCustomBorderColorFeaturesEXT()
                {
                    SType = StructureType.PhysicalDeviceCustomBorderColorFeaturesExt,
                    PNext = pExtendedFeatures,
                    CustomBorderColors = true,
                    CustomBorderColorWithoutFormat = true,
                };

                pExtendedFeatures = &featuresCustomBorderColor;
            }

            var enabledExtensions = RequiredExtensions.Union(DesirableExtensions.Intersect(supportedExtensions)).ToArray();

            IntPtr* ppEnabledExtensions = stackalloc IntPtr[enabledExtensions.Length];

            for (int i = 0; i < enabledExtensions.Length; i++)
            {
                ppEnabledExtensions[i] = Marshal.StringToHGlobalAnsi(enabledExtensions[i]);
            }

            var deviceCreateInfo = new DeviceCreateInfo()
            {
                SType = StructureType.DeviceCreateInfo,
                PNext = pExtendedFeatures,
                QueueCreateInfoCount = 1,
                PQueueCreateInfos = &queueCreateInfo,
                PpEnabledExtensionNames = (byte**)ppEnabledExtensions,
                EnabledExtensionCount = (uint)enabledExtensions.Length,
                PEnabledFeatures = &features
            };

            api.CreateDevice(physicalDevice, in deviceCreateInfo, null, out var device).ThrowOnError();

            for (int i = 0; i < enabledExtensions.Length; i++)
            {
                Marshal.FreeHGlobal(ppEnabledExtensions[i]);
            }

            return device;
        }

        public static string[] GetSupportedExtensions(Vk api, PhysicalDevice physicalDevice)
        {
            uint propertiesCount;

            api.EnumerateDeviceExtensionProperties(physicalDevice, (byte*)null, &propertiesCount, null).ThrowOnError();

            ExtensionProperties[] extensionProperties = new ExtensionProperties[propertiesCount];

            fixed (ExtensionProperties* pExtensionProperties = extensionProperties)
            {
                api.EnumerateDeviceExtensionProperties(physicalDevice, (byte*)null, &propertiesCount, pExtensionProperties).ThrowOnError();
            }

            return extensionProperties.Select(x => Marshal.PtrToStringAnsi((IntPtr)x.ExtensionName)).ToArray();
        }

        internal static CommandBufferPool CreateCommandBufferPool(Vk api, Device device, Queue queue, object queueLock, uint queueFamilyIndex)
        {
            return new CommandBufferPool(api, device, queue, queueLock, queueFamilyIndex);
        }

        internal unsafe static void CreateDebugMessenger(
            Vk api,
            GraphicsDebugLevel logLevel,
            Instance instance,
            out ExtDebugUtils debugUtils,
            out DebugUtilsMessengerEXT debugUtilsMessenger)
        {
            debugUtils = default;

            if (logLevel != GraphicsDebugLevel.None)
            {
                if (!api.TryGetInstanceExtension(instance, out debugUtils))
                {
                    debugUtilsMessenger = default;
                    return;
                }

                var filterLogType = logLevel switch
                {
                    GraphicsDebugLevel.Error => DebugUtilsMessageTypeFlagsEXT.ValidationBitExt,
                    GraphicsDebugLevel.Slowdowns => DebugUtilsMessageTypeFlagsEXT.ValidationBitExt |
                                                    DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt,
                    GraphicsDebugLevel.All => DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                                              DebugUtilsMessageTypeFlagsEXT.ValidationBitExt |
                                              DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt,
                    _ => throw new ArgumentException($"Invalid log level \"{logLevel}\".")
                };

                var filterLogSeverity = logLevel switch
                {
                    GraphicsDebugLevel.Error => DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt,
                    GraphicsDebugLevel.Slowdowns => DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt |
                                                    DebugUtilsMessageSeverityFlagsEXT.WarningBitExt,
                    GraphicsDebugLevel.All => DebugUtilsMessageSeverityFlagsEXT.InfoBitExt |
                                              DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                                              DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt |
                                              DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt,
                    _ => throw new ArgumentException($"Invalid log level \"{logLevel}\".")
                };

                var debugUtilsMessengerCreateInfo = new DebugUtilsMessengerCreateInfoEXT()
                {
                    SType = StructureType.DebugUtilsMessengerCreateInfoExt,
                    MessageType = filterLogType,
                    MessageSeverity = filterLogSeverity,
                    PfnUserCallback = new PfnDebugUtilsMessengerCallbackEXT(DebugMessenger)
                };

                debugUtils.CreateDebugUtilsMessenger(instance, in debugUtilsMessengerCreateInfo, null, out debugUtilsMessenger).ThrowOnError();
            }
            else
            {
                debugUtilsMessenger = default;
            }
        }
    }
}
