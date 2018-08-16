using System;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KTlsPageManager
    {
        private const int TlsEntrySize = 0x200;

        private long PagePosition;

        private int UsedSlots;

        private bool[] Slots;

        public bool IsEmpty => UsedSlots == 0;
        public bool IsFull  => UsedSlots == Slots.Length;

        public KTlsPageManager(long PagePosition)
        {
            this.PagePosition = PagePosition;

            Slots = new bool[KMemoryManager.PageSize / TlsEntrySize];
        }

        public bool TryGetFreeTlsAddr(out long Position)
        {
            Position = PagePosition;

            for (int Index = 0; Index < Slots.Length; Index++)
            {
                if (!Slots[Index])
                {
                    Slots[Index] = true;

                    UsedSlots++;

                    return true;
                }

                Position += TlsEntrySize;
            }

            Position = 0;

            return false;
        }

        public void FreeTlsSlot(int Slot)
        {
            if ((uint)Slot > Slots.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(Slot));
            }

            Slots[Slot] = false;

            UsedSlots--;
        }
    }
}