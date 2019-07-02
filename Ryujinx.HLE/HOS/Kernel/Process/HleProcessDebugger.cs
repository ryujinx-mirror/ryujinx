using ChocolArm64.Memory;
using ChocolArm64.State;
using Ryujinx.HLE.HOS.Diagnostics.Demangler;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.Loaders.Elf;
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
            public long BaseAddress { get; private set; }

            public ElfSymbol[] Symbols { get; private set; }

            public Image(long baseAddress, ElfSymbol[] symbols)
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

        public string GetGuestStackTrace(CpuThreadState threadState)
        {
            EnsureLoaded();

            StringBuilder trace = new StringBuilder();

            void AppendTrace(long address)
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
                    long offset = address - image.BaseAddress;

                    string imageName = GetGuessedNsoNameFromIndex(imageIndex);

                    trace.AppendLine($"   {imageName}:0x{offset:x8} {subName}");
                }
                else
                {
                    trace.AppendLine($"   ??? {subName}");
                }
            }

            // TODO: ARM32.
            long framePointer = (long)threadState.X29;

            trace.AppendLine($"Process: {_owner.Name}, PID: {_owner.Pid}");

            while (framePointer != 0)
            {
                if ((framePointer & 7) != 0 ||
                    !_owner.CpuMemory.IsMapped(framePointer) ||
                    !_owner.CpuMemory.IsMapped(framePointer + 8))
                {
                    break;
                }

                // Note: This is the return address, we need to subtract one instruction
                // worth of bytes to get the branch instruction address.
                AppendTrace(_owner.CpuMemory.ReadInt64(framePointer + 8) - 4);

                framePointer = _owner.CpuMemory.ReadInt64(framePointer);
            }

            return trace.ToString();
        }

        private bool TryGetSubName(Image image, long address, out string name)
        {
            address -= image.BaseAddress;

            int left  = 0;
            int right = image.Symbols.Length - 1;

            while (left <= right)
            {
                int size = right - left;

                int middle = left + (size >> 1);

                ElfSymbol symbol = image.Symbols[middle];

                long endAddr = symbol.Value + symbol.Size;

                if ((ulong)address >= (ulong)symbol.Value && (ulong)address < (ulong)endAddr)
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

        private Image GetImage(long address, out int index)
        {
            lock (_images)
            {
                for (index = _images.Count - 1; index >= 0; index--)
                {
                    if ((ulong)address >= (ulong)_images[index].BaseAddress)
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

                if (info.State == MemoryState.CodeStatic && info.Permission == MemoryPermission.ReadAndExecute)
                {
                    LoadMod0Symbols(_owner.CpuMemory, (long)info.Address);
                }

                oldAddress = address;

                address = info.Address + info.Size;
            }
        }

        private void LoadMod0Symbols(MemoryManager memory, long textOffset)
        {
            long mod0Offset = textOffset + memory.ReadUInt32(textOffset + 4);

            if (mod0Offset < textOffset || !memory.IsMapped(mod0Offset) || (mod0Offset & 3) != 0)
            {
                return;
            }

            Dictionary<ElfDynamicTag, long> dynamic = new Dictionary<ElfDynamicTag, long>();

            int mod0Magic = memory.ReadInt32(mod0Offset + 0x0);

            if (mod0Magic != Mod0)
            {
                return;
            }

            long dynamicOffset    = memory.ReadInt32(mod0Offset + 0x4)  + mod0Offset;
            long bssStartOffset   = memory.ReadInt32(mod0Offset + 0x8)  + mod0Offset;
            long bssEndOffset     = memory.ReadInt32(mod0Offset + 0xc)  + mod0Offset;
            long ehHdrStartOffset = memory.ReadInt32(mod0Offset + 0x10) + mod0Offset;
            long ehHdrEndOffset   = memory.ReadInt32(mod0Offset + 0x14) + mod0Offset;
            long modObjOffset     = memory.ReadInt32(mod0Offset + 0x18) + mod0Offset;

            // TODO: Elf32.
            while (true)
            {
                long tagVal = memory.ReadInt64(dynamicOffset + 0);
                long value  = memory.ReadInt64(dynamicOffset + 8);

                dynamicOffset += 0x10;

                ElfDynamicTag tag = (ElfDynamicTag)tagVal;

                if (tag == ElfDynamicTag.DT_NULL)
                {
                    break;
                }

                dynamic[tag] = value;
            }

            if (!dynamic.TryGetValue(ElfDynamicTag.DT_STRTAB, out long strTab) ||
                !dynamic.TryGetValue(ElfDynamicTag.DT_SYMTAB, out long symTab) ||
                !dynamic.TryGetValue(ElfDynamicTag.DT_SYMENT, out long symEntSize))
            {
                return;
            }

            long strTblAddr = textOffset + strTab;
            long symTblAddr = textOffset + symTab;

            List<ElfSymbol> symbols = new List<ElfSymbol>();

            while ((ulong)symTblAddr < (ulong)strTblAddr)
            {
                ElfSymbol sym = GetSymbol(memory, symTblAddr, strTblAddr);

                symbols.Add(sym);

                symTblAddr += symEntSize;
            }

            lock (_images)
            {
                _images.Add(new Image(textOffset, symbols.OrderBy(x => x.Value).ToArray()));
            }
        }

        private ElfSymbol GetSymbol(MemoryManager memory, long address, long strTblAddr)
        {
            int  nameIndex = memory.ReadInt32(address + 0);
            int  info      = memory.ReadByte (address + 4);
            int  other     = memory.ReadByte (address + 5);
            int  shIdx     = memory.ReadInt16(address + 6);
            long value     = memory.ReadInt64(address + 8);
            long size      = memory.ReadInt64(address + 16);

            string name = string.Empty;

            for (int chr; (chr = memory.ReadByte(strTblAddr + nameIndex++)) != 0;)
            {
                name += (char)chr;
            }

            return new ElfSymbol(name, info, other, shIdx, value, size);
        }
    }
}