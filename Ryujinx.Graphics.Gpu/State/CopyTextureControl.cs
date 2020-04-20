namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Texture to texture copy control.
    /// </summary>
    struct CopyTextureControl
    {
#pragma warning disable CS0649
        public uint Packed;
#pragma warning restore CS0649

        public bool UnpackLinearFilter()
        {
            return (Packed & (1u << 4)) != 0;
        }
    }
}