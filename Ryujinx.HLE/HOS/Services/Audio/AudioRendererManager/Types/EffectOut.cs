using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Audio.AudioRendererManager
{
    [StructLayout(LayoutKind.Sequential, Size = 0x10, Pack = 1)]
    unsafe struct EffectOut
    {
        public EffectState State;
        public fixed byte  Reserved[15];
    }
}
