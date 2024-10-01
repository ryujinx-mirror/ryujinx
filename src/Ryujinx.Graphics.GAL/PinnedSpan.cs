using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Graphics.GAL
{
    public readonly unsafe struct PinnedSpan<T> : IDisposable where T : unmanaged
    {
        private readonly void* _ptr;
        private readonly int _size;
        private readonly Action _disposeAction;

        /// <summary>
        /// Creates a new PinnedSpan from an existing ReadOnlySpan. The span *must* be pinned in memory.
        /// The data must be guaranteed to live until disposeAction is called.
        /// </summary>
        /// <param name="span">Existing span</param>
        /// <param name="disposeAction">Action to call on dispose</param>
        /// <remarks>
        /// If a dispose action is not provided, it is safe to assume the resource will be available until the next call.
        /// </remarks>
        public static PinnedSpan<T> UnsafeFromSpan(ReadOnlySpan<T> span, Action disposeAction = null)
        {
            return new PinnedSpan<T>(Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), span.Length, disposeAction);
        }

        /// <summary>
        /// Creates a new PinnedSpan from an existing unsafe region. The data must be guaranteed to live until disposeAction is called.
        /// </summary>
        /// <param name="ptr">Pointer to the region</param>
        /// <param name="size">The total items of T the region contains</param>
        /// <param name="disposeAction">Action to call on dispose</param>
        /// <remarks>
        /// If a dispose action is not provided, it is safe to assume the resource will be available until the next call.
        /// </remarks>
        public PinnedSpan(void* ptr, int size, Action disposeAction = null)
        {
            _ptr = ptr;
            _size = size;
            _disposeAction = disposeAction;
        }

        public ReadOnlySpan<T> Get()
        {
            return new ReadOnlySpan<T>(_ptr, _size * Unsafe.SizeOf<T>());
        }

        public void Dispose()
        {
            _disposeAction?.Invoke();
        }
    }
}
