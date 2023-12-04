using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Texture
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 12)]
    public readonly struct Bpp12Pixel
    {
        private readonly ulong _elem1;
        private readonly uint _elem2;
    }
}
