namespace Ryujinx.Graphics.GAL
{
    public struct Capabilities
    {
        public bool SupportsAstcCompression { get; }

        public int MaximumViewportDimensions    { get; }
        public int StorageBufferOffsetAlignment { get; }

        public Capabilities(
            bool supportsAstcCompression,
            int  maximumViewportDimensions,
            int  storageBufferOffsetAlignment)
        {
            SupportsAstcCompression      = supportsAstcCompression;
            MaximumViewportDimensions    = maximumViewportDimensions;
            StorageBufferOffsetAlignment = storageBufferOffsetAlignment;
        }
    }
}