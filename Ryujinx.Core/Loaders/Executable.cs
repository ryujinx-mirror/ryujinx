using ChocolArm64.Memory;
using Ryujinx.Core.Loaders.Executables;
using Ryujinx.Core.OsHle;
using System.Collections.Generic;

namespace Ryujinx.Core.Loaders
{
    class Executable
    {
        private List<ElfDyn> Dynamic;

        private Dictionary<long, string> m_SymbolTable;

        public IReadOnlyDictionary<long, string> SymbolTable => m_SymbolTable;

        public string Name { get; private set; }

        private AMemory Memory;

        public long ImageBase { get; private set; }
        public long ImageEnd  { get; private set; }

        public Executable(IExecutable Exe, AMemory Memory, long ImageBase)
        {
            Dynamic = new List<ElfDyn>();

            m_SymbolTable = new Dictionary<long, string>();

            Name = Exe.Name;

            this.Memory    = Memory;
            this.ImageBase = ImageBase;
            this.ImageEnd  = ImageBase;

            WriteData(ImageBase + Exe.TextOffset, Exe.Text, MemoryType.CodeStatic, AMemoryPerm.RX);
            WriteData(ImageBase + Exe.ROOffset,   Exe.RO,   MemoryType.Normal,     AMemoryPerm.Read);
            WriteData(ImageBase + Exe.DataOffset, Exe.Data, MemoryType.Normal,     AMemoryPerm.RW);

            if (Exe.Mod0Offset == 0)
            {
                int BssOffset = Exe.DataOffset + Exe.Data.Length;
                int BssSize   = Exe.BssSize;

                MapBss(ImageBase + BssOffset, BssSize);

                ImageEnd = ImageBase + BssOffset + BssSize;

                return;
            }

            long Mod0Offset = ImageBase + Exe.Mod0Offset;

            int  Mod0Magic        = Memory.ReadInt32(Mod0Offset + 0x0);
            long DynamicOffset    = Memory.ReadInt32(Mod0Offset + 0x4)  + Mod0Offset;
            long BssStartOffset   = Memory.ReadInt32(Mod0Offset + 0x8)  + Mod0Offset;
            long BssEndOffset     = Memory.ReadInt32(Mod0Offset + 0xc)  + Mod0Offset;
            long EhHdrStartOffset = Memory.ReadInt32(Mod0Offset + 0x10) + Mod0Offset;
            long EhHdrEndOffset   = Memory.ReadInt32(Mod0Offset + 0x14) + Mod0Offset;
            long ModObjOffset     = Memory.ReadInt32(Mod0Offset + 0x18) + Mod0Offset;

            MapBss(BssStartOffset, BssEndOffset - BssStartOffset);

            ImageEnd = BssEndOffset;

            while (true)
            {
                long TagVal = Memory.ReadInt64(DynamicOffset + 0);
                long Value  = Memory.ReadInt64(DynamicOffset + 8);

                DynamicOffset += 0x10;

                ElfDynTag Tag = (ElfDynTag)TagVal;

                if (Tag == ElfDynTag.DT_NULL)
                {
                    break;
                }

                Dynamic.Add(new ElfDyn(Tag, Value));
            }

            long StrTblAddr = ImageBase + GetFirstValue(ElfDynTag.DT_STRTAB);
            long SymTblAddr = ImageBase + GetFirstValue(ElfDynTag.DT_SYMTAB);

            long SymEntSize = GetFirstValue(ElfDynTag.DT_SYMENT);

            while ((ulong)SymTblAddr < (ulong)StrTblAddr)
            {
                ElfSym Sym = GetSymbol(SymTblAddr, StrTblAddr);

                m_SymbolTable.TryAdd(Sym.Value, Sym.Name);

                SymTblAddr += SymEntSize;
            }
        }

        private void WriteData(
            long        Position,
            byte[]      Data,
            MemoryType  Type,
            AMemoryPerm Perm)
        {
            Memory.Manager.Map(Position, Data.Length, (int)Type, AMemoryPerm.Write);

            AMemoryHelper.WriteBytes(Memory, Position, Data);

            Memory.Manager.Reprotect(Position, Data.Length, Perm);
        }

        private void MapBss(long Position, long Size)
        {
            Memory.Manager.Map(Position, Size, (int)MemoryType.Normal, AMemoryPerm.RW);
        }

        private ElfRel GetRelocation(long Position)
        {
            long Offset = Memory.ReadInt64(Position + 0);
            long Info   = Memory.ReadInt64(Position + 8);
            long Addend = Memory.ReadInt64(Position + 16);

            int RelType = (int)(Info >> 0);
            int SymIdx  = (int)(Info >> 32);

            ElfSym Symbol = GetSymbol(SymIdx);

            return new ElfRel(Offset, Addend, Symbol, (ElfRelType)RelType);
        }

        private ElfSym GetSymbol(int Index)
        {
            long StrTblAddr = ImageBase + GetFirstValue(ElfDynTag.DT_STRTAB);
            long SymTblAddr = ImageBase + GetFirstValue(ElfDynTag.DT_SYMTAB);

            long SymEntSize = GetFirstValue(ElfDynTag.DT_SYMENT);

            long Position = SymTblAddr + Index * SymEntSize;

            return GetSymbol(Position, StrTblAddr);
        }

        private ElfSym GetSymbol(long Position, long StrTblAddr)
        {
            int  NameIndex = Memory.ReadInt32(Position + 0);
            int  Info      = Memory.ReadByte(Position + 4);
            int  Other     = Memory.ReadByte(Position + 5);
            int  SHIdx     = Memory.ReadInt16(Position + 6);
            long Value     = Memory.ReadInt64(Position + 8);
            long Size      = Memory.ReadInt64(Position + 16);

            string Name = string.Empty;

            for (int Chr; (Chr = Memory.ReadByte(StrTblAddr + NameIndex++)) != 0;)
            {
                Name += (char)Chr;
            }

            return new ElfSym(Name, Info, Other, SHIdx, Value, Size);
        }

        private long GetFirstValue(ElfDynTag Tag)
        {
            foreach (ElfDyn Entry in Dynamic)
            {
                if (Entry.Tag == Tag)
                {
                    return Entry.Value;
                }
            }

            return 0;
        }
    }
}