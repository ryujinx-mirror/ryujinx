using System;

namespace ChocolArm64.Memory
{
    public class AMemoryMgr
    {
        public const long RamSize  = 4L * 1024 * 1024 * 1024;
        public const long AddrSize = RamSize;

        private const int PTLvl0Bits = 10;
        private const int PTLvl1Bits = 10;
        private const int PTPageBits = 12;

        private const int PTLvl0Size = 1 << PTLvl0Bits;
        private const int PTLvl1Size = 1 << PTLvl1Bits;
        public  const int PageSize   = 1 << PTPageBits;

        private const int PTLvl0Mask = PTLvl0Size - 1;
        private const int PTLvl1Mask = PTLvl1Size - 1;
        public  const int PageMask   = PageSize   - 1;

        private const int PTLvl0Bit = PTPageBits + PTLvl1Bits;
        private const int PTLvl1Bit = PTPageBits;

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

        public AMemoryMgr()
        {
            PageTable = new PTEntry[PTLvl0Size][];
        }

        public void Map(long Position, long Size, int Type, AMemoryPerm Perm)
        {
            SetPTEntry(Position, Size, new PTEntry(PTMap.Mapped, Perm, Type, 0));
        }

        public void Unmap(long Position, long Size)
        {
            SetPTEntry(Position, Size, new PTEntry(PTMap.Unmapped, 0, 0, 0));
        }

        public void Unmap(long Position, long Size, int Type)
        {
            SetPTEntry(Position, Size, Type, new PTEntry(PTMap.Unmapped, 0, 0, 0));
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
            if (!IsValidPosition(Position))
            {
                return null;
            }

            Position = AMemoryHelper.PageRoundDown(Position);

            PTEntry BaseEntry = GetPTEntry(Position);

            bool IsSameSegment(long Pos)
            {
                if (!IsValidPosition(Pos))
                {
                    return false;
                }

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

        public bool IsValidPosition(long Position)
        {
            if (Position >> PTLvl0Bits + PTLvl1Bits + PTPageBits != 0)
            {
                return false;
            }

            return true;
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

        private void SetPTEntry(long Position, long Size, PTEntry Entry)
        {
            while (Size > 0)
            {
                SetPTEntry(Position, Entry);

                Position += PageSize;
                Size     -= PageSize;
            }
        }

        private void SetPTEntry(long Position, long Size, int Type, PTEntry Entry)
        {
            while (Size > 0)
            {
                if (GetPTEntry(Position).Type == Type)
                {
                    SetPTEntry(Position, Entry);
                }

                Position += PageSize;
                Size     -= PageSize;
            }
        }

        private void SetPTEntry(long Position, PTEntry Entry)
        {
            if (!IsValidPosition(Position))
            {
                throw new ArgumentOutOfRangeException(nameof(Position));
            }

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