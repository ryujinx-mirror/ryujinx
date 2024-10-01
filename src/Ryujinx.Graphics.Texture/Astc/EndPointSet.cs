using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Texture.Astc
{
    [StructLayout(LayoutKind.Sequential, Size = AstcPixel.StructSize * 8)]
    internal struct EndPointSet
    {
        private AstcPixel _start;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<AstcPixel> Get(int index)
        {
            Debug.Assert(index < 4);

            ref AstcPixel start = ref Unsafe.Add(ref _start, index * 2);

            return MemoryMarshal.CreateSpan(ref start, 2);
        }
    }
}
