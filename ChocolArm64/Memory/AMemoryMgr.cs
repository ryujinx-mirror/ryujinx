namespace ChocolArm64.Memory
{
    public class AMemoryMgr
    {
        public const long AddrSize = RamSize;
        public const long RamSize  = 4L * 1024 * 1024 * 1024;

        private const int  PTLvl0Bits = 11;
        private const int  PTLvl1Bits = 13;
        private const int  PTPageBits = 12;

        private const int  PTLvl0Size = 1 << PTLvl0Bits;
        private const int  PTLvl1Size = 1 << PTLvl1Bits;
        public  const int  PageSize   = 1 << PTPageBits;

        private const int  PTLvl0Mask = PTLvl0Size - 1;
        private const int  PTLvl1Mask = PTLvl1Size - 1;
        public  const int  PageMask   = PageSize   - 1;

        private const int  PTLvl0Bit  = PTPageBits + PTLvl0Bits;
        private const int  PTLvl1Bit  = PTPageBits;

        private AMemoryAlloc Allocator;

        private enum PTMap
        {
            Unmapped,
            Mapped
        }

        private struct PTEntry
        {
            public PTMap       Map;
            public AMemoryPerm Perm;

            public int Type;
            public int Attr;

            public PTEntry(PTMap Map, AMemoryPerm Perm, int Type, int Attr)
            {
                this.Map  = Map;
                this.Perm = Perm;
                this.Type = Type;
                this.Attr = Attr;
            }
        }

        private PTEntry[][] PageTable;

        private bool IsHeapInitialized;

        public long HeapAddr { get; private set; }
        public long HeapSize { get; private set; }

        public AMemoryMgr(AMemoryAlloc Allocator)
        {
            this.Allocator = Allocator;

            PageTable = new PTEntry[PTLvl0Size][];
        }

        public long GetTotalMemorySize()
        {
            return Allocator.GetFreeMem() + GetUsedMemorySize();
        }

        public long GetUsedMemorySize()
        {
            long Size = 0;

            for (int L0 = 0; L0 < PageTable.Length; L0++)
            {
                if (PageTable[L0] == null)
                {
                    continue;
                }

                for (int L1 = 0; L1 < PageTable[L0].Length; L1++)
                {
                    Size += PageTable[L0][L1].Map != PTMap.Unmapped ? PageSize : 0;
                }
            }

            return Size;
        }

        public bool SetHeapAddr(long Position)
        {
            if (!IsHeapInitialized)
            {
                HeapAddr = Position;

                IsHeapInitialized = true;

                return true;
            }

            return false;
        }

        public void SetHeapSize(long Size, int Type)
        {
            //TODO: Return error when theres no enough space to allocate heap.
            Size = AMemoryHelper.PageRoundUp(Size);

            long Position = HeapAddr;

            if ((ulong)Size < (ulong)HeapSize)
            {
                //Try to free now free area if size is smaller than old size.
                Position += Size;

                while ((ulong)Size < (ulong)HeapSize)
                {
                    Allocator.Free(Position);

                    Position += PageSize;
                }
            }
            else
            {
                //Allocate extra needed size.
                Position += HeapSize;
                Size     -= HeapSize;

                MapPhys(Position, Size, Type, AMemoryPerm.RW);
            }

            HeapSize = Size;
        }

        public void MapPhys(long Position, long Size, int Type, AMemoryPerm Perm)
        {
            while (Size > 0)
            {
                if (!IsMapped(Position))
                {
                    SetPTEntry(Position, new PTEntry(PTMap.Mapped, Perm, Type, 0));
                }

                long CPgSize = PageSize - (Position & PageMask);

                Position += CPgSize;
                Size     -= CPgSize;
            }
        }

        public void MapMirror(long Src, long Dst, long Size, int Type)
        {
            Src = AMemoryHelper.PageRoundDown(Src);
            Dst = AMemoryHelper.PageRoundDown(Dst);

            Size = AMemoryHelper.PageRoundUp(Size);

            long PagesCount = Size / PageSize;

            while (PagesCount-- > 0)
            {
                PTEntry SrcEntry = GetPTEntry(Src);
                PTEntry DstEntry = GetPTEntry(Dst);

                DstEntry.Map  = PTMap.Mapped;
                DstEntry.Type = Type;
                DstEntry.Perm = SrcEntry.Perm;

                SrcEntry.Perm = AMemoryPerm.None;

                SrcEntry.Attr |= 1;

                SetPTEntry(Src, SrcEntry);
                SetPTEntry(Dst, DstEntry);

                Src += PageSize;
                Dst += PageSize;
            }
        }

        public void Reprotect(long Position, long Size, AMemoryPerm Perm)
        {
            Position = AMemoryHelper.PageRoundDown(Position);

            Size = AMemoryHelper.PageRoundUp(Size);

            long PagesCount = Size / PageSize;

            while (PagesCount-- > 0)
            {
                PTEntry Entry = GetPTEntry(Position);

                Entry.Perm = Perm;

                SetPTEntry(Position, Entry);

                Position += PageSize;
            }
        }

        public AMemoryMapInfo GetMapInfo(long Position)
        {
            Position = AMemoryHelper.PageRoundDown(Position);

            PTEntry BaseEntry = GetPTEntry(Position);

            bool IsSameSegment(long Pos)
            {
                PTEntry Entry = GetPTEntry(Pos);

                return Entry.Map  == BaseEntry.Map  &&
                       Entry.Perm == BaseEntry.Perm &&
                       Entry.Type == BaseEntry.Type &&
                       Entry.Attr == BaseEntry.Attr;
            }

            long Start = Position;
            long End   = Position + PageSize;

            while (Start > 0 && IsSameSegment(Start - PageSize))
            {
                Start -= PageSize;
            }

            while (End < AddrSize && IsSameSegment(End))
            {
                End += PageSize;
            }

            long Size = End - Start;

            return new AMemoryMapInfo(
                Start,
                Size,
                BaseEntry.Type,
                BaseEntry.Attr,
                BaseEntry.Perm);
        }

        public void ClearAttrBit(long Position, long Size, int Bit)
        {
            while (Size > 0)
            {
                PTEntry Entry = GetPTEntry(Position);

                Entry.Attr &= ~(1 << Bit);

                SetPTEntry(Position, Entry);

                Position += PageSize;
                Size     -= PageSize;
            }
        }

        public void SetAttrBit(long Position, long Size, int Bit)
        {
            while (Size > 0)
            {
                PTEntry Entry = GetPTEntry(Position);

                Entry.Attr |= (1 << Bit);

                SetPTEntry(Position, Entry);

                Position += PageSize;
                Size     -= PageSize;
            }
        }

        public bool HasPermission(long Position, AMemoryPerm Perm)
        {
            return GetPTEntry(Position).Perm.HasFlag(Perm);
        }

        public bool IsMapped(long Position)
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

            return PageTable[L0][L1].Map != PTMap.Unmapped;
        }

        private PTEntry GetPTEntry(long Position)
        {
            long L0 = (Position >> PTLvl0Bit) & PTLvl0Mask;
            long L1 = (Position >> PTLvl1Bit) & PTLvl1Mask;

            if (PageTable[L0] == null)
            {
                return default(PTEntry);
            }

            return PageTable[L0][L1];
        }

        private void SetPTEntry(long Position, PTEntry Entry)
        {
            long L0 = (Position >> PTLvl0Bit) & PTLvl0Mask;
            long L1 = (Position >> PTLvl1Bit) & PTLvl1Mask;

            if (PageTable[L0] == null)
            {
                PageTable[L0] = new PTEntry[PTLvl1Size];
            }

            PageTable[L0][L1] = Entry;
        }
    }
}