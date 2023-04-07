using Ryujinx.HLE.Exceptions;
using System;
using System.IO;

namespace Ryujinx.HLE.Loaders.Npdm
{
    class FsAccessHeader
    {
        public int   Version            { get; private set; }
        public ulong PermissionsBitmask { get; private set; }

        public FsAccessHeader(Stream stream, int offset, int size)
        {
            stream.Seek(offset, SeekOrigin.Begin);

            BinaryReader reader = new BinaryReader(stream);

            Version            = reader.ReadInt32();
            PermissionsBitmask = reader.ReadUInt64();

            int dataSize = reader.ReadInt32();

            if (dataSize != 0x1c)
            {
                throw new InvalidNpdmException("FsAccessHeader is corrupted!");
            }

            int contentOwnerIdSize        = reader.ReadInt32();
            int dataAndContentOwnerIdSize = reader.ReadInt32();

            if (dataAndContentOwnerIdSize != 0x1c)
            {
                throw new NotImplementedException("ContentOwnerId section is not implemented!");
            }
        }
    }
}
