using System;

namespace Ryujinx.Tests.Unicorn
{
    public readonly struct SimdValue : IEquatable<SimdValue>
    {
        private readonly ulong _e0;
        private readonly ulong _e1;

        public SimdValue(ulong e0, ulong e1)
        {
            _e0 = e0;
            _e1 = e1;
        }

        public SimdValue(byte[] data)
        {
            _e0 = (ulong)BitConverter.ToInt64(data, 0);
            _e1 = (ulong)BitConverter.ToInt64(data, 8);
        }

        public float AsFloat()
        {
            return GetFloat(0);
        }

        public double AsDouble()
        {
            return GetDouble(0);
        }

        public float GetFloat(int index)
        {
            return BitConverter.Int32BitsToSingle(GetInt32(index));
        }

        public double GetDouble(int index)
        {
            return BitConverter.Int64BitsToDouble(GetInt64(index));
        }

        public int GetInt32(int index) => (int)GetUInt32(index);
        public long GetInt64(int index) => (long)GetUInt64(index);

        public uint GetUInt32(int index)
        {
            return index switch
            {
                0 => (uint)(_e0 >> 0),
                1 => (uint)(_e0 >> 32),
                2 => (uint)(_e1 >> 0),
                3 => (uint)(_e1 >> 32),
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }

        public ulong GetUInt64(int index)
        {
            return index switch
            {
                0 => _e0,
                1 => _e1,
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }

        public byte[] ToArray()
        {
            byte[] e0Data = BitConverter.GetBytes(_e0);
            byte[] e1Data = BitConverter.GetBytes(_e1);

            byte[] data = new byte[16];

            Buffer.BlockCopy(e0Data, 0, data, 0, 8);
            Buffer.BlockCopy(e1Data, 0, data, 8, 8);

            return data;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_e0, _e1);
        }

        public static bool operator ==(SimdValue x, SimdValue y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(SimdValue x, SimdValue y)
        {
            return !x.Equals(y);
        }

        public override bool Equals(object obj)
        {
            return obj is SimdValue vector && Equals(vector);
        }

        public bool Equals(SimdValue other)
        {
            return other._e0 == _e0 && other._e1 == _e1;
        }

        public override string ToString()
        {
            return $"0x{_e1:X16}{_e0:X16}";
        }
    }
}
