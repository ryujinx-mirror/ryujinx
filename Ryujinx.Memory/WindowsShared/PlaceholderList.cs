using Ryujinx.Memory.Range;
using System;
using System.Diagnostics;

namespace Ryujinx.Memory.WindowsShared
{
    /// <summary>
    /// A specialized list used for keeping track of Windows 10's memory placeholders.
    /// This is used to make splitting a large placeholder into equally small
    /// granular chunks much easier, while avoiding slowdown due to a large number of
    /// placeholders by coalescing adjacent granular placeholders after they are unused.
    /// </summary>
    class PlaceholderList
    {
        private class PlaceholderBlock : IRange
        {
            public ulong Address { get; }
            public ulong Size { get; private set; }
            public ulong EndAddress { get; private set; }
            public bool IsGranular { get; set; }

            public PlaceholderBlock(ulong id, ulong size, bool isGranular)
            {
                Address = id;
                Size = size;
                EndAddress = id + size;
                IsGranular = isGranular;
            }

            public bool OverlapsWith(ulong address, ulong size)
            {
                return Address < address + size && address < EndAddress;
            }

            public void ExtendTo(ulong end)
            {
                EndAddress = end;
                Size = end - Address;
            }
        }

        private RangeList<PlaceholderBlock> _placeholders;
        private PlaceholderBlock[] _foundBlocks = new PlaceholderBlock[32];

        /// <summary>
        /// Create a new list to manage placeholders.
        /// Note that a size is measured in granular placeholders.
        /// If the placeholder granularity is 65536 bytes, then a 65536 region will be covered by 1 placeholder granularity.
        /// </summary>
        /// <param name="size">Size measured in granular placeholders</param>
        public PlaceholderList(ulong size)
        {
            _placeholders = new RangeList<PlaceholderBlock>();

            _placeholders.Add(new PlaceholderBlock(0, size, false));
        }

        /// <summary>
        /// Ensure that the given range of placeholders is granular.
        /// </summary>
        /// <param name="id">Start of the range, measured in granular placeholders</param>
        /// <param name="size">Size of the range, measured in granular placeholders</param>
        /// <param name="splitPlaceholderCallback">Callback function to run when splitting placeholders, calls with (start, middle)</param>
        public void EnsurePlaceholders(ulong id, ulong size, Action<ulong, ulong> splitPlaceholderCallback)
        {
            // Search 1 before and after the placeholders, as we may need to expand/join granular regions surrounding the requested area.

            ulong endId = id + size;
            ulong searchStartId = id == 0 ? 0 : (id - 1);
            int blockCount = _placeholders.FindOverlapsNonOverlapping(searchStartId, (endId - searchStartId) + 1, ref _foundBlocks);

            PlaceholderBlock first = _foundBlocks[0];
            PlaceholderBlock last = _foundBlocks[blockCount - 1];
            bool overlapStart = first.EndAddress >= id && id != 0;
            bool overlapEnd = last.Address <= endId;

            for (int i = 0; i < blockCount; i++)
            {
                // Go through all non-granular blocks in the range and create placeholders.
                PlaceholderBlock block = _foundBlocks[i];

                if (block.Address <= id && block.EndAddress >= endId && block.IsGranular)
                {
                    return; // The region we're searching for is already granular.
                }

                if (!block.IsGranular)
                {
                    ulong placeholderStart = Math.Max(block.Address, id);
                    ulong placeholderEnd = Math.Min(block.EndAddress - 1, endId);

                    if (placeholderStart != block.Address && placeholderStart != block.EndAddress)
                    {
                        splitPlaceholderCallback(block.Address, placeholderStart - block.Address);
                    }

                    for (ulong j = placeholderStart; j < placeholderEnd; j++)
                    {
                        splitPlaceholderCallback(j, 1);
                    }
                }

                if (!((block == first && overlapStart) || (block == last && overlapEnd)))
                {
                    // Remove blocks that will be replaced
                    _placeholders.Remove(block);
                }
            }

            if (overlapEnd)
            {
                if (!(first == last && overlapStart))
                {
                    _placeholders.Remove(last);
                }

                if (last.IsGranular)
                {
                    endId = last.EndAddress;
                }
                else if (last.EndAddress != endId)
                {
                    _placeholders.Add(new PlaceholderBlock(endId, last.EndAddress - endId, false));
                }
            }

            if (overlapStart && first.IsGranular)
            {
                first.ExtendTo(endId);
            }
            else
            {
                if (overlapStart)
                {
                    first.ExtendTo(id);
                }

                _placeholders.Add(new PlaceholderBlock(id, endId - id, true));
            }

            ValidateList();
        }

        /// <summary>
        /// Coalesces placeholders in a given region, as they are not being used.
        /// This assumes that the region only contains placeholders - all views and allocations must have been replaced with placeholders.
        /// </summary>
        /// <param name="id">Start of the range, measured in granular placeholders</param>
        /// <param name="size">Size of the range, measured in granular placeholders</param>
        /// <param name="coalescePlaceholderCallback">Callback function to run when coalescing two placeholders, calls with (start, end)</param>
        public void RemovePlaceholders(ulong id, ulong size, Action<ulong, ulong> coalescePlaceholderCallback)
        {
            ulong endId = id + size;
            int blockCount = _placeholders.FindOverlapsNonOverlapping(id, size, ref _foundBlocks);

            PlaceholderBlock first = _foundBlocks[0];
            PlaceholderBlock last = _foundBlocks[blockCount - 1];

            // All granular blocks must have non-granular blocks surrounding them, unless they start at 0.
            // We must extend the non-granular blocks into the granular ones. This does mean that we need to search twice.

            if (first.IsGranular || last.IsGranular)
            {
                ulong surroundStart = Math.Max(0, (first.IsGranular && first.Address != 0) ? first.Address - 1 : id);
                blockCount = _placeholders.FindOverlapsNonOverlapping(
                    surroundStart,
                    (last.IsGranular ? last.EndAddress + 1 : endId) - surroundStart,
                    ref _foundBlocks);

                first = _foundBlocks[0];
                last = _foundBlocks[blockCount - 1];
            }

            if (first == last)
            {
                return; // Already coalesced.
            }

            PlaceholderBlock extendBlock = id == 0 ? null : first;
            bool newBlock = false;
            for (int i = extendBlock == null ? 0 : 1; i < blockCount; i++)
            {
                // Go through all granular blocks in the range and extend placeholders.
                PlaceholderBlock block = _foundBlocks[i];

                ulong blockEnd = block.EndAddress;
                ulong extendFrom;
                ulong extent = Math.Min(blockEnd, endId);

                if (block.Address < id && blockEnd > id)
                {
                    block.ExtendTo(id);
                    extendBlock = null;
                }
                else
                {
                    _placeholders.Remove(block);
                }

                if (extendBlock == null)
                {
                    extendFrom = id;
                    extendBlock = new PlaceholderBlock(id, extent - id, false);
                    _placeholders.Add(extendBlock);

                    if (blockEnd > extent)
                    {
                        _placeholders.Add(new PlaceholderBlock(extent, blockEnd - extent, true));

                        // Skip the next non-granular block, and extend from that into the granular block afterwards.
                        // (assuming that one is still in the requested range)

                        if (i + 1 < blockCount)
                        {
                            extendBlock = _foundBlocks[i + 1];
                        }

                        i++;
                    }

                    newBlock = true;
                }
                else
                {
                    extendFrom = extendBlock.Address;
                    extendBlock.ExtendTo(block.IsGranular ? extent : block.EndAddress);
                }

                if (block.IsGranular)
                {
                    ulong placeholderStart = Math.Max(block.Address, id);
                    ulong placeholderEnd = extent;

                    if (newBlock)
                    {
                        placeholderStart++;
                        newBlock = false;
                    }

                    for (ulong j = placeholderStart; j < placeholderEnd; j++)
                    {
                        coalescePlaceholderCallback(extendFrom, (j + 1) - extendFrom);
                    }

                    if (extent < block.EndAddress)
                    {
                        _placeholders.Add(new PlaceholderBlock(placeholderEnd, block.EndAddress - placeholderEnd, true));
                        ValidateList();
                        return;
                    }
                }
                else
                {
                    coalescePlaceholderCallback(extendFrom, block.EndAddress - extendFrom);
                }
            }

            ValidateList();
        }

        /// <summary>
        /// Ensure that the placeholder list is valid.
        /// A valid list should not have any gaps between the placeholders,
        /// and there may be no placehonders with the same IsGranular value next to each other.
        /// </summary>
        [Conditional("DEBUG")]
        private void ValidateList()
        {
            bool isGranular = false;
            bool first = true;
            ulong lastAddress = 0;

            foreach (var placeholder in _placeholders)
            {
                if (placeholder.Address != lastAddress)
                {
                    throw new InvalidOperationException("Gap in placeholder list.");
                }

                if (isGranular == placeholder.IsGranular && !first)
                {
                    throw new InvalidOperationException("Placeholder list not alternating.");
                }

                first = false;
                isGranular = placeholder.IsGranular;
                lastAddress = placeholder.EndAddress;
            }
        }
    }
}
