using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Utils
{
    public sealed unsafe class SpanMemoryManager<T> : MemoryManager<T>
        where T : unmanaged
    {
        private readonly T* _pointer;
        private readonly int _length;

        public SpanMemoryManager(Span<T> span)
        {
            fixed (T* ptr = &MemoryMarshal.GetReference(span))
            {
                _pointer = ptr;
                _length = span.Length;
            }
        }

        public override Span<T> GetSpan() => new(_pointer, _length);

        public override MemoryHandle Pin(int elementIndex = 0)
        {
            if (elementIndex < 0 || elementIndex >= _length)
            {
                throw new ArgumentOutOfRangeException(nameof(elementIndex));
            }

            return new MemoryHandle(_pointer + elementIndex);
        }

        public override void Unpin() { }

        protected override void Dispose(bool disposing) { }

        public static Memory<T> Cast<TFrom>(Memory<TFrom> memory) where TFrom : unmanaged
        {
            return new SpanMemoryManager<T>(MemoryMarshal.Cast<TFrom, T>(memory.Span)).Memory;
        }
    }
}
