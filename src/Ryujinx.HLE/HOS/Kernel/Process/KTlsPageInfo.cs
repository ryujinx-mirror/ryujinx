using Ryujinx.HLE.HOS.Kernel.Memory;

namespace Ryujinx.HLE.HOS.Kernel.Process
{
    class KTlsPageInfo
    {
        public const int TlsEntrySize = 0x200;

        public ulong PageVirtualAddress { get; }
        public ulong PagePhysicalAddress { get; }

        private readonly bool[] _isSlotFree;

        public KTlsPageInfo(ulong pageVirtualAddress, ulong pagePhysicalAddress)
        {
            PageVirtualAddress = pageVirtualAddress;
            PagePhysicalAddress = pagePhysicalAddress;

            _isSlotFree = new bool[KPageTableBase.PageSize / TlsEntrySize];

            for (int index = 0; index < _isSlotFree.Length; index++)
            {
                _isSlotFree[index] = true;
            }
        }

        public bool TryGetFreePage(out ulong address)
        {
            address = PageVirtualAddress;

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
            _isSlotFree[(address - PageVirtualAddress) / TlsEntrySize] = true;
        }
    }
}
