using Ryujinx.Graphics.Shader.Translation;

namespace Ryujinx.Graphics.GAL
{
    public readonly struct Capabilities
    {
        public readonly TargetApi Api;
        public readonly string VendorName;

        public readonly bool HasFrontFacingBug;
        public readonly bool HasVectorIndexingBug;
        public readonly bool NeedsFragmentOutputSpecialization;
        public readonly bool ReduceShaderPrecision;

        public readonly bool SupportsAstcCompression;
        public readonly bool SupportsBc123Compression;
        public readonly bool SupportsBc45Compression;
        public readonly bool SupportsBc67Compression;
        public readonly bool SupportsEtc2Compression;
        public readonly bool Supports3DTextureCompression;
        public readonly bool SupportsBgraFormat;
        public readonly bool SupportsR4G4Format;
        public readonly bool SupportsR4G4B4A4Format;
        public readonly bool SupportsSnormBufferTextureFormat;
        public readonly bool Supports5BitComponentFormat;
        public readonly bool SupportsBlendEquationAdvanced;
        public readonly bool SupportsFragmentShaderInterlock;
        public readonly bool SupportsFragmentShaderOrderingIntel;
        public readonly bool SupportsGeometryShader;
        public readonly bool SupportsGeometryShaderPassthrough;
        public readonly bool SupportsImageLoadFormatted;
        public readonly bool SupportsLayerVertexTessellation;
        public readonly bool SupportsMismatchingViewFormat;
        public readonly bool SupportsCubemapView;
        public readonly bool SupportsNonConstantTextureOffset;
        public readonly bool SupportsShaderBallot;
        public readonly bool SupportsTextureShadowLod;
        public readonly bool SupportsViewportIndexVertexTessellation;
        public readonly bool SupportsViewportMask;
        public readonly bool SupportsViewportSwizzle;
        public readonly bool SupportsIndirectParameters;
        public readonly bool SupportsDepthClipControl;

        public readonly uint MaximumUniformBuffersPerStage;
        public readonly uint MaximumStorageBuffersPerStage;
        public readonly uint MaximumTexturesPerStage;
        public readonly uint MaximumImagesPerStage;

        public readonly int MaximumComputeSharedMemorySize;
        public readonly float MaximumSupportedAnisotropy;
        public readonly int StorageBufferOffsetAlignment;

        public readonly int GatherBiasPrecision;

        public Capabilities(
            TargetApi api,
            string vendorName,
            bool hasFrontFacingBug,
            bool hasVectorIndexingBug,
            bool needsFragmentOutputSpecialization,
            bool reduceShaderPrecision,
            bool supportsAstcCompression,
            bool supportsBc123Compression,
            bool supportsBc45Compression,
            bool supportsBc67Compression,
            bool supportsEtc2Compression,
            bool supports3DTextureCompression,
            bool supportsBgraFormat,
            bool supportsR4G4Format,
            bool supportsR4G4B4A4Format,
            bool supportsSnormBufferTextureFormat,
            bool supports5BitComponentFormat,
            bool supportsBlendEquationAdvanced,
            bool supportsFragmentShaderInterlock,
            bool supportsFragmentShaderOrderingIntel,
            bool supportsGeometryShader,
            bool supportsGeometryShaderPassthrough,
            bool supportsImageLoadFormatted,
            bool supportsLayerVertexTessellation,
            bool supportsMismatchingViewFormat,
            bool supportsCubemapView,
            bool supportsNonConstantTextureOffset,
            bool supportsShaderBallot,
            bool supportsTextureShadowLod,
            bool supportsViewportIndexVertexTessellation,
            bool supportsViewportMask,
            bool supportsViewportSwizzle,
            bool supportsIndirectParameters,
            bool supportsDepthClipControl,
            uint maximumUniformBuffersPerStage,
            uint maximumStorageBuffersPerStage,
            uint maximumTexturesPerStage,
            uint maximumImagesPerStage,
            int maximumComputeSharedMemorySize,
            float maximumSupportedAnisotropy,
            int storageBufferOffsetAlignment,
            int gatherBiasPrecision)
        {
            Api = api;
            VendorName = vendorName;
            HasFrontFacingBug = hasFrontFacingBug;
            HasVectorIndexingBug = hasVectorIndexingBug;
            NeedsFragmentOutputSpecialization = needsFragmentOutputSpecialization;
            ReduceShaderPrecision = reduceShaderPrecision;
            SupportsAstcCompression = supportsAstcCompression;
            SupportsBc123Compression = supportsBc123Compression;
            SupportsBc45Compression = supportsBc45Compression;
            SupportsBc67Compression = supportsBc67Compression;
            SupportsEtc2Compression = supportsEtc2Compression;
            Supports3DTextureCompression = supports3DTextureCompression;
            SupportsBgraFormat = supportsBgraFormat;
            SupportsR4G4Format = supportsR4G4Format;
            SupportsR4G4B4A4Format = supportsR4G4B4A4Format;
            SupportsSnormBufferTextureFormat = supportsSnormBufferTextureFormat;
            Supports5BitComponentFormat = supports5BitComponentFormat;
            SupportsBlendEquationAdvanced = supportsBlendEquationAdvanced;
            SupportsFragmentShaderInterlock = supportsFragmentShaderInterlock;
            SupportsFragmentShaderOrderingIntel = supportsFragmentShaderOrderingIntel;
            SupportsGeometryShader = supportsGeometryShader;
            SupportsGeometryShaderPassthrough = supportsGeometryShaderPassthrough;
            SupportsImageLoadFormatted = supportsImageLoadFormatted;
            SupportsLayerVertexTessellation = supportsLayerVertexTessellation;
            SupportsMismatchingViewFormat = supportsMismatchingViewFormat;
            SupportsCubemapView = supportsCubemapView;
            SupportsNonConstantTextureOffset = supportsNonConstantTextureOffset;
            SupportsShaderBallot = supportsShaderBallot;
            SupportsTextureShadowLod = supportsTextureShadowLod;
            SupportsViewportIndexVertexTessellation = supportsViewportIndexVertexTessellation;
            SupportsViewportMask = supportsViewportMask;
            SupportsViewportSwizzle = supportsViewportSwizzle;
            SupportsIndirectParameters = supportsIndirectParameters;
            SupportsDepthClipControl = supportsDepthClipControl;
            MaximumUniformBuffersPerStage = maximumUniformBuffersPerStage;
            MaximumStorageBuffersPerStage = maximumStorageBuffersPerStage;
            MaximumTexturesPerStage = maximumTexturesPerStage;
            MaximumImagesPerStage = maximumImagesPerStage;
            MaximumComputeSharedMemorySize = maximumComputeSharedMemorySize;
            MaximumSupportedAnisotropy = maximumSupportedAnisotropy;
            StorageBufferOffsetAlignment = storageBufferOffsetAlignment;
            GatherBiasPrecision = gatherBiasPrecision;
        }
    }
}