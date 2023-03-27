using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Shader;
using Ryujinx.Graphics.Shader.Translation;
using Ryujinx.Graphics.Vulkan.MoltenVK;
using Ryujinx.Graphics.Vulkan.Queries;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Vulkan
{
    public sealed class VulkanRenderer : IRenderer
    {
        private Instance _instance;
        private SurfaceKHR _surface;
        private PhysicalDevice _physicalDevice;
        private Device _device;
        private WindowBase _window;

        private bool _initialized;

        internal FormatCapabilities FormatCapabilities { get; private set; }
        internal HardwareCapabilities Capabilities;

        internal Vk Api { get; private set; }
        internal KhrSurface SurfaceApi { get; private set; }
        internal KhrSwapchain SwapchainApi { get; private set; }
        internal ExtConditionalRendering ConditionalRenderingApi { get; private set; }
        internal ExtExtendedDynamicState ExtendedDynamicStateApi { get; private set; }
        internal KhrPushDescriptor PushDescriptorApi { get; private set; }
        internal ExtTransformFeedback TransformFeedbackApi { get; private set; }
        internal KhrDrawIndirectCount DrawIndirectCountApi { get; private set; }
        internal ExtDebugUtils DebugUtilsApi { get; private set; }

        internal uint QueueFamilyIndex { get; private set; }
        internal Queue Queue { get; private set; }
        internal Queue BackgroundQueue { get; private set; }
        internal object BackgroundQueueLock { get; private set; }
        internal object QueueLock { get; private set; }

        internal MemoryAllocator MemoryAllocator { get; private set; }
        internal CommandBufferPool CommandBufferPool { get; private set; }
        internal DescriptorSetManager DescriptorSetManager { get; private set; }
        internal PipelineLayoutCache PipelineLayoutCache { get; private set; }
        internal BackgroundResources BackgroundResources { get; private set; }
        internal Action<Action> InterruptAction { get; private set; }

        internal BufferManager BufferManager { get; private set; }

        internal HashSet<ShaderCollection> Shaders { get; }
        internal HashSet<ITexture> Textures { get; }
        internal HashSet<SamplerHolder> Samplers { get; }

        private Counters _counters;
        private SyncManager _syncManager;

        private PipelineFull _pipeline;
        private DebugUtilsMessengerEXT _debugUtilsMessenger;

        internal HelperShader HelperShader { get; private set; }
        internal PipelineFull PipelineInternal => _pipeline;

        public IPipeline Pipeline => _pipeline;

        public IWindow Window => _window;

        private readonly Func<Instance, Vk, SurfaceKHR> _getSurface;
        private readonly Func<string[]> _getRequiredExtensions;
        private readonly string _preferredGpuId;

        internal Vendor Vendor { get; private set; }
        internal bool IsAmdWindows { get; private set; }
        internal bool IsIntelWindows { get; private set; }
        internal bool IsAmdGcn { get; private set; }
        internal bool IsMoltenVk { get; private set; }
        internal bool IsTBDR { get; private set; }
        internal bool IsSharedMemory { get; private set; }
        public string GpuVendor { get; private set; }
        public string GpuRenderer { get; private set; }
        public string GpuVersion { get; private set; }

        public bool PreferThreading => true;

        public event EventHandler<ScreenCaptureImageInfo> ScreenCaptured;

        public VulkanRenderer(Func<Instance, Vk, SurfaceKHR> surfaceFunc, Func<string[]> requiredExtensionsFunc, string preferredGpuId)
        {
            _getSurface = surfaceFunc;
            _getRequiredExtensions = requiredExtensionsFunc;
            _preferredGpuId = preferredGpuId;
            Shaders = new HashSet<ShaderCollection>();
            Textures = new HashSet<ITexture>();
            Samplers = new HashSet<SamplerHolder>();

            if (OperatingSystem.IsMacOS())
            {
                MVKInitialization.Initialize();

                // Any device running on MacOS is using MoltenVK, even Intel and AMD vendors.
                IsMoltenVk = true;
            }
        }

        private unsafe void LoadFeatures(string[] supportedExtensions, uint maxQueueCount, uint queueFamilyIndex)
        {
            FormatCapabilities = new FormatCapabilities(Api, _physicalDevice);

            var supportedFeatures = Api.GetPhysicalDeviceFeature(_physicalDevice);

            if (Api.TryGetDeviceExtension(_instance, _device, out ExtConditionalRendering conditionalRenderingApi))
            {
                ConditionalRenderingApi = conditionalRenderingApi;
            }

            if (Api.TryGetDeviceExtension(_instance, _device, out ExtExtendedDynamicState extendedDynamicStateApi))
            {
                ExtendedDynamicStateApi = extendedDynamicStateApi;
            }

            if (Api.TryGetDeviceExtension(_instance, _device, out KhrPushDescriptor pushDescriptorApi))
            {
                PushDescriptorApi = pushDescriptorApi;
            }

            if (Api.TryGetDeviceExtension(_instance, _device, out ExtTransformFeedback transformFeedbackApi))
            {
                TransformFeedbackApi = transformFeedbackApi;
            }

            if (Api.TryGetDeviceExtension(_instance, _device, out KhrDrawIndirectCount drawIndirectCountApi))
            {
                DrawIndirectCountApi = drawIndirectCountApi;
            }

            if (maxQueueCount >= 2)
            {
                Api.GetDeviceQueue(_device, queueFamilyIndex, 1, out var backgroundQueue);
                BackgroundQueue = backgroundQueue;
                BackgroundQueueLock = new object();
            }

            PhysicalDeviceProperties2 properties2 = new PhysicalDeviceProperties2()
            {
                SType = StructureType.PhysicalDeviceProperties2
            };

            PhysicalDeviceBlendOperationAdvancedPropertiesEXT propertiesBlendOperationAdvanced = new PhysicalDeviceBlendOperationAdvancedPropertiesEXT()
            {
                SType = StructureType.PhysicalDeviceBlendOperationAdvancedPropertiesExt
            };

            bool supportsBlendOperationAdvanced = supportedExtensions.Contains("VK_EXT_blend_operation_advanced");

            if (supportsBlendOperationAdvanced)
            {
                propertiesBlendOperationAdvanced.PNext = properties2.PNext;
                properties2.PNext = &propertiesBlendOperationAdvanced;
            }

            PhysicalDeviceSubgroupSizeControlPropertiesEXT propertiesSubgroupSizeControl = new PhysicalDeviceSubgroupSizeControlPropertiesEXT()
            {
                SType = StructureType.PhysicalDeviceSubgroupSizeControlPropertiesExt
            };

            bool supportsSubgroupSizeControl = supportedExtensions.Contains("VK_EXT_subgroup_size_control");

            if (supportsSubgroupSizeControl)
            {
                properties2.PNext = &propertiesSubgroupSizeControl;
            }

            bool supportsTransformFeedback = supportedExtensions.Contains(ExtTransformFeedback.ExtensionName);

            PhysicalDeviceTransformFeedbackPropertiesEXT propertiesTransformFeedback = new PhysicalDeviceTransformFeedbackPropertiesEXT()
            {
                SType = StructureType.PhysicalDeviceTransformFeedbackPropertiesExt
            };

            if (supportsTransformFeedback)
            {
                propertiesTransformFeedback.PNext = properties2.PNext;
                properties2.PNext = &propertiesTransformFeedback;
            }

            PhysicalDevicePortabilitySubsetPropertiesKHR propertiesPortabilitySubset = new PhysicalDevicePortabilitySubsetPropertiesKHR()
            {
                SType = StructureType.PhysicalDevicePortabilitySubsetPropertiesKhr
            };

            PhysicalDeviceFeatures2 features2 = new PhysicalDeviceFeatures2()
            {
                SType = StructureType.PhysicalDeviceFeatures2
            };

            PhysicalDevicePrimitiveTopologyListRestartFeaturesEXT featuresPrimitiveTopologyListRestart = new PhysicalDevicePrimitiveTopologyListRestartFeaturesEXT()
            {
                SType = StructureType.PhysicalDevicePrimitiveTopologyListRestartFeaturesExt
            };

            PhysicalDeviceRobustness2FeaturesEXT featuresRobustness2 = new PhysicalDeviceRobustness2FeaturesEXT()
            {
                SType = StructureType.PhysicalDeviceRobustness2FeaturesExt
            };

            PhysicalDeviceShaderFloat16Int8FeaturesKHR featuresShaderInt8 = new PhysicalDeviceShaderFloat16Int8FeaturesKHR()
            {
                SType = StructureType.PhysicalDeviceShaderFloat16Int8Features
            };

            PhysicalDeviceCustomBorderColorFeaturesEXT featuresCustomBorderColor = new PhysicalDeviceCustomBorderColorFeaturesEXT()
            {
                SType = StructureType.PhysicalDeviceCustomBorderColorFeaturesExt
            };

            PhysicalDevicePortabilitySubsetFeaturesKHR featuresPortabilitySubset = new PhysicalDevicePortabilitySubsetFeaturesKHR()
            {
                SType = StructureType.PhysicalDevicePortabilitySubsetFeaturesKhr
            };

            if (supportedExtensions.Contains("VK_EXT_primitive_topology_list_restart"))
            {
                features2.PNext = &featuresPrimitiveTopologyListRestart;
            }

            if (supportedExtensions.Contains("VK_EXT_robustness2"))
            {
                featuresRobustness2.PNext = features2.PNext;
                features2.PNext = &featuresRobustness2;
            }

            if (supportedExtensions.Contains("VK_KHR_shader_float16_int8"))
            {
                featuresShaderInt8.PNext = features2.PNext;
                features2.PNext = &featuresShaderInt8;
            }

            if (supportedExtensions.Contains("VK_EXT_custom_border_color"))
            {
                featuresCustomBorderColor.PNext = features2.PNext;
                features2.PNext = &featuresCustomBorderColor;
            }

            bool usePortability = supportedExtensions.Contains("VK_KHR_portability_subset");

            if (usePortability)
            {
                propertiesPortabilitySubset.PNext = properties2.PNext;
                properties2.PNext = &propertiesPortabilitySubset;

                featuresPortabilitySubset.PNext = features2.PNext;
                features2.PNext = &featuresPortabilitySubset;
            }

            Api.GetPhysicalDeviceProperties2(_physicalDevice, &properties2);
            Api.GetPhysicalDeviceFeatures2(_physicalDevice, &features2);

            var portabilityFlags = PortabilitySubsetFlags.None;
            uint vertexBufferAlignment = 1;

            if (usePortability)
            {
                vertexBufferAlignment = propertiesPortabilitySubset.MinVertexInputBindingStrideAlignment;

                portabilityFlags |= featuresPortabilitySubset.TriangleFans ? 0 : PortabilitySubsetFlags.NoTriangleFans;
                portabilityFlags |= featuresPortabilitySubset.PointPolygons ? 0 : PortabilitySubsetFlags.NoPointMode;
                portabilityFlags |= featuresPortabilitySubset.ImageView2DOn3DImage ? 0 : PortabilitySubsetFlags.No3DImageView;
                portabilityFlags |= featuresPortabilitySubset.SamplerMipLodBias ? 0 : PortabilitySubsetFlags.NoLodBias;
            }

            bool supportsCustomBorderColor = supportedExtensions.Contains("VK_EXT_custom_border_color") &&
                                             featuresCustomBorderColor.CustomBorderColors &&
                                             featuresCustomBorderColor.CustomBorderColorWithoutFormat;

            ref var properties = ref properties2.Properties;

            SampleCountFlags supportedSampleCounts =
                properties.Limits.FramebufferColorSampleCounts &
                properties.Limits.FramebufferDepthSampleCounts &
                properties.Limits.FramebufferStencilSampleCounts;

            Capabilities = new HardwareCapabilities(
                supportedExtensions.Contains("VK_EXT_index_type_uint8"),
                supportsCustomBorderColor,
                supportsBlendOperationAdvanced,
                propertiesBlendOperationAdvanced.AdvancedBlendCorrelatedOverlap,
                propertiesBlendOperationAdvanced.AdvancedBlendNonPremultipliedSrcColor,
                propertiesBlendOperationAdvanced.AdvancedBlendNonPremultipliedDstColor,
                supportedExtensions.Contains(KhrDrawIndirectCount.ExtensionName),
                supportedExtensions.Contains("VK_EXT_fragment_shader_interlock"),
                supportedExtensions.Contains("VK_NV_geometry_shader_passthrough"),
                supportsSubgroupSizeControl,
                featuresShaderInt8.ShaderInt8,
                supportedExtensions.Contains("VK_EXT_shader_stencil_export"),
                supportedExtensions.Contains(ExtConditionalRendering.ExtensionName),
                supportedExtensions.Contains(ExtExtendedDynamicState.ExtensionName),
                features2.Features.MultiViewport,
                featuresRobustness2.NullDescriptor || IsMoltenVk,
                supportedExtensions.Contains(KhrPushDescriptor.ExtensionName),
                featuresPrimitiveTopologyListRestart.PrimitiveTopologyListRestart,
                featuresPrimitiveTopologyListRestart.PrimitiveTopologyPatchListRestart,
                supportsTransformFeedback,
                propertiesTransformFeedback.TransformFeedbackQueries,
                features2.Features.OcclusionQueryPrecise,
                supportedFeatures.PipelineStatisticsQuery,
                supportedFeatures.GeometryShader,
                propertiesSubgroupSizeControl.MinSubgroupSize,
                propertiesSubgroupSizeControl.MaxSubgroupSize,
                propertiesSubgroupSizeControl.RequiredSubgroupSizeStages,
                supportedSampleCounts,
                portabilityFlags,
                vertexBufferAlignment);

            IsSharedMemory = MemoryAllocator.IsDeviceMemoryShared(Api, _physicalDevice);

            MemoryAllocator = new MemoryAllocator(Api, _physicalDevice, _device, properties.Limits.MaxMemoryAllocationCount);

            CommandBufferPool = new CommandBufferPool(Api, _device, Queue, QueueLock, queueFamilyIndex);

            DescriptorSetManager = new DescriptorSetManager(_device);

            PipelineLayoutCache = new PipelineLayoutCache();

            BackgroundResources = new BackgroundResources(this, _device);

            BufferManager = new BufferManager(this, _device);

            _syncManager = new SyncManager(this, _device);
            _pipeline = new PipelineFull(this, _device);
            _pipeline.Initialize();

            HelperShader = new HelperShader(this, _device);

            _counters = new Counters(this, _device, _pipeline);
        }

        private unsafe void SetupContext(GraphicsDebugLevel logLevel)
        {
            var api = Vk.GetApi();

            Api = api;

            _instance = VulkanInitialization.CreateInstance(api, logLevel, _getRequiredExtensions(), out ExtDebugUtils debugUtils, out _debugUtilsMessenger);

            DebugUtilsApi = debugUtils;

            if (api.TryGetInstanceExtension(_instance, out KhrSurface surfaceApi))
            {
                SurfaceApi = surfaceApi;
            }

            _surface = _getSurface(_instance, api);
            _physicalDevice = VulkanInitialization.FindSuitablePhysicalDevice(api, _instance, _surface, _preferredGpuId);

            var queueFamilyIndex = VulkanInitialization.FindSuitableQueueFamily(api, _physicalDevice, _surface, out uint maxQueueCount);
            var supportedExtensions = VulkanInitialization.GetSupportedExtensions(api, _physicalDevice);

            _device = VulkanInitialization.CreateDevice(api, _physicalDevice, queueFamilyIndex, supportedExtensions, maxQueueCount);

            if (api.TryGetDeviceExtension(_instance, _device, out KhrSwapchain swapchainApi))
            {
                SwapchainApi = swapchainApi;
            }

            api.GetDeviceQueue(_device, queueFamilyIndex, 0, out var queue);
            Queue = queue;
            QueueLock = new object();

            LoadFeatures(supportedExtensions, maxQueueCount, queueFamilyIndex);

            _window = new Window(this, _surface, _physicalDevice, _device);

            _initialized = true;
        }

        public BufferHandle CreateBuffer(int size, BufferHandle storageHint)
        {
            return BufferManager.CreateWithHandle(this, size, BufferAllocationType.Auto, storageHint);
        }

        public IProgram CreateProgram(ShaderSource[] sources, ShaderInfo info)
        {
            bool isCompute = sources.Length == 1 && sources[0].Stage == ShaderStage.Compute;

            if (info.State.HasValue || isCompute)
            {
                return new ShaderCollection(this, _device, sources, info.State ?? default, info.FromCache);
            }
            else
            {
                return new ShaderCollection(this, _device, sources);
            }
        }

        internal ShaderCollection CreateProgramWithMinimalLayout(ShaderSource[] sources, SpecDescription[] specDescription = null)
        {
            return new ShaderCollection(this, _device, sources, specDescription: specDescription, isMinimal: true);
        }

        public ISampler CreateSampler(GAL.SamplerCreateInfo info)
        {
            return new SamplerHolder(this, _device, info);
        }

        public ITexture CreateTexture(TextureCreateInfo info, float scale)
        {
            if (info.Target == Target.TextureBuffer)
            {
                return new TextureBuffer(this, info, scale);
            }

            return CreateTextureView(info, scale);
        }

        internal TextureView CreateTextureView(TextureCreateInfo info, float scale)
        {
            // This should be disposed when all views are destroyed.
            var storage = CreateTextureStorage(info, scale);
            return storage.CreateView(info, 0, 0);
        }

        internal TextureStorage CreateTextureStorage(TextureCreateInfo info, float scale)
        {
            return new TextureStorage(this, _device, info, scale);
        }

        public void DeleteBuffer(BufferHandle buffer)
        {
            BufferManager.Delete(buffer);
        }

        internal void FlushAllCommands()
        {
            _pipeline?.FlushCommandsImpl();
        }

        internal void RegisterFlush()
        {
            _syncManager.RegisterFlush();
        }

        public PinnedSpan<byte> GetBufferData(BufferHandle buffer, int offset, int size)
        {
            return BufferManager.GetData(buffer, offset, size);
        }

        public unsafe Capabilities GetCapabilities()
        {
            FormatFeatureFlags compressedFormatFeatureFlags =
                FormatFeatureFlags.SampledImageBit |
                FormatFeatureFlags.SampledImageFilterLinearBit |
                FormatFeatureFlags.BlitSrcBit |
                FormatFeatureFlags.TransferSrcBit |
                FormatFeatureFlags.TransferDstBit;

            bool supportsBc123CompressionFormat = FormatCapabilities.OptimalFormatsSupport(compressedFormatFeatureFlags,
                GAL.Format.Bc1RgbaSrgb,
                GAL.Format.Bc1RgbaUnorm,
                GAL.Format.Bc2Srgb,
                GAL.Format.Bc2Unorm,
                GAL.Format.Bc3Srgb,
                GAL.Format.Bc3Unorm);

            bool supportsBc45CompressionFormat = FormatCapabilities.OptimalFormatsSupport(compressedFormatFeatureFlags,
                GAL.Format.Bc4Snorm,
                GAL.Format.Bc4Unorm,
                GAL.Format.Bc5Snorm,
                GAL.Format.Bc5Unorm);

            bool supportsBc67CompressionFormat = FormatCapabilities.OptimalFormatsSupport(compressedFormatFeatureFlags,
                GAL.Format.Bc6HSfloat,
                GAL.Format.Bc6HUfloat,
                GAL.Format.Bc7Srgb,
                GAL.Format.Bc7Unorm);

            bool supportsEtc2CompressionFormat = FormatCapabilities.OptimalFormatsSupport(compressedFormatFeatureFlags,
                GAL.Format.Etc2RgbaSrgb,
                GAL.Format.Etc2RgbaUnorm,
                GAL.Format.Etc2RgbPtaSrgb,
                GAL.Format.Etc2RgbPtaUnorm,
                GAL.Format.Etc2RgbSrgb,
                GAL.Format.Etc2RgbUnorm);

            bool supports5BitComponentFormat = FormatCapabilities.OptimalFormatsSupport(compressedFormatFeatureFlags,
                GAL.Format.R5G6B5Unorm,
                GAL.Format.R5G5B5A1Unorm,
                GAL.Format.R5G5B5X1Unorm,
                GAL.Format.B5G6R5Unorm,
                GAL.Format.B5G5R5A1Unorm,
                GAL.Format.A1B5G5R5Unorm);

            bool supportsR4G4B4A4Format = FormatCapabilities.OptimalFormatsSupport(compressedFormatFeatureFlags,
                GAL.Format.R4G4B4A4Unorm);

            bool supportsAstcFormats = FormatCapabilities.OptimalFormatsSupport(compressedFormatFeatureFlags,
                GAL.Format.Astc4x4Unorm,
                GAL.Format.Astc5x4Unorm,
                GAL.Format.Astc5x5Unorm,
                GAL.Format.Astc6x5Unorm,
                GAL.Format.Astc6x6Unorm,
                GAL.Format.Astc8x5Unorm,
                GAL.Format.Astc8x6Unorm,
                GAL.Format.Astc8x8Unorm,
                GAL.Format.Astc10x5Unorm,
                GAL.Format.Astc10x6Unorm,
                GAL.Format.Astc10x8Unorm,
                GAL.Format.Astc10x10Unorm,
                GAL.Format.Astc12x10Unorm,
                GAL.Format.Astc12x12Unorm,
                GAL.Format.Astc4x4Srgb,
                GAL.Format.Astc5x4Srgb,
                GAL.Format.Astc5x5Srgb,
                GAL.Format.Astc6x5Srgb,
                GAL.Format.Astc6x6Srgb,
                GAL.Format.Astc8x5Srgb,
                GAL.Format.Astc8x6Srgb,
                GAL.Format.Astc8x8Srgb,
                GAL.Format.Astc10x5Srgb,
                GAL.Format.Astc10x6Srgb,
                GAL.Format.Astc10x8Srgb,
                GAL.Format.Astc10x10Srgb,
                GAL.Format.Astc12x10Srgb,
                GAL.Format.Astc12x12Srgb);

            PhysicalDeviceVulkan12Features featuresVk12 = new PhysicalDeviceVulkan12Features()
            {
                SType = StructureType.PhysicalDeviceVulkan12Features
            };

            PhysicalDeviceFeatures2 features2 = new PhysicalDeviceFeatures2()
            {
                SType = StructureType.PhysicalDeviceFeatures2,
                PNext = &featuresVk12
            };

            Api.GetPhysicalDeviceFeatures2(_physicalDevice, &features2);
            Api.GetPhysicalDeviceProperties(_physicalDevice, out var properties);

            var limits = properties.Limits;

            return new Capabilities(
                api: TargetApi.Vulkan,
                GpuVendor,
                hasFrontFacingBug: IsIntelWindows,
                hasVectorIndexingBug: Vendor == Vendor.Qualcomm,
                needsFragmentOutputSpecialization: IsMoltenVk,
                reduceShaderPrecision: IsMoltenVk,
                supportsAstcCompression: features2.Features.TextureCompressionAstcLdr && supportsAstcFormats,
                supportsBc123Compression: supportsBc123CompressionFormat,
                supportsBc45Compression: supportsBc45CompressionFormat,
                supportsBc67Compression: supportsBc67CompressionFormat,
                supportsEtc2Compression: supportsEtc2CompressionFormat,
                supports3DTextureCompression: true,
                supportsBgraFormat: true,
                supportsR4G4Format: false,
                supportsR4G4B4A4Format: supportsR4G4B4A4Format,
                supportsSnormBufferTextureFormat: true,
                supports5BitComponentFormat: supports5BitComponentFormat,
                supportsBlendEquationAdvanced: Capabilities.SupportsBlendEquationAdvanced,
                supportsFragmentShaderInterlock: Capabilities.SupportsFragmentShaderInterlock,
                supportsFragmentShaderOrderingIntel: false,
                supportsGeometryShader: Capabilities.SupportsGeometryShader,
                supportsGeometryShaderPassthrough: Capabilities.SupportsGeometryShaderPassthrough,
                supportsImageLoadFormatted: features2.Features.ShaderStorageImageReadWithoutFormat,
                supportsLayerVertexTessellation: featuresVk12.ShaderOutputLayer,
                supportsMismatchingViewFormat: true,
                supportsCubemapView: !IsAmdGcn,
                supportsNonConstantTextureOffset: false,
                supportsShaderBallot: false,
                supportsTextureShadowLod: false,
                supportsViewportIndex: featuresVk12.ShaderOutputViewportIndex,
                supportsViewportSwizzle: false,
                supportsIndirectParameters: true,
                maximumUniformBuffersPerStage: Constants.MaxUniformBuffersPerStage,
                maximumStorageBuffersPerStage: Constants.MaxStorageBuffersPerStage,
                maximumTexturesPerStage: Constants.MaxTexturesPerStage,
                maximumImagesPerStage: Constants.MaxImagesPerStage,
                maximumComputeSharedMemorySize: (int)limits.MaxComputeSharedMemorySize,
                maximumSupportedAnisotropy: (int)limits.MaxSamplerAnisotropy,
                storageBufferOffsetAlignment: (int)limits.MinStorageBufferOffsetAlignment);
        }

        public HardwareInfo GetHardwareInfo()
        {
            return new HardwareInfo(GpuVendor, GpuRenderer);
        }

        public static DeviceInfo[] GetPhysicalDevices()
        {
            try
            {
                return VulkanInitialization.GetSuitablePhysicalDevices(Vk.GetApi());
            }
            catch (Exception)
            {
                // If we got an exception here, Vulkan is most likely not supported.
                return Array.Empty<DeviceInfo>();
            }
        }

        private static string ParseStandardVulkanVersion(uint version)
        {
            return $"{version >> 22}.{(version >> 12) & 0x3FF}.{version & 0xFFF}";
        }

        private static string ParseDriverVersion(ref PhysicalDeviceProperties properties)
        {
            uint driverVersionRaw = properties.DriverVersion;

            // NVIDIA differ from the standard here and uses a different format.
            if (properties.VendorID == 0x10DE)
            {
                return $"{(driverVersionRaw >> 22) & 0x3FF}.{(driverVersionRaw >> 14) & 0xFF}.{(driverVersionRaw >> 6) & 0xFF}.{driverVersionRaw & 0x3F}";
            }
            else
            {
                return ParseStandardVulkanVersion(driverVersionRaw);
            }
        }

        private unsafe void PrintGpuInformation()
        {
            Api.GetPhysicalDeviceProperties(_physicalDevice, out var properties);

            string vendorName = VendorUtils.GetNameFromId(properties.VendorID);

            Vendor = VendorUtils.FromId(properties.VendorID);

            IsAmdWindows = Vendor == Vendor.Amd && OperatingSystem.IsWindows();
            IsIntelWindows = Vendor == Vendor.Intel && OperatingSystem.IsWindows();
            IsTBDR = IsMoltenVk ||
                Vendor == Vendor.Qualcomm ||
                Vendor == Vendor.ARM ||
                Vendor == Vendor.Broadcom ||
                Vendor == Vendor.ImgTec;

            GpuVendor = vendorName;
            GpuRenderer = Marshal.PtrToStringAnsi((IntPtr)properties.DeviceName);
            GpuVersion = $"Vulkan v{ParseStandardVulkanVersion(properties.ApiVersion)}, Driver v{ParseDriverVersion(ref properties)}";

            IsAmdGcn = !IsMoltenVk && Vendor == Vendor.Amd && VendorUtils.AmdGcnRegex().IsMatch(GpuRenderer);

            Logger.Notice.Print(LogClass.Gpu, $"{GpuVendor} {GpuRenderer} ({GpuVersion})");
        }

        public GAL.PrimitiveTopology TopologyRemap(GAL.PrimitiveTopology topology)
        {
            return topology switch
            {
                GAL.PrimitiveTopology.Quads => GAL.PrimitiveTopology.Triangles,
                GAL.PrimitiveTopology.QuadStrip => GAL.PrimitiveTopology.TriangleStrip,
                GAL.PrimitiveTopology.TriangleFan => Capabilities.PortabilitySubset.HasFlag(PortabilitySubsetFlags.NoTriangleFans) ? GAL.PrimitiveTopology.Triangles : topology,
                _ => topology
            };
        }

        public bool TopologyUnsupported(GAL.PrimitiveTopology topology)
        {
            return topology switch
            {
                GAL.PrimitiveTopology.Quads => true,
                GAL.PrimitiveTopology.TriangleFan => Capabilities.PortabilitySubset.HasFlag(PortabilitySubsetFlags.NoTriangleFans),
                _ => false
            };
        }

        public void Initialize(GraphicsDebugLevel logLevel)
        {
            SetupContext(logLevel);

            PrintGpuInformation();
        }

        internal bool NeedsVertexBufferAlignment(int attrScalarAlignment, out int alignment)
        {
            if (Capabilities.VertexBufferAlignment > 1)
            {
                alignment = (int)Capabilities.VertexBufferAlignment;

                return true;
            }
            else if (Vendor != Vendor.Nvidia)
            {
                // Vulkan requires that vertex attributes are globally aligned by their component size,
                // so buffer strides that don't divide by the largest scalar element are invalid.
                // Guest applications do this, NVIDIA GPUs are OK with it, others are not.

                alignment = attrScalarAlignment;

                return true;
            }

            alignment = 1;

            return false;
        }

        public void PreFrame()
        {
            _syncManager.Cleanup();
        }

        public ICounterEvent ReportCounter(CounterType type, EventHandler<ulong> resultHandler, bool hostReserved)
        {
            return _counters.QueueReport(type, resultHandler, hostReserved);
        }

        public void ResetCounter(CounterType type)
        {
            _counters.QueueReset(type);
        }

        public void SetBufferData(BufferHandle buffer, int offset, ReadOnlySpan<byte> data)
        {
            BufferManager.SetData(buffer, offset, data, _pipeline.CurrentCommandBuffer, _pipeline.EndRenderPass);
        }

        public void UpdateCounters()
        {
            _counters.Update();
        }

        public void ResetCounterPool()
        {
            _counters.ResetCounterPool();
        }

        public void ResetFutureCounters(CommandBuffer cmd, int count)
        {
            _counters?.ResetFutureCounters(cmd, count);
        }

        public void BackgroundContextAction(Action action, bool alwaysBackground = false)
        {
            action();
        }

        public void CreateSync(ulong id, bool strict)
        {
            _syncManager.Create(id, strict);
        }

        public IProgram LoadProgramBinary(byte[] programBinary, bool isFragment, ShaderInfo info)
        {
            throw new NotImplementedException();
        }

        public void WaitSync(ulong id)
        {
            _syncManager.Wait(id);
        }

        public ulong GetCurrentSync()
        {
            return _syncManager.GetCurrent();
        }

        public void SetInterruptAction(Action<Action> interruptAction)
        {
            InterruptAction = interruptAction;
        }

        public void Screenshot()
        {
            _window.ScreenCaptureRequested = true;
        }

        public void OnScreenCaptured(ScreenCaptureImageInfo bitmap)
        {
            ScreenCaptured?.Invoke(this, bitmap);
        }

        public unsafe void Dispose()
        {
            if (!_initialized)
            {
                return;
            }

            CommandBufferPool.Dispose();
            BackgroundResources.Dispose();
            _counters.Dispose();
            _window.Dispose();
            HelperShader.Dispose();
            _pipeline.Dispose();
            BufferManager.Dispose();
            DescriptorSetManager.Dispose();
            PipelineLayoutCache.Dispose();

            MemoryAllocator.Dispose();

            if (_debugUtilsMessenger.Handle != 0)
            {
                DebugUtilsApi.DestroyDebugUtilsMessenger(_instance, _debugUtilsMessenger, null);
            }

            foreach (var shader in Shaders)
            {
                shader.Dispose();
            }

            foreach (var texture in Textures)
            {
                texture.Release();
            }

            foreach (var sampler in Samplers)
            {
                sampler.Dispose();
            }

            SurfaceApi.DestroySurface(_instance, _surface, null);

            Api.DestroyDevice(_device, null);

            // Last step destroy the instance
            Api.DestroyInstance(_instance, null);
        }
    }
}