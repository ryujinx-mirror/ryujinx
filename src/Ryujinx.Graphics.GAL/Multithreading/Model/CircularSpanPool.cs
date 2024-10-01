using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ryujinx.Graphics.GAL.Multithreading.Model
{
    /// <summary>
    /// A memory pool for passing through Span<T> resources with one producer and consumer.
    /// Data is copied on creation to part of the pool, then that region is reserved until it is disposed by the consumer.
    /// Similar to the command queue, this pool assumes that data is created and disposed in the same order.
    /// </summary>
    class CircularSpanPool
    {
        private readonly ThreadedRenderer _renderer;
        private readonly byte[] _pool;
        private readonly int _size;

        private int _producerPtr;
        private int _producerSkipPosition = -1;
        private int _consumerPtr;

        public CircularSpanPool(ThreadedRenderer renderer, int size)
        {
            _renderer = renderer;
            _size = size;
            _pool = new byte[size];
        }

        public SpanRef<T> Insert<T>(ReadOnlySpan<T> data) where T : unmanaged
        {
            int size = data.Length * Unsafe.SizeOf<T>();

            // Wrapping aware circular queue.
            // If there's no space at the end of the pool for this span, we can't fragment it.
            // So just loop back around to the start. Remember the last skipped position.

            bool wraparound = _producerPtr + size >= _size;
            int index = wraparound ? 0 : _producerPtr;

            // _consumerPtr is from another thread, and we're taking it without a lock, so treat this as a snapshot in the past.
            // We know that it will always be before or equal to the producer pointer, and it cannot pass it.
            // This is enough to reason about if there is space in the queue for the data, even if we're checking against an outdated value.

            int consumer = _consumerPtr;
            bool beforeConsumer = _producerPtr < consumer;

            if (size > _size - 1 || (wraparound && beforeConsumer) || ((index < consumer || wraparound) && index + size >= consumer))
            {
                // Just get an array in the following situations:
                // - The data is too large to fit in the pool.
                // - A wraparound would happen but the consumer would be covered by it.
                // - The producer would catch up to the consumer as a result.

                return new SpanRef<T>(_renderer, data.ToArray());
            }

            data.CopyTo(MemoryMarshal.Cast<byte, T>(new Span<byte>(_pool).Slice(index, size)));

            if (wraparound)
            {
                _producerSkipPosition = _producerPtr;
            }

            _producerPtr = index + size;

            return new SpanRef<T>(data.Length);
        }

        public Span<T> Get<T>(int length) where T : unmanaged
        {
            int size = length * Unsafe.SizeOf<T>();

            if (_consumerPtr == Interlocked.CompareExchange(ref _producerSkipPosition, -1, _consumerPtr))
            {
                _consumerPtr = 0;
            }

            return MemoryMarshal.Cast<byte, T>(new Span<byte>(_pool).Slice(_consumerPtr, size));
        }

        public void Dispose<T>(int length) where T : unmanaged
        {
            int size = length * Unsafe.SizeOf<T>();

            _consumerPtr += size;
        }
    }
}
