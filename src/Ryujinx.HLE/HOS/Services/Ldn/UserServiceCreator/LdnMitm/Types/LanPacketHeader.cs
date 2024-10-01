using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator.LdnMitm.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 12)]
    internal struct LanPacketHeader
    {
        public uint Magic;
        public LanPacketType Type;
        public byte Compressed;
        public ushort Length;
        public ushort DecompressLength;
        public Array2<byte> Reserved;
    }
}
