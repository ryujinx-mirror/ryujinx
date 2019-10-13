namespace Ryujinx.Graphics.Gpu.State
{
    struct CopyTextureControl
    {
        public uint Packed;

        public bool UnpackLinearFilter()
        {
            return (Packed & (1u << 4)) != 0;
        }
    }
}