namespace Ryujinx.Horizon.Sdk.Sf.Hipc
{
    readonly struct HipcReceiveListEntry
    {
#pragma warning disable IDE0052 // Remove unread private member
        private readonly uint _addressLow;
        private readonly uint _word1;
#pragma warning restore IDE0052

        public HipcReceiveListEntry(ulong address, ulong size)
        {
            _addressLow = (uint)address;
            _word1 = (ushort)(address >> 32) | (uint)(size << 16);
        }
    }
}
