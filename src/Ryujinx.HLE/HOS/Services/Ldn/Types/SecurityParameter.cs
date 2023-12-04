using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ldn.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x20)]
    struct SecurityParameter
    {
        public Array16<byte> Data;
        public Array16<byte> SessionId;
    }
}
