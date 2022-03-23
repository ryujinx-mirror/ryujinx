using System;

namespace Ryujinx.Graphics.Vic.Image
{
    class BufferPool<T>
    {
        /// <summary>
        /// Maximum number of buffers on the pool.
        /// </summary>
        private const int MaxBuffers = 4;

        /// <summary>
        /// Maximum size of a buffer that can be added on the pool.
        /// If the required buffer is larger than this, it won't be
        /// added to the pool to avoid long term high memory usage.
        /// </summary>
        private const int MaxBufferSize = 2048 * 2048;

        private struct PoolItem
        {
            public bool InUse;
            public T[] Buffer;
        }

        private readonly PoolItem[] _pool = new PoolItem[MaxBuffers];

        /// <summary>
        /// Rents a buffer with the exact size requested.
        /// </summary>
        /// <param name="length">Size of the buffer</param>
        /// <param name="buffer">Span of the requested size</param>
        /// <returns>The index of the buffer on the pool</returns>
        public int Rent(int length, out Span<T> buffer)
        {
            int index = RentMinimum(length, out T[] bufferArray);

            buffer = new Span<T>(bufferArray).Slice(0, length);

            return index;
        }

        /// <summary>
        /// Rents a buffer with a size greater than or equal to the requested size.
        /// </summary>
        /// <param name="length">Size of the buffer</param>
        /// <param name="buffer">Array with a length greater than or equal to the requested length</param>
        /// <returns>The index of the buffer on the pool</returns>
        public int RentMinimum(int length, out T[] buffer)
        {
            if ((uint)length > MaxBufferSize)
            {
                buffer = new T[length];
                return -1;
            }

            // Try to find a buffer that is larger or the same size of the requested one.
            // This will avoid an allocation.
            for (int i = 0; i < MaxBuffers; i++)
            {
                ref PoolItem item = ref _pool[i];

                if (!item.InUse && item.Buffer != null && item.Buffer.Length >= length)
                {
                    buffer = item.Buffer;
                    item.InUse = true;
                    return i;
                }
            }

            buffer = new T[length];

            // Try to add the new buffer to the pool.
            // We try to find a slot that is not in use, and replace the buffer in it.
            for (int i = 0; i < MaxBuffers; i++)
            {
                ref PoolItem item = ref _pool[i];

                if (!item.InUse)
                {
                    item.Buffer = buffer;
                    item.InUse = true;
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns a buffer returned from <see cref="Rent(int)"/> to the pool.
        /// </summary>
        /// <param name="index">Index of the buffer on the pool</param>
        public void Return(int index)
        {
            if (index < 0)
            {
                return;
            }

            _pool[index].InUse = false;
        }
    }
}
