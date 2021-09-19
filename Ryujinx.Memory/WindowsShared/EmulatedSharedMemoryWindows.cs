using Ryujinx.Memory.Range;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ryujinx.Memory.WindowsShared
{
    class EmulatedSharedMemoryWindows : IDisposable
    {
        private static readonly IntPtr InvalidHandleValue = new IntPtr(-1);
        private static readonly IntPtr CurrentProcessHandle = new IntPtr(-1);

        public const int MappingBits = 16; // Windows 64kb granularity.
        public const ulong MappingGranularity = 1 << MappingBits;
        public const ulong MappingMask = MappingGranularity - 1;

        public const ulong BackingSize32GB = 32UL * 1024UL * 1024UL * 1024UL; // Reasonable max size of 32GB.

        private class SharedMemoryMapping : INonOverlappingRange
        {
            public ulong Address { get; }

            public ulong Size { get; private set; }

            public ulong EndAddress { get; private set; }

            public List<int> Blocks;

            public SharedMemoryMapping(ulong address, ulong size, List<int> blocks = null)
            {
                Address = address;
                Size = size;
                EndAddress = address + size;

                Blocks = blocks ?? new List<int>();
            }

            public bool OverlapsWith(ulong address, ulong size)
            {
                return Address < address + size && address < EndAddress;
            }

            public void ExtendTo(ulong endAddress, RangeList<SharedMemoryMapping> list)
            {
                EndAddress = endAddress;
                Size = endAddress - Address;

                list.UpdateEndAddress(this);
            }

            public void AddBlocks(IEnumerable<int> blocks)
            {
                if (Blocks.Count > 0 && blocks.Count() > 0 && Blocks.Last() == blocks.First())
                {
                    Blocks.AddRange(blocks.Skip(1));
                }
                else
                {
                    Blocks.AddRange(blocks);
                }
            }

            public INonOverlappingRange Split(ulong splitAddress)
            {
                SharedMemoryMapping newRegion = new SharedMemoryMapping(splitAddress, EndAddress - splitAddress);

                int end = (int)((EndAddress + MappingMask) >> MappingBits);
                int start = (int)(Address >> MappingBits);

                Size = splitAddress - Address;
                EndAddress = splitAddress;

                int splitEndBlock = (int)((splitAddress + MappingMask) >> MappingBits);
                int splitStartBlock = (int)(splitAddress >> MappingBits);

                newRegion.AddBlocks(Blocks.Skip(splitStartBlock - start));
                Blocks.RemoveRange(splitEndBlock - start, end - splitEndBlock);

                return newRegion;
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFileMapping(
            IntPtr hFile,
            IntPtr lpFileMappingAttributes,
            FileMapProtection flProtect,
            uint dwMaximumSizeHigh,
            uint dwMaximumSizeLow,
            [MarshalAs(UnmanagedType.LPWStr)] string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("KernelBase.dll", SetLastError = true)]
        private static extern IntPtr VirtualAlloc2(
            IntPtr process,
            IntPtr lpAddress,
            IntPtr dwSize,
            AllocationType flAllocationType,
            MemoryProtection flProtect,
            IntPtr extendedParameters,
            ulong parameterCount);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool VirtualFree(IntPtr lpAddress, IntPtr dwSize, AllocationType dwFreeType);

        [DllImport("KernelBase.dll", SetLastError = true)]
        private static extern IntPtr MapViewOfFile3(
            IntPtr hFileMappingObject,
            IntPtr process,
            IntPtr baseAddress,
            ulong offset,
            IntPtr dwNumberOfBytesToMap,
            ulong allocationType,
            MemoryProtection dwDesiredAccess,
            IntPtr extendedParameters,
            ulong parameterCount);

        [DllImport("KernelBase.dll", SetLastError = true)]
        private static extern bool UnmapViewOfFile2(IntPtr process, IntPtr lpBaseAddress, ulong unmapFlags);

        private ulong _size;

        private object _lock = new object();

        private ulong _backingSize;
        private IntPtr _backingMemHandle;
        private int _backingEnd;
        private int _backingAllocated;
        private Queue<int> _backingFreeList;

        private List<ulong> _mappedBases;
        private RangeList<SharedMemoryMapping> _mappings;
        private SharedMemoryMapping[] _foundMappings = new SharedMemoryMapping[32];
        private PlaceholderList _placeholders;

        public EmulatedSharedMemoryWindows(ulong size)
        {
            ulong backingSize = BackingSize32GB;

            _size = size;
            _backingSize = backingSize;

            _backingMemHandle = CreateFileMapping(
                InvalidHandleValue,
                IntPtr.Zero,
                FileMapProtection.PageReadWrite | FileMapProtection.SectionReserve,
                (uint)(backingSize >> 32),
                (uint)backingSize,
                null);

            if (_backingMemHandle == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            _backingFreeList = new Queue<int>();
            _mappings = new RangeList<SharedMemoryMapping>();
            _mappedBases = new List<ulong>();
            _placeholders = new PlaceholderList(size >> MappingBits);
        }

        private (ulong granularStart, ulong granularEnd) GetAlignedRange(ulong address, ulong size)
        {
            return (address & (~MappingMask), (address + size + MappingMask) & (~MappingMask));
        }

        private void Commit(ulong address, ulong size)
        {
            (ulong granularStart, ulong granularEnd) = GetAlignedRange(address, size);

            ulong endAddress = address + size;

            lock (_lock)
            {
                // Search a bit before and after the new mapping.
                // When adding our new mapping, we may need to join an existing mapping into our new mapping (or in some cases, to the other side!)
                ulong searchStart = granularStart == 0 ? 0 : (granularStart - 1);
                int mappingCount = _mappings.FindOverlapsNonOverlapping(searchStart, (granularEnd - searchStart) + 1, ref _foundMappings);

                int first = -1;
                int last = -1;
                SharedMemoryMapping startOverlap = null;
                SharedMemoryMapping endOverlap = null;

                int lastIndex = (int)(address >> MappingBits);
                int endIndex = (int)((endAddress + MappingMask) >> MappingBits);
                int firstBlock = -1;
                int endBlock = -1;

                for (int i = 0; i < mappingCount; i++)
                {
                    SharedMemoryMapping mapping = _foundMappings[i];

                    if (mapping.Address < address)
                    {
                        if (mapping.EndAddress >= address)
                        {
                            startOverlap = mapping;
                        }

                        if ((int)((mapping.EndAddress - 1) >> MappingBits) == lastIndex)
                        {
                            lastIndex = (int)((mapping.EndAddress + MappingMask) >> MappingBits);
                            firstBlock = mapping.Blocks.Last();
                        }
                    }

                    if (mapping.EndAddress > endAddress)
                    {
                        if (mapping.Address <= endAddress)
                        {
                            endOverlap = mapping;
                        }

                        if ((int)((mapping.Address) >> MappingBits) + 1 == endIndex)
                        {
                            endIndex = (int)((mapping.Address) >> MappingBits);
                            endBlock = mapping.Blocks.First();
                        }
                    }

                    if (mapping.OverlapsWith(address, size))
                    {
                        if (first == -1)
                        {
                            first = i;
                        }

                        last = i;
                    }
                }

                if (startOverlap == endOverlap && startOverlap != null)
                {
                    // Already fully committed.
                    return;
                }

                var blocks = new List<int>();
                int lastBlock = -1;

                if (firstBlock != -1)
                {
                    blocks.Add(firstBlock);
                    lastBlock = firstBlock;
                }

                bool hasMapped = false;
                Action map = () =>
                {
                    if (!hasMapped)
                    {
                        _placeholders.EnsurePlaceholders(address >> MappingBits, (granularEnd - granularStart) >> MappingBits, SplitPlaceholder);
                        hasMapped = true;
                    }

                    // There's a gap between this index and the last. Allocate blocks to fill it.
                    blocks.Add(MapBackingBlock(MappingGranularity * (ulong)lastIndex++));
                };

                if (first != -1)
                {
                    for (int i = first; i <= last; i++)
                    {
                        SharedMemoryMapping mapping = _foundMappings[i];
                        int mapIndex = (int)(mapping.Address >> MappingBits);

                        while (lastIndex < mapIndex)
                        {
                            map();
                        }

                        if (lastBlock == mapping.Blocks[0])
                        {
                            blocks.AddRange(mapping.Blocks.Skip(1));
                        }
                        else
                        {
                            blocks.AddRange(mapping.Blocks);
                        }

                        lastIndex = (int)((mapping.EndAddress - 1) >> MappingBits) + 1;
                    }
                }

                while (lastIndex < endIndex)
                {
                    map();
                }

                if (endBlock != -1 && endBlock != lastBlock)
                {
                    blocks.Add(endBlock);
                }

                if (startOverlap != null && endOverlap != null)
                {
                    // Both sides should be coalesced. Extend the start overlap to contain the end overlap, and add together their blocks.

                    _mappings.Remove(endOverlap);

                    startOverlap.ExtendTo(endOverlap.EndAddress, _mappings);

                    startOverlap.AddBlocks(blocks);
                    startOverlap.AddBlocks(endOverlap.Blocks);
                }
                else if (startOverlap != null)
                {
                    startOverlap.ExtendTo(endAddress, _mappings);

                    startOverlap.AddBlocks(blocks);
                }
                else
                {
                    var mapping = new SharedMemoryMapping(address, size, blocks);

                    if (endOverlap != null)
                    {
                        mapping.ExtendTo(endOverlap.EndAddress, _mappings);

                        mapping.AddBlocks(endOverlap.Blocks);

                        _mappings.Remove(endOverlap);
                    }

                    _mappings.Add(mapping);
                }
            }
        }

        private void Decommit(ulong address, ulong size)
        {
            (ulong granularStart, ulong granularEnd) = GetAlignedRange(address, size);
            ulong endAddress = address + size;

            lock (_lock)
            {
                int mappingCount = _mappings.FindOverlapsNonOverlapping(granularStart, granularEnd - granularStart, ref _foundMappings);

                int first = -1;
                int last = -1;

                for (int i = 0; i < mappingCount; i++)
                {
                    SharedMemoryMapping mapping = _foundMappings[i];

                    if (mapping.OverlapsWith(address, size))
                    {
                        if (first == -1)
                        {
                            first = i;
                        }

                        last = i;
                    }
                }

                if (first == -1)
                {
                    return; // Could not find any regions to decommit.
                }

                int lastReleasedBlock = -1;

                bool releasedFirst = false;
                bool releasedLast = false;

                for (int i = last; i >= first; i--)
                {
                    SharedMemoryMapping mapping = _foundMappings[i];
                    bool releaseEnd = true;
                    bool releaseStart = true;

                    if (i == last)
                    {
                        // If this is the last region, do not release the block if there is a page ahead of us, or the block continues after us. (it is keeping the block alive)
                        releaseEnd = last == mappingCount - 1;

                        // If the end region starts after the decommit end address, split and readd it after modifying its base address.
                        if (mapping.EndAddress > endAddress)
                        {
                            var newMapping = (SharedMemoryMapping)mapping.Split(endAddress);
                            _mappings.UpdateEndAddress(mapping);
                            _mappings.Add(newMapping);

                            if ((endAddress & MappingMask) != 0)
                            {
                                releaseEnd = false;
                            }
                        }

                        releasedLast = releaseEnd;
                    }

                    if (i == first)
                    {
                        // If this is the first region, do not release the block if there is a region behind us. (it is keeping the block alive)
                        releaseStart = first == 0;

                        // If the first region starts before the decommit address, split it by modifying its end address.
                        if (mapping.Address < address)
                        {
                            var oldMapping = mapping;
                            mapping = (SharedMemoryMapping)mapping.Split(address);
                            _mappings.UpdateEndAddress(oldMapping);

                            if ((address & MappingMask) != 0)
                            {
                                releaseStart = false;
                            }
                        }

                        releasedFirst = releaseStart;
                    }

                    _mappings.Remove(mapping);

                    ulong releasePointer = (mapping.EndAddress + MappingMask) & (~MappingMask);
                    for (int j = mapping.Blocks.Count - 1; j >= 0; j--)
                    {
                        int blockId = mapping.Blocks[j];

                        releasePointer -= MappingGranularity;

                        if (lastReleasedBlock == blockId)
                        {
                            // When committed regions are fragmented, multiple will have the same block id for their start/end granular block.
                            // Avoid releasing these blocks twice.
                            continue;
                        }

                        if ((j != 0 || releaseStart) && (j != mapping.Blocks.Count - 1 || releaseEnd))
                        {
                            ReleaseBackingBlock(releasePointer, blockId);
                        }

                        lastReleasedBlock = blockId;
                    }
                }

                ulong placeholderStart = (granularStart >> MappingBits) + (releasedFirst ? 0UL : 1UL);
                ulong placeholderEnd = (granularEnd >> MappingBits) - (releasedLast ? 0UL : 1UL);

                if (placeholderEnd > placeholderStart)
                {
                    _placeholders.RemovePlaceholders(placeholderStart, placeholderEnd - placeholderStart, CoalescePlaceholder);
                }
            }
        }

        public bool CommitMap(IntPtr address, IntPtr size)
        {
            lock (_lock)
            {
                foreach (ulong mapping in _mappedBases)
                {
                    ulong offset = (ulong)address - mapping;

                    if (offset < _size)
                    {
                        Commit(offset, (ulong)size);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool DecommitMap(IntPtr address, IntPtr size)
        {
            lock (_lock)
            {
                foreach (ulong mapping in _mappedBases)
                {
                    ulong offset = (ulong)address - mapping;

                    if (offset < _size)
                    {
                        Decommit(offset, (ulong)size);
                        return true;
                    }
                }
            }

            return false;
        }

        private int MapBackingBlock(ulong offset)
        {
            bool allocate = false;
            int backing;

            if (_backingFreeList.Count > 0)
            {
                backing = _backingFreeList.Dequeue();
            }
            else
            {
                if (_backingAllocated == _backingEnd)
                {
                    // Allocate the backing.
                    _backingAllocated++;
                    allocate = true;
                }

                backing = _backingEnd++;
            }

            ulong backingOffset = MappingGranularity * (ulong)backing;

            foreach (ulong baseAddress in _mappedBases)
            {
                CommitToMap(baseAddress, offset, MappingGranularity, backingOffset, allocate);
                allocate = false;
            }

            return backing;
        }

        private void ReleaseBackingBlock(ulong offset, int id)
        {
            foreach (ulong baseAddress in _mappedBases)
            {
                DecommitFromMap(baseAddress, offset);
            }

            if (_backingEnd - 1 == id)
            {
                _backingEnd = id;
            }
            else
            {
                _backingFreeList.Enqueue(id);
            }
        }

        public IntPtr Map()
        {
            IntPtr newMapping = VirtualAlloc2(
                CurrentProcessHandle,
                IntPtr.Zero,
                (IntPtr)_size,
                AllocationType.Reserve | AllocationType.ReservePlaceholder,
                MemoryProtection.NoAccess,
                IntPtr.Zero,
                0);

            if (newMapping == IntPtr.Zero)
            {
                throw new OutOfMemoryException();
            }

            // Apply all existing mappings to the new mapping
            lock (_lock)
            {
                int lastBlock = -1;
                foreach (SharedMemoryMapping mapping in _mappings)
                {
                    ulong blockAddress = mapping.Address & (~MappingMask);
                    foreach (int block in mapping.Blocks)
                    {
                        if (block != lastBlock)
                        {
                            ulong backingOffset = MappingGranularity * (ulong)block;

                            CommitToMap((ulong)newMapping, blockAddress, MappingGranularity, backingOffset, false);

                            lastBlock = block;
                        }

                        blockAddress += MappingGranularity;
                    }
                }

                _mappedBases.Add((ulong)newMapping);
            }

            return newMapping;
        }

        private void SplitPlaceholder(ulong address, ulong size)
        {
            ulong byteAddress = address << MappingBits;
            IntPtr byteSize = (IntPtr)(size << MappingBits);

            foreach (ulong mapAddress in _mappedBases)
            {
                bool result = VirtualFree((IntPtr)(mapAddress + byteAddress), byteSize, AllocationType.PreservePlaceholder | AllocationType.Release);

                if (!result)
                {
                    throw new InvalidOperationException("Placeholder could not be split.");
                }
            }
        }

        private void CoalescePlaceholder(ulong address, ulong size)
        {
            ulong byteAddress = address << MappingBits;
            IntPtr byteSize = (IntPtr)(size << MappingBits);

            foreach (ulong mapAddress in _mappedBases)
            {
                bool result = VirtualFree((IntPtr)(mapAddress + byteAddress), byteSize, AllocationType.CoalescePlaceholders | AllocationType.Release);

                if (!result)
                {
                    throw new InvalidOperationException("Placeholder could not be coalesced.");
                }
            }
        }

        private void CommitToMap(ulong mapAddress, ulong address, ulong size, ulong backingOffset, bool allocate)
        {
            IntPtr targetAddress = (IntPtr)(mapAddress + address);

            // Assume the placeholder worked (or already exists)
            // Map the backing memory into the mapped location.

            IntPtr mapped = MapViewOfFile3(
                _backingMemHandle,
                CurrentProcessHandle,
                targetAddress,
                backingOffset,
                (IntPtr)MappingGranularity,
                0x4000, // REPLACE_PLACEHOLDER
                MemoryProtection.ReadWrite,
                IntPtr.Zero,
                0);

            if (mapped == IntPtr.Zero)
            {
                throw new InvalidOperationException($"Could not map view of backing memory. (va=0x{address:X16} size=0x{size:X16}, error code {Marshal.GetLastWin32Error()})");
            }

            if (allocate)
            {
                // Commit this part of the shared memory.
                VirtualAlloc2(CurrentProcessHandle, targetAddress, (IntPtr)MappingGranularity, AllocationType.Commit, MemoryProtection.ReadWrite, IntPtr.Zero, 0);
            }
        }

        private void DecommitFromMap(ulong baseAddress, ulong address)
        {
            UnmapViewOfFile2(CurrentProcessHandle, (IntPtr)(baseAddress + address), 2);
        }

        public bool Unmap(ulong baseAddress)
        {
            lock (_lock)
            {
                if (_mappedBases.Remove(baseAddress))
                {
                    int lastBlock = -1;

                    foreach (SharedMemoryMapping mapping in _mappings)
                    {
                        ulong blockAddress = mapping.Address & (~MappingMask);
                        foreach (int block in mapping.Blocks)
                        {
                            if (block != lastBlock)
                            {
                                DecommitFromMap(baseAddress, blockAddress);

                                lastBlock = block;
                            }

                            blockAddress += MappingGranularity;
                        }
                    }

                    if (!VirtualFree((IntPtr)baseAddress, (IntPtr)0, AllocationType.Release))
                    {
                        throw new InvalidOperationException("Couldn't free mapping placeholder.");
                    }

                    return true;
                }

                return false;
            }
        }

        public void Dispose()
        {
            // Remove all file mappings
            lock (_lock)
            {
                foreach (ulong baseAddress in _mappedBases.ToArray())
                {
                    Unmap(baseAddress);
                }
            }

            // Finally, delete the file mapping.
            CloseHandle(_backingMemHandle);
        }
    }
}
