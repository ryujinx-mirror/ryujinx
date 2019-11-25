using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Memory
{
    class RangeList<T> where T : IRange<T>
    {
        private const int ArrayGrowthSize = 32;

        private List<T> _items;

        public RangeList()
        {
            _items = new List<T>();
        }

        public void Add(T item)
        {
            int index = BinarySearch(item.Address);

            if (index < 0)
            {
                index = ~index;
            }

            _items.Insert(index, item);
        }

        public bool Remove(T item)
        {
            int index = BinarySearch(item.Address);

            if (index >= 0)
            {
                while (index > 0 && _items[index - 1].Address == item.Address)
                {
                    index--;
                }

                while (index < _items.Count)
                {
                    if (_items[index].Equals(item))
                    {
                        _items.RemoveAt(index);

                        return true;
                    }

                    if (_items[index].Address > item.Address)
                    {
                        break;
                    }

                    index++;
                }
            }

            return false;
        }

        public T FindFirstOverlap(T item)
        {
            return FindFirstOverlap(item.Address, item.Size);
        }

        public T FindFirstOverlap(ulong address, ulong size)
        {
            int index = BinarySearch(address, size);

            if (index < 0)
            {
                return default(T);
            }

            return _items[index];
        }

        public int FindOverlaps(T item, ref T[] output)
        {
            return FindOverlaps(item.Address, item.Size, ref output);
        }

        public int FindOverlaps(ulong address, ulong size, ref T[] output)
        {
            int outputIndex = 0;

            ulong endAddress = address + size;

            lock (_items)
            {
                foreach (T item in _items)
                {
                    if (item.Address >= endAddress)
                    {
                        break;
                    }

                    if (item.OverlapsWith(address, size))
                    {
                        if (outputIndex == output.Length)
                        {
                            Array.Resize(ref output, outputIndex + ArrayGrowthSize);
                        }

                        output[outputIndex++] = item;
                    }
                }
            }

            return outputIndex;
        }

        public int FindOverlapsNonOverlapping(T item, ref T[] output)
        {
            return FindOverlapsNonOverlapping(item.Address, item.Size, ref output);
        }

        public int FindOverlapsNonOverlapping(ulong address, ulong size, ref T[] output)
        {
            // This is a bit faster than FindOverlaps, but only works
            // when none of the items on the list overlaps with each other.
            int outputIndex = 0;

            ulong endAddress = address + size;

            int index = BinarySearch(address, size);

            if (index >= 0)
            {
                while (index > 0 && _items[index - 1].OverlapsWith(address, size))
                {
                    index--;
                }

                do
                {
                    if (outputIndex == output.Length)
                    {
                        Array.Resize(ref output, outputIndex + ArrayGrowthSize);
                    }

                    output[outputIndex++] = _items[index++];
                }
                while (index < _items.Count && _items[index].OverlapsWith(address, size));
            }

            return outputIndex;
        }

        public int FindOverlaps(ulong address, ref T[] output)
        {
            int index = BinarySearch(address);

            int outputIndex = 0;

            if (index >= 0)
            {
                while (index > 0 && _items[index - 1].Address == address)
                {
                    index--;
                }

                while (index < _items.Count)
                {
                    T overlap = _items[index++];

                    if (overlap.Address != address)
                    {
                        break;
                    }

                    if (outputIndex == output.Length)
                    {
                        Array.Resize(ref output, outputIndex + ArrayGrowthSize);
                    }

                    output[outputIndex++] = overlap;
                }
            }

            return outputIndex;
        }

        private int BinarySearch(ulong address)
        {
            int left  = 0;
            int right = _items.Count - 1;

            while (left <= right)
            {
                int range = right - left;

                int middle = left + (range >> 1);

                T item = _items[middle];

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

        private int BinarySearch(ulong address, ulong size)
        {
            int left  = 0;
            int right = _items.Count - 1;

            while (left <= right)
            {
                int range = right - left;

                int middle = left + (range >> 1);

                T item = _items[middle];

                if (item.OverlapsWith(address, size))
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