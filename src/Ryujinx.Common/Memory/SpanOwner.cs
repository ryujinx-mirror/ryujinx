using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ryujinx.Common.Memory
{
    /// <summary>
    /// A stack-only type that rents a buffer of a specified length from <seealso cref="ArrayPool{T}.Shared"/>.
    /// It does not implement <see cref="IDisposable"/> to avoid being boxed, but should still be disposed. This
    /// is easy since C# 8, which allows use of C# `using` constructs on any type that has a public Dispose() method.
    /// To keep this type simple, fast, and read-only, it does not check or guard against multiple disposals.
    /// For all these reasons, all usage should be with a `using` block or statement.
    /// </summary>
    /// <typeparam name="T">The type of item to store.</typeparam>
    public readonly ref struct SpanOwner<T>
    {
        private readonly int _length;
        private readonly T[] _array;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpanOwner{T}"/> struct with the specified parameters.
        /// </summary>
        /// <param name="length">The length of the new memory buffer to use</param>
        private SpanOwner(int length)
        {
            _length = length;
            _array = ArrayPool<T>.Shared.Rent(length);
        }

        /// <summary>
        /// Gets an empty <see cref="SpanOwner{T}"/> instance.
        /// </summary>
        public static SpanOwner<T> Empty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(0);
        }

        /// <summary>
        /// Creates a new <see cref="SpanOwner{T}"/> instance with the specified length.
        /// </summary>
        /// <param name="length">The length of the new memory buffer to use</param>
        /// <returns>A <see cref="SpanOwner{T}"/> instance of the requested length</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is not valid</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SpanOwner<T> Rent(int length) => new(length);

        /// <summary>
        /// Creates a new <see cref="SpanOwner{T}"/> instance with the length and the content cleared.
        /// </summary>
        /// <param name="length">The length of the new memory buffer to use</param>
        /// <returns>A <see cref="SpanOwner{T}"/> instance of the requested length and the content cleared</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is not valid</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SpanOwner<T> RentCleared(int length)
        {
            SpanOwner<T> result = new(length);

            result._array.AsSpan(0, length).Clear();

            return result;
        }

        /// <summary>
        /// Creates a new <see cref="SpanOwner{T}"/> instance with the content copied from the specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer to copy</param>
        /// <returns>A <see cref="SpanOwner{T}"/> instance with the same length and content as <paramref name="buffer"/></returns>
        public static SpanOwner<T> RentCopy(ReadOnlySpan<T> buffer)
        {
            SpanOwner<T> result = new(buffer.Length);

            buffer.CopyTo(result._array);

            return result;
        }

        /// <summary>
        /// Gets the number of items in the current instance
        /// </summary>
        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _length;
        }

        /// <summary>
        /// Gets a <see cref="Span{T}"/> wrapping the memory belonging to the current instance.
        /// </summary>
        /// <remarks>
        /// Uses a trick made possible by the .NET 6+ runtime array layout.
        /// </remarks>
        public Span<T> Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ref T firstElementRef = ref MemoryMarshal.GetArrayDataReference(_array);

                return MemoryMarshal.CreateSpan(ref firstElementRef, _length);
            }
        }

        /// <summary>
        /// Implements the duck-typed <see cref="IDisposable.Dispose"/> method.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(_array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
        }
    }
}
