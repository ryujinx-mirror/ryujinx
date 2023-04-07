using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Texture.Astc
{
    [StructLayout(LayoutKind.Sequential)]
    struct AstcPixel
    {
        internal const int StructSize = 12;

        public short A;
        public short R;
        public short G;
        public short B;

        private uint _bitDepthInt;

        private Span<byte> BitDepth => MemoryMarshal.CreateSpan(ref Unsafe.As<uint, byte>(ref _bitDepthInt), 4);
        private Span<short> Components => MemoryMarshal.CreateSpan(ref A, 4);

        public AstcPixel(short a, short r, short g, short b)
        {
            A = a;
            R = r;
            G = g;
            B = b;

            _bitDepthInt = 0x08080808;
        }

        public void ClampByte()
        {
            R = Math.Min(Math.Max(R, (short)0), (short)255);
            G = Math.Min(Math.Max(G, (short)0), (short)255);
            B = Math.Min(Math.Max(B, (short)0), (short)255);
            A = Math.Min(Math.Max(A, (short)0), (short)255);
        }

        public short GetComponent(int index)
        {
            return Components[index];
        }

        public void SetComponent(int index, int value)
        {
            Components[index] = (short)value;
        }

        public int Pack()
        {
            return A << 24 |
                   B << 16 |
                   G << 8 |
                   R << 0;
        }

        // Adds more precision to the blue channel as described
        // in C.2.14
        public static AstcPixel BlueContract(int a, int r, int g, int b)
        {
            return new AstcPixel((short)(a),
                                 (short)((r + b) >> 1),
                                 (short)((g + b) >> 1),
                                 (short)(b));
        }
    }
}
