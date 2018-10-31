using ChocolArm64.Memory;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Ryujinx.HLE.Loaders
{
    class Executable
    {
        private MemoryManager Memory;

        private List<ElfDyn> Dynamic;

        public ReadOnlyCollection<ElfSym> SymbolTable;

        public string Name { get; private set; }

        public string FilePath { get; private set; }

        public long ImageBase { get; private set; }
        public long ImageEnd  { get; private set; }

        private KMemoryManager MemoryManager;

        public Executable(IExecutable Exe, KMemoryManager MemoryManager, MemoryManager Memory, long ImageBase)
        {
            Dynamic = new List<ElfDyn>();

            FilePath = Exe.FilePath;

            if (FilePath != null)
            {
                Name = Path.GetFileNameWithoutExtension(FilePath.Replace(Homebrew.TemporaryNroSuffix, ""));
            }

            this.Memory        = Memory;
            this.MemoryManager = MemoryManager;
            this.ImageBase     = ImageBase;
            this.ImageEnd      = ImageBase;

            long TextPosition = ImageBase + (uint)Exe.TextOffset;
            long ROPosition   = ImageBase + (uint)Exe.ROOffset;
            long DataPosition = ImageBase + (uint)Exe.DataOffset;

            long TextSize = (uint)IntUtils.AlignUp(Exe.Text.Length, KMemoryManager.PageSize);
            long ROSize   = (uint)IntUtils.AlignUp(Exe.RO.Length, KMemoryManager.PageSize);
            long DataSize = (uint)IntUtils.AlignUp(Exe.Data.Length, KMemoryManager.PageSize);
            long BssSize  = (uint)IntUtils.AlignUp(Exe.BssSize, KMemoryManager.PageSize);

            long DataAndBssSize = BssSize + DataSize;

            ImageEnd = DataPosition + DataAndBssSize;

            if (Exe.SourceAddress == 0)
            {
                MemoryManager.HleMapProcessCode(TextPosition, TextSize + ROSize + DataAndBssSize);

                MemoryManager.SetProcessMemoryPermission(ROPosition, ROSize, MemoryPermission.Read);
                MemoryManager.SetProcessMemoryPermission(DataPosition, DataAndBssSize, MemoryPermission.ReadAndWrite);

                Memory.WriteBytes(TextPosition, Exe.Text);
                Memory.WriteBytes(ROPosition, Exe.RO);
                Memory.WriteBytes(DataPosition, Exe.Data);
            }
            else
            {
                long Result = MemoryManager.MapProcessCodeMemory(TextPosition, Exe.SourceAddress, TextSize + ROSize + DataSize);

                if (Result != 0)
                {
                    throw new InvalidOperationException();
                }

                MemoryManager.SetProcessMemoryPermission(ROPosition, ROSize, MemoryPermission.Read);
                MemoryManager.SetProcessMemoryPermission(DataPosition, DataSize, MemoryPermission.ReadAndWrite);

                if (Exe.BssAddress != 0 && Exe.BssSize != 0)
                {
                    Result = MemoryManager.MapProcessCodeMemory(DataPosition + DataSize, Exe.BssAddress, BssSize);

                    if (Result != 0)
                    {
                        throw new InvalidOperationException();
                    }

                    MemoryManager.SetProcessMemoryPermission(DataPosition + DataSize, BssSize, MemoryPermission.ReadAndWrite);
                }
            }

            if (Exe.Mod0Offset == 0)
            {
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

            List<ElfSym> Symbols = new List<ElfSym>();

            while ((ulong)SymTblAddr < (ulong)StrTblAddr)
            {
                ElfSym Sym = GetSymbol(SymTblAddr, StrTblAddr);

                Symbols.Add(Sym);

                SymTblAddr += SymEntSize;
            }

            SymbolTable = Array.AsReadOnly(Symbols.OrderBy(x => x.Value).ToArray());
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