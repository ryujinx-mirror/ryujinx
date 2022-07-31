using Ryujinx.Graphics.Shader.Translation;

namespace Ryujinx.Graphics.GAL
{
    public struct Capabilities
    {
        public readonly TargetApi Api;
        public readonly string VendorName;

        public readonly bool HasFrontFacingBug;
        public readonly bool HasVectorIndexingBug;

        public readonly bool SupportsAstcCompression;
        public readonly bool SupportsBc123Compression;
        public readonly bool SupportsBc45Compression;
        public readonly bool SupportsBc67Compression;
        public readonly bool Supports3DTextureCompression;
        public readonly bool SupportsBgraFormat;
        public readonly bool SupportsR4G4Format;
        public readonly bool SupportsFragmentShaderInterlock;
        public readonly bool SupportsFragmentShaderOrderingIntel;
        public readonly bool SupportsGeometryShaderPassthrough;
        public readonly bool SupportsImageLoadFormatted;
        public readonly bool SupportsMismatchingViewFormat;
        public readonly bool SupportsCubemapView;
        public readonly bool SupportsNonConstantTextureOffset;
        public readonly bool SupportsShaderBallot;
        public readonly bool SupportsTextureShadowLod;
        public readonly bool SupportsViewportSwizzle;
        public readonly bool SupportsIndirectParameters;

        public readonly uint MaximumUniformBuffersPerStage;
        public readonly uint MaximumStorageBuffersPerStage;
        public readonly uint MaximumTexturesPerStage;
        public readonly uint MaximumImagesPerStage;

        public readonly int MaximumComputeSharedMemorySize;
        public readonly float MaximumSupportedAnisotropy;
        public readonly int StorageBufferOffsetAlignment;

        public Capabilities(
            TargetApi api,
            string vendorName,
            bool hasFrontFacingBug,
            bool hasVectorIndexingBug,
            bool supportsAstcCompression,
            bool supportsBc123Compression,
            bool supportsBc45Compression,
            bool supportsBc67Compression,
            bool supports3DTextureCompression,
            bool supportsBgraFormat,
            bool supportsR4G4Format,
            bool supportsFragmentShaderInterlock,
            bool supportsFragmentShaderOrderingIntel,
            bool supportsGeometryShaderPassthrough,
            bool supportsImageLoadFormatted,
            bool supportsMismatchingViewFormat,
            bool supportsCubemapView,
            bool supportsNonConstantTextureOffset,
            bool supportsShaderBallot,
            bool supportsTextureShadowLod,
            bool supportsViewportSwizzle,
            bool supportsIndirectParameters,
            uint maximumUniformBuffersPerStage,
            uint maximumStorageBuffersPerStage,
            uint maximumTexturesPerStage,
            uint maximumImagesPerStage,
            int maximumComputeSharedMemorySize,
            float maximumSupportedAnisotropy,
            int storageBufferOffsetAlignment)
        {
            Api = api;
            VendorName = vendorName;
            HasFrontFacingBug = hasFrontFacingBug;
            HasVectorIndexingBug = hasVectorIndexingBug;
            SupportsAstcCompression = supportsAstcCompression;
            SupportsBc123Compression = supportsBc123Compression;
            SupportsBc45Compression = supportsBc45Compression;
            SupportsBc67Compression = supportsBc67Compression;
            Supports3DTextureCompression = supports3DTextureCompression;
            SupportsBgraFormat = supportsBgraFormat;
            SupportsR4G4Format = supportsR4G4Format;
            SupportsFragmentShaderInterlock = supportsFragmentShaderInterlock;
            SupportsFragmentShaderOrderingIntel = supportsFragmentShaderOrderingIntel;
            SupportsGeometryShaderPassthrough = supportsGeometryShaderPassthrough;
            SupportsImageLoadFormatted = supportsImageLoadFormatted;
            SupportsMismatchingViewFormat = supportsMismatchingViewFormat;
            SupportsCubemapView = supportsCubemapView;
            SupportsNonConstantTextureOffset = supportsNonConstantTextureOffset;
            SupportsShaderBallot = supportsShaderBallot;
            SupportsTextureShadowLod = supportsTextureShadowLod;
            SupportsViewportSwizzle = supportsViewportSwizzle;
            SupportsIndirectParameters = supportsIndirectParameters;
            MaximumUniformBuffersPerStage = maximumUniformBuffersPerStage;
            MaximumStorageBuffersPerStage = maximumStorageBuffersPerStage;
            MaximumTexturesPerStage = maximumTexturesPerStage;
            MaximumImagesPerStage = maximumImagesPerStage;
            MaximumComputeSharedMemorySize = maximumComputeSharedMemorySize;
            MaximumSupportedAnisotropy = maximumSupportedAnisotropy;
            StorageBufferOffsetAlignment = storageBufferOffsetAlignment;
        }
    }
}