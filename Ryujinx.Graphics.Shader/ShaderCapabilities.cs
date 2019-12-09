namespace Ryujinx.Graphics.Shader
{
    public struct ShaderCapabilities
    {
        // Initialize with default values for Maxwell.
        private static readonly ShaderCapabilities _default = new ShaderCapabilities(32768, 49152, 16);

        public static ShaderCapabilities Default => _default;

        public int MaximumViewportDimensions      { get; }
        public int MaximumComputeSharedMemorySize { get; }
        public int StorageBufferOffsetAlignment   { get; }

        public ShaderCapabilities(
            int maximumViewportDimensions,
            int maximumComputeSharedMemorySize,
            int storageBufferOffsetAlignment)
        {
            MaximumViewportDimensions      = maximumViewportDimensions;
            MaximumComputeSharedMemorySize = maximumComputeSharedMemorySize;
            StorageBufferOffsetAlignment   = storageBufferOffsetAlignment;
        }
    }
}