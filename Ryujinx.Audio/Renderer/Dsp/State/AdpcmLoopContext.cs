using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Dsp.State
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 6)]
    public struct AdpcmLoopContext
    {
        public short PredScale;
        public short History0;
        public short History1;
    }
}
