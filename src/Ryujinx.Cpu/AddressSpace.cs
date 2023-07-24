using Ryujinx.Common;
using Ryujinx.Common.Collections;
using Ryujinx.Memory;
using System;

namespace Ryujinx.Cpu
{
    public class AddressSpace : IDisposable
    {
        private const int DefaultBlockAlignment = 1 << 20;

        private enum MappingType : byte
        {
            None,
            Private,
            Shared,
        }

        private class Mapping : IntrusiveRedBlackTreeNode<Mapping>, IComparable<Mapping>
        {
            public ulong Address { get; private set; }
            public ulong Size { get; private set; }
            public ulong EndAddress => Address + Size;
            public MappingType Type { get; private set; }

            public Mapping(ulong address, ulong size, MappingType type)
            {
                Address = address;
                Size = size;
                Type = type;
            }

            public Mapping Split(ulong splitAddress)
            {
                ulong leftSize = splitAddress - Address;
                ulong rightSize = EndAddress - splitAddress;

                Mapping left = new(Address, leftSize, Type);

                Address = splitAddress;
                Size = rightSize;

                return left;
            }

            public void UpdateState(MappingType newType)
            {
                Type = newType;
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

        private class PrivateMapping : IntrusiveRedBlackTreeNode<PrivateMapping>, IComparable<PrivateMapping>
        {
            public ulong Address { get; private set; }
            public ulong Size { get; private set; }
            public ulong EndAddress => Address + Size;
            public PrivateMemoryAllocation PrivateAllocation { get; private set; }

            public PrivateMapping(ulong address, ulong size, PrivateMemoryAllocation privateAllocation)
            {
                Address = address;
                Size = size;
                PrivateAllocation = privateAllocation;
            }

            public PrivateMapping Split(ulong splitAddress)
            {
                ulong leftSize = splitAddress - Address;
                ulong rightSize = EndAddress - splitAddress;

                (var leftAllocation, PrivateAllocation) = PrivateAllocation.Split(leftSize);

                PrivateMapping left = new(Address, leftSize, leftAllocation);

                Address = splitAddress;
                Size = rightSize;

                return left;
            }

            public void Map(MemoryBlock baseBlock, MemoryBlock mirrorBlock, PrivateMemoryAllocation newAllocation)
            {
                baseBlock.MapView(newAllocation.Memory, newAllocation.Offset, Address, Size);
                mirrorBlock.MapView(newAllocation.Memory, newAllocation.Offset, Address, Size);
                PrivateAllocation = newAllocation;
            }

            public void Unmap(MemoryBlock baseBlock, MemoryBlock mirrorBlock)
            {
                if (PrivateAllocation.IsValid)
                {
                    baseBlock.UnmapView(PrivateAllocation.Memory, Address, Size);
                    mirrorBlock.UnmapView(PrivateAllocation.Memory, Address, Size);
                    PrivateAllocation.Dispose();
                }

                PrivateAllocation = default;
            }

            public void Extend(ulong sizeDelta)
            {
                Size += sizeDelta;
            }

            public int CompareTo(PrivateMapping other)
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

        private readonly MemoryBlock _backingMemory;
        private readonly PrivateMemoryAllocator _privateMemoryAllocator;
        private readonly IntrusiveRedBlackTree<Mapping> _mappingTree;
        private readonly IntrusiveRedBlackTree<PrivateMapping> _privateTree;

        private readonly object _treeLock;

        private readonly bool _supports4KBPages;

        public MemoryBlock Base { get; }
        public MemoryBlock Mirror { get; }

        public ulong AddressSpaceSize { get; }

        public AddressSpace(MemoryBlock backingMemory, MemoryBlock baseMemory, MemoryBlock mirrorMemory, ulong addressSpaceSize, bool supports4KBPages)
        {
            if (!supports4KBPages)
            {
                _privateMemoryAllocator = new PrivateMemoryAllocator(DefaultBlockAlignment, MemoryAllocationFlags.Mirrorable | MemoryAllocationFlags.NoMap);
                _mappingTree = new IntrusiveRedBlackTree<Mapping>();
                _privateTree = new IntrusiveRedBlackTree<PrivateMapping>();
                _treeLock = new object();

                _mappingTree.Add(new Mapping(0UL, addressSpaceSize, MappingType.None));
                _privateTree.Add(new PrivateMapping(0UL, addressSpaceSize, default));
            }

            _backingMemory = backingMemory;
            _supports4KBPages = supports4KBPages;

            Base = baseMemory;
            Mirror = mirrorMemory;
            AddressSpaceSize = addressSpaceSize;
        }

        public static bool TryCreate(MemoryBlock backingMemory, ulong asSize, bool supports4KBPages, out AddressSpace addressSpace)
        {
            addressSpace = null;

            const MemoryAllocationFlags AsFlags = MemoryAllocationFlags.Reserve | MemoryAllocationFlags.ViewCompatible;

            ulong minAddressSpaceSize = Math.Min(asSize, 1UL << 36);

            // Attempt to create the address space with expected size or try to reduce it until it succeed.
            for (ulong addressSpaceSize = asSize; addressSpaceSize >= minAddressSpaceSize; addressSpaceSize >>= 1)
            {
                MemoryBlock baseMemory = null;
                MemoryBlock mirrorMemory = null;

                try
                {
                    baseMemory = new MemoryBlock(addressSpaceSize, AsFlags);
                    mirrorMemory = new MemoryBlock(addressSpaceSize, AsFlags);
                    addressSpace = new AddressSpace(backingMemory, baseMemory, mirrorMemory, addressSpaceSize, supports4KBPages);

                    break;
                }
                catch (SystemException)
                {
                    baseMemory?.Dispose();
                    mirrorMemory?.Dispose();
                }
            }

            return addressSpace != null;
        }

        public void Map(ulong va, ulong pa, ulong size, MemoryMapFlags flags)
        {
            if (_supports4KBPages)
            {
                Base.MapView(_backingMemory, pa, va, size);
                Mirror.MapView(_backingMemory, pa, va, size);

                return;
            }

            lock (_treeLock)
            {
                ulong alignment = MemoryBlock.GetPageSize();
                bool isAligned = ((va | pa | size) & (alignment - 1)) == 0;

                if (flags.HasFlag(MemoryMapFlags.Private) && !isAligned)
                {
                    Update(va, pa, size, MappingType.Private);
                }
                else
                {
                    // The update method assumes that shared mappings are already aligned.

                    if (!flags.HasFlag(MemoryMapFlags.Private))
                    {
                        if ((va & (alignment - 1)) != (pa & (alignment - 1)))
                        {
                            throw new InvalidMemoryRegionException($"Virtual address 0x{va:X} and physical address 0x{pa:X} are misaligned and can't be aligned.");
                        }

                        ulong endAddress = va + size;
                        va = BitUtils.AlignDown(va, alignment);
                        pa = BitUtils.AlignDown(pa, alignment);
                        size = BitUtils.AlignUp(endAddress, alignment) - va;
                    }

                    Update(va, pa, size, MappingType.Shared);
                }
            }
        }

        public void Unmap(ulong va, ulong size)
        {
            if (_supports4KBPages)
            {
                Base.UnmapView(_backingMemory, va, size);
                Mirror.UnmapView(_backingMemory, va, size);

                return;
            }

            lock (_treeLock)
            {
                Update(va, 0UL, size, MappingType.None);
            }
        }

        private void Update(ulong va, ulong pa, ulong size, MappingType type)
        {
            Mapping map = _mappingTree.GetNode(new Mapping(va, 1UL, MappingType.None));

            Update(map, va, pa, size, type);
        }

        private Mapping Update(Mapping map, ulong va, ulong pa, ulong size, MappingType type)
        {
            ulong endAddress = va + size;

            for (; map != null; map = map.Successor)
            {
                if (map.Address < va)
                {
                    _mappingTree.Add(map.Split(va));
                }

                if (map.EndAddress > endAddress)
                {
                    Mapping newMap = map.Split(endAddress);
                    _mappingTree.Add(newMap);
                    map = newMap;
                }

                switch (type)
                {
                    case MappingType.None:
                        if (map.Type == MappingType.Shared)
                        {
                            ulong startOffset = map.Address - va;
                            ulong mapVa = va + startOffset;
                            ulong mapSize = Math.Min(size - startOffset, map.Size);
                            ulong mapEndAddress = mapVa + mapSize;
                            ulong alignment = MemoryBlock.GetPageSize();

                            mapVa = BitUtils.AlignDown(mapVa, alignment);
                            mapEndAddress = BitUtils.AlignUp(mapEndAddress, alignment);

                            mapSize = mapEndAddress - mapVa;

                            Base.UnmapView(_backingMemory, mapVa, mapSize);
                            Mirror.UnmapView(_backingMemory, mapVa, mapSize);
                        }
                        else
                        {
                            UnmapPrivate(va, size);
                        }
                        break;
                    case MappingType.Private:
                        if (map.Type == MappingType.Shared)
                        {
                            throw new InvalidMemoryRegionException($"Private mapping request at 0x{va:X} with size 0x{size:X} overlaps shared mapping at 0x{map.Address:X} with size 0x{map.Size:X}.");
                        }
                        else
                        {
                            MapPrivate(va, size);
                        }
                        break;
                    case MappingType.Shared:
                        if (map.Type != MappingType.None)
                        {
                            throw new InvalidMemoryRegionException($"Shared mapping request at 0x{va:X} with size 0x{size:X} overlaps mapping at 0x{map.Address:X} with size 0x{map.Size:X}.");
                        }
                        else
                        {
                            ulong startOffset = map.Address - va;
                            ulong mapPa = pa + startOffset;
                            ulong mapVa = va + startOffset;
                            ulong mapSize = Math.Min(size - startOffset, map.Size);

                            Base.MapView(_backingMemory, mapPa, mapVa, mapSize);
                            Mirror.MapView(_backingMemory, mapPa, mapVa, mapSize);
                        }
                        break;
                }

                map.UpdateState(type);
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
                _mappingTree.Remove(map);
                map = previousMap;
            }

            if (nextMap != null && CanCoalesce(map, nextMap))
            {
                map.Extend(nextMap.Size);
                _mappingTree.Remove(nextMap);
            }

            return map;
        }

        private static bool CanCoalesce(Mapping left, Mapping right)
        {
            return left.Type == right.Type;
        }

        private void MapPrivate(ulong va, ulong size)
        {
            ulong endAddress = va + size;

            ulong alignment = MemoryBlock.GetPageSize();

            // Expand the range outwards based on page size to ensure that at least the requested region is mapped.
            ulong vaAligned = BitUtils.AlignDown(va, alignment);
            ulong endAddressAligned = BitUtils.AlignUp(endAddress, alignment);

            PrivateMapping map = _privateTree.GetNode(new PrivateMapping(va, 1UL, default));

            for (; map != null; map = map.Successor)
            {
                if (!map.PrivateAllocation.IsValid)
                {
                    if (map.Address < vaAligned)
                    {
                        _privateTree.Add(map.Split(vaAligned));
                    }

                    if (map.EndAddress > endAddressAligned)
                    {
                        PrivateMapping newMap = map.Split(endAddressAligned);
                        _privateTree.Add(newMap);
                        map = newMap;
                    }

                    map.Map(Base, Mirror, _privateMemoryAllocator.Allocate(map.Size, MemoryBlock.GetPageSize()));
                }

                if (map.EndAddress >= endAddressAligned)
                {
                    break;
                }
            }
        }

        private void UnmapPrivate(ulong va, ulong size)
        {
            ulong endAddress = va + size;

            ulong alignment = MemoryBlock.GetPageSize();

            // Shrink the range inwards based on page size to ensure we won't unmap memory that might be still in use.
            ulong vaAligned = BitUtils.AlignUp(va, alignment);
            ulong endAddressAligned = BitUtils.AlignDown(endAddress, alignment);

            if (endAddressAligned <= vaAligned)
            {
                return;
            }

            PrivateMapping map = _privateTree.GetNode(new PrivateMapping(va, 1UL, default));

            for (; map != null; map = map.Successor)
            {
                if (map.PrivateAllocation.IsValid)
                {
                    if (map.Address < vaAligned)
                    {
                        _privateTree.Add(map.Split(vaAligned));
                    }

                    if (map.EndAddress > endAddressAligned)
                    {
                        PrivateMapping newMap = map.Split(endAddressAligned);
                        _privateTree.Add(newMap);
                        map = newMap;
                    }

                    map.Unmap(Base, Mirror);
                    map = TryCoalesce(map);
                }

                if (map.EndAddress >= endAddressAligned)
                {
                    break;
                }
            }
        }

        private PrivateMapping TryCoalesce(PrivateMapping map)
        {
            PrivateMapping previousMap = map.Predecessor;
            PrivateMapping nextMap = map.Successor;

            if (previousMap != null && CanCoalesce(previousMap, map))
            {
                previousMap.Extend(map.Size);
                _privateTree.Remove(map);
                map = previousMap;
            }

            if (nextMap != null && CanCoalesce(map, nextMap))
            {
                map.Extend(nextMap.Size);
                _privateTree.Remove(nextMap);
            }

            return map;
        }

        private static bool CanCoalesce(PrivateMapping left, PrivateMapping right)
        {
            return !left.PrivateAllocation.IsValid && !right.PrivateAllocation.IsValid;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            _privateMemoryAllocator?.Dispose();
            Base.Dispose();
            Mirror.Dispose();
        }
    }
}
