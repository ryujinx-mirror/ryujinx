namespace Ryujinx.Graphics.Shader
{
    public struct ShaderCapabilities
    {
        private static readonly ShaderCapabilities _default = new ShaderCapabilities(32768, 16);

        public static ShaderCapabilities Default => _default;

        public int MaximumViewportDimensions    { get; }
        public int StorageBufferOffsetAlignment { get; }

        public ShaderCapabilities(
            int maximumViewportDimensions,
            int storageBufferOffsetAlignment)
        {
            MaximumViewportDimensions    = maximumViewportDimensions;
            StorageBufferOffsetAlignment = storageBufferOffsetAlignment;
        }
    }
}