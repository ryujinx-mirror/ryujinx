using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryujinx.Memory.Range
{
    /// <summary>
    /// Sorted list of ranges that supports binary search.
    /// </summary>
    /// <typeparam name="T">Type of the range.</typeparam>
    public class MultiRangeList<T> : IEnumerable<T> where T : IMultiRangeItem
    {
        private const int ArrayGrowthSize = 32;

        private readonly List<T> _items;

        public int Count => _items.Count;

        /// <summary>
        /// Creates a new range list.
        /// </summary>
        public MultiRangeList()
        {
            _items = new List<T>();
        }

        /// <summary>
        /// Adds a new item to the list.
        /// </summary>
        /// <param name="item">The item to be added</param>
        public void Add(T item)
        {
            int index = BinarySearch(item.BaseAddress);

            if (index < 0)
            {
                index = ~index;
            }

            _items.Insert(index, item);
        }

        /// <summary>
        /// Removes an item from the list.
        /// </summary>
        /// <param name="item">The item to be removed</param>
        /// <returns>True if the item was removed, or false if it was not found</returns>
        public bool Remove(T item)
        {
            int index = BinarySearch(item.BaseAddress);

            if (index >= 0)
            {
                while (index > 0 && _items[index - 1].BaseAddress == item.BaseAddress)
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

                    if (_items[index].BaseAddress > item.BaseAddress)
                    {
                        break;
                    }

                    index++;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets all items on the list overlapping the specified memory range.
        /// </summary>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size in bytes of the range</param>
        /// <param name="output">Output array where matches will be written. It is automatically resized to fit the results</param>
        /// <returns>The number of overlapping items found</returns>
        public int FindOverlaps(ulong address, ulong size, ref T[] output)
        {
            return FindOverlaps(new MultiRange(address, size), ref output);
        }

        /// <summary>
        /// Gets all items on the list overlapping the specified memory ranges.
        /// </summary>
        /// <param name="range">Ranges of memory being searched</param>
        /// <param name="output">Output array where matches will be written. It is automatically resized to fit the results</param>
        /// <returns>The number of overlapping items found</returns>
        public int FindOverlaps(MultiRange range, ref T[] output)
        {
            int outputIndex = 0;

            foreach (T item in _items)
            {
                if (item.Range.OverlapsWith(range))
                {
                    if (outputIndex == output.Length)
                    {
                        Array.Resize(ref output, outputIndex + ArrayGrowthSize);
                    }

                    output[outputIndex++] = item;
                }
            }

            return outputIndex;
        }

        /// <summary>
        /// Gets all items on the list starting at the specified memory address.
        /// </summary>
        /// <param name="baseAddress">Base address to find</param>
        /// <param name="output">Output array where matches will be written. It is automatically resized to fit the results</param>
        /// <returns>The number of matches found</returns>
        public int FindOverlaps(ulong baseAddress, ref T[] output)
        {
            int index = BinarySearch(baseAddress);

            int outputIndex = 0;

            if (index >= 0)
            {
                while (index > 0 && _items[index - 1].BaseAddress == baseAddress)
                {
                    index--;
                }

                while (index < _items.Count)
                {
                    T overlap = _items[index++];

                    if (overlap.BaseAddress != baseAddress)
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

        /// <summary>
        /// Performs binary search on the internal list of items.
        /// </summary>
        /// <param name="address">Address to find</param>
        /// <returns>List index of the item, or complement index of nearest item with lower value on the list</returns>
        private int BinarySearch(ulong address)
        {
            int left = 0;
            int right = _items.Count - 1;

            while (left <= right)
            {
                int range = right - left;

                int middle = left + (range >> 1);

                T item = _items[middle];

                if (item.BaseAddress == address)
                {
                    return middle;
                }

                if (address < item.BaseAddress)
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

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }
    }
}