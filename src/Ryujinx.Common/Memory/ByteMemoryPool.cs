using System;
using System.Buffers;

namespace Ryujinx.Common.Memory
{
    /// <summary>
    /// Provides a pool of re-usable byte array instances.
    /// </summary>
    public sealed partial class ByteMemoryPool
    {
        private static readonly ByteMemoryPool _shared = new();

        /// <summary>
        /// Constructs a <see cref="ByteMemoryPool"/> instance. Private to force access through
        /// the <see cref="ByteMemoryPool.Shared"/> instance.
        /// </summary>
        private ByteMemoryPool()
        {
            // No implementation
        }

        /// <summary>
        /// Retrieves a shared <see cref="ByteMemoryPool"/> instance.
        /// </summary>
        public static ByteMemoryPool Shared => _shared;

        /// <summary>
        /// Returns the maximum buffer size supported by this pool.
        /// </summary>
        public static int MaxBufferSize => Array.MaxLength;

        /// <summary>
        /// Rents a byte memory buffer from <see cref="ArrayPool{Byte}.Shared"/>.
        /// The buffer may contain data from a prior use.
        /// </summary>
        /// <param name="length">The buffer's required length in bytes</param>
        /// <returns>A <see cref="IMemoryOwner{Byte}"/> wrapping the rented memory</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IMemoryOwner<byte> Rent(long length)
            => RentImpl(checked((int)length));

        /// <summary>
        /// Rents a byte memory buffer from <see cref="ArrayPool{Byte}.Shared"/>.
        /// The buffer may contain data from a prior use.
        /// </summary>
        /// <param name="length">The buffer's required length in bytes</param>
        /// <returns>A <see cref="IMemoryOwner{Byte}"/> wrapping the rented memory</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IMemoryOwner<byte> Rent(ulong length)
            => RentImpl(checked((int)length));

        /// <summary>
        /// Rents a byte memory buffer from <see cref="ArrayPool{Byte}.Shared"/>.
        /// The buffer may contain data from a prior use.
        /// </summary>
        /// <param name="length">The buffer's required length in bytes</param>
        /// <returns>A <see cref="IMemoryOwner{Byte}"/> wrapping the rented memory</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IMemoryOwner<byte> Rent(int length)
            => RentImpl(length);

        /// <summary>
        /// Rents a byte memory buffer from <see cref="ArrayPool{Byte}.Shared"/>.
        /// The buffer's contents are cleared (set to all 0s) before returning.
        /// </summary>
        /// <param name="length">The buffer's required length in bytes</param>
        /// <returns>A <see cref="IMemoryOwner{Byte}"/> wrapping the rented memory</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IMemoryOwner<byte> RentCleared(long length)
            => RentCleared(checked((int)length));

        /// <summary>
        /// Rents a byte memory buffer from <see cref="ArrayPool{Byte}.Shared"/>.
        /// The buffer's contents are cleared (set to all 0s) before returning.
        /// </summary>
        /// <param name="length">The buffer's required length in bytes</param>
        /// <returns>A <see cref="IMemoryOwner{Byte}"/> wrapping the rented memory</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IMemoryOwner<byte> RentCleared(ulong length)
            => RentCleared(checked((int)length));

        /// <summary>
        /// Rents a byte memory buffer from <see cref="ArrayPool{Byte}.Shared"/>.
        /// The buffer's contents are cleared (set to all 0s) before returning.
        /// </summary>
        /// <param name="length">The buffer's required length in bytes</param>
        /// <returns>A <see cref="IMemoryOwner{Byte}"/> wrapping the rented memory</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static IMemoryOwner<byte> RentCleared(int length)
        {
            var buffer = RentImpl(length);

            buffer.Memory.Span.Clear();

            return buffer;
        }

        private static ByteMemoryPoolBuffer RentImpl(int length)
        {
            if ((uint)length > Array.MaxLength)
            {
                throw new ArgumentOutOfRangeException(nameof(length), length, null);
            }

            return new ByteMemoryPoolBuffer(length);
        }
    }
}
