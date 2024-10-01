namespace Ryujinx.Graphics.Gpu.Engine.Types
{
    /// <summary>
    /// Memory layout parameters, for block linear textures.
    /// </summary>
    struct MemoryLayout
    {
#pragma warning disable CS0649 // Field is never assigned to
        public uint Packed;
#pragma warning restore CS0649

        public readonly int UnpackGobBlocksInX()
        {
            return 1 << (int)(Packed & 0xf);
        }

        public readonly int UnpackGobBlocksInY()
        {
            return 1 << (int)((Packed >> 4) & 0xf);
        }

        public readonly int UnpackGobBlocksInZ()
        {
            return 1 << (int)((Packed >> 8) & 0xf);
        }

        public readonly bool UnpackIsLinear()
        {
            return (Packed & 0x1000) != 0;
        }

        public readonly bool UnpackIsTarget3D()
        {
            return (Packed & 0x10000) != 0;
        }
    }
}
