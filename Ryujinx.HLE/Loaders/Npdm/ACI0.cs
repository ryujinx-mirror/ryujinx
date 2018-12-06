using Ryujinx.HLE.Exceptions;
using System.IO;

namespace Ryujinx.HLE.Loaders.Npdm
{
    class Aci0
    {
        private const int Aci0Magic = 'A' << 0 | 'C' << 8 | 'I' << 16 | '0' << 24;

        public long TitleId { get; private set; }

        public int   FsVersion            { get; private set; }
        public ulong FsPermissionsBitmask { get; private set; }

        public ServiceAccessControl ServiceAccessControl { get; private set; }
        public KernelAccessControl  KernelAccessControl  { get; private set; }

        public Aci0(Stream stream, int offset)
        {
            stream.Seek(offset, SeekOrigin.Begin);

            BinaryReader reader = new BinaryReader(stream);

            if (reader.ReadInt32() != Aci0Magic)
            {
                throw new InvalidNpdmException("ACI0 Stream doesn't contain ACI0 section!");
            }

            stream.Seek(0xc, SeekOrigin.Current);

            TitleId = reader.ReadInt64();

            //Reserved.
            stream.Seek(8, SeekOrigin.Current);

            int fsAccessHeaderOffset       = reader.ReadInt32();
            int fsAccessHeaderSize         = reader.ReadInt32();
            int serviceAccessControlOffset = reader.ReadInt32();
            int serviceAccessControlSize   = reader.ReadInt32();
            int kernelAccessControlOffset  = reader.ReadInt32();
            int kernelAccessControlSize    = reader.ReadInt32();

            FsAccessHeader fsAccessHeader = new FsAccessHeader(stream, offset + fsAccessHeaderOffset, fsAccessHeaderSize);

            FsVersion            = fsAccessHeader.Version;
            FsPermissionsBitmask = fsAccessHeader.PermissionsBitmask;

            ServiceAccessControl = new ServiceAccessControl(stream, offset + serviceAccessControlOffset, serviceAccessControlSize);

            KernelAccessControl = new KernelAccessControl(stream, offset + kernelAccessControlOffset, kernelAccessControlSize);
        }
    }
}
