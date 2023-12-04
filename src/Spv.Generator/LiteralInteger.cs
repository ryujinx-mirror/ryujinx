using System;
using System.IO;

namespace Spv.Generator
{
    public class LiteralInteger : IOperand, IEquatable<LiteralInteger>
    {
        [ThreadStatic]
        private static GeneratorPool<LiteralInteger> _pool;

        internal static void RegisterPool(GeneratorPool<LiteralInteger> pool)
        {
            _pool = pool;
        }

        internal static void UnregisterPool()
        {
            _pool = null;
        }

        public OperandType Type => OperandType.Number;

        private enum IntegerType
        {
            UInt32,
            Int32,
            UInt64,
            Int64,
            Float32,
            Float64,
        }

        private IntegerType _integerType;
        private ulong _data;

        public ushort WordCount { get; private set; }

        public LiteralInteger() { }

        private static LiteralInteger New()
        {
            return _pool.Allocate();
        }

        private LiteralInteger Set(ulong data, IntegerType integerType, ushort wordCount)
        {
            _data = data;
            _integerType = integerType;

            WordCount = wordCount;

            return this;
        }

        public static implicit operator LiteralInteger(int value) => New().Set((ulong)value, IntegerType.Int32, 1);
        public static implicit operator LiteralInteger(uint value) => New().Set(value, IntegerType.UInt32, 1);
        public static implicit operator LiteralInteger(long value) => New().Set((ulong)value, IntegerType.Int64, 2);
        public static implicit operator LiteralInteger(ulong value) => New().Set(value, IntegerType.UInt64, 2);
        public static implicit operator LiteralInteger(float value) => New().Set(BitConverter.SingleToUInt32Bits(value), IntegerType.Float32, 1);
        public static implicit operator LiteralInteger(double value) => New().Set(BitConverter.DoubleToUInt64Bits(value), IntegerType.Float64, 2);
        public static implicit operator LiteralInteger(Enum value) => New().Set((ulong)(int)(object)value, IntegerType.Int32, 1);

        // NOTE: this is not in the standard, but this is some syntax sugar useful in some instructions (TypeInt ect)
        public static implicit operator LiteralInteger(bool value) => New().Set(Convert.ToUInt64(value), IntegerType.Int32, 1);

        public static LiteralInteger CreateForEnum<T>(T value) where T : Enum
        {
            return value;
        }

        public void WriteOperand(BinaryWriter writer)
        {
            if (WordCount == 1)
            {
                writer.Write((uint)_data);
            }
            else
            {
                writer.Write(_data);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is LiteralInteger literalInteger && Equals(literalInteger);
        }

        public bool Equals(LiteralInteger cmpObj)
        {
            return Type == cmpObj.Type && _integerType == cmpObj._integerType && _data == cmpObj._data;
        }

        public override int GetHashCode()
        {
            return DeterministicHashCode.Combine(Type, _data);
        }

        public bool Equals(IOperand obj)
        {
            return obj is LiteralInteger literalInteger && Equals(literalInteger);
        }

        public override string ToString() => $"{_integerType} {_data}";
    }
}
