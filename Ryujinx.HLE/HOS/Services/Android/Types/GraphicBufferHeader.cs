using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Android
{
    [StructLayout(LayoutKind.Sequential, Size = 0x28)]
    struct GraphicBufferHeader
    {
        public int Magic;
        public int Width;
        public int Height;
        public int Stride;
        public int Format;
        public int Usage;

        public int Pid;
        public int RefCount;

        public int FdsCount;
        public int IntsCount;
    }
}