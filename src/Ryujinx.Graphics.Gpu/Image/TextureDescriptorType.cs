namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// The texture descriptor type.
    /// This specifies the texture memory layout.
    /// The texture descriptor structure depends on the type.
    /// </summary>
    enum TextureDescriptorType
    {
        Buffer,
        LinearColorKey,
        Linear,
        BlockLinear,
        BlockLinearColorKey
    }
}