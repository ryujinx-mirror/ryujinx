namespace Ryujinx.HLE.HOS.Kernel
{
    class KTlsPageInfo
    {
        public const int TlsEntrySize = 0x200;

        public ulong PageAddr { get; private set; }

        private bool[] IsSlotFree;

        public KTlsPageInfo(ulong PageAddress)
        {
            this.PageAddr = PageAddress;

            IsSlotFree = new bool[KMemoryManager.PageSize / TlsEntrySize];

            for (int Index = 0; Index < IsSlotFree.Length; Index++)
            {
                IsSlotFree[Index] = true;
            }
        }

        public bool TryGetFreePage(out ulong Address)
        {
            Address = PageAddr;

            for (int Index = 0; Index < IsSlotFree.Length; Index++)
            {
                if (IsSlotFree[Index])
                {
                    IsSlotFree[Index] = false;

                    return true;
                }

                Address += TlsEntrySize;
            }

            Address = 0;

            return false;
        }

        public bool IsFull()
        {
            bool HasFree = false;

            for (int Index = 0; Index < IsSlotFree.Length; Index++)
            {
                HasFree |= IsSlotFree[Index];
            }

            return !HasFree;
        }

        public bool IsEmpty()
        {
            bool AllFree = true;

            for (int Index = 0; Index < IsSlotFree.Length; Index++)
            {
                AllFree &= IsSlotFree[Index];
            }

            return AllFree;
        }

        public void FreeTlsSlot(ulong Address)
        {
            IsSlotFree[(Address - PageAddr) / TlsEntrySize] = true;
        }
    }
}