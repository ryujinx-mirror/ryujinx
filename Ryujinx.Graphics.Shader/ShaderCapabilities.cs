namespace Ryujinx.Graphics.Shader
{
    public struct ShaderCapabilities
    {
        private static readonly ShaderCapabilities _default = new ShaderCapabilities(16);

        public static ShaderCapabilities Default => _default;

        public int StorageBufferOffsetAlignment { get; }

        public ShaderCapabilities(int storageBufferOffsetAlignment)
        {
            StorageBufferOffsetAlignment = storageBufferOffsetAlignment;
        }
    }
}