using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.GAL.Multithreading.Model
{
    unsafe struct PinnedSpan<T> where T : unmanaged
    {
        private void* _ptr;
        private int _size;

        public PinnedSpan(ReadOnlySpan<T> span)
        {
            _ptr = Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
            _size = span.Length;
        }

        public ReadOnlySpan<T> Get()
        {
            return new ReadOnlySpan<T>(_ptr, _size * Unsafe.SizeOf<T>());
        }
    }
}
