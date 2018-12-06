namespace Ryujinx.HLE.HOS.Kernel
{
    class KTlsPageInfo
    {
        public const int TlsEntrySize = 0x200;

        public ulong PageAddr { get; private set; }

        private bool[] _isSlotFree;

        public KTlsPageInfo(ulong pageAddress)
        {
            PageAddr = pageAddress;

            _isSlotFree = new bool[KMemoryManager.PageSize / TlsEntrySize];

            for (int index = 0; index < _isSlotFree.Length; index++)
            {
                _isSlotFree[index] = true;
            }
        }

        public bool TryGetFreePage(out ulong address)
        {
            address = PageAddr;

            for (int index = 0; index < _isSlotFree.Length; index++)
            {
                if (_isSlotFree[index])
                {
                    _isSlotFree[index] = false;

                    return true;
                }

                address += TlsEntrySize;
            }

            address = 0;

            return false;
        }

        public bool IsFull()
        {
            bool hasFree = false;

            for (int index = 0; index < _isSlotFree.Length; index++)
            {
                hasFree |= _isSlotFree[index];
            }

            return !hasFree;
        }

        public bool IsEmpty()
        {
            bool allFree = true;

            for (int index = 0; index < _isSlotFree.Length; index++)
            {
                allFree &= _isSlotFree[index];
            }

            return allFree;
        }

        public void FreeTlsSlot(ulong address)
        {
            _isSlotFree[(address - PageAddr) / TlsEntrySize] = true;
        }
    }
}