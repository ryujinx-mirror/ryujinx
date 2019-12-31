namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Texture to texture copy control.
    /// </summary>
    struct CopyTextureControl
    {
        public uint Packed;

        public bool UnpackLinearFilter()
        {
            return (Packed & (1u << 4)) != 0;
        }
    }
}