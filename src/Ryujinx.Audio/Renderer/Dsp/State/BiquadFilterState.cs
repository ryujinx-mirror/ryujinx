using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Dsp.State
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x20)]
    public struct BiquadFilterState
    {
        public float State0;
        public float State1;
        public float State2;
        public float State3;
        public float State4;
        public float State5;
        public float State6;
        public float State7;
    }
}
