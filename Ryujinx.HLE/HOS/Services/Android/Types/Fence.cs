using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Android
{
    [StructLayout(LayoutKind.Sequential, Size = 0x8)]
    struct Fence
    {
        public int Id;
        public int Value;
    }
}