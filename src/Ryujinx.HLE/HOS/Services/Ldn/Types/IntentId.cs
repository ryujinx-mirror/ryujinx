using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ldn.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    struct IntentId
    {
        public long LocalCommunicationId;
        public ushort Reserved1;
        public ushort SceneId;
        public uint Reserved2;
    }
}
