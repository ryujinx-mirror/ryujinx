namespace Ryujinx.Graphics.Gpu.State
{
    struct MemoryLayout
    {
        public uint Packed;

        public int UnpackGobBlocksInX()
        {
            return 1 << (int)(Packed & 0xf);
        }

        public int UnpackGobBlocksInY()
        {
            return 1 << (int)((Packed >> 4) & 0xf);
        }

        public int UnpackGobBlocksInZ()
        {
            return 1 << (int)((Packed >> 8) & 0xf);
        }

        public bool UnpackIsLinear()
        {
            return (Packed & 0x1000) != 0;
        }

        public bool UnpackIsTarget3D()
        {
            return (Packed & 0x10000) != 0;
        }
    }
}
