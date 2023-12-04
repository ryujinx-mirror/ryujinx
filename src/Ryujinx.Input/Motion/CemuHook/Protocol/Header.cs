using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Input.Motion.CemuHook.Protocol
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Header
    {
        public uint MagicString;
        public ushort Version;
        public ushort Length;
        public Array4<byte> Crc32;
        public uint Id;
    }
}
