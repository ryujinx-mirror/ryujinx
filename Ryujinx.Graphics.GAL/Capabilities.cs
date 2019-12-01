namespace Ryujinx.Graphics.GAL
{
    public struct Capabilities
    {
        public bool SupportsAstcCompression { get; }

        public int StorageBufferOffsetAlignment { get; }

        public Capabilities(
            bool supportsAstcCompression,
            int  storageBufferOffsetAlignment)
        {
            SupportsAstcCompression      = supportsAstcCompression;
            StorageBufferOffsetAlignment = storageBufferOffsetAlignment;
        }
    }
}