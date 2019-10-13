namespace Ryujinx.Graphics.Gpu.State
{
    struct RtColorMask
    {
        public uint Packed;

        public bool UnpackRed()
        {
            return (Packed & 0x1) != 0;
        }

        public bool UnpackGreen()
        {
            return (Packed & 0x10) != 0;
        }

        public bool UnpackBlue()
        {
            return (Packed & 0x100) != 0;
        }

        public bool UnpackAlpha()
        {
            return (Packed & 0x1000) != 0;
        }
    }
}
