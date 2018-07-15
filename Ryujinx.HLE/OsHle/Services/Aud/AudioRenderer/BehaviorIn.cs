using System.Runtime.InteropServices;

namespace Ryujinx.HLE.OsHle.Services.Aud.AudioRenderer
{
    [StructLayout(LayoutKind.Sequential, Size = 0x10, Pack = 4)]
    struct BehaviorIn
    {
        public long Unknown0;
        public long Unknown8;
    }
}