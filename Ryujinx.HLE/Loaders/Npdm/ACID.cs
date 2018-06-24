using System;
using System.IO;

namespace Ryujinx.HLE.Loaders.Npdm
{
    class ACID
    {
        public byte[] RSA2048Signature;
        public byte[] RSA2048Modulus;
        public int    Unknown1;
        public int    Flags;

        public string TitleIdRangeMin;
        public string TitleIdRangeMax;

        private int FSAccessControlOffset;
        private int FSAccessControlSize;
        private int ServiceAccessControlOffset;
        private int ServiceAccessControlSize;
        private int KernelAccessControlOffset;
        private int KernelAccessControlSize;

        public FSAccessControl      FSAccessControl;
        public ServiceAccessControl ServiceAccessControl;
        public KernelAccessControl  KernelAccessControl;

        public const long ACIDMagic = 'A' << 0 | 'C' << 8 | 'I' << 16 | 'D' << 24;

        public ACID(Stream ACIDStream, int Offset)
        {
            ACIDStream.Seek(Offset, SeekOrigin.Begin);

            BinaryReader Reader = new BinaryReader(ACIDStream);

            RSA2048Signature = Reader.ReadBytes(0x100);
            RSA2048Modulus   = Reader.ReadBytes(0x100);

            if (Reader.ReadInt32() != ACIDMagic)
            {
                throw new InvalidNpdmException("ACID Stream doesn't contain ACID section!");
            }

            Unknown1 = Reader.ReadInt32(); // Size field used with the above signature(?).
            Reader.ReadInt32(); // Padding / Unused
            Flags = Reader.ReadInt32(); // Bit0 must be 1 on retail, on devunit 0 is also allowed. Bit1 is unknown.

            byte[] TempTitleIdRangeMin = Reader.ReadBytes(8);
            Array.Reverse(TempTitleIdRangeMin);
            TitleIdRangeMin = BitConverter.ToString(TempTitleIdRangeMin).Replace("-", "");

            byte[] TempTitleIdRangeMax = Reader.ReadBytes(8);
            Array.Reverse(TempTitleIdRangeMax);
            TitleIdRangeMax = BitConverter.ToString(TempTitleIdRangeMax).Replace("-", "");

            FSAccessControlOffset      = Reader.ReadInt32();
            FSAccessControlSize        = Reader.ReadInt32();
            ServiceAccessControlOffset = Reader.ReadInt32();
            ServiceAccessControlSize   = Reader.ReadInt32();
            KernelAccessControlOffset  = Reader.ReadInt32();
            KernelAccessControlSize    = Reader.ReadInt32();

            FSAccessControl      = new FSAccessControl(ACIDStream, Offset + FSAccessControlOffset, FSAccessControlSize);
            ServiceAccessControl = new ServiceAccessControl(ACIDStream, Offset + ServiceAccessControlOffset, ServiceAccessControlSize);
            KernelAccessControl  = new KernelAccessControl(ACIDStream, Offset + KernelAccessControlOffset, KernelAccessControlSize);
        }
    }
}
