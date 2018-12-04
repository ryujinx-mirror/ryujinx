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

        public byte   MmuFlags            { get; }
        public bool   Is64Bits            { get; }
        public byte   MainThreadPriority  { get; }
        public byte   DefaultCpuId        { get; }
        public int    PersonalMmHeapSize  { get; }
        public int    ProcessCategory     { get; }
        public int    MainThreadStackSize { get; }
        public string TitleName           { get; }
        public byte[] ProductCode         { get; }

        public Aci0 Aci0 { get; }
        public Acid Acid { get; }

        public Npdm(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            if (reader.ReadInt32() != MetaMagic)
            {
                throw new InvalidNpdmException("NPDM Stream doesn't contain NPDM file!");
            }

            reader.ReadInt64();

            MmuFlags = reader.ReadByte();

            Is64Bits = (MmuFlags & 1) != 0;

            reader.ReadByte();

            MainThreadPriority = reader.ReadByte();
            DefaultCpuId       = reader.ReadByte();

            reader.ReadInt32();

            PersonalMmHeapSize = reader.ReadInt32();

            ProcessCategory = reader.ReadInt32();

            MainThreadStackSize = reader.ReadInt32();

            byte[] tempTitleName = reader.ReadBytes(0x10);

            TitleName = Encoding.UTF8.GetString(tempTitleName, 0, tempTitleName.Length).Trim('\0');

            ProductCode = reader.ReadBytes(0x10);

            stream.Seek(0x30, SeekOrigin.Current);

            int aci0Offset = reader.ReadInt32();
            int aci0Size   = reader.ReadInt32();
            int acidOffset = reader.ReadInt32();
            int acidSize   = reader.ReadInt32();

            Aci0 = new Aci0(stream, aci0Offset);
            Acid = new Acid(stream, acidOffset);
        }
    }
}
