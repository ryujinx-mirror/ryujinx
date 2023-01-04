namespace Ryujinx.Horizon.Sdk.Sf.Hipc
{
    struct HipcReceiveListEntry
    {
        private uint _addressLow;
        private uint _word1;

        public HipcReceiveListEntry(ulong address, ulong size)
        {
            _addressLow = (uint)address;
            _word1 = (ushort)(address >> 32) | (uint)(size << 16);
        }
    }
}
