namespace Ryujinx.Graphics.Gpu.State
{
    /// <summary>
    /// Storage buffer address and size information.
    /// </summary>
    struct SbDescriptor
    {
#pragma warning disable CS0649
        public uint AddressLow;
        public uint AddressHigh;
        public int Size;
        public int Padding;
#pragma warning restore CS0649

        public ulong PackAddress()
        {
            return AddressLow | ((ulong)AddressHigh << 32);
        }
    }
}
