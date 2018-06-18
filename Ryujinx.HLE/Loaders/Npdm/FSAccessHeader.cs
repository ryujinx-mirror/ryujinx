using System.IO;

namespace Ryujinx.HLE.Loaders.Npdm
{
    public class FSAccessHeader
    {
        public int   Version;
        public ulong PermissionsBitmask;
        public int   DataSize;
        public int   ContentOwnerIDSize;
        public int   DataAndContentOwnerIDSize;

        public FSAccessHeader(Stream FSAccessHeaderStream, int Offset, int Size)
        {
            FSAccessHeaderStream.Seek(Offset, SeekOrigin.Begin);

            BinaryReader Reader = new BinaryReader(FSAccessHeaderStream);

            Version            = Reader.ReadInt32();
            PermissionsBitmask = Reader.ReadUInt64();
            DataSize           = Reader.ReadInt32();

            if (DataSize != 0x1C)
            {
                throw new InvalidNpdmException("FSAccessHeader is corrupted!");
            }

            ContentOwnerIDSize        = Reader.ReadInt32();
            DataAndContentOwnerIDSize = Reader.ReadInt32();

            if (DataAndContentOwnerIDSize != 0x1C)
            {
                throw new InvalidNpdmException("ContentOwnerID section is not implemented!");
            }
        }
    }
}
