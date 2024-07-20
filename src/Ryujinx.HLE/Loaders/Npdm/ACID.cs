using Ryujinx.HLE.Exceptions;
using System.IO;

namespace Ryujinx.HLE.Loaders.Npdm
{
    public class Acid
    {
        private const int AcidMagic = 'A' << 0 | 'C' << 8 | 'I' << 16 | 'D' << 24;

        public byte[] Rsa2048Signature { get; private set; }
        public byte[] Rsa2048Modulus { get; private set; }
        public int Unknown1 { get; private set; }
        public int Flags { get; private set; }

        public long TitleIdRangeMin { get; private set; }
        public long TitleIdRangeMax { get; private set; }

        public FsAccessControl FsAccessControl { get; private set; }
        public ServiceAccessControl ServiceAccessControl { get; private set; }
        public KernelAccessControl KernelAccessControl { get; private set; }

        /// <exception cref="InvalidNpdmException">The stream doesn't contain valid ACID data.</exception>
        /// <exception cref="System.ArgumentException">The stream does not support reading, is <see langword="null"/>, or is already closed.</exception>
        /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public Acid(Stream stream, int offset)
        {
            stream.Seek(offset, SeekOrigin.Begin);

            BinaryReader reader = new(stream);

            Rsa2048Signature = reader.ReadBytes(0x100);
            Rsa2048Modulus = reader.ReadBytes(0x100);

            if (reader.ReadInt32() != AcidMagic)
            {
                throw new InvalidNpdmException("ACID Stream doesn't contain ACID section!");
            }

            // Size field used with the above signature (?).
            Unknown1 = reader.ReadInt32();

            reader.ReadInt32();

            // Bit0 must be 1 on retail, on devunit 0 is also allowed. Bit1 is unknown.
            Flags = reader.ReadInt32();

            TitleIdRangeMin = reader.ReadInt64();
            TitleIdRangeMax = reader.ReadInt64();

            int fsAccessControlOffset = reader.ReadInt32();
            int fsAccessControlSize = reader.ReadInt32();
            int serviceAccessControlOffset = reader.ReadInt32();
            int serviceAccessControlSize = reader.ReadInt32();
            int kernelAccessControlOffset = reader.ReadInt32();
            int kernelAccessControlSize = reader.ReadInt32();

            FsAccessControl = new FsAccessControl(stream, offset + fsAccessControlOffset, fsAccessControlSize);

            ServiceAccessControl = new ServiceAccessControl(stream, offset + serviceAccessControlOffset, serviceAccessControlSize);

            KernelAccessControl = new KernelAccessControl(stream, offset + kernelAccessControlOffset, kernelAccessControlSize);
        }
    }
}
