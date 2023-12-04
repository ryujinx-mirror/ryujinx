namespace Ryujinx.Graphics.Gpu.Engine.Types
{
    /// <summary>
    /// Storage buffer address and size information.
    /// </summary>
    struct SbDescriptor
    {
#pragma warning disable CS0649 // Field is never assigned to
        public uint AddressLow;
        public uint AddressHigh;
        public int Size;
        public int Padding;
#pragma warning restore CS0649

        public readonly ulong PackAddress()
        {
            return AddressLow | ((ulong)AddressHigh << 32);
        }
    }
}
