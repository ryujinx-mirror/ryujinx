using Ryujinx.HLE.Exceptions;
using System.IO;

namespace Ryujinx.HLE.Loaders.Npdm
{
    public class Aci0
    {
        private const int Aci0Magic = 'A' << 0 | 'C' << 8 | 'I' << 16 | '0' << 24;

        public ulong TitleId { get; set; }

        public int FsVersion { get; private set; }
        public ulong FsPermissionsBitmask { get; private set; }

        public ServiceAccessControl ServiceAccessControl { get; private set; }
        public KernelAccessControl KernelAccessControl { get; private set; }

        /// <exception cref="InvalidNpdmException">The stream doesn't contain valid ACI0 data.</exception>
        /// <exception cref="System.ArgumentException">The stream does not support reading, is <see langword="null"/>, or is already closed.</exception>
        /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="System.NotImplementedException">The FsAccessHeader.ContentOwnerId section is not implemented.</exception>
        public Aci0(Stream stream, int offset)
        {
            stream.Seek(offset, SeekOrigin.Begin);

            BinaryReader reader = new(stream);

            if (reader.ReadInt32() != Aci0Magic)
            {
                throw new InvalidNpdmException("ACI0 Stream doesn't contain ACI0 section!");
            }

            stream.Seek(0xc, SeekOrigin.Current);

            TitleId = reader.ReadUInt64();

            // Reserved.
            stream.Seek(8, SeekOrigin.Current);

            int fsAccessHeaderOffset = reader.ReadInt32();
            int fsAccessHeaderSize = reader.ReadInt32();
            int serviceAccessControlOffset = reader.ReadInt32();
            int serviceAccessControlSize = reader.ReadInt32();
            int kernelAccessControlOffset = reader.ReadInt32();
            int kernelAccessControlSize = reader.ReadInt32();

            FsAccessHeader fsAccessHeader = new(stream, offset + fsAccessHeaderOffset, fsAccessHeaderSize);

            FsVersion = fsAccessHeader.Version;
            FsPermissionsBitmask = fsAccessHeader.PermissionsBitmask;

            ServiceAccessControl = new ServiceAccessControl(stream, offset + serviceAccessControlOffset, serviceAccessControlSize);

            KernelAccessControl = new KernelAccessControl(stream, offset + kernelAccessControlOffset, kernelAccessControlSize);
        }
    }
}
