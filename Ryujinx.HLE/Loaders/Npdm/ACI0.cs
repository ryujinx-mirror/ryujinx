using Ryujinx.HLE.Exceptions;
using System.IO;

namespace Ryujinx.HLE.Loaders.Npdm
{
    class ACI0
    {
        private const int ACI0Magic = 'A' << 0 | 'C' << 8 | 'I' << 16 | '0' << 24;

        public long TitleId { get; private set; }

        public int   FsVersion            { get; private set; }
        public ulong FsPermissionsBitmask { get; private set; }

        public ServiceAccessControl ServiceAccessControl { get; private set; }
        public KernelAccessControl  KernelAccessControl  { get; private set; }

        public ACI0(Stream Stream, int Offset)
        {
            Stream.Seek(Offset, SeekOrigin.Begin);

            BinaryReader Reader = new BinaryReader(Stream);

            if (Reader.ReadInt32() != ACI0Magic)
            {
                throw new InvalidNpdmException("ACI0 Stream doesn't contain ACI0 section!");
            }

            Stream.Seek(0xc, SeekOrigin.Current);

            TitleId = Reader.ReadInt64();

            //Reserved.
            Stream.Seek(8, SeekOrigin.Current);

            int FsAccessHeaderOffset       = Reader.ReadInt32();
            int FsAccessHeaderSize         = Reader.ReadInt32();
            int ServiceAccessControlOffset = Reader.ReadInt32();
            int ServiceAccessControlSize   = Reader.ReadInt32();
            int KernelAccessControlOffset  = Reader.ReadInt32();
            int KernelAccessControlSize    = Reader.ReadInt32();

            FsAccessHeader FsAccessHeader = new FsAccessHeader(Stream, Offset + FsAccessHeaderOffset, FsAccessHeaderSize);

            FsVersion            = FsAccessHeader.Version;
            FsPermissionsBitmask = FsAccessHeader.PermissionsBitmask;

            ServiceAccessControl = new ServiceAccessControl(Stream, Offset + ServiceAccessControlOffset, ServiceAccessControlSize);

            KernelAccessControl = new KernelAccessControl(Stream, Offset + KernelAccessControlOffset, KernelAccessControlSize);
        }
    }
}
