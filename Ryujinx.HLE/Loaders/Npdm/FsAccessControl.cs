using System.IO;

namespace Ryujinx.HLE.Loaders.Npdm
{
    class FsAccessControl
    {
        public int   Version            { get; private set; }
        public ulong PermissionsBitmask { get; private set; }
        public int   Unknown1           { get; private set; }
        public int   Unknown2           { get; private set; }
        public int   Unknown3           { get; private set; }
        public int   Unknown4           { get; private set; }

        public FsAccessControl(Stream Stream, int Offset, int Size)
        {
            Stream.Seek(Offset, SeekOrigin.Begin);

            BinaryReader Reader = new BinaryReader(Stream);

            Version            = Reader.ReadInt32();
            PermissionsBitmask = Reader.ReadUInt64();
            Unknown1           = Reader.ReadInt32();
            Unknown2           = Reader.ReadInt32();
            Unknown3           = Reader.ReadInt32();
            Unknown4           = Reader.ReadInt32();
        }
    }
}
