namespace Ryujinx.Horizon.Sdk.Sf.Hipc
{
    readonly struct HipcBufferDescriptor
    {
#pragma warning disable CS0649 // Field is never assigned to
        private readonly uint _sizeLow;
        private readonly uint _addressLow;
        private readonly uint _word2;
#pragma warning restore CS0649

        public ulong Address => _addressLow | (((ulong)_word2 << 4) & 0xf00000000UL) | (((ulong)_word2 << 34) & 0x7000000000UL);
        public ulong Size => _sizeLow | ((ulong)_word2 << 8) & 0xf00000000UL;
        public HipcBufferMode Mode => (HipcBufferMode)(_word2 & 3);

        public HipcBufferDescriptor(ulong address, ulong size, HipcBufferMode mode)
        {
            _sizeLow = (uint)size;
            _addressLow = (uint)address;
            _word2 = (uint)mode | ((uint)(address >> 34) & 0x1c) | ((uint)(size >> 32) << 24) | ((uint)(address >> 4) & 0xf0000000);
        }
    }
}
