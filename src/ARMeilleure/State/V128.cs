using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ARMeilleure.State
{
    /// <summary>
    /// Represents a 128-bit vector.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct V128 : IEquatable<V128>
    {
        // _e0 & _e1 could be marked as readonly, however they are not readonly because we modify them through the Unsafe
        // APIs. This also means that one should be careful when changing the layout of this struct.

        private readonly ulong _e0;
        private readonly ulong _e1;

        /// <summary>
        /// Gets a new <see cref="V128"/> with all bits set to zero.
        /// </summary>
        public static V128 Zero => new(0, 0);

        /// <summary>
        /// Initializes a new instance of the <see cref="V128"/> struct with the specified <see cref="double"/> value
        /// as a scalar.
        /// </summary>
        /// <param name="value">Scalar value</param>
        public V128(double value) : this(value, 0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="V128"/> struct with the specified <see cref="double"/> elements.
        /// </summary>
        /// <param name="e0">Element 0</param>
        /// <param name="e1">Element 1</param>
        public V128(double e0, double e1)
        {
            _e0 = (ulong)BitConverter.DoubleToInt64Bits(e0);
            _e1 = (ulong)BitConverter.DoubleToInt64Bits(e1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="V128"/> struct with the specified <see cref="float"/> value as a
        /// scalar.
        /// </summary>
        /// <param name="value">Scalar value</param>
        public V128(float value) : this(value, 0, 0, 0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="V128"/> struct with the specified <see cref="float"/> elements.
        /// </summary>
        /// <param name="e0">Element 0</param>
        /// <param name="e1">Element 1</param>
        /// <param name="e2">Element 2</param>
        /// <param name="e3">Element 3</param>
        public V128(float e0, float e1, float e2, float e3)
        {
            _e0 = (ulong)(uint)BitConverter.SingleToInt32Bits(e0) << 0;
            _e0 |= (ulong)(uint)BitConverter.SingleToInt32Bits(e1) << 32;
            _e1 = (ulong)(uint)BitConverter.SingleToInt32Bits(e2) << 0;
            _e1 |= (ulong)(uint)BitConverter.SingleToInt32Bits(e3) << 32;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="V128"/> struct with the specified <see cref="ulong"/>
        /// elements.
        /// </summary>
        /// <param name="e0">Element 0</param>
        /// <param name="e1">Element 1</param>
        public V128(long e0, long e1) : this((ulong)e0, (ulong)e1) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="V128"/> struct with the specified <see cref="long"/> elements.
        /// </summary>
        /// <param name="e0">Element 0</param>
        /// <param name="e1">Element 1</param>
        public V128(ulong e0, ulong e1)
        {
            _e0 = e0;
            _e1 = e1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="V128"/> struct with the specified <see cref="int"/> elements.
        /// </summary>
        /// <param name="e0">Element 0</param>
        /// <param name="e1">Element 1</param>
        /// <param name="e2">Element 2</param>
        /// <param name="e3">Element 3</param>
        public V128(int e0, int e1, int e2, int e3) : this((uint)e0, (uint)e1, (uint)e2, (uint)e3) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="V128"/> struct with the specified <see cref="uint"/> elements.
        /// </summary>
        /// <param name="e0">Element 0</param>
        /// <param name="e1">Element 1</param>
        /// <param name="e2">Element 2</param>
        /// <param name="e3">Element 3</param>
        public V128(uint e0, uint e1, uint e2, uint e3)
        {
            _e0 = (ulong)e0 << 0;
            _e0 |= (ulong)e1 << 32;
            _e1 = (ulong)e2 << 0;
            _e1 |= (ulong)e3 << 32;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="V128"/> struct from the specified <see cref="byte"/> array.
        /// </summary>
        /// <param name="data"><see cref="byte"/> array to use</param>
        public V128(byte[] data)
        {
            _e0 = (ulong)BitConverter.ToInt64(data, 0);
            _e1 = (ulong)BitConverter.ToInt64(data, 8);
        }

        /// <summary>
        /// Returns the value of the <see cref="V128"/> as a <typeparamref name="T"/> scalar.
        /// </summary>
        /// <typeparam name="T">Type of scalar</typeparam>
        /// <returns>Value of the <see cref="V128"/> as a <typeparamref name="T"/> scalar</returns>
        /// <exception cref="ArgumentOutOfRangeException">Size of <typeparamref name="T"/> is larger than 16 bytes</exception>
        public T As<T>() where T : unmanaged
        {
            return Extract<T>(0);
        }

        /// <summary>
        /// Extracts the element at the specified index as a <typeparamref name="T"/> from the <see cref="V128"/>.
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="index">Index of element</param>
        /// <returns>Element at the specified index as a <typeparamref name="T"/> from the <see cref="V128"/></returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is out of bound or the size of <typeparamref name="T"/> is larger than 16 bytes
        /// </exception>
        public T Extract<T>(int index) where T : unmanaged
        {
            if ((uint)index >= GetElementCount<T>())
            {
                ThrowIndexOutOfRange();
            }

            // Performs:
            //  return *((*T)this + index);
            return Unsafe.Add(ref Unsafe.As<V128, T>(ref this), index);
        }

        /// <summary>
        /// Inserts the specified value into the element at the specified index in the <see cref="V128"/>.
        /// </summary>
        /// <typeparam name="T">Element type</typeparam>
        /// <param name="index">Index of element</param>
        /// <param name="value">Value to insert</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is out of bound or the size of <typeparamref name="T"/> is larger than 16 bytes
        /// </exception>
        public void Insert<T>(int index, T value) where T : unmanaged
        {
            if ((uint)index >= GetElementCount<T>())
            {
                ThrowIndexOutOfRange();
            }

            // Performs:
            //  *((*T)this + index) = value;
            Unsafe.Add(ref Unsafe.As<V128, T>(ref this), index) = value;
        }

        /// <summary>
        /// Returns a new <see cref="byte"/> array which represents the <see cref="V128"/>.
        /// </summary>
        /// <returns>A new <see cref="byte"/> array which represents the <see cref="V128"/></returns>
        public readonly byte[] ToArray()
        {
            byte[] data = new byte[16];
            Span<byte> span = data;

            BitConverter.TryWriteBytes(span, _e0);
            BitConverter.TryWriteBytes(span[8..], _e1);

            return data;
        }

        /// <summary>
        /// Performs a bitwise logical left shift on the specified <see cref="V128"/> by the specified shift count.
        /// </summary>
        /// <param name="x"><see cref="V128"/> instance</param>
        /// <param name="shift">Number of shifts</param>
        /// <returns>Result of left shift</returns>
        /// <remarks>
        /// This supports shift counts up to 63; anything above may result in unexpected behaviour.
        /// </remarks>
        public static V128 operator <<(V128 x, int shift)
        {
            if (shift == 0)
            {
                return new V128(x._e0, x._e1);
            }

            ulong shiftOut = x._e0 >> (64 - shift);

            return new V128(x._e0 << shift, (x._e1 << shift) | shiftOut);
        }

        /// <summary>
        /// Performs a bitwise logical right shift on the specified <see cref="V128"/> by the specified shift count.
        /// </summary>
        /// <param name="x"><see cref="V128"/> instance</param>
        /// <param name="shift">Number of shifts</param>
        /// <returns>Result of right shift</returns>
        /// <remarks>
        /// This supports shift counts up to 63; anything above may result in unexpected behaviour.
        /// </remarks>
        public static V128 operator >>(V128 x, int shift)
        {
            if (shift == 0)
            {
                return new V128(x._e0, x._e1);
            }

            ulong shiftOut = x._e1 & ((1UL << shift) - 1);

            return new V128((x._e0 >> shift) | (shiftOut << (64 - shift)), x._e1 >> shift);
        }

        /// <summary>
        /// Performs a bitwise not on the specified <see cref="V128"/>.
        /// </summary>
        /// <param name="x">Target <see cref="V128"/></param>
        /// <returns>Result of not operation</returns>
        public static V128 operator ~(V128 x) => new(~x._e0, ~x._e1);

        /// <summary>
        /// Performs a bitwise and on the specified <see cref="V128"/> instances.
        /// </summary>
        /// <param name="x">First instance</param>
        /// <param name="y">Second instance</param>
        /// <returns>Result of and operation</returns>
        public static V128 operator &(V128 x, V128 y) => new(x._e0 & y._e0, x._e1 & y._e1);

        /// <summary>
        /// Performs a bitwise or on the specified <see cref="V128"/> instances.
        /// </summary>
        /// <param name="x">First instance</param>
        /// <param name="y">Second instance</param>
        /// <returns>Result of or operation</returns>
        public static V128 operator |(V128 x, V128 y) => new(x._e0 | y._e0, x._e1 | y._e1);

        /// <summary>
        /// Performs a bitwise exlusive or on the specified <see cref="V128"/> instances.
        /// </summary>
        /// <param name="x">First instance</param>
        /// <param name="y">Second instance</param>
        /// <returns>Result of exclusive or operation</returns>
        public static V128 operator ^(V128 x, V128 y) => new(x._e0 ^ y._e0, x._e1 ^ y._e1);

        /// <summary>
        /// Determines if the specified <see cref="V128"/> instances are equal.
        /// </summary>
        /// <param name="x">First instance</param>
        /// <param name="y">Second instance</param>
        /// <returns>true if equal; otherwise false</returns>
        public static bool operator ==(V128 x, V128 y) => x.Equals(y);

        /// <summary>
        /// Determines if the specified <see cref="V128"/> instances are not equal.
        /// </summary>
        /// <param name="x">First instance</param>
        /// <param name="y">Second instance</param>
        /// <returns>true if not equal; otherwise false</returns>
        public static bool operator !=(V128 x, V128 y) => !x.Equals(y);

        /// <summary>
        /// Determines if the specified <see cref="V128"/> is equal to this <see cref="V128"/> instance.
        /// </summary>
        /// <param name="other">Other <see cref="V128"/> instance</param>
        /// <returns>true if equal; otherwise false</returns>
        public readonly bool Equals(V128 other)
        {
            return other._e0 == _e0 && other._e1 == _e1;
        }

        /// <summary>
        /// Determines if the specified <see cref="object"/> is equal to this <see cref="V128"/> instance.
        /// </summary>
        /// <param name="obj">Other <see cref="object"/> instance</param>
        /// <returns>true if equal; otherwise false</returns>
        public readonly override bool Equals(object obj)
        {
            return obj is V128 vector && Equals(vector);
        }

        /// <inheritdoc/>
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(_e0, _e1);
        }

        /// <inheritdoc/>
        public readonly override string ToString()
        {
            return $"0x{_e1:X16}{_e0:X16}";
        }

        private static uint GetElementCount<T>() where T : unmanaged
        {
            return (uint)(Unsafe.SizeOf<V128>() / Unsafe.SizeOf<T>());
        }

        private static void ThrowIndexOutOfRange()
        {
            throw new ArgumentOutOfRangeException("index");
        }
    }
}
