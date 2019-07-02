using ChocolArm64.Memory;
using Ryujinx.Graphics.Gal;
using System;

namespace Ryujinx.Graphics.Memory
{
    public class NvGpuVmm : IMemory, IGalMemory
    {
        public const long AddrSize = 1L << 40;

        private const int PtLvl0Bits = 14;
        private const int PtLvl1Bits = 14;
        private const int PtPageBits = 12;

        private const int PtLvl0Size = 1 << PtLvl0Bits;
        private const int PtLvl1Size = 1 << PtLvl1Bits;
        public  const int PageSize   = 1 << PtPageBits;

        private const int PtLvl0Mask = PtLvl0Size - 1;
        private const int PtLvl1Mask = PtLvl1Size - 1;
        public  const int PageMask   = PageSize   - 1;

        private const int PtLvl0Bit = PtPageBits + PtLvl1Bits;
        private const int PtLvl1Bit = PtPageBits;

        public MemoryManager Memory { get; private set; }

        private NvGpuVmmCache _cache;

        private const long PteUnmapped = -1;
        private const long PteReserved = -2;

        private long[][] _pageTable;

        public NvGpuVmm(MemoryManager memory)
        {
            Memory = memory;

            _cache = new NvGpuVmmCache(memory);

            _pageTable = new long[PtLvl0Size][];
        }

        public long Map(long pa, long va, long size)
        {
            lock (_pageTable)
            {
                for (long offset = 0; offset < size; offset += PageSize)
                {
                    SetPte(va + offset, pa + offset);
                }
            }

            return va;
        }

        public long Map(long pa, long size)
        {
            lock (_pageTable)
            {
                long va = GetFreePosition(size);

                if (va != -1)
                {
                    for (long offset = 0; offset < size; offset += PageSize)
                    {
                        SetPte(va + offset, pa + offset);
                    }
                }

                return va;
            }
        }

        public long MapLow(long pa, long size)
        {
            lock (_pageTable)
            {
                long va = GetFreePosition(size, 1, PageSize);

                if (va != -1 && (ulong)va <= uint.MaxValue && (ulong)(va + size) <= uint.MaxValue)
                {
                    for (long offset = 0; offset < size; offset += PageSize)
                    {
                        SetPte(va + offset, pa + offset);
                    }
                }
                else
                {
                    va = -1;
                }

                return va;
            }
        }

        public long ReserveFixed(long va, long size)
        {
            lock (_pageTable)
            {
                for (long offset = 0; offset < size; offset += PageSize)
                {
                    if (IsPageInUse(va + offset))
                    {
                        return -1;
                    }
                }

                for (long offset = 0; offset < size; offset += PageSize)
                {
                    SetPte(va + offset, PteReserved);
                }
            }

            return va;
        }

        public long Reserve(long size, long align)
        {
            lock (_pageTable)
            {
                long position = GetFreePosition(size, align);

                if (position != -1)
                {
                    for (long offset = 0; offset < size; offset += PageSize)
                    {
                        SetPte(position + offset, PteReserved);
                    }
                }

                return position;
            }
        }

        public void Free(long va, long size)
        {
            lock (_pageTable)
            {
                for (long offset = 0; offset < size; offset += PageSize)
                {
                    SetPte(va + offset, PteUnmapped);
                }
            }
        }

        private long GetFreePosition(long size, long align = 1, long start = 1L << 32)
        {
            // Note: Address 0 is not considered valid by the driver,
            // when 0 is returned it's considered a mapping error.
            long position = start;
            long freeSize = 0;

            if (align < 1)
            {
                align = 1;
            }

            align = (align + PageMask) & ~PageMask;

            while (position + freeSize < AddrSize)
            {
                if (!IsPageInUse(position + freeSize))
                {
                    freeSize += PageSize;

                    if (freeSize >= size)
                    {
                        return position;
                    }
                }
                else
                {
                    position += freeSize + PageSize;
                    freeSize  = 0;

                    long remainder = position % align;

                    if (remainder != 0)
                    {
                        position = (position - remainder) + align;
                    }
                }
            }

            return -1;
        }

        public long GetPhysicalAddress(long va)
        {
            long basePos = GetPte(va);

            if (basePos < 0)
            {
                return -1;
            }

            return basePos + (va & PageMask);
        }

        public bool IsRegionFree(long va, long size)
        {
            for (long offset = 0; offset < size; offset += PageSize)
            {
                if (IsPageInUse(va + offset))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsPageInUse(long va)
        {
            if (va >> PtLvl0Bits + PtLvl1Bits + PtPageBits != 0)
            {
                return false;
            }

            long l0 = (va >> PtLvl0Bit) & PtLvl0Mask;
            long l1 = (va >> PtLvl1Bit) & PtLvl1Mask;

            if (_pageTable[l0] == null)
            {
                return false;
            }

            return _pageTable[l0][l1] != PteUnmapped;
        }

        private long GetPte(long position)
        {
            long l0 = (position >> PtLvl0Bit) & PtLvl0Mask;
            long l1 = (position >> PtLvl1Bit) & PtLvl1Mask;

            if (_pageTable[l0] == null)
            {
                return -1;
            }

            return _pageTable[l0][l1];
        }

        private void SetPte(long position, long tgtAddr)
        {
            long l0 = (position >> PtLvl0Bit) & PtLvl0Mask;
            long l1 = (position >> PtLvl1Bit) & PtLvl1Mask;

            if (_pageTable[l0] == null)
            {
                _pageTable[l0] = new long[PtLvl1Size];

                for (int index = 0; index < PtLvl1Size; index++)
                {
                    _pageTable[l0][index] = PteUnmapped;
                }
            }

            _pageTable[l0][l1] = tgtAddr;
        }

        public bool IsRegionModified(long pa, long size, NvGpuBufferType bufferType)
        {
            return _cache.IsRegionModified(pa, size, bufferType);
        }

        public bool TryGetHostAddress(long position, long size, out IntPtr ptr)
        {
            return Memory.TryGetHostAddress(GetPhysicalAddress(position), size, out ptr);
        }

        public byte ReadByte(long position)
        {
            position = GetPhysicalAddress(position);

            return Memory.ReadByte(position);
        }

        public ushort ReadUInt16(long position)
        {
            position = GetPhysicalAddress(position);

            return Memory.ReadUInt16(position);
        }

        public uint ReadUInt32(long position)
        {
            position = GetPhysicalAddress(position);

            return Memory.ReadUInt32(position);
        }

        public ulong ReadUInt64(long position)
        {
            position = GetPhysicalAddress(position);

            return Memory.ReadUInt64(position);
        }

        public sbyte ReadSByte(long position)
        {
            position = GetPhysicalAddress(position);

            return Memory.ReadSByte(position);
        }

        public short ReadInt16(long position)
        {
            position = GetPhysicalAddress(position);

            return Memory.ReadInt16(position);
        }

        public int ReadInt32(long position)
        {
            position = GetPhysicalAddress(position);

            return Memory.ReadInt32(position);
        }

        public long ReadInt64(long position)
        {
            position = GetPhysicalAddress(position);

            return Memory.ReadInt64(position);
        }

        public byte[] ReadBytes(long position, long size)
        {
            position = GetPhysicalAddress(position);

            return Memory.ReadBytes(position, size);
        }

        public void WriteByte(long position, byte value)
        {
            position = GetPhysicalAddress(position);

            Memory.WriteByte(position, value);
        }

        public void WriteUInt16(long position, ushort value)
        {
            position = GetPhysicalAddress(position);

            Memory.WriteUInt16(position, value);
        }

        public void WriteUInt32(long position, uint value)
        {
            position = GetPhysicalAddress(position);

            Memory.WriteUInt32(position, value);
        }

        public void WriteUInt64(long position, ulong value)
        {
            position = GetPhysicalAddress(position);

            Memory.WriteUInt64(position, value);
        }

        public void WriteSByte(long position, sbyte value)
        {
            position = GetPhysicalAddress(position);

            Memory.WriteSByte(position, value);
        }

        public void WriteInt16(long position, short value)
        {
            position = GetPhysicalAddress(position);

            Memory.WriteInt16(position, value);
        }

        public void WriteInt32(long position, int value)
        {
            position = GetPhysicalAddress(position);

            Memory.WriteInt32(position, value);
        }

        public void WriteInt64(long position, long value)
        {
            position = GetPhysicalAddress(position);

            Memory.WriteInt64(position, value);
        }

        public void WriteBytes(long position, byte[] data)
        {
            position = GetPhysicalAddress(position);

            Memory.WriteBytes(position, data);
        }
    }
}