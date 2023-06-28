using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ryujinx.Memory.Range
{
    /// <summary>
    /// Sorted list of ranges that supports binary search.
    /// </summary>
    /// <typeparam name="T">Type of the range.</typeparam>
    public class RangeList<T> : IEnumerable<T> where T : IRange
    {
        private readonly struct RangeItem<TValue> where TValue : IRange
        {
            public readonly ulong Address;
            public readonly ulong EndAddress;

            public readonly TValue Value;

            public RangeItem(TValue value)
            {
                Value = value;

                Address = value.Address;
                EndAddress = value.Address + value.Size;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool OverlapsWith(ulong address, ulong endAddress)
            {
                return Address < endAddress && address < EndAddress;
            }
        }

        private const int BackingInitialSize = 1024;
        private const int ArrayGrowthSize = 32;

        private RangeItem<T>[] _items;
        private readonly int _backingGrowthSize;

        public int Count { get; protected set; }

        /// <summary>
        /// Creates a new range list.
        /// </summary>
        /// <param name="backingInitialSize">The initial size of the backing array</param>
        public RangeList(int backingInitialSize = BackingInitialSize)
        {
            _backingGrowthSize = backingInitialSize;
            _items = new RangeItem<T>[backingInitialSize];
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

            Insert(index, new RangeItem<T>(item));
        }

        /// <summary>
        /// Updates an item's end address on the list. Address must be the same.
        /// </summary>
        /// <param name="item">The item to be updated</param>
        /// <returns>True if the item was located and updated, false otherwise</returns>
        public bool Update(T item)
        {
            int index = BinarySearch(item.Address);

            if (index >= 0)
            {
                while (index > 0 && _items[index - 1].Address == item.Address)
                {
                    index--;
                }

                while (index < Count)
                {
                    if (_items[index].Value.Equals(item))
                    {
                        _items[index] = new RangeItem<T>(item);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Insert(int index, RangeItem<T> item)
        {
            if (Count + 1 > _items.Length)
            {
                Array.Resize(ref _items, _items.Length + _backingGrowthSize);
            }

            if (index >= Count)
            {
                if (index == Count)
                {
                    _items[Count++] = item;
                }
            }
            else
            {
                Array.Copy(_items, index, _items, index + 1, Count - index);

                _items[index] = item;
                Count++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveAt(int index)
        {
            if (index < --Count)
            {
                Array.Copy(_items, index + 1, _items, index, Count - index);
            }
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
                while (index > 0 && _items[index - 1].Address == item.Address)
                {
                    index--;
                }

                while (index < Count)
                {
                    if (_items[index].Value.Equals(item))
                    {
                        RemoveAt(index);

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

        /// <summary>
        /// Updates an item's end address.
        /// </summary>
        /// <param name="item">The item to be updated</param>
        public void UpdateEndAddress(T item)
        {
            int index = BinarySearch(item.Address);

            if (index >= 0)
            {
                while (index > 0 && _items[index - 1].Address == item.Address)
                {
                    index--;
                }

                while (index < Count)
                {
                    if (_items[index].Value.Equals(item))
                    {
                        _items[index] = new RangeItem<T>(item);

                        return;
                    }

                    if (_items[index].Address > item.Address)
                    {
                        break;
                    }

                    index++;
                }
            }
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
            int index = BinarySearch(address, address + size);

            if (index < 0)
            {
                return default;
            }

            return _items[index].Value;
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

            for (int i = 0; i < Count; i++)
            {
                ref RangeItem<T> item = ref _items[i];

                if (item.Address >= endAddress)
                {
                    break;
                }

                if (item.OverlapsWith(address, endAddress))
                {
                    if (outputIndex == output.Length)
                    {
                        Array.Resize(ref output, outputIndex + ArrayGrowthSize);
                    }

                    output[outputIndex++] = item.Value;
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

            ulong endAddress = address + size;

            int index = BinarySearch(address, endAddress);

            if (index >= 0)
            {
                while (index > 0 && _items[index - 1].OverlapsWith(address, endAddress))
                {
                    index--;
                }

                do
                {
                    if (outputIndex == output.Length)
                    {
                        Array.Resize(ref output, outputIndex + ArrayGrowthSize);
                    }

                    output[outputIndex++] = _items[index++].Value;
                }
                while (index < Count && _items[index].OverlapsWith(address, endAddress));
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
                while (index > 0 && _items[index - 1].Address == address)
                {
                    index--;
                }

                while (index < Count)
                {
                    ref RangeItem<T> overlap = ref _items[index++];

                    if (overlap.Address != address)
                    {
                        break;
                    }

                    if (outputIndex == output.Length)
                    {
                        Array.Resize(ref output, outputIndex + ArrayGrowthSize);
                    }

                    output[outputIndex++] = overlap.Value;
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
            int right = Count - 1;

            while (left <= right)
            {
                int range = right - left;

                int middle = left + (range >> 1);

                ref RangeItem<T> item = ref _items[middle];

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
        /// <param name="endAddress">End address of the range</param>
        /// <returns>List index of the item, or complement index of nearest item with lower value on the list</returns>
        private int BinarySearch(ulong address, ulong endAddress)
        {
            int left = 0;
            int right = Count - 1;

            while (left <= right)
            {
                int range = right - left;

                int middle = left + (range >> 1);

                ref RangeItem<T> item = ref _items[middle];

                if (item.OverlapsWith(address, endAddress))
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
            for (int i = 0; i < Count; i++)
            {
                yield return _items[i].Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _items[i].Value;
            }
        }
    }
}
