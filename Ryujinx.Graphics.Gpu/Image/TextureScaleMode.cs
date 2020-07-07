namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// The scale mode for a given texture.
    /// Blacklisted textures cannot be scaled, Eligible textures have not been scaled yet,
    /// and Scaled textures have been scaled already.
    /// </summary>
    enum TextureScaleMode
    {
        Eligible = 0,
        Scaled = 1,
        Blacklisted = 2
    }
}
