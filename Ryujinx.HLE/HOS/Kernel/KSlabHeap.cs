using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KSlabHeap
    {
        private LinkedList<ulong> Items;

        public KSlabHeap(ulong Pa, ulong ItemSize, ulong Size)
        {
            Items = new LinkedList<ulong>();

            int ItemsCount = (int)(Size / ItemSize);

            for (int Index = 0; Index < ItemsCount; Index++)
            {
                Items.AddLast(Pa);

                Pa += ItemSize;
            }
        }

        public bool TryGetItem(out ulong Pa)
        {
            lock (Items)
            {
                if (Items.First != null)
                {
                    Pa = Items.First.Value;

                    Items.RemoveFirst();

                    return true;
                }
            }

            Pa = 0;

            return false;
        }

        public void Free(ulong Pa)
        {
            lock (Items)
            {
                Items.AddFirst(Pa);
            }
        }
    }
}