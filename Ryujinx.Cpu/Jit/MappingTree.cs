using Ryujinx.Common.Collections;
using Ryujinx.Memory;
using Ryujinx.Memory.Range;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Cpu.Jit
{
    class MappingTree
    {
        private const ulong PageSize = 0x1000;

        private enum MappingState : byte
        {
            Unmapped,
            Mapped,
            MappedWithMirror
        }

        private class Mapping : IntrusiveRedBlackTreeNode<Mapping>, IComparable<Mapping>
        {
            public ulong Address { get; private set; }
            public ulong Size { get; private set; }
            public ulong EndAddress => Address + Size;
            public ulong BackingOffset { get; private set; }
            public MappingState State { get; private set; }

            public Mapping(ulong address, ulong size, ulong backingOffset, MappingState state)
            {
                Address = address;
                Size = size;
                BackingOffset = backingOffset;
                State = state;
            }

            public Mapping Split(ulong splitAddress)
            {
                ulong leftSize = splitAddress - Address;
                ulong rightSize = EndAddress - splitAddress;

                Mapping left = new Mapping(Address, leftSize, BackingOffset, State);

                Address = splitAddress;
                Size = rightSize;

                if (State != MappingState.Unmapped)
                {
                    BackingOffset += leftSize;
                }

                return left;
            }

            public void UpdateState(ulong newBackingOffset, MappingState newState)
            {
                BackingOffset = newBackingOffset;
                State = newState;
            }

            public void Extend(ulong sizeDelta)
            {
                Size += sizeDelta;
            }

            public int CompareTo(Mapping other)
            {
                if (Address < other.Address)
                {
                    return -1;
                }
                else if (Address <= other.EndAddress - 1UL)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
        }

        private readonly IntrusiveRedBlackTree<Mapping> _tree;
        private readonly ReaderWriterLock _treeLock;

        public MappingTree(ulong addressSpaceSize)
        {
            _tree = new IntrusiveRedBlackTree<Mapping>();
            _treeLock = new ReaderWriterLock();

            _tree.Add(new Mapping(0UL, addressSpaceSize, 0UL, MappingState.Unmapped));
        }

        public void Map(ulong va, ulong pa, ulong size)
        {
            _treeLock.AcquireWriterLock(Timeout.Infinite);
            Update(va, pa, size, MappingState.Mapped);
            _treeLock.ReleaseWriterLock();
        }

        public void Unmap(ulong va, ulong size)
        {
            _treeLock.AcquireWriterLock(Timeout.Infinite);
            Update(va, 0UL, size, MappingState.Unmapped);
            _treeLock.ReleaseWriterLock();
        }

        public IEnumerable<MemoryRange> GetPhysicalRegions(ulong va, ulong size)
        {
            _treeLock.AcquireReaderLock(Timeout.Infinite);
            var regions = GetPhysicalRegionsImpl(va, size);
            _treeLock.ReleaseReaderLock();

            return regions;
        }

        public (MemoryBlock, ulong) GetContiguousBlock(MemoryBlock backingMemory, MemoryBlock mirror, ulong va, ulong size)
        {
            _treeLock.AcquireReaderLock(Timeout.Infinite);
            var result = GetContiguousBlockImpl(backingMemory, mirror, va, size);
            _treeLock.ReleaseReaderLock();

            return result;
        }

        private void Update(ulong va, ulong pa, ulong size, MappingState state)
        {
            Mapping map = _tree.GetNode(new Mapping(va, 1UL, 0UL, MappingState.Unmapped));

            Update(map, va, pa, size, state);
        }

        private Mapping Update(Mapping map, ulong va, ulong pa, ulong size, MappingState state)
        {
            ulong endAddress = va + size;

            for (; map != null; map = map.Successor)
            {
                if (map.Address < va)
                {
                    _tree.Add(map.Split(va));
                }

                if (map.EndAddress > endAddress)
                {
                    Mapping newMap = map.Split(endAddress);
                    _tree.Add(newMap);
                    map = newMap;
                }

                map.UpdateState(pa, state);
                map = TryCoalesce(map);

                if (map.EndAddress >= endAddress)
                {
                    break;
                }
            }

            return map;
        }

        private Mapping TryCoalesce(Mapping map)
        {
            Mapping previousMap = map.Predecessor;
            Mapping nextMap = map.Successor;

            if (previousMap != null && CanCoalesce(previousMap, map))
            {
                previousMap.Extend(map.Size);
                _tree.Remove(map);
                map = previousMap;
            }

            if (nextMap != null && CanCoalesce(map, nextMap))
            {
                map.Extend(nextMap.Size);
                _tree.Remove(nextMap);
            }

            return map;
        }

        private static bool CanCoalesce(Mapping left, Mapping right)
        {
            if (left.State != right.State)
            {
                return false;
            }

            return left.State == MappingState.Unmapped || (left.BackingOffset + left.Size == right.BackingOffset);
        }

        private IEnumerable<MemoryRange> GetPhysicalRegionsImpl(ulong va, ulong size)
        {
            Mapping map = _tree.GetNode(new Mapping(va, 1UL, 0UL, MappingState.Unmapped));

            if (map == null)
            {
                ThrowInvalidMemoryRegionException($"Not mapped: va=0x{va:X16}, size=0x{size:X16}");
            }

            var regions = new List<MemoryRange>();

            ulong endAddress = va + size;
            ulong regionStart = 0;
            ulong regionSize = 0;

            for (; map != null; map = map.Successor)
            {
                if (map.State == MappingState.Unmapped)
                {
                    ThrowInvalidMemoryRegionException($"Not mapped: va=0x{va:X16}, size=0x{size:X16}");
                }

                ulong clampedAddress = Math.Max(map.Address, va);
                ulong clampedEndAddress = Math.Min(map.EndAddress, endAddress);
                ulong clampedSize = clampedEndAddress - clampedAddress;

                ulong pa = map.BackingOffset + (clampedAddress - map.Address);

                if (pa != regionStart + regionSize)
                {
                    if (regionSize != 0)
                    {
                        regions.Add(new MemoryRange(regionStart, regionSize));
                    }

                    regionStart = pa;
                    regionSize = clampedSize;
                }
                else
                {
                    regionSize += clampedSize;
                }

                if (map.EndAddress >= endAddress)
                {
                    break;
                }
            }

            if (regionSize != 0)
            {
                regions.Add(new MemoryRange(regionStart, regionSize));
            }

            return regions;
        }

        private (MemoryBlock, ulong) GetContiguousBlockImpl(MemoryBlock backingMemory, MemoryBlock mirror, ulong va, ulong size)
        {
            Mapping map = _tree.GetNode(new Mapping(va, 1UL, 0UL, MappingState.Unmapped));

            ulong endAddress = va + size;

            if (map != null && map.Address <= va && map.EndAddress >= endAddress)
            {
                ulong pa = map.BackingOffset + (va - map.Address);
                return (backingMemory, pa);
            }

            if (map != null)
            {
                Mapping firstMap = map;

                bool contiguous = true;
                ulong expectedPa = map.BackingOffset + map.Size;

                while ((map = map.Successor) != null && map.Address < endAddress)
                {
                    if (map.State == MappingState.Unmapped || map.BackingOffset != expectedPa)
                    {
                        contiguous = false;
                        break;
                    }

                    if (map.EndAddress >= endAddress)
                    {
                        break;
                    }

                    expectedPa = map.BackingOffset + map.Size;
                }

                if (contiguous && map != null)
                {
                    ulong pa = firstMap.BackingOffset + (va - firstMap.Address);
                    return (backingMemory, pa);
                }

                map = firstMap;
            }

            ulong endVaAligned = (endAddress + PageSize - 1) & ~(PageSize - 1);
            ulong vaAligned = va & ~(PageSize - 1);

            // Make sure the range that will be accessed on the mirror is fully mapped.
            for (; map != null; map = map.Successor)
            {
                if (map.State == MappingState.Mapped)
                {
                    ulong clampedAddress = Math.Max(map.Address, vaAligned);
                    ulong clampedEndAddress = Math.Min(map.EndAddress, endVaAligned);
                    ulong clampedSize = clampedEndAddress - clampedAddress;
                    ulong backingOffset = map.BackingOffset + (clampedAddress - map.Address);

                    LockCookie lockCookie = _treeLock.UpgradeToWriterLock(Timeout.Infinite);

                    mirror.MapView(backingMemory, backingOffset, clampedAddress, clampedSize);

                    map = Update(map, clampedAddress, backingOffset, clampedSize, MappingState.MappedWithMirror);

                    _treeLock.DowngradeFromWriterLock(ref lockCookie);
                }

                if (map.EndAddress >= endAddress)
                {
                    break;
                }
            }

            return (mirror, va);
        }

        private static void ThrowInvalidMemoryRegionException(string message) => throw new InvalidMemoryRegionException(message);
    }
}