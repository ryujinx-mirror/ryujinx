using System.IO;

namespace Ryujinx.HLE.Loaders.Npdm
{
    public class FsAccessControl
    {
        public int Version { get; private set; }
        public ulong PermissionsBitmask { get; private set; }
        public int Unknown1 { get; private set; }
        public int Unknown2 { get; private set; }
        public int Unknown3 { get; private set; }
        public int Unknown4 { get; private set; }

        public FsAccessControl(Stream stream, int offset, int size)
        {
            stream.Seek(offset, SeekOrigin.Begin);

            BinaryReader reader = new(stream);

            Version = reader.ReadInt32();
            PermissionsBitmask = reader.ReadUInt64();
            Unknown1 = reader.ReadInt32();
            Unknown2 = reader.ReadInt32();
            Unknown3 = reader.ReadInt32();
            Unknown4 = reader.ReadInt32();
        }
    }
}
