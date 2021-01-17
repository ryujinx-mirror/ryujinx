using System;
using System.Collections;
using System.Collections.Generic;

namespace Ryujinx.Memory.Range
{
    /// <summary>
    /// Sorted list of ranges that supports binary search.
    /// </summary>
    /// <typeparam name="T">Type of the range.</typeparam>
    public class RangeList<T> : IEnumerable<T> where T : IRange
    {
        private const int ArrayGrowthSize = 32;

        protected readonly List<T> Items;

        public int Count => Items.Count;

        /// <summary>
        /// Creates a new range list.
        /// </summary>
        public RangeList()
        {
            Items = new List<T>();
        }

        /// <summary>
        /// Adds a new item to the list.
        /// </summary>
        /// <param name="item">The item to be added</param>
        public void Add(T item)
        {
            int index = BinarySearch(item.Address);

            if (index < 0)
            {
                index = ~index;
            }

            Items.Insert(index, item);
        }

        /// <summary>
        /// Removes an item from the list.
        /// </summary>
        /// <param name="item">The item to be removed</param>
        /// <returns>True if the item was removed, or false if it was not found</returns>
        public bool Remove(T item)
        {
            int index = BinarySearch(item.Address);

            if (index >= 0)
            {
                while (index > 0 && Items[index - 1].Address == item.Address)
                {
                    index--;
                }

                while (index < Items.Count)
                {
                    if (Items[index].Equals(item))
                    {
                        Items.RemoveAt(index);

                        return true;
                    }

                    if (Items[index].Address > item.Address)
                    {
                        break;
                    }

                    index++;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the first item on the list overlapping in memory with the specified item.
        /// </summary>
        /// <remarks>
        /// Despite the name, this has no ordering guarantees of the returned item.
        /// It only ensures that the item returned overlaps the specified item.
        /// </remarks>
        /// <param name="item">Item to check for overlaps</param>
        /// <returns>The overlapping item, or the default value for the type if none found</returns>
        public T FindFirstOverlap(T item)
        {
            return FindFirstOverlap(item.Address, item.Size);
        }

        /// <summary>
        /// Gets the first item on the list overlapping the specified memory range.
        /// </summary>
        /// <remarks>
        /// Despite the name, this has no ordering guarantees of the returned item.
        /// It only ensures that the item returned overlaps the specified memory range.
        /// </remarks>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size in bytes of the range</param>
        /// <returns>The overlapping item, or the default value for the type if none found</returns>
        public T FindFirstOverlap(ulong address, ulong size)
        {
            int index = BinarySearch(address, size);

            if (index < 0)
            {
                return default(T);
            }

            return Items[index];
        }

        /// <summary>
        /// Gets all items overlapping with the specified item in memory.
        /// </summary>
        /// <param name="item">Item to check for overlaps</param>
        /// <param name="output">Output array where matches will be written. It is automatically resized to fit the results</param>
        /// <returns>The number of overlapping items found</returns>
        public int FindOverlaps(T item, ref T[] output)
        {
            return FindOverlaps(item.Address, item.Size, ref output);
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
            int outputIndex = 0;

            ulong endAddress = address + size;

            foreach (T item in Items)
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

            return outputIndex;
        }

        /// <summary>
        /// Gets all items overlapping with the specified item in memory.
        /// </summary>
        /// <remarks>
        /// This method only returns correct results if none of the items on the list overlaps with
        /// each other. If that is not the case, this method should not be used.
        /// This method is faster than the regular method to find all overlaps.
        /// </remarks>
        /// <param name="item">Item to check for overlaps</param>
        /// <param name="output">Output array where matches will be written. It is automatically resized to fit the results</param>
        /// <returns>The number of overlapping items found</returns>
        public int FindOverlapsNonOverlapping(T item, ref T[] output)
        {
            return FindOverlapsNonOverlapping(item.Address, item.Size, ref output);
        }

        /// <summary>
        /// Gets all items on the list overlapping the specified memory range.
        /// </summary>
        /// <remarks>
        /// This method only returns correct results if none of the items on the list overlaps with
        /// each other. If that is not the case, this method should not be used.
        /// This method is faster than the regular method to find all overlaps.
        /// </remarks>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size in bytes of the range</param>
        /// <param name="output">Output array where matches will be written. It is automatically resized to fit the results</param>
        /// <returns>The number of overlapping items found</returns>
        public int FindOverlapsNonOverlapping(ulong address, ulong size, ref T[] output)
        {
            // This is a bit faster than FindOverlaps, but only works
            // when none of the items on the list overlaps with each other.
            int outputIndex = 0;

            int index = BinarySearch(address, size);

            if (index >= 0)
            {
                while (index > 0 && Items[index - 1].OverlapsWith(address, size))
                {
                    index--;
                }

                do
                {
                    if (outputIndex == output.Length)
                    {
                        Array.Resize(ref output, outputIndex + ArrayGrowthSize);
                    }

                    output[outputIndex++] = Items[index++];
                }
                while (index < Items.Count && Items[index].OverlapsWith(address, size));
            }

            return outputIndex;
        }

        /// <summary>
        /// Gets all items on the list with the specified memory address.
        /// </summary>
        /// <param name="address">Address to find</param>
        /// <param name="output">Output array where matches will be written. It is automatically resized to fit the results</param>
        /// <returns>The number of matches found</returns>
        public int FindOverlaps(ulong address, ref T[] output)
        {
            int index = BinarySearch(address);

            int outputIndex = 0;

            if (index >= 0)
            {
                while (index > 0 && Items[index - 1].Address == address)
                {
                    index--;
                }

                while (index < Items.Count)
                {
                    T overlap = Items[index++];

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

        /// <summary>
        /// Performs binary search on the internal list of items.
        /// </summary>
        /// <param name="address">Address to find</param>
        /// <returns>List index of the item, or complement index of nearest item with lower value on the list</returns>
        private int BinarySearch(ulong address)
        {
            int left  = 0;
            int right = Items.Count - 1;

            while (left <= right)
            {
                int range = right - left;

                int middle = left + (range >> 1);

                T item = Items[middle];

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

        /// <summary>
        /// Performs binary search for items overlapping a given memory range.
        /// </summary>
        /// <param name="address">Start address of the range</param>
        /// <param name="size">Size in bytes of the range</param>
        /// <returns>List index of the item, or complement index of nearest item with lower value on the list</returns>
        private int BinarySearch(ulong address, ulong size)
        {
            int left  = 0;
            int right = Items.Count - 1;

            while (left <= right)
            {
                int range = right - left;

                int middle = left + (range >> 1);

                T item = Items[middle];

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

        public IEnumerator<T> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }
    }
}