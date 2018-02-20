namespace Ryujinx.Graphics.Gpu
{
    class NsGpuMemoryMgr
    {
        private const long AddrSize   = 1L << 40;

        private const int  PTLvl0Bits = 14;
        private const int  PTLvl1Bits = 14;
        private const int  PTPageBits = 12;

        private const int  PTLvl0Size = 1 << PTLvl0Bits;
        private const int  PTLvl1Size = 1 << PTLvl1Bits;
        private const int  PageSize   = 1 << PTPageBits;

        private const int  PTLvl0Mask = PTLvl0Size - 1;
        private const int  PTLvl1Mask = PTLvl1Size - 1;
        private const int  PageMask   = PageSize   - 1;

        private const int  PTLvl0Bit  = PTPageBits + PTLvl0Bits;
        private const int  PTLvl1Bit  = PTPageBits;

        private const long PteUnmapped = -1;
        private const long PteReserved = -2;

        private long[][] PageTable;

        public NsGpuMemoryMgr()
        {
            PageTable = new long[PTLvl0Size][];
        }

        public long Map(long CpuAddr, long GpuAddr, long Size)
        {
            CpuAddr &= ~PageMask;
            GpuAddr &= ~PageMask;

            for (long Offset = 0; Offset < Size; Offset += PageSize)
            {
                if (GetPTAddr(GpuAddr + Offset) != PteReserved)
                {
                    return Map(CpuAddr, Size);
                }
            }

            for (long Offset = 0; Offset < Size; Offset += PageSize)
            {
                SetPTAddr(GpuAddr + Offset, CpuAddr + Offset);
            }

            return GpuAddr;
        }

        public long Map(long CpuAddr, long Size)
        {
            CpuAddr &= ~PageMask;

            long Position = GetFreePosition(Size);
            
            if (Position != -1)
            {
                for (long Offset = 0; Offset < Size; Offset += PageSize)
                {
                    SetPTAddr(Position + Offset, CpuAddr + Offset);
                }
            }

            return Position;
        }

        public long Reserve(long GpuAddr, long Size, long Align)
        {
            for (long Offset = 0; Offset < Size; Offset += PageSize)
            {
                if (HasPTAddr(GpuAddr + Offset))
                {
                    return Reserve(Size, Align);
                }
            }

            for (long Offset = 0; Offset < Size; Offset += PageSize)
            {
                SetPTAddr(GpuAddr + Offset, PteReserved);
            }

            return GpuAddr;
        }

        public long Reserve(long Size, long Align)
        {
            long Position = GetFreePosition(Size, Align);

            if (Position != -1)
            {
                for (long Offset = 0; Offset < Size; Offset += PageSize)
                {
                    SetPTAddr(Position + Offset, PteReserved);
                }
            }

            return Position;
        }

        private long GetFreePosition(long Size, long Align = 1)
        {
            long Position = 0;
            long FreeSize = 0;

            if (Align < 1)
            {
                Align = 1;
            }

            Align = (Align + PageMask) & ~PageMask;

            while (Position + FreeSize < AddrSize)
            {
                if (!HasPTAddr(Position + FreeSize))
                {
                    FreeSize += PageSize;

                    if (FreeSize >= Size)
                    {
                        return Position;
                    }
                }
                else
                {
                    Position += FreeSize + PageSize;
                    FreeSize  = 0;

                    long Remainder = Position % Align;

                    if (Remainder != 0)
                    {
                        Position = (Position - Remainder) + Align;
                    }
                }
            }

            return -1;
        }

        public long GetCpuAddr(long Position)
        {
            long BasePos = GetPTAddr(Position);

            if (BasePos < 0)
            {
                return -1;
            }

            return BasePos + (Position & PageMask);
        }

        private bool HasPTAddr(long Position)
        {
            if (Position >> PTLvl0Bits + PTLvl1Bits + PTPageBits != 0)
            {
                return false;
            }

            long L0 = (Position >> PTLvl0Bit) & PTLvl0Mask;
            long L1 = (Position >> PTLvl1Bit) & PTLvl1Mask;

            if (PageTable[L0] == null)
            {
                return false;
            }

            return PageTable[L0][L1] != PteUnmapped;
        }

        private long GetPTAddr(long Position)
        {
            long L0 = (Position >> PTLvl0Bit) & PTLvl0Mask;
            long L1 = (Position >> PTLvl1Bit) & PTLvl1Mask;

            if (PageTable[L0] == null)
            {
                return -1;
            }

            return PageTable[L0][L1];
        }

        private void SetPTAddr(long Position, long TgtAddr)
        {
            long L0 = (Position >> PTLvl0Bit) & PTLvl0Mask;
            long L1 = (Position >> PTLvl1Bit) & PTLvl1Mask;

            if (PageTable[L0] == null)
            {
                PageTable[L0] = new long[PTLvl1Size];

                for (int Index = 0; Index < PTLvl1Size; Index++)
                {
                    PageTable[L0][Index] = PteUnmapped;
                }
            }

            PageTable[L0][L1] = TgtAddr;
        }
    }
}