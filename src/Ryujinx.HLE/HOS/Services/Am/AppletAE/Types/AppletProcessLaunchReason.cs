using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE
{
    [StructLayout(LayoutKind.Sequential, Size = 0x4)]
    struct AppletProcessLaunchReason
    {
        public byte Flag;
        public ushort Unknown1;
        public byte Unknown2;
    }
}
