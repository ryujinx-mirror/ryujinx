using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Texture
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 12)]
    public struct Bpp12Pixel
    {
        private ulong _elem1;
        private uint _elem2;
    }
}
