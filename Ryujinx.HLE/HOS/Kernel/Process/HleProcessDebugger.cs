using Ryujinx.HLE.HOS.Diagnostics.Demangler;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.Loaders.Elf;
using Ryujinx.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel.Process
{
    class HleProcessDebugger
    {
        private const int Mod0 = 'M' << 0 | 'O' << 8 | 'D' << 16 | '0' << 24;

        private KProcess _owner;

        private class Image
        {
            public ulong BaseAddress { get; }

            public ElfSymbol[] Symbols { get; }

            public Image(ulong baseAddress, ElfSymbol[] symbols)
            {
                BaseAddress = baseAddress;
                Symbols     = symbols;
            }
        }

        private List<Image> _images;

        private int _loaded;

        public HleProcessDebugger(KProcess owner)
        {
            _owner = owner;

            _images = new List<Image>();
        }

        public string GetGuestStackTrace(ARMeilleure.State.ExecutionContext context)
        {
            EnsureLoaded();

            StringBuilder trace = new StringBuilder();

            void AppendTrace(ulong address)
            {
                Image image = GetImage(address, out int imageIndex);

                if (image == null || !TryGetSubName(image, address, out string subName))
                {
                    subName = $"Sub{address:x16}";
                }
                else if (subName.StartsWith("_Z"))
                {
                    subName = Demangler.Parse(subName);
                }

                if (image != null)
                {
                    ulong offset = address - image.BaseAddress;

                    string imageName = GetGuessedNsoNameFromIndex(imageIndex);

                    trace.AppendLine($"   {imageName}:0x{offset:x8} {subName}");
                }
                else
                {
                    trace.AppendLine($"   ??? {subName}");
                }
            }

            trace.AppendLine($"Process: {_owner.Name}, PID: {_owner.Pid}");

            if (context.IsAarch32)
            {
                ulong framePointer = context.GetX(11);

                while (framePointer != 0)
                {
                    if ((framePointer & 3) != 0 ||
                        !_owner.CpuMemory.IsMapped(framePointer) ||
                        !_owner.CpuMemory.IsMapped(framePointer + 4))
                    {
                        break;
                    }

                    AppendTrace(_owner.CpuMemory.Read<uint>(framePointer + 4));

                    framePointer = _owner.CpuMemory.Read<uint>(framePointer);
                }
            }
            else
            {
                ulong framePointer = context.GetX(29);

                while (framePointer != 0)
                {
                    if ((framePointer & 7) != 0 ||
                        !_owner.CpuMemory.IsMapped(framePointer) ||
                        !_owner.CpuMemory.IsMapped(framePointer + 8))
                    {
                        break;
                    }

                    AppendTrace(_owner.CpuMemory.Read<ulong>(framePointer + 8));

                    framePointer = _owner.CpuMemory.Read<ulong>(framePointer);
                }
            }

            return trace.ToString();
        }

        private bool TryGetSubName(Image image, ulong address, out string name)
        {
            address -= image.BaseAddress;

            int left  = 0;
            int right = image.Symbols.Length - 1;

            while (left <= right)
            {
                int size = right - left;

                int middle = left + (size >> 1);

                ElfSymbol symbol = image.Symbols[middle];

                ulong endAddr = symbol.Value + symbol.Size;

                if ((ulong)address >= symbol.Value && (ulong)address < endAddr)
                {
                    name = symbol.Name;

                    return true;
                }

                if ((ulong)address < (ulong)symbol.Value)
                {
                    right = middle - 1;
                }
                else
                {
                    left = middle + 1;
                }
            }

            name = null;

            return false;
        }

        private Image GetImage(ulong address, out int index)
        {
            lock (_images)
            {
                for (index = _images.Count - 1; index >= 0; index--)
                {
                    if (address >= _images[index].BaseAddress)
                    {
                        return _images[index];
                    }
                }
            }

            return null;
        }

        private string GetGuessedNsoNameFromIndex(int index)
        {
            if ((uint)index > 11)
            {
                return "???";
            }

            if (index == 0)
            {
                return "rtld";
            }
            else if (index == 1)
            {
                return "main";
            }
            else if (index == GetImagesCount() - 1)
            {
                return "sdk";
            }
            else
            {
                return "subsdk" + (index - 2);
            }
        }

        private int GetImagesCount()
        {
            lock (_images)
            {
                return _images.Count;
            }
        }

        private void EnsureLoaded()
        {
            if (Interlocked.CompareExchange(ref _loaded, 1, 0) == 0)
            {
                ScanMemoryForTextSegments();
            }
        }

        private void ScanMemoryForTextSegments()
        {
            ulong oldAddress = 0;
            ulong address    = 0;

            while (address >= oldAddress)
            {
                KMemoryInfo info = _owner.MemoryManager.QueryMemory(address);

                if (info.State == MemoryState.Reserved)
                {
                    break;
                }

                if (info.State == MemoryState.CodeStatic && info.Permission == KMemoryPermission.ReadAndExecute)
                {
                    LoadMod0Symbols(_owner.CpuMemory, info.Address);
                }

                oldAddress = address;

                address = info.Address + info.Size;
            }
        }

        private void LoadMod0Symbols(IVirtualMemoryManager memory, ulong textOffset)
        {
            ulong mod0Offset = textOffset + memory.Read<uint>(textOffset + 4);

            if (mod0Offset < textOffset || !memory.IsMapped(mod0Offset) || (mod0Offset & 3) != 0)
            {
                return;
            }

            Dictionary<ElfDynamicTag, ulong> dynamic = new Dictionary<ElfDynamicTag, ulong>();

            int mod0Magic = memory.Read<int>(mod0Offset + 0x0);

            if (mod0Magic != Mod0)
            {
                return;
            }

            ulong dynamicOffset    = memory.Read<uint>(mod0Offset + 0x4)  + mod0Offset;
            ulong bssStartOffset   = memory.Read<uint>(mod0Offset + 0x8)  + mod0Offset;
            ulong bssEndOffset     = memory.Read<uint>(mod0Offset + 0xc)  + mod0Offset;
            ulong ehHdrStartOffset = memory.Read<uint>(mod0Offset + 0x10) + mod0Offset;
            ulong ehHdrEndOffset   = memory.Read<uint>(mod0Offset + 0x14) + mod0Offset;
            ulong modObjOffset     = memory.Read<uint>(mod0Offset + 0x18) + mod0Offset;

            bool isAArch32 = memory.Read<ulong>(dynamicOffset) > 0xFFFFFFFF || memory.Read<ulong>(dynamicOffset + 0x10) > 0xFFFFFFFF;

            while (true)
            {
                ulong tagVal;
                ulong value;

                if (isAArch32)
                {
                    tagVal = memory.Read<uint>(dynamicOffset + 0);
                    value  = memory.Read<uint>(dynamicOffset + 4);

                    dynamicOffset += 0x8;
                }
                else
                {
                    tagVal = memory.Read<ulong>(dynamicOffset + 0);
                    value  = memory.Read<ulong>(dynamicOffset + 8);

                    dynamicOffset += 0x10;
                }

                ElfDynamicTag tag = (ElfDynamicTag)tagVal;

                if (tag == ElfDynamicTag.DT_NULL)
                {
                    break;
                }

                dynamic[tag] = value;
            }

            if (!dynamic.TryGetValue(ElfDynamicTag.DT_STRTAB, out ulong strTab) ||
                !dynamic.TryGetValue(ElfDynamicTag.DT_SYMTAB, out ulong symTab) ||
                !dynamic.TryGetValue(ElfDynamicTag.DT_SYMENT, out ulong symEntSize))
            {
                return;
            }

            ulong strTblAddr = textOffset + strTab;
            ulong symTblAddr = textOffset + symTab;

            List<ElfSymbol> symbols = new List<ElfSymbol>();

            while (symTblAddr < strTblAddr)
            {
                ElfSymbol sym = isAArch32 ? GetSymbol32(memory, symTblAddr, strTblAddr) : GetSymbol64(memory, symTblAddr, strTblAddr);

                symbols.Add(sym);

                symTblAddr += symEntSize;
            }

            lock (_images)
            {
                _images.Add(new Image(textOffset, symbols.OrderBy(x => x.Value).ToArray()));
            }
        }

        private ElfSymbol GetSymbol64(IVirtualMemoryManager memory, ulong address, ulong strTblAddr)
        {
            ElfSymbol64 sym = memory.Read<ElfSymbol64>(address);

            uint nameIndex = sym.NameOffset;

            string name = string.Empty;

            for (int chr; (chr = memory.Read<byte>(strTblAddr + nameIndex++)) != 0;)
            {
                name += (char)chr;
            }

            return new ElfSymbol(name, sym.Info, sym.Other, sym.SectionIndex, sym.ValueAddress, sym.Size);
        }

        private ElfSymbol GetSymbol32(IVirtualMemoryManager memory, ulong address, ulong strTblAddr)
        {
            ElfSymbol32 sym = memory.Read<ElfSymbol32>(address);

            uint nameIndex = sym.NameOffset;

            string name = string.Empty;

            for (int chr; (chr = memory.Read<byte>(strTblAddr + nameIndex++)) != 0;)
            {
                name += (char)chr;
            }

            return new ElfSymbol(name, sym.Info, sym.Other, sym.SectionIndex, sym.ValueAddress, sym.Size);
        }
    }
}