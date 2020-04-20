namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Memory layout parameters, for block linear textures.
    /// </summary>
    struct MemoryLayout
    {
#pragma warning disable CS0649
        public uint Packed;
#pragma warning restore CS0649

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
