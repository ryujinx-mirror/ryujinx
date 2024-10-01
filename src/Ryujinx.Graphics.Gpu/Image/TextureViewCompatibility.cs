namespace Ryujinx.Graphics.Gpu.Image
{
    /// <summary>
    /// The level of view compatibility one texture has to another. 
    /// Values are increasing in compatibility from 0 (incompatible).
    /// </summary>
    enum TextureViewCompatibility
    {
        Incompatible = 0,
        LayoutIncompatible,
        CopyOnly,
        FormatAlias,
        Full,
    }
}
