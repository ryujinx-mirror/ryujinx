using Ryujinx.HLE.Exceptions;
using System.IO;
using System.Text;

namespace Ryujinx.HLE.Loaders.Npdm
{
    //https://github.com/SciresM/hactool/blob/master/npdm.c
    //https://github.com/SciresM/hactool/blob/master/npdm.h
    //http://switchbrew.org/index.php?title=NPDM
    class Npdm
    {
        private const int MetaMagic = 'M' << 0 | 'E' << 8 | 'T' << 16 | 'A' << 24;

        public byte   MmuFlags            { get; private set; }
        public bool   Is64Bits            { get; private set; }
        public byte   MainThreadPriority  { get; private set; }
        public byte   DefaultCpuId        { get; private set; }
        public int    PersonalMmHeapSize  { get; private set; }
        public int    ProcessCategory     { get; private set; }
        public int    MainThreadStackSize { get; private set; }
        public string TitleName           { get; private set; }
        public byte[] ProductCode         { get; private set; }

        public ACI0 ACI0 { get; private set; }
        public ACID ACID { get; private set; }

        public Npdm(Stream Stream)
        {
            BinaryReader Reader = new BinaryReader(Stream);

            if (Reader.ReadInt32() != MetaMagic)
            {
                throw new InvalidNpdmException("NPDM Stream doesn't contain NPDM file!");
            }

            Reader.ReadInt64();

            MmuFlags = Reader.ReadByte();

            Is64Bits = (MmuFlags & 1) != 0;

            Reader.ReadByte();

            MainThreadPriority = Reader.ReadByte();
            DefaultCpuId       = Reader.ReadByte();

            Reader.ReadInt32();

            PersonalMmHeapSize = Reader.ReadInt32();

            ProcessCategory = Reader.ReadInt32();

            MainThreadStackSize = Reader.ReadInt32();

            byte[] TempTitleName = Reader.ReadBytes(0x10);

            TitleName = Encoding.UTF8.GetString(TempTitleName, 0, TempTitleName.Length).Trim('\0');

            ProductCode = Reader.ReadBytes(0x10);

            Stream.Seek(0x30, SeekOrigin.Current);

            int ACI0Offset = Reader.ReadInt32();
            int ACI0Size   = Reader.ReadInt32();
            int ACIDOffset = Reader.ReadInt32();
            int ACIDSize   = Reader.ReadInt32();

            ACI0 = new ACI0(Stream, ACI0Offset);
            ACID = new ACID(Stream, ACIDOffset);
        }
    }
}
