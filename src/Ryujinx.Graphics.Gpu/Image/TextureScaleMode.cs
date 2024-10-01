namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// The scale mode for a given texture.
    /// Blacklisted textures cannot be scaled, Eligible textures have not been scaled yet,
    /// and Scaled textures have been scaled already.
    /// Undesired textures will stay at 1x until a situation where they must match a scaled texture.
    /// </summary>
    enum TextureScaleMode
    {
        Eligible = 0,
        Scaled = 1,
        Blacklisted = 2,
        Undesired = 3,
    }
}
