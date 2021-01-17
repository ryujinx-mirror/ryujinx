using System;

namespace Ryujinx.Memory.Range
{
    /// <summary>
    /// Sequence of physical memory regions that a single non-contiguous virtual memory region maps to.
    /// </summary>
    public struct MultiRange : IEquatable<MultiRange>
    {
        private readonly MemoryRange _singleRange;
        private readonly MemoryRange[] _ranges;

        private bool HasSingleRange => _ranges == null;

        /// <summary>
        /// Total of physical sub-ranges on the virtual memory region.
        /// </summary>
        public int Count => HasSingleRange ? 1 : _ranges.Length;

        /// <summary>
        /// Minimum start address of all sub-ranges.
        /// </summary>
        public ulong MinAddress { get; }

        /// <summary>
        /// Maximum end address of all sub-ranges.
        /// </summary>
        public ulong MaxAddress { get; }

        /// <summary>
        /// Creates a new multi-range with a single physical region.
        /// </summary>
        /// <param name="address">Start address of the region</param>
        /// <param name="size">Size of the region in bytes</param>
        public MultiRange(ulong address, ulong size)
        {
            _singleRange = new MemoryRange(address, size);
            _ranges = null;
            MinAddress = address;
            MaxAddress = address + size;
        }

        /// <summary>
        /// Creates a new multi-range with multiple physical regions.
        /// </summary>
        /// <param name="ranges">Array of physical regions</param>
        /// <exception cref="ArgumentNullException"><paramref name="ranges"/> is null</exception>
        public MultiRange(MemoryRange[] ranges)
        {
            _singleRange = MemoryRange.Empty;
            _ranges = ranges ?? throw new ArgumentNullException(nameof(ranges));

            if (ranges.Length != 0)
            {
                MinAddress = ulong.MaxValue;
                MaxAddress = 0UL;

                foreach (MemoryRange range in ranges)
                {
                    if (MinAddress > range.Address)
                    {
                        MinAddress = range.Address;
                    }

                    if (MaxAddress < range.EndAddress)
                    {
                        MaxAddress = range.EndAddress;
                    }
                }
            }
            else
            {
                MinAddress = 0UL;
                MaxAddress = 0UL;
            }
        }

        /// <summary>
        /// Gets the physical region at the specified index.
        /// </summary>
        /// <param name="index">Index of the physical region</param>
        /// <returns>Region at the index specified</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is invalid</exception>
        public MemoryRange GetSubRange(int index)
        {
            if (HasSingleRange)
            {
                if (index != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return _singleRange;
            }
            else
            {
                if ((uint)index >= _ranges.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return _ranges[index];
            }
        }

        /// <summary>
        /// Gets the physical region at the specified index, without explicit bounds checking.
        /// </summary>
        /// <param name="index">Index of the physical region</param>
        /// <returns>Region at the index specified</returns>
        private MemoryRange GetSubRangeUnchecked(int index)
        {
            return HasSingleRange ? _singleRange : _ranges[index];
        }

        /// <summary>
        /// Check if two multi-ranges overlap with each other.
        /// </summary>
        /// <param name="other">Other multi-range to check for overlap</param>
        /// <returns>True if any sub-range overlaps, false otherwise</returns>
        public bool OverlapsWith(MultiRange other)
        {
            if (HasSingleRange && other.HasSingleRange)
            {
                return _singleRange.OverlapsWith(other._singleRange);
            }
            else
            {
                for (int i = 0; i < Count; i++)
                {
                    MemoryRange currentRange = GetSubRangeUnchecked(i);

                    for (int j = 0; j < other.Count; j++)
                    {
                        if (currentRange.OverlapsWith(other.GetSubRangeUnchecked(j)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a given multi-range is fully contained inside another.
        /// </summary>
        /// <param name="other">Multi-range to be checked</param>
        /// <returns>True if all the sub-ranges on <paramref name="other"/> are contained inside the multi-range, with the same order, false otherwise</returns>
        public bool Contains(MultiRange other)
        {
            return FindOffset(other) >= 0;
        }

        /// <summary>
        /// Calculates the offset of a given multi-range inside another, when the multi-range is fully contained
        /// inside the other multi-range, otherwise returns -1.
        /// </summary>
        /// <param name="other">Multi-range that should be fully contained inside this one</param>
        /// <returns>Offset in bytes if fully contained, otherwise -1</returns>
        public int FindOffset(MultiRange other)
        {
            int thisCount = Count;
            int otherCount = other.Count;

            if (thisCount == 1 && otherCount == 1)
            {
                MemoryRange otherFirstRange = other.GetSubRangeUnchecked(0);
                MemoryRange currentFirstRange = GetSubRangeUnchecked(0);

                if (otherFirstRange.Address >= currentFirstRange.Address &&
                    otherFirstRange.EndAddress <= currentFirstRange.EndAddress)
                {
                    return (int)(otherFirstRange.Address - currentFirstRange.Address);
                }
            }
            else if (thisCount >= otherCount)
            {
                ulong baseOffset = 0;

                MemoryRange otherFirstRange = other.GetSubRangeUnchecked(0);
                MemoryRange otherLastRange = other.GetSubRangeUnchecked(otherCount - 1);

                for (int i = 0; i < (thisCount - otherCount) + 1; baseOffset += GetSubRangeUnchecked(i).Size, i++)
                {
                    MemoryRange currentFirstRange = GetSubRangeUnchecked(i);
                    MemoryRange currentLastRange = GetSubRangeUnchecked(i + otherCount - 1);

                    if (otherCount > 1)
                    {
                        if (otherFirstRange.Address < currentFirstRange.Address ||
                            otherFirstRange.EndAddress != currentFirstRange.EndAddress)
                        {
                            continue;
                        }

                        if (otherLastRange.Address != currentLastRange.Address ||
                            otherLastRange.EndAddress > currentLastRange.EndAddress)
                        {
                            continue;
                        }

                        bool fullMatch = true;

                        for (int j = 1; j < otherCount - 1; j++)
                        {
                            if (!GetSubRangeUnchecked(i + j).Equals(other.GetSubRangeUnchecked(j)))
                            {
                                fullMatch = false;
                                break;
                            }
                        }

                        if (!fullMatch)
                        {
                            continue;
                        }
                    }
                    else if (currentFirstRange.Address > otherFirstRange.Address ||
                             currentFirstRange.EndAddress < otherFirstRange.EndAddress)
                    {
                        continue;
                    }

                    return (int)(baseOffset + (otherFirstRange.Address - currentFirstRange.Address));
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the total size of all sub-ranges in bytes.
        /// </summary>
        /// <returns>Total size in bytes</returns>
        public ulong GetSize()
        {
            ulong sum = 0;

            foreach (MemoryRange range in _ranges)
            {
                sum += range.Size;
            }

            return sum;
        }

        public override bool Equals(object obj)
        {
            return obj is MultiRange other && Equals(other);
        }

        public bool Equals(MultiRange other)
        {
            if (HasSingleRange && other.HasSingleRange)
            {
                return _singleRange.Equals(other._singleRange);
            }

            int thisCount = Count;
            if (thisCount != other.Count)
            {
                return false;
            }

            for (int i = 0; i < thisCount; i++)
            {
                if (!GetSubRangeUnchecked(i).Equals(other.GetSubRangeUnchecked(i)))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            if (HasSingleRange)
            {
                return _singleRange.GetHashCode();
            }

            HashCode hash = new HashCode();

            foreach (MemoryRange range in _ranges)
            {
                hash.Add(range);
            }

            return hash.ToHashCode();
        }
    }
}
