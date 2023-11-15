using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Audio.Renderer.Utils
{
    /// <summary>
    /// Helper for IO operations on <see cref="Span{T}"/> and <see cref="Memory{T}"/>.
    /// </summary>
    public static class SpanIOHelper
    {
        /// <summary>
        /// Write the given data to the given backing <see cref="Memory{T}"/> and move cursor after the written data.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="backingMemory">The backing <see cref="Memory{T}"/> to store the data.</param>
        /// <param name="data">The data to write to the backing <see cref="Memory{T}"/>.</param>
        public static void Write<T>(ref Memory<byte> backingMemory, ref T data) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();

            if (size > backingMemory.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(backingMemory), backingMemory.Length, null);
            }

            MemoryMarshal.Write(backingMemory.Span[..size], in data);

            backingMemory = backingMemory[size..];
        }

        /// <summary>
        /// Write the given data to the given backing <see cref="Span{T}"/> and move cursor after the written data.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="backingMemory">The backing <see cref="Span{T}"/> to store the data.</param>
        /// <param name="data">The data to write to the backing <see cref="Span{T}"/>.</param>
        public static void Write<T>(ref Span<byte> backingMemory, ref T data) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();

            if (size > backingMemory.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(backingMemory), backingMemory.Length, null);
            }

            MemoryMarshal.Write(backingMemory[..size], in data);

            backingMemory = backingMemory[size..];
        }

        /// <summary>
        /// Get a <see cref="Span{T}"/> out of a <paramref name="backingMemory"/> and move cursor after T size.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="backingMemory">The backing <see cref="Memory{T}"/> to get a <see cref="Span{T}"/> from.</param>
        /// <returns>A <see cref="Span{T}"/> from backing <see cref="Memory{T}"/>.</returns>
        public static Span<T> GetWriteRef<T>(ref Memory<byte> backingMemory) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();

            if (size > backingMemory.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(backingMemory), backingMemory.Length, null);
            }

            Span<T> result = MemoryMarshal.Cast<byte, T>(backingMemory.Span[..size]);

            backingMemory = backingMemory[size..];

            return result;
        }

        /// <summary>
        /// Get a <see cref="Span{T}"/> out of a backingMemory and move cursor after T size.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="backingMemory">The backing <see cref="Span{T}"/> to get a <see cref="Span{T}"/> from.</param>
        /// <returns>A <see cref="Span{T}"/> from backing <see cref="Span{T}"/>.</returns>
        public static Span<T> GetWriteRef<T>(ref Span<byte> backingMemory) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();

            if (size > backingMemory.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(backingMemory), backingMemory.Length, null);
            }

            Span<T> result = MemoryMarshal.Cast<byte, T>(backingMemory[..size]);

            backingMemory = backingMemory[size..];

            return result;
        }

        /// <summary>
        /// Read data from the given backing <see cref="ReadOnlyMemory{T}"/> and move cursor after the read data.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="backingMemory">The backing <see cref="ReadOnlyMemory{T}"/> to read data from.</param>
        /// <returns>Return the read data.</returns>
        public static T Read<T>(ref ReadOnlyMemory<byte> backingMemory) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();

            if (size > backingMemory.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(backingMemory), backingMemory.Length, null);
            }

            T result = MemoryMarshal.Read<T>(backingMemory.Span[..size]);

            backingMemory = backingMemory[size..];

            return result;
        }

        /// <summary>
        /// Read data from the given backing <see cref="ReadOnlySpan{T}"/> and move cursor after the read data.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="backingMemory">The backing <see cref="ReadOnlySpan{T}"/> to read data from.</param>
        /// <returns>Return the read data.</returns>
        public static T Read<T>(ref ReadOnlySpan<byte> backingMemory) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();

            if (size > backingMemory.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(backingMemory), backingMemory.Length, null);
            }

            T result = MemoryMarshal.Read<T>(backingMemory[..size]);

            backingMemory = backingMemory[size..];

            return result;
        }

        /// <summary>
        /// Extract a <see cref="Memory{T}"/> at the given index.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="memory">The <see cref="Memory{T}"/> to extract the data from.</param>
        /// <param name="id">The id in the provided memory.</param>
        /// <param name="count">The max allowed count. (for bound checking of the id in debug mode)</param>
        /// <returns>a <see cref="Memory{T}"/> at the given id.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<T> GetMemory<T>(Memory<T> memory, int id, uint count) where T : unmanaged
        {
            Debug.Assert(id >= 0 && id < count);

            return memory.Slice(id, 1);
        }

        /// <summary>
        /// Extract a ref T at the given index.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="memory">The <see cref="Memory{T}"/> to extract the data from.</param>
        /// <param name="id">The id in the provided memory.</param>
        /// <param name="count">The max allowed count. (for bound checking of the id in debug mode)</param>
        /// <returns>a ref T at the given id.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetFromMemory<T>(Memory<T> memory, int id, uint count) where T : unmanaged
        {
            return ref GetMemory(memory, id, count).Span[0];
        }
    }
}
