using System;
using System.Runtime.CompilerServices;

namespace Ryujinx.Audio.Renderer.Utils
{
    /// <summary>
    /// A simple bit array implementation backed by a <see cref="Memory{T}"/>.
    /// </summary>
    public class BitArray
    {
        /// <summary>
        /// The backing storage of the <see cref="BitArray"/>.
        /// </summary>
        private readonly Memory<byte> _storage;

        /// <summary>
        /// Create a new <see cref="BitArray"/> from <see cref="Memory{T}"/>.
        /// </summary>
        /// <param name="storage">The backing storage of the <see cref="BitArray"/>.</param>
        public BitArray(Memory<byte> storage)
        {
            _storage = storage;
        }

        /// <summary>
        /// Get the byte position of a given bit index.
        /// </summary>
        /// <param name="index">A bit index.</param>
        /// <returns>The byte position of a given bit index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ToPosition(int index) => index / 8;

        /// <summary>
        /// Get the bit position of a given bit index inside a byte.
        /// </summary>
        /// <param name="index">A bit index.</param>
        /// <returns>The bit position of a given bit index inside a byte.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ToBitPosition(int index) => index % 8;

        /// <summary>
        /// Test if the bit at the given index is set.
        /// </summary>
        /// <param name="index">A bit index.</param>
        /// <returns>Return true if the bit at the given index is set</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Test(int index)
        {
            ulong mask = 1ul << ToBitPosition(index);

            return (_storage.Span[ToPosition(index)] & mask) == mask;
        }

        /// <summary>
        /// Set the bit at the given index.
        /// </summary>
        /// <param name="index">A bit index.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int index)
        {
            Set(index, true);
        }

        /// <summary>
        /// Reset the bit at the given index.
        /// </summary>
        /// <param name="index">A bit index.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset(int index)
        {
            Set(index, false);
        }

        /// <summary>
        /// Set a bit value at the given index.
        /// </summary>
        /// <param name="index">A bit index.</param>
        /// <param name="value">The new bit value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Set(int index, bool value)
        {
            byte mask = (byte)(1 << ToBitPosition(index));

            if (value)
            {
                _storage.Span[ToPosition(index)] |= mask;
            }
            else
            {
                _storage.Span[ToPosition(index)] &= (byte)~mask;
            }
        }

        /// <summary>
        /// Reset all bits in the storage.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _storage.Span.Clear();
        }
    }
}
