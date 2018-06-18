using Ryujinx.HLE.OsHle.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ryujinx.HLE.Loaders.Npdm
{
    //https://github.com/SciresM/hactool/blob/master/npdm.c
    //https://github.com/SciresM/hactool/blob/master/npdm.h
    //http://switchbrew.org/index.php?title=NPDM
    class Npdm
    {
        public bool   Is64Bits;
        public int    AddressSpaceWidth;
        public byte   MainThreadPriority;
        public byte   DefaultCpuId;
        public int    SystemResourceSize;
        public int    ProcessCategory;
        public int    MainEntrypointStackSize;
        public string TitleName;
        public byte[] ProductCode;
        public ulong  FSPerms;

        private int ACI0Offset;
        private int ACI0Size;
        private int ACIDOffset;
        private int ACIDSize;

        public ACI0 ACI0;
        public ACID ACID;
        
        public const long NpdmMagic = 'M' << 0 | 'E' << 8 | 'T' << 16 | 'A' << 24;

        public Npdm(Stream NPDMStream)
        {
            BinaryReader Reader = new BinaryReader(NPDMStream);

            if (Reader.ReadInt32() != NpdmMagic)
            {
                throw new InvalidNpdmException("NPDM Stream doesn't contain NPDM file!");
            }

            Reader.ReadInt64(); // Padding / Unused

            // MmuFlags, bit0: 64-bit instructions, bits1-3: address space width (1=64-bit, 2=32-bit). Needs to be <= 0xF
            byte MmuFlags     = Reader.ReadByte();
            Is64Bits          = (MmuFlags & 1) != 0;
            AddressSpaceWidth = (MmuFlags >> 1) & 7;

            Reader.ReadByte(); // Padding / Unused

            MainThreadPriority = Reader.ReadByte(); // (0-63)
            DefaultCpuId       = Reader.ReadByte();

            Reader.ReadInt32(); // Padding / Unused

            // System resource size (max size as of 5.x: 534773760). Unknown usage.
            SystemResourceSize = EndianSwap.Swap32(Reader.ReadInt32());

            // ProcessCategory (0: regular title, 1: kernel built-in). Should be 0 here.
            ProcessCategory = EndianSwap.Swap32(Reader.ReadInt32());

            // Main entrypoint stack size 
            // (Should(?) be page-aligned. In non-nspwn scenarios, values of 0 can also rarely break in Horizon.
            // This might be something auto-adapting or a security feature of some sort ?)
            MainEntrypointStackSize = Reader.ReadInt32();

            byte[] TempTitleName = Reader.ReadBytes(0x10);
            TitleName            = Encoding.UTF8.GetString(TempTitleName, 0, TempTitleName.Length).Trim('\0');

            ProductCode = Reader.ReadBytes(0x10); // Unknown value

            NPDMStream.Seek(0x30, SeekOrigin.Current); // Skip reserved bytes

            ACI0Offset = Reader.ReadInt32();
            ACI0Size   = Reader.ReadInt32();
            ACIDOffset = Reader.ReadInt32();
            ACIDSize   = Reader.ReadInt32();

            ACI0       = new ACI0(NPDMStream, ACI0Offset);
            ACID       = new ACID(NPDMStream, ACIDOffset);

            FSPerms    = ACI0.FSAccessHeader.PermissionsBitmask & ACID.FSAccessControl.PermissionsBitmask;
        }
    }
}
