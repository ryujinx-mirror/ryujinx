using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    [StructLayout(LayoutKind.Sequential, Size = 0xc0, Pack = 1)]
    unsafe struct EffectIn
    {
        public byte Unknown0x0;
        public byte IsNew;
        public fixed byte Unknown[0xbe];
    }
}
