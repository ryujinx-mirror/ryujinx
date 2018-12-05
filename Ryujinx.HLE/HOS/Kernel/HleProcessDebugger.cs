using ChocolArm64.Memory;
using ChocolArm64.State;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Diagnostics.Demangler;
using Ryujinx.HLE.Loaders.Elf;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel
{
    class HleProcessDebugger
    {
        private const int Mod0 = 'M' << 0 | 'O' << 8 | 'D' << 16 | '0' << 24;

        private KProcess Owner;

        private class Image
        {
            public long BaseAddress { get; private set; }

            public ElfSymbol[] Symbols { get; private set; }

            public Image(long BaseAddress, ElfSymbol[] Symbols)
            {
                this.BaseAddress = BaseAddress;
                this.Symbols     = Symbols;
            }
        }

        private List<Image> Images;

        private int Loaded;

        public HleProcessDebugger(KProcess Owner)
        {
            this.Owner = Owner;

            Images = new List<Image>();
        }

        public void PrintGuestStackTrace(CpuThreadState ThreadState)
        {
            EnsureLoaded();

            StringBuilder Trace = new StringBuilder();

            Trace.AppendLine("Guest stack trace:");

            void AppendTrace(long Address)
            {
                Image Image = GetImage(Address, out int ImageIndex);

                if (Image == null || !TryGetSubName(Image, Address, out string SubName))
                {
                    SubName = $"Sub{Address:x16}";
                }
                else if (SubName.StartsWith("_Z"))
                {
                    SubName = Demangler.Parse(SubName);
                }

                if (Image != null)
                {
                    long Offset = Address - Image.BaseAddress;

                    string ImageName = GetGuessedNsoNameFromIndex(ImageIndex);

                    string ImageNameAndOffset = $"[{Owner.Name}] {ImageName}:0x{Offset:x8}";

                    Trace.AppendLine($" {ImageNameAndOffset} {SubName}");
                }
                else
                {
                    Trace.AppendLine($" [{Owner.Name}] ??? {SubName}");
                }
            }

            long FramePointer = (long)ThreadState.X29;

            while (FramePointer != 0)
            {
                if ((FramePointer & 7) != 0                 ||
                    !Owner.CpuMemory.IsMapped(FramePointer) ||
                    !Owner.CpuMemory.IsMapped(FramePointer + 8))
                {
                    break;
                }

                //Note: This is the return address, we need to subtract one instruction
                //worth of bytes to get the branch instruction address.
                AppendTrace(Owner.CpuMemory.ReadInt64(FramePointer + 8) - 4);

                FramePointer = Owner.CpuMemory.ReadInt64(FramePointer);
            }

            Logger.PrintInfo(LogClass.Cpu, Trace.ToString());
        }

        private bool TryGetSubName(Image Image, long Address, out string Name)
        {
            Address -= Image.BaseAddress;

            int Left  = 0;
            int Right = Image.Symbols.Length - 1;

            while (Left <= Right)
            {
                int Size = Right - Left;

                int Middle = Left + (Size >> 1);

                ElfSymbol Symbol = Image.Symbols[Middle];

                long EndAddr = Symbol.Value + Symbol.Size;

                if ((ulong)Address >= (ulong)Symbol.Value && (ulong)Address < (ulong)EndAddr)
                {
                    Name = Symbol.Name;

                    return true;
                }

                if ((ulong)Address < (ulong)Symbol.Value)
                {
                    Right = Middle - 1;
                }
                else
                {
                    Left = Middle + 1;
                }
            }

            Name = null;

            return false;
        }

        private Image GetImage(long Address, out int Index)
        {
            lock (Images)
            {
                for (Index = Images.Count - 1; Index >= 0; Index--)
                {
                    if ((ulong)Address >= (ulong)Images[Index].BaseAddress)
                    {
                        return Images[Index];
                    }
                }
            }

            return null;
        }

        private string GetGuessedNsoNameFromIndex(int Index)
        {
            if ((uint)Index > 11)
            {
                return "???";
            }

            if (Index == 0)
            {
                return "rtld";
            }
            else if (Index == 1)
            {
                return "main";
            }
            else if (Index == GetImagesCount() - 1)
            {
                return "sdk";
            }
            else
            {
                return "subsdk" + (Index - 2);
            }
        }

        private int GetImagesCount()
        {
            lock (Images)
            {
                return Images.Count;
            }
        }

        private void EnsureLoaded()
        {
            if (Interlocked.CompareExchange(ref Loaded, 1, 0) == 0)
            {
                ScanMemoryForTextSegments();
            }
        }

        private void ScanMemoryForTextSegments()
        {
            ulong OldAddress = 0;
            ulong Address    = 0;

            while (Address >= OldAddress)
            {
                KMemoryInfo Info = Owner.MemoryManager.QueryMemory(Address);

                if (Info.State == MemoryState.Reserved)
                {
                    break;
                }

                if (Info.State == MemoryState.CodeStatic && Info.Permission == MemoryPermission.ReadAndExecute)
                {
                    LoadMod0Symbols(Owner.CpuMemory, (long)Info.Address);
                }

                OldAddress = Address;

                Address = Info.Address + Info.Size;
            }
        }

        private void LoadMod0Symbols(MemoryManager Memory, long TextOffset)
        {
            long Mod0Offset = TextOffset + Memory.ReadUInt32(TextOffset + 4);

            if (Mod0Offset < TextOffset || !Memory.IsMapped(Mod0Offset) || (Mod0Offset & 3) != 0)
            {
                return;
            }

            Dictionary<ElfDynamicTag, long> Dynamic = new Dictionary<ElfDynamicTag, long>();

            int Mod0Magic = Memory.ReadInt32(Mod0Offset + 0x0);

            if (Mod0Magic != Mod0)
            {
                return;
            }

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

                ElfDynamicTag Tag = (ElfDynamicTag)TagVal;

                if (Tag == ElfDynamicTag.DT_NULL)
                {
                    break;
                }

                Dynamic[Tag] = Value;
            }

            if (!Dynamic.TryGetValue(ElfDynamicTag.DT_STRTAB, out long StrTab) ||
                !Dynamic.TryGetValue(ElfDynamicTag.DT_SYMTAB, out long SymTab) ||
                !Dynamic.TryGetValue(ElfDynamicTag.DT_SYMENT, out long SymEntSize))
            {
                return;
            }

            long StrTblAddr = TextOffset + StrTab;
            long SymTblAddr = TextOffset + SymTab;

            List<ElfSymbol> Symbols = new List<ElfSymbol>();

            while ((ulong)SymTblAddr < (ulong)StrTblAddr)
            {
                ElfSymbol Sym = GetSymbol(Memory, SymTblAddr, StrTblAddr);

                Symbols.Add(Sym);

                SymTblAddr += SymEntSize;
            }

            lock (Images)
            {
                Images.Add(new Image(TextOffset, Symbols.OrderBy(x => x.Value).ToArray()));
            }
        }

        private ElfSymbol GetSymbol(MemoryManager Memory, long Address, long StrTblAddr)
        {
            int  NameIndex = Memory.ReadInt32(Address + 0);
            int  Info      = Memory.ReadByte (Address + 4);
            int  Other     = Memory.ReadByte (Address + 5);
            int  SHIdx     = Memory.ReadInt16(Address + 6);
            long Value     = Memory.ReadInt64(Address + 8);
            long Size      = Memory.ReadInt64(Address + 16);

            string Name = string.Empty;

            for (int Chr; (Chr = Memory.ReadByte(StrTblAddr + NameIndex++)) != 0;)
            {
                Name += (char)Chr;
            }

            return new ElfSymbol(Name, Info, Other, SHIdx, Value, Size);
        }
    }
}