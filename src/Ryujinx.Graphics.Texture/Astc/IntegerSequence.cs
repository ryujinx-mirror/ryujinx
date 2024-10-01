using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.Texture.Astc
{
    [StructLayout(LayoutKind.Sequential, Size = IntegerEncoded.StructSize * Capacity + sizeof(int))]
    internal struct IntegerSequence
    {
        private const int Capacity = 100;

        private int _length;
        private IntegerEncoded _start;

        public Span<IntegerEncoded> List => MemoryMarshal.CreateSpan(ref _start, _length);

        public void Reset() => _length = 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ref IntegerEncoded item)
        {
            Debug.Assert(_length < Capacity);

            int oldLength = _length;
            _length++;

            List[oldLength] = item;
        }
    }
}
