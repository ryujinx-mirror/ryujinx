namespace Ryujinx.Graphics.GAL
{
    public struct Capabilities
    {
        public bool SupportsAstcCompression { get; }

        public int MaximumViewportDimensions      { get; }
        public int MaximumComputeSharedMemorySize { get; }
        public int StorageBufferOffsetAlignment   { get; }

        public Capabilities(
            bool supportsAstcCompression,
            int  maximumViewportDimensions,
            int  maximumComputeSharedMemorySize,
            int  storageBufferOffsetAlignment)
        {
            SupportsAstcCompression        = supportsAstcCompression;
            MaximumViewportDimensions      = maximumViewportDimensions;
            MaximumComputeSharedMemorySize = maximumComputeSharedMemorySize;
            StorageBufferOffsetAlignment   = storageBufferOffsetAlignment;
        }
    }
}