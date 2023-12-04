using System;

namespace Ryujinx.Common.Memory
{
    /// <summary>
    /// A struct that can represent both a Span and Array.
    /// This is useful to keep the Array representation when possible to avoid copies.
    /// </summary>
    /// <typeparam name="T">Element Type</typeparam>
    public readonly ref struct SpanOrArray<T> where T : unmanaged
    {
        public readonly T[] Array;
        public readonly ReadOnlySpan<T> Span;

        /// <summary>
        /// Create a new SpanOrArray from an array.
        /// </summary>
        /// <param name="array">Array to store</param>
        public SpanOrArray(T[] array)
        {
            Array = array;
            Span = ReadOnlySpan<T>.Empty;
        }

        /// <summary>
        /// Create a new SpanOrArray from a readonly span.
        /// </summary>
        /// <param name="array">Span to store</param>
        public SpanOrArray(ReadOnlySpan<T> span)
        {
            Array = null;
            Span = span;
        }

        /// <summary>
        /// Return the contained array, or convert the span if necessary.
        /// </summary>
        /// <returns>An array containing the data</returns>
        public T[] ToArray()
        {
            return Array ?? Span.ToArray();
        }

        /// <summary>
        /// Return a ReadOnlySpan from either the array or ReadOnlySpan.
        /// </summary>
        /// <returns>A ReadOnlySpan containing the data</returns>
        public ReadOnlySpan<T> AsSpan()
        {
            return Array ?? Span;
        }

        /// <summary>
        /// Cast an array to a SpanOrArray.
        /// </summary>
        /// <param name="array">Source array</param>
        public static implicit operator SpanOrArray<T>(T[] array)
        {
            return new SpanOrArray<T>(array);
        }

        /// <summary>
        /// Cast a ReadOnlySpan to a SpanOrArray.
        /// </summary>
        /// <param name="span">Source ReadOnlySpan</param>
        public static implicit operator SpanOrArray<T>(ReadOnlySpan<T> span)
        {
            return new SpanOrArray<T>(span);
        }

        /// <summary>
        /// Cast a Span to a SpanOrArray.
        /// </summary>
        /// <param name="span">Source Span</param>
        public static implicit operator SpanOrArray<T>(Span<T> span)
        {
            return new SpanOrArray<T>(span);
        }

        /// <summary>
        /// Cast a SpanOrArray to a ReadOnlySpan
        /// </summary>
        /// <param name="spanOrArray">Source SpanOrArray</param>
        public static implicit operator ReadOnlySpan<T>(SpanOrArray<T> spanOrArray)
        {
            return spanOrArray.AsSpan();
        }
    }
}
