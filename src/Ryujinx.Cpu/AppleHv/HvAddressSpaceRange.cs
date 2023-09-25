using Ryujinx.Cpu.AppleHv.Arm;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

namespace Ryujinx.Cpu.AppleHv
{
    [SupportedOSPlatform("macos")]
    class HvAddressSpaceRange : IDisposable
    {
        private const ulong AllocationGranule = 1UL << 14;

        private const ulong AttributesMask = (0x3ffUL << 2) | (0x3fffUL << 50);

        private const ulong BaseAttributes = (1UL << 10) | (3UL << 8); // Access flag set, inner shareable.

        private const int LevelBits = 9;
        private const int LevelCount = 1 << LevelBits;
        private const int LevelMask = LevelCount - 1;
        private const int PageBits = 12;
        private const int PageSize = 1 << PageBits;
        private const int PageMask = PageSize - 1;
        private const int AllLevelsMask = PageMask | (LevelMask << PageBits) | (LevelMask << (PageBits + LevelBits));

        private class PtLevel
        {
            public ulong Address => Allocation.Ipa + Allocation.Offset;
            public int EntriesCount;
            public readonly HvMemoryBlockAllocation Allocation;
            public readonly PtLevel[] Next;

            public PtLevel(HvMemoryBlockAllocator blockAllocator, int count, bool hasNext)
            {
                ulong size = (ulong)count * sizeof(ulong);
                Allocation = blockAllocator.Allocate(size, PageSize);

                AsSpan().Clear();

                if (hasNext)
                {
                    Next = new PtLevel[count];
                }
            }

            public Span<ulong> AsSpan()
            {
                return MemoryMarshal.Cast<byte, ulong>(Allocation.Memory.GetSpan(Allocation.Offset, (int)Allocation.Size));
            }
        }

        private PtLevel _level0;

        private int _tlbInvalidationPending;

        private readonly HvMemoryBlockAllocator _blockAllocator;

        public HvAddressSpaceRange(HvIpaAllocator ipaAllocator)
        {
            _blockAllocator = new HvMemoryBlockAllocator(ipaAllocator, (int)AllocationGranule);
        }

        public ulong GetIpaBase()
        {
            return EnsureLevel0().Address;
        }

        public bool GetAndClearTlbInvalidationPending()
        {
            return Interlocked.Exchange(ref _tlbInvalidationPending, 0) != 0;
        }

        public void Map(ulong va, ulong pa, ulong size, ApFlags accessPermission)
        {
            MapImpl(va, pa, size, (ulong)accessPermission | BaseAttributes);
        }

        public void Unmap(ulong va, ulong size)
        {
            UnmapImpl(EnsureLevel0(), 0, va, size);
            Interlocked.Exchange(ref _tlbInvalidationPending, 1);
        }

        public void Reprotect(ulong va, ulong size, ApFlags accessPermission)
        {
            UpdateAttributes(va, size, (ulong)accessPermission | BaseAttributes);
        }

        private void MapImpl(ulong va, ulong pa, ulong size, ulong attr)
        {
            PtLevel level0 = EnsureLevel0();

            ulong endVa = va + size;

            while (va < endVa)
            {
                (ulong mapSize, int depth) = GetMapSizeAndDepth(va, pa, endVa);

                PtLevel currentLevel = level0;

                for (int i = 0; i < depth; i++)
                {
                    int l = (int)(va >> (PageBits + (2 - i) * LevelBits)) & LevelMask;
                    EnsureTable(currentLevel, l, i == 0);
                    currentLevel = currentLevel.Next[l];
                }

                (ulong blockSize, int blockShift) = GetBlockSizeAndShift(depth);

                for (ulong i = 0; i < mapSize; i += blockSize)
                {
                    if ((va >> blockShift) << blockShift != va ||
                        (pa >> blockShift) << blockShift != pa)
                    {
                        Debug.Fail($"Block size 0x{blockSize:X} (log2: {blockShift}) is invalid for VA 0x{va:X} or PA 0x{pa:X}.");
                    }

                    WriteBlock(currentLevel, (int)(va >> blockShift) & LevelMask, depth, pa, attr);

                    va += blockSize;
                    pa += blockSize;
                }
            }
        }

        private void UnmapImpl(PtLevel level, int depth, ulong va, ulong size)
        {
            ulong endVa = (va + size + PageMask) & ~((ulong)PageMask);
            va &= ~((ulong)PageMask);

            (ulong blockSize, _) = GetBlockSizeAndShift(depth);

            while (va < endVa)
            {
                ulong nextEntryVa = GetNextAddress(va, blockSize);
                ulong chunckSize = Math.Min(endVa - va, nextEntryVa - va);

                int l = (int)(va >> (PageBits + (2 - depth) * LevelBits)) & LevelMask;

                PtLevel nextTable = level.Next?[l];

                if (nextTable != null)
                {
                    // Entry is a table, visit it and update attributes as required.
                    UnmapImpl(nextTable, depth + 1, va, chunckSize);
                }
                else if (chunckSize != blockSize)
                {
                    // Entry is a block but is not aligned, we need to turn it into a table.
                    ref ulong pte = ref level.AsSpan()[l];
                    nextTable = CreateTable(pte, depth + 1);
                    level.Next[l] = nextTable;

                    // Now that we have a table, we can handle it like the first case.
                    UnmapImpl(nextTable, depth + 1, va, chunckSize);

                    // Update PTE to point to the new table.
                    pte = (nextTable.Address & ~(ulong)PageMask) | 3UL;
                }

                // If entry is a block, or if entry is a table but it is empty, we can remove it.
                if (nextTable == null || nextTable.EntriesCount == 0)
                {
                    // Entry is a block and is fully aligned, so we can just set it to 0.
                    if (nextTable != null)
                    {
                        nextTable.Allocation.Dispose();
                        level.Next[l] = null;
                    }

                    level.AsSpan()[l] = 0UL;
                    level.EntriesCount--;
                    ValidateEntriesCount(level.EntriesCount);
                }

                va += chunckSize;
            }
        }

        private void UpdateAttributes(ulong va, ulong size, ulong newAttr)
        {
            UpdateAttributes(EnsureLevel0(), 0, va, size, newAttr);

            Interlocked.Exchange(ref _tlbInvalidationPending, 1);
        }

        private void UpdateAttributes(PtLevel level, int depth, ulong va, ulong size, ulong newAttr)
        {
            ulong endVa = (va + size + PageSize - 1) & ~((ulong)PageSize - 1);
            va &= ~((ulong)PageSize - 1);

            (ulong blockSize, _) = GetBlockSizeAndShift(depth);

            while (va < endVa)
            {
                ulong nextEntryVa = GetNextAddress(va, blockSize);
                ulong chunckSize = Math.Min(endVa - va, nextEntryVa - va);

                int l = (int)(va >> (PageBits + (2 - depth) * LevelBits)) & LevelMask;

                ref ulong pte = ref level.AsSpan()[l];

                // First check if the region is mapped.
                if ((pte & 3) != 0)
                {
                    PtLevel nextTable = level.Next?[l];

                    if (nextTable != null)
                    {
                        // Entry is a table, visit it and update attributes as required.
                        UpdateAttributes(nextTable, depth + 1, va, chunckSize, newAttr);
                    }
                    else if (chunckSize != blockSize)
                    {
                        // Entry is a block but is not aligned, we need to turn it into a table.
                        nextTable = CreateTable(pte, depth + 1);
                        level.Next[l] = nextTable;

                        // Now that we have a table, we can handle it like the first case.
                        UpdateAttributes(nextTable, depth + 1, va, chunckSize, newAttr);

                        // Update PTE to point to the new table.
                        pte = (nextTable.Address & ~(ulong)PageMask) | 3UL;
                    }
                    else
                    {
                        // Entry is a block and is fully aligned, so we can just update the attributes.
                        // Update PTE with the new attributes.
                        pte = (pte & ~AttributesMask) | newAttr;
                    }
                }

                va += chunckSize;
            }
        }

        private PtLevel CreateTable(ulong pte, int depth)
        {
            pte &= ~3UL;
            pte |= (depth == 2 ? 3UL : 1UL);

            PtLevel level = new(_blockAllocator, LevelCount, depth < 2);
            Span<ulong> currentLevel = level.AsSpan();

            (_, int blockShift) = GetBlockSizeAndShift(depth);

            // Fill in the blocks.
            for (int i = 0; i < LevelCount; i++)
            {
                ulong offset = (ulong)i << blockShift;
                currentLevel[i] = pte + offset;
            }

            level.EntriesCount = LevelCount;

            return level;
        }

        private static (ulong, int) GetBlockSizeAndShift(int depth)
        {
            int blockShift = PageBits + (2 - depth) * LevelBits;
            ulong blockSize = 1UL << blockShift;

            return (blockSize, blockShift);
        }

        private static (ulong, int) GetMapSizeAndDepth(ulong va, ulong pa, ulong endVa)
        {
            // Both virtual and physical addresses must be aligned to the block size.
            ulong combinedAddress = va | pa;

            ulong l0Alignment = 1UL << (PageBits + LevelBits * 2);
            ulong l1Alignment = 1UL << (PageBits + LevelBits);

            if ((combinedAddress & (l0Alignment - 1)) == 0 && AlignDown(endVa, l0Alignment) > va)
            {
                return (AlignDown(endVa, l0Alignment) - va, 0);
            }
            else if ((combinedAddress & (l1Alignment - 1)) == 0 && AlignDown(endVa, l1Alignment) > va)
            {
                ulong nextOrderVa = GetNextAddress(va, l0Alignment);

                if (nextOrderVa <= endVa)
                {
                    return (nextOrderVa - va, 1);
                }
                else
                {
                    return (AlignDown(endVa, l1Alignment) - va, 1);
                }
            }
            else
            {
                ulong nextOrderVa = GetNextAddress(va, l1Alignment);

                if (nextOrderVa <= endVa)
                {
                    return (nextOrderVa - va, 2);
                }
                else
                {
                    return (endVa - va, 2);
                }
            }
        }

        private static ulong AlignDown(ulong va, ulong alignment)
        {
            return va & ~(alignment - 1);
        }

        private static ulong GetNextAddress(ulong va, ulong alignment)
        {
            return (va + alignment) & ~(alignment - 1);
        }

        private PtLevel EnsureLevel0()
        {
            PtLevel level0 = _level0;

            if (level0 == null)
            {
                level0 = new PtLevel(_blockAllocator, LevelCount, true);
                _level0 = level0;
            }

            return level0;
        }

        private void EnsureTable(PtLevel level, int index, bool hasNext)
        {
            Span<ulong> currentTable = level.AsSpan();

            if ((currentTable[index] & 1) == 0)
            {
                PtLevel nextLevel = new(_blockAllocator, LevelCount, hasNext);

                currentTable[index] = (nextLevel.Address & ~(ulong)PageMask) | 3UL;
                level.Next[index] = nextLevel;
                level.EntriesCount++;
                ValidateEntriesCount(level.EntriesCount);
            }
            else if (level.Next[index] == null)
            {
                Debug.Fail($"Index {index} is block, expected a table.");
            }
        }

        private static void WriteBlock(PtLevel level, int index, int depth, ulong pa, ulong attr)
        {
            Span<ulong> currentTable = level.AsSpan();

            currentTable[index] = (pa & ~((ulong)AllLevelsMask >> (depth * LevelBits))) | (depth == 2 ? 3UL : 1UL) | attr;

            level.EntriesCount++;
            ValidateEntriesCount(level.EntriesCount);
        }

        private static void ValidateEntriesCount(int count)
        {
            Debug.Assert(count >= 0 && count <= LevelCount, $"Entries count {count} is invalid.");
        }

        public void Dispose()
        {
            _blockAllocator.Dispose();
        }
    }
}
