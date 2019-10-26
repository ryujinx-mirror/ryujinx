namespace Ryujinx.Graphics.Gpu.Memory
{
    public class MemoryManager
    {
        private const ulong AddressSpaceSize = 1UL << 40;

        public const ulong BadAddress = ulong.MaxValue;

        private const int PtLvl0Bits = 14;
        private const int PtLvl1Bits = 14;
        public  const int PtPageBits = 12;

        private const ulong PtLvl0Size = 1UL << PtLvl0Bits;
        private const ulong PtLvl1Size = 1UL << PtLvl1Bits;
        public  const ulong PageSize   = 1UL << PtPageBits;

        private const ulong PtLvl0Mask = PtLvl0Size - 1;
        private const ulong PtLvl1Mask = PtLvl1Size - 1;
        public  const ulong PageMask   = PageSize   - 1;

        private const int PtLvl0Bit = PtPageBits + PtLvl1Bits;
        private const int PtLvl1Bit = PtPageBits;

        private const ulong PteUnmapped = 0xffffffff_ffffffff;
        private const ulong PteReserved = 0xffffffff_fffffffe;

        private ulong[][] _pageTable;

        public MemoryManager()
        {
            _pageTable = new ulong[PtLvl0Size][];
        }

        public ulong Map(ulong pa, ulong va, ulong size)
        {
            lock (_pageTable)
            {
                for (ulong offset = 0; offset < size; offset += PageSize)
                {
                    SetPte(va + offset, pa + offset);
                }
            }

            return va;
        }

        public ulong Map(ulong pa, ulong size)
        {
            lock (_pageTable)
            {
                ulong va = GetFreePosition(size);

                if (va != PteUnmapped)
                {
                    for (ulong offset = 0; offset < size; offset += PageSize)
                    {
                        SetPte(va + offset, pa + offset);
                    }
                }

                return va;
            }
        }

        public ulong MapLow(ulong pa, ulong size)
        {
            lock (_pageTable)
            {
                ulong va = GetFreePosition(size, 1, PageSize);

                if (va != PteUnmapped && va <= uint.MaxValue && (va + size) <= uint.MaxValue)
                {
                    for (ulong offset = 0; offset < size; offset += PageSize)
                    {
                        SetPte(va + offset, pa + offset);
                    }
                }
                else
                {
                    va = PteUnmapped;
                }

                return va;
            }
        }

        public ulong ReserveFixed(ulong va, ulong size)
        {
            lock (_pageTable)
            {
                for (ulong offset = 0; offset < size; offset += PageSize)
                {
                    if (IsPageInUse(va + offset))
                    {
                        return PteUnmapped;
                    }
                }

                for (ulong offset = 0; offset < size; offset += PageSize)
                {
                    SetPte(va + offset, PteReserved);
                }
            }

            return va;
        }

        public ulong Reserve(ulong size, ulong alignment)
        {
            lock (_pageTable)
            {
                ulong address = GetFreePosition(size, alignment);

                if (address != PteUnmapped)
                {
                    for (ulong offset = 0; offset < size; offset += PageSize)
                    {
                        SetPte(address + offset, PteReserved);
                    }
                }

                return address;
            }
        }

        public void Free(ulong va, ulong size)
        {
            lock (_pageTable)
            {
                for (ulong offset = 0; offset < size; offset += PageSize)
                {
                    SetPte(va + offset, PteUnmapped);
                }
            }
        }

        private ulong GetFreePosition(ulong size, ulong alignment = 1, ulong start = 1UL << 32)
        {
            // Note: Address 0 is not considered valid by the driver,
            // when 0 is returned it's considered a mapping error.
            ulong address  = start;
            ulong freeSize = 0;

            if (alignment == 0)
            {
                alignment = 1;
            }

            alignment = (alignment + PageMask) & ~PageMask;

            while (address + freeSize < AddressSpaceSize)
            {
                if (!IsPageInUse(address + freeSize))
                {
                    freeSize += PageSize;

                    if (freeSize >= size)
                    {
                        return address;
                    }
                }
                else
                {
                    address += freeSize + PageSize;
                    freeSize = 0;

                    ulong remainder = address % alignment;

                    if (remainder != 0)
                    {
                        address = (address - remainder) + alignment;
                    }
                }
            }

            return PteUnmapped;
        }

        internal ulong GetSubSize(ulong gpuVa)
        {
            ulong size = 0;

            while (GetPte(gpuVa + size) != PteUnmapped)
            {
                size += PageSize;
            }

            return size;
        }

        internal ulong Translate(ulong gpuVa)
        {
            ulong baseAddress = GetPte(gpuVa);

            if (baseAddress == PteUnmapped || baseAddress == PteReserved)
            {
                return PteUnmapped;
            }

            return baseAddress + (gpuVa & PageMask);
        }

        public bool IsRegionFree(ulong va, ulong size)
        {
            for (ulong offset = 0; offset < size; offset += PageSize)
            {
                if (IsPageInUse(va + offset))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsPageInUse(ulong va)
        {
            if (va >> PtLvl0Bits + PtLvl1Bits + PtPageBits != 0)
            {
                return false;
            }

            ulong l0 = (va >> PtLvl0Bit) & PtLvl0Mask;
            ulong l1 = (va >> PtLvl1Bit) & PtLvl1Mask;

            if (_pageTable[l0] == null)
            {
                return false;
            }

            return _pageTable[l0][l1] != PteUnmapped;
        }

        private ulong GetPte(ulong address)
        {
            ulong l0 = (address >> PtLvl0Bit) & PtLvl0Mask;
            ulong l1 = (address >> PtLvl1Bit) & PtLvl1Mask;

            if (_pageTable[l0] == null)
            {
                return PteUnmapped;
            }

            return _pageTable[l0][l1];
        }

        private void SetPte(ulong address, ulong tgtAddr)
        {
            ulong l0 = (address >> PtLvl0Bit) & PtLvl0Mask;
            ulong l1 = (address >> PtLvl1Bit) & PtLvl1Mask;

            if (_pageTable[l0] == null)
            {
                _pageTable[l0] = new ulong[PtLvl1Size];

                for (ulong index = 0; index < PtLvl1Size; index++)
                {
                    _pageTable[l0][index] = PteUnmapped;
                }
            }

            _pageTable[l0][l1] = tgtAddr;
        }
    }
}