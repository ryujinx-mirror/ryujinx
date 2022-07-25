using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Dsp.State
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x10)]
    public struct BiquadFilterState
    {
        public float State0;
        public float State1;
        public float State2;
        public float State3;
    }
}