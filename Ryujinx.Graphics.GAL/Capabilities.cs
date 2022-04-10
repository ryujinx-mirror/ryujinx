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
        public readonly bool Supports3DTextureCompression;
        public readonly bool SupportsBgraFormat;
        public readonly bool SupportsR4G4Format;
        public readonly bool SupportsFragmentShaderInterlock;
        public readonly bool SupportsFragmentShaderOrderingIntel;
        public readonly bool SupportsImageLoadFormatted;
        public readonly bool SupportsMismatchingViewFormat;
        public readonly bool SupportsNonConstantTextureOffset;
        public readonly bool SupportsShaderBallot;
        public readonly bool SupportsTextureShadowLod;
        public readonly bool SupportsViewportSwizzle;
        public readonly bool SupportsIndirectParameters;

        public readonly int MaximumComputeSharedMemorySize;
        public readonly float MaximumSupportedAnisotropy;
        public readonly int StorageBufferOffsetAlignment;

        public Capabilities(
            TargetApi api,
            string vendorName,
            bool hasFrontFacingBug,
            bool hasVectorIndexingBug,
            bool supportsAstcCompression,
            bool supports3DTextureCompression,
            bool supportsBgraFormat,
            bool supportsR4G4Format,
            bool supportsFragmentShaderInterlock,
            bool supportsFragmentShaderOrderingIntel,
            bool supportsImageLoadFormatted,
            bool supportsMismatchingViewFormat,
            bool supportsNonConstantTextureOffset,
            bool supportsShaderBallot,
            bool supportsTextureShadowLod,
            bool supportsViewportSwizzle,
            bool supportsIndirectParameters,
            int maximumComputeSharedMemorySize,
            float maximumSupportedAnisotropy,
            int storageBufferOffsetAlignment)
        {
            Api = api;
            VendorName = vendorName;
            HasFrontFacingBug = hasFrontFacingBug;
            HasVectorIndexingBug = hasVectorIndexingBug;
            SupportsAstcCompression = supportsAstcCompression;
            Supports3DTextureCompression = supports3DTextureCompression;
            SupportsBgraFormat = supportsBgraFormat;
            SupportsR4G4Format = supportsR4G4Format;
            SupportsFragmentShaderInterlock = supportsFragmentShaderInterlock;
            SupportsFragmentShaderOrderingIntel = supportsFragmentShaderOrderingIntel;
            SupportsImageLoadFormatted = supportsImageLoadFormatted;
            SupportsMismatchingViewFormat = supportsMismatchingViewFormat;
            SupportsNonConstantTextureOffset = supportsNonConstantTextureOffset;
            SupportsShaderBallot = supportsShaderBallot;
            SupportsTextureShadowLod = supportsTextureShadowLod;
            SupportsViewportSwizzle = supportsViewportSwizzle;
            SupportsIndirectParameters = supportsIndirectParameters;
            MaximumComputeSharedMemorySize = maximumComputeSharedMemorySize;
            MaximumSupportedAnisotropy = maximumSupportedAnisotropy;
            StorageBufferOffsetAlignment = storageBufferOffsetAlignment;
        }
    }
}