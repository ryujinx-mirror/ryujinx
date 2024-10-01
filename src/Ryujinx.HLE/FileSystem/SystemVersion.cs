using Ryujinx.HLE.Utilities;
using System.IO;

namespace Ryujinx.HLE.FileSystem
{
    public class SystemVersion
    {
        public byte Major { get; }
        public byte Minor { get; }
        public byte Micro { get; }
        public byte RevisionMajor { get; }
        public byte RevisionMinor { get; }
        public string PlatformString { get; }
        public string Hex { get; }
        public string VersionString { get; }
        public string VersionTitle { get; }

        public SystemVersion(Stream systemVersionFile)
        {
            using BinaryReader reader = new(systemVersionFile);
            Major = reader.ReadByte();
            Minor = reader.ReadByte();
            Micro = reader.ReadByte();

            reader.ReadByte(); // Padding

            RevisionMajor = reader.ReadByte();
            RevisionMinor = reader.ReadByte();

            reader.ReadBytes(2); // Padding

            PlatformString = StringUtils.ReadInlinedAsciiString(reader, 0x20);
            Hex = StringUtils.ReadInlinedAsciiString(reader, 0x40);
            VersionString = StringUtils.ReadInlinedAsciiString(reader, 0x18);
            VersionTitle = StringUtils.ReadInlinedAsciiString(reader, 0x80);
        }
    }
}
