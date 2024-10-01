using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Caps.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x40)]
    struct ScreenShotAttribute
    {
        public uint Unknown0x00; // Always 0
        public AlbumImageOrientation AlbumImageOrientation;
        public uint Unknown0x08; // Always 0
        public uint Unknown0x0C; // Always 1
        public Array30<byte> Unknown0x10; // Always 0
    }
}
