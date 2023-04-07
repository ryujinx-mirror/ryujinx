using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Friend.ServiceCreator
{
    [StructLayout(LayoutKind.Sequential, Size = 0x20)]
    struct PlayHistoryRegistrationKey
    {
        public ushort        Type;
        public byte          KeyIndex;
        public byte          UserIdBool;
        public byte          UnknownBool;
        public Array11<byte> Reserved;
        public Array16<byte> Uuid;
    }
}
