namespace Ryujinx.Horizon.Sdk.Sf.Hipc
{
    readonly struct HipcStaticDescriptor
    {
        private readonly ulong _data;

        public ulong Address => ((((_data >> 2) & 0x70) | ((_data >> 12) & 0xf)) << 32) | (_data >> 32);
        public ushort Size => (ushort)(_data >> 16);
        public int ReceiveIndex => (int)(_data & 0xf);

        public HipcStaticDescriptor(ulong address, ushort size, int receiveIndex)
        {
            ulong data = (uint)(receiveIndex & 0xf) | ((uint)size << 16);

            data |= address << 32;
            data |= (address >> 20) & 0xf000;
            data |= (address >> 30) & 0xffc0;

            _data = data;
        }
    }
}
