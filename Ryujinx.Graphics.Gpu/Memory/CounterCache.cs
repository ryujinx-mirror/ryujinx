using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Represents the GPU counter cache.
    /// </summary>
    class CounterCache
    {
        private struct CounterEntry
        {
            public ulong Address { get; }

            public CounterEntry(ulong address)
            {
                Address = address;
            }
        }

        private readonly List<CounterEntry> _items;

        /// <summary>
        /// Creates a new instance of the GPU counter cache.
        /// </summary>
        public CounterCache()
        {
            _items = new List<CounterEntry>();
        }

        /// <summary>
        /// Adds a new counter to the counter cache, or updates a existing one.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address where the counter will be written in memory</param>
        public void AddOrUpdate(ulong gpuVa)
        {
            int index = BinarySearch(gpuVa);

            CounterEntry entry = new CounterEntry(gpuVa);

            if (index < 0)
            {
                _items.Insert(~index, entry);
            }
            else
            {
                _items[index] = entry;
            }
        }

        /// <summary>
        /// Handles removal of counters written to a memory region being unmapped.
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">Event arguments</param>
        public void MemoryUnmappedHandler(object sender, UnmapEventArgs e) => RemoveRange(e.Address, e.Size);

        private void RemoveRange(ulong gpuVa, ulong size)
        {
            int index = BinarySearch(gpuVa + size - 1);

            if (index < 0)
            {
                index = ~index;
            }

            if (index >= _items.Count || !InRange(gpuVa, size, _items[index].Address))
            {
                return;
            }

            int count = 1;

            while (index > 0 && InRange(gpuVa, size, _items[index - 1].Address))
            {
                index--;
                count++;
            }

            _items.RemoveRange(index, count);
        }

        /// <summary>
        /// Checks whenever an address falls inside a given range.
        /// </summary>
        /// <param name="startVa">Range start address</param>
        /// <param name="size">Range size</param>
        /// <param name="gpuVa">Address being checked</param>
        /// <returns>True if the address falls inside the range, false otherwise</returns>
        private static bool InRange(ulong startVa, ulong size, ulong gpuVa)
        {
            return gpuVa >= startVa && gpuVa < startVa + size;
        }

        /// <summary>
        /// Check if any counter value was written to the specified GPU virtual memory address.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address</param>
        /// <returns>True if any counter value was written on the specified address, false otherwise</returns>
        public bool Contains(ulong gpuVa)
        {
            return BinarySearch(gpuVa) >= 0;
        }

        /// <summary>
        /// Performs binary search of an address on the list.
        /// </summary>
        /// <param name="address">Address to search</param>
        /// <returns>Index of the item, or complement of the index of the nearest item with lower value</returns>
        private int BinarySearch(ulong address)
        {
            int left = 0;
            int right = _items.Count - 1;

            while (left <= right)
            {
                int range = right - left;

                int middle = left + (range >> 1);

               CounterEntry item = _items[middle];

                if (item.Address == address)
                {
                    return middle;
                }

                if (address < item.Address)
                {
                    right = middle - 1;
                }
                else
                {
                    left = middle + 1;
                }
            }

            return ~left;
        }
    }
}
