using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.GAL
{
    [StructLayout(LayoutKind.Sequential, Size = 8)]
    public readonly record struct BufferHandle
    {
        private readonly ulong _value;

        public static BufferHandle Null => new(0);

        private BufferHandle(ulong value) => _value = value;
    }
}
