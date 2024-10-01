using Ryujinx.HLE.Exceptions;
using System;
using System.IO;

namespace Ryujinx.HLE.Loaders.Npdm
{
    class FsAccessHeader
    {
        public int Version { get; private set; }
        public ulong PermissionsBitmask { get; private set; }

        /// <exception cref="InvalidNpdmException">The stream contains invalid data.</exception>
        /// <exception cref="NotImplementedException">The ContentOwnerId section is not implemented.</exception>
        /// <exception cref="ArgumentException">The stream does not support reading, is <see langword="null"/>, or is already closed.</exception>
        /// <exception cref="EndOfStreamException">The end of the stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        public FsAccessHeader(Stream stream, int offset, int size)
        {
            stream.Seek(offset, SeekOrigin.Begin);

            BinaryReader reader = new(stream);

            Version = reader.ReadInt32();
            PermissionsBitmask = reader.ReadUInt64();

            int dataSize = reader.ReadInt32();

            if (dataSize != 0x1c)
            {
                throw new InvalidNpdmException("FsAccessHeader is corrupted!");
            }
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            int contentOwnerIdSize = reader.ReadInt32();
#pragma warning restore IDE0059
            int dataAndContentOwnerIdSize = reader.ReadInt32();

            if (dataAndContentOwnerIdSize != 0x1c)
            {
                throw new NotImplementedException("ContentOwnerId section is not implemented!");
            }
        }
    }
}
