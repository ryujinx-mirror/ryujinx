namespace Ryujinx.Graphics.Gpu.State
{
    struct RtControl
    {
        public uint Packed;

        public int UnpackCount()
        {
            return (int)(Packed & 0xf);
        }

        public int UnpackPermutationIndex(int index)
        {
            return (int)((Packed >> (4 + index * 3)) & 7);
        }
    }
}
