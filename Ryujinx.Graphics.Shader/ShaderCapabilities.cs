namespace Ryujinx.Graphics.Shader
{
    public struct ShaderCapabilities
    {
        // Initialize with default values for Maxwell.
        private static readonly ShaderCapabilities _default = new ShaderCapabilities(0x8000, 0xc000, 16, true);

        public static ShaderCapabilities Default => _default;

        public int  MaximumViewportDimensions        { get; }
        public int  MaximumComputeSharedMemorySize   { get; }
        public int  StorageBufferOffsetAlignment     { get; }
        public bool SupportsNonConstantTextureOffset { get; }

        public ShaderCapabilities(
            int  maximumViewportDimensions,
            int  maximumComputeSharedMemorySize,
            int  storageBufferOffsetAlignment,
            bool supportsNonConstantTextureOffset)
        {
            MaximumViewportDimensions        = maximumViewportDimensions;
            MaximumComputeSharedMemorySize   = maximumComputeSharedMemorySize;
            StorageBufferOffsetAlignment     = storageBufferOffsetAlignment;
            SupportsNonConstantTextureOffset = supportsNonConstantTextureOffset;
        }
    }
}