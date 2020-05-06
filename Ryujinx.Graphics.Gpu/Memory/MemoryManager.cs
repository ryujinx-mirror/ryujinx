using System;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// GPU memory manager.
    /// </summary>
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

        private readonly ulong[][] _pageTable;

        public event EventHandler<UnmapEventArgs> MemoryUnmapped;

        /// <summary>
        /// Creates a new instance of the GPU memory manager.
        /// </summary>
        public MemoryManager()
        {
            _pageTable = new ulong[PtLvl0Size][];
        }

        /// <summary>
        /// Maps a given range of pages to the specified CPU virtual address.
        /// </summary>
        /// <remarks>
        /// All addresses and sizes must be page aligned.
        /// </remarks>
        /// <param name="pa">CPU virtual address to map into</param>
        /// <param name="va">GPU virtual address to be mapped</param>
        /// <param name="size">Size in bytes of the mapping</param>
        /// <returns>GPU virtual address of the mapping</returns>
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

        /// <summary>
        /// Maps a given range of pages to an allocated GPU virtual address.
        /// The memory is automatically allocated by the memory manager.
        /// </summary>
        /// <param name="pa">CPU virtual address to map into</param>
        /// <param name="size">Size in bytes of the mapping</param>
        /// <param name="alignment">Required alignment of the GPU virtual address in bytes</param>
        /// <returns>GPU virtual address where the range was mapped, or an all ones mask in case of failure</returns>
        public ulong MapAllocate(ulong pa, ulong size, ulong alignment)
        {
            lock (_pageTable)
            {
                ulong va = GetFreePosition(size, alignment);

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

        /// <summary>
        /// Maps a given range of pages to an allocated GPU virtual address.
        /// The memory is automatically allocated by the memory manager.
        /// This also ensures that the mapping is always done in the first 4GB of GPU address space.
        /// </summary>
        /// <param name="pa">CPU virtual address to map into</param>
        /// <param name="size">Size in bytes of the mapping</param>
        /// <returns>GPU virtual address where the range was mapped, or an all ones mask in case of failure</returns>
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

        /// <summary>
        /// Reserves memory at a fixed GPU memory location.
        /// This prevents the reserved region from being used for memory allocation for map.
        /// </summary>
        /// <param name="va">GPU virtual address to reserve</param>
        /// <param name="size">Size in bytes of the reservation</param>
        /// <returns>GPU virtual address of the reservation, or an all ones mask in case of failure</returns>
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

        /// <summary>
        /// Reserves memory at any GPU memory location.
        /// </summary>
        /// <param name="size">Size in bytes of the reservation</param>
        /// <param name="alignment">Reservation address alignment in bytes</param>
        /// <returns>GPU virtual address of the reservation, or an all ones mask in case of failure</returns>
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

        /// <summary>
        /// Frees memory that was previously allocated by a map or reserved.
        /// </summary>
        /// <param name="va">GPU virtual address to free</param>
        /// <param name="size">Size in bytes of the region being freed</param>
        public void Free(ulong va, ulong size)
        {
            lock (_pageTable)
            {
                // Event handlers are not expected to be thread safe.
                MemoryUnmapped?.Invoke(this, new UnmapEventArgs(va, size));

                for (ulong offset = 0; offset < size; offset += PageSize)
                {
                    SetPte(va + offset, PteUnmapped);
                }
            }
        }

        /// <summary>
        /// Gets the address of an unused (free) region of the specified size.
        /// </summary>
        /// <param name="size">Size of the region in bytes</param>
        /// <param name="alignment">Required alignment of the region address in bytes</param>
        /// <param name="start">Start address of the search on the address space</param>
        /// <returns>GPU virtual address of the allocation, or an all ones mask in case of failure</returns>
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

        /// <summary>
        /// Translates a GPU virtual address to a CPU virtual address.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address to be translated</param>
        /// <returns>CPU virtual address</returns>
        public ulong Translate(ulong gpuVa)
        {
            ulong baseAddress = GetPte(gpuVa);

            if (baseAddress == PteUnmapped || baseAddress == PteReserved)
            {
                return PteUnmapped;
            }

            return baseAddress + (gpuVa & PageMask);
        }

        /// <summary>
        /// Checks if a given memory page is mapped or reserved.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address of the page</param>
        /// <returns>True if the page is mapped or reserved, false otherwise</returns>
        private bool IsPageInUse(ulong gpuVa)
        {
            if (gpuVa >> PtLvl0Bits + PtLvl1Bits + PtPageBits != 0)
            {
                return false;
            }

            ulong l0 = (gpuVa >> PtLvl0Bit) & PtLvl0Mask;
            ulong l1 = (gpuVa >> PtLvl1Bit) & PtLvl1Mask;

            if (_pageTable[l0] == null)
            {
                return false;
            }

            return _pageTable[l0][l1] != PteUnmapped;
        }

        /// <summary>
        /// Gets the Page Table entry for a given GPU virtual address.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address</param>
        /// <returns>Page table entry (CPU virtual address)</returns>
        private ulong GetPte(ulong gpuVa)
        {
            ulong l0 = (gpuVa >> PtLvl0Bit) & PtLvl0Mask;
            ulong l1 = (gpuVa >> PtLvl1Bit) & PtLvl1Mask;

            if (_pageTable[l0] == null)
            {
                return PteUnmapped;
            }

            return _pageTable[l0][l1];
        }

        /// <summary>
        /// Sets a Page Table entry at a given GPU virtual address.
        /// </summary>
        /// <param name="gpuVa">GPU virtual address</param>
        /// <param name="pte">Page table entry (CPU virtual address)</param>
        private void SetPte(ulong gpuVa, ulong pte)
        {
            ulong l0 = (gpuVa >> PtLvl0Bit) & PtLvl0Mask;
            ulong l1 = (gpuVa >> PtLvl1Bit) & PtLvl1Mask;

            if (_pageTable[l0] == null)
            {
                _pageTable[l0] = new ulong[PtLvl1Size];

                for (ulong index = 0; index < PtLvl1Size; index++)
                {
                    _pageTable[l0][index] = PteUnmapped;
                }
            }

            _pageTable[l0][l1] = pte;
        }
    }
}