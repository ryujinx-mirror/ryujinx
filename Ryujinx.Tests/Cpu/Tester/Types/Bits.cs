// https://github.com/LDj3SNuD/ARM_v8-A_AArch64_Instructions_Tester/blob/master/Tester/Types/Bits.cs

// https://github.com/dotnet/corefx/blob/master/src/System.Collections/src/System/Collections/BitArray.cs

using System;
using System.Collections;
using System.Numerics;

namespace Ryujinx.Tests.Cpu.Tester.Types
{
    internal sealed class Bits : ICollection, IEnumerable, IEquatable<Bits>
    {
        private BitArray bits;

        public Bits(bool[] values) => bits = new BitArray(values);
        public Bits(byte[] bytes) => bits = new BitArray(bytes);
        public Bits(Bits bits) => this.bits = new BitArray(bits.bits);
        public Bits(int length) => bits = new BitArray(length);
        public Bits(int length, bool defaultValue) => bits = new BitArray(length, defaultValue);
        private Bits(BitArray bitArray) => bits = new BitArray(bitArray);
        public Bits(ulong value) => bits = new BitArray(BitConverter.GetBytes(value));
        public Bits(uint value) => bits = new BitArray(BitConverter.GetBytes(value));
        public Bits(ushort value) => bits = new BitArray(BitConverter.GetBytes(value));
        public Bits(byte value) => bits = new BitArray(new byte[1] {value});

        private BitArray ToBitArray() => new BitArray(bits);
        public ulong ToUInt64()
        {
            byte[] dst = new byte[8];

            bits.CopyTo(dst, 0);

            return BitConverter.ToUInt64(dst, 0);
        }
        public uint ToUInt32()
        {
            byte[] dst = new byte[4];

            bits.CopyTo(dst, 0);

            return BitConverter.ToUInt32(dst, 0);
        }
        public ushort ToUInt16()
        {
            byte[] dst = new byte[2];

            bits.CopyTo(dst, 0);

            return BitConverter.ToUInt16(dst, 0);
        }
        public byte ToByte()
        {
            byte[] dst = new byte[1];

            bits.CopyTo(dst, 0);

            return dst[0];
        }

        public bool this[int index] // ASL: "<>".
        {
            get
            {
                return bits.Get(index);
            }
            set
            {
                bits.Set(index, value);
            }
        }
        public Bits this[int highIndex, int lowIndex] // ASL: "<:>".
        {
            get
            {
                if (highIndex < lowIndex)
                {
                    throw new IndexOutOfRangeException();
                }

                bool[] dst = new bool[highIndex - lowIndex + 1];

                for (int i = lowIndex, n = 0; i <= highIndex; i++, n++)
                {
                    dst[n] = bits.Get(i);
                }

                return new Bits(dst);
            }
            set
            {
                if (highIndex < lowIndex)
                {
                    throw new IndexOutOfRangeException();
                }

                for (int i = lowIndex, n = 0; i <= highIndex; i++, n++)
                {
                    bits.Set(i, value.Get(n));
                }
            }
        }

        public bool IsReadOnly { get => false; } // Mutable.
        public int Count { get => bits.Count; }
        public bool IsSynchronized { get => bits.IsSynchronized; }
        public object SyncRoot { get => bits.SyncRoot; }
        public Bits And(Bits value) => new Bits(new BitArray(this.bits).And(value.bits)); // Immutable.
        public void CopyTo(Array array, int index) => bits.CopyTo(array, index);
        public bool Get(int index) => bits.Get(index);
        public IEnumerator GetEnumerator() => bits.GetEnumerator();
        //public Bits LeftShift(int count) => new Bits(new BitArray(bits).LeftShift(count)); // Immutable.
        public Bits Not() => new Bits(new BitArray(bits).Not()); // Immutable.
        public Bits Or(Bits value) => new Bits(new BitArray(this.bits).Or(value.bits)); // Immutable.
        //public Bits RightShift(int count) => new Bits(new BitArray(bits).RightShift(count)); // Immutable.
        public void Set(int index, bool value) => bits.Set(index, value);
        public void SetAll(bool value) => bits.SetAll(value);
        public Bits Xor(Bits value) => new Bits(new BitArray(this.bits).Xor(value.bits)); // Immutable.

        public static Bits Concat(Bits highBits, Bits lowBits) // ASL: ":".
        {
            if (((object)lowBits == null) || ((object)highBits == null))
            {
                throw new ArgumentNullException();
            }

            bool[] dst = new bool[lowBits.Count + highBits.Count];

            lowBits.CopyTo(dst, 0);
            highBits.CopyTo(dst, lowBits.Count);

            return new Bits(dst);
        }
        public static Bits Concat(bool bit3, bool bit2, bool bit1, bool bit0) // ASL: ":::".
        {
            return new Bits(new bool[] {bit0, bit1, bit2, bit3});
        }

        public static implicit operator Bits(bool value) => new Bits(1, value);
        public static implicit operator Bits(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new InvalidCastException();
            }

            bool[] dst = new bool[value.Length];

            for (int i = value.Length - 1, n = 0; i >= 0; i--, n++)
            {
                if (value[i] == '1')
                {
                    dst[n] = true;
                }
                else if (value[i] == '0')
                {
                    dst[n] = false;
                }
                else
                {
                    throw new InvalidCastException();
                }
            }

            return new Bits(dst);
        }
        public static explicit operator bool(Bits bit)
        {
            if (((object)bit == null) || (bit.Count != 1))
            {
                throw new InvalidCastException();
            }

            return bit.Get(0);
        }

        public static Bits operator +(Bits left, BigInteger right) // ASL: "+".
        {
            if (((object)left == null) || ((object)right == null))
            {
                throw new ArgumentNullException();
            }

            BigInteger dst;

            switch (left.Count)
            {
                case  8: dst = left.ToByte()   + right; break;
                case 16: dst = left.ToUInt16() + right; break;
                case 32: dst = left.ToUInt32() + right; break;
                case 64: dst = left.ToUInt64() + right; break;

                default: throw new ArgumentOutOfRangeException();
            }

            return dst.SubBigInteger(left.Count - 1, 0);
        }
        public static Bits operator +(Bits left, Bits right) // ASL: "+".
        {
            if (((object)left == null) || ((object)right == null))
            {
                throw new ArgumentNullException();
            }

            if (left.Count != right.Count)
            {
                throw new ArgumentException();
            }

            BigInteger dst;

            switch (left.Count)
            {
                case  8: dst = left.ToByte()   + (BigInteger)right.ToByte();   break;
                case 16: dst = left.ToUInt16() + (BigInteger)right.ToUInt16(); break;
                case 32: dst = left.ToUInt32() + (BigInteger)right.ToUInt32(); break;
                case 64: dst = left.ToUInt64() + (BigInteger)right.ToUInt64(); break;

                default: throw new ArgumentOutOfRangeException();
            }

            return dst.SubBigInteger(left.Count - 1, 0);
        }
        public static Bits operator -(Bits left, Bits right) // ASL: "-".
        {
            if (((object)left == null) || ((object)right == null))
            {
                throw new ArgumentNullException();
            }

            if (left.Count != right.Count)
            {
                throw new ArgumentException();
            }

            BigInteger dst;

            switch (left.Count)
            {
                case  8: dst = left.ToByte()   - (BigInteger)right.ToByte();   break;
                case 16: dst = left.ToUInt16() - (BigInteger)right.ToUInt16(); break;
                case 32: dst = left.ToUInt32() - (BigInteger)right.ToUInt32(); break;
                case 64: dst = left.ToUInt64() - (BigInteger)right.ToUInt64(); break;

                default: throw new ArgumentOutOfRangeException();
            }

            return dst.SubBigInteger(left.Count - 1, 0);
        }
        public static bool operator ==(Bits left, Bits right) // ASL: "==".
        {
            if (((object)left == null) || ((object)right == null))
            {
                throw new ArgumentNullException();
            }

            if (left.Count != right.Count)
            {
                return false;
            }

            for (int i = 0; i <= left.Count - 1; i++)
            {
                if (left.Get(i) != right.Get(i))
                {
                    return false;
                }
            }

            return true;
        }
        public static bool operator !=(Bits left, Bits right) // ASL: "!=".
        {
            return !(left == right);
        }

        public bool Equals(Bits right) // ASL: "==".
        {
            if ((object)right == null)
            {
                throw new ArgumentNullException();
            }

            Bits left = this;

            if (left.Count != right.Count)
            {
                return false;
            }

            for (int i = 0; i <= left.Count - 1; i++)
            {
                if (left.Get(i) != right.Get(i))
                {
                    return false;
                }
            }

            return true;
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException();
            }

            Bits right = obj as Bits;

            return Equals(right);
        }
        public override int GetHashCode() => bits.GetHashCode();
    }
}
