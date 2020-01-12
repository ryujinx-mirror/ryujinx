using System;
using System.IO;
using System.Text;

namespace Ryujinx.HLE.FileSystem.Content
{
    public class SystemVersion
    {
        public byte   Major          { get; }
        public byte   Minor          { get; }
        public byte   Micro          { get; }
        public byte   RevisionMajor  { get; }
        public byte   RevisionMinor  { get; }
        public string PlatformString { get; }
        public string Hex            { get; }
        public string VersionString  { get; }
        public string VersionTitle   { get; }

        public SystemVersion(Stream systemVersionFile)
        {
            using (BinaryReader reader = new BinaryReader(systemVersionFile))
            {
                Major = reader.ReadByte();
                Minor = reader.ReadByte();
                Micro = reader.ReadByte();

                reader.ReadByte(); // Padding

                RevisionMajor = reader.ReadByte();
                RevisionMinor = reader.ReadByte();

                reader.ReadBytes(2); // Padding

                PlatformString = Encoding.ASCII.GetString(reader.ReadBytes(0x20)).TrimEnd('\0');
                Hex            = Encoding.ASCII.GetString(reader.ReadBytes(0x40)).TrimEnd('\0');
                VersionString  = Encoding.ASCII.GetString(reader.ReadBytes(0x18)).TrimEnd('\0');
                VersionTitle   = Encoding.ASCII.GetString(reader.ReadBytes(0x80)).TrimEnd('\0');
            }
        }
    }
}