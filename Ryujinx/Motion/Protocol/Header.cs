using System.Runtime.InteropServices;

namespace Ryujinx.Motion
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public uint MagicString;
        public ushort Version;
        public ushort Length;
        public uint Crc32;
        public uint ID;
    }
}
