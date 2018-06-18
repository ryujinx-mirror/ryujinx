using System.IO;

namespace Ryujinx.HLE.Loaders.Npdm
{
    public class FSAccessControl
    {
        public int   Version;
        public ulong PermissionsBitmask;
        public int   Unknown1;
        public int   Unknown2;
        public int   Unknown3;
        public int   Unknown4;

        public FSAccessControl(Stream FSAccessHeaderStream, int Offset, int Size)
        {
            FSAccessHeaderStream.Seek(Offset, SeekOrigin.Begin);

            BinaryReader Reader = new BinaryReader(FSAccessHeaderStream);

            Version            = Reader.ReadInt32();
            PermissionsBitmask = Reader.ReadUInt64();
            Unknown1           = Reader.ReadInt32();
            Unknown2           = Reader.ReadInt32();
            Unknown3           = Reader.ReadInt32();
            Unknown4           = Reader.ReadInt32();
        }
    }
}
