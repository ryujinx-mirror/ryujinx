using System;
using System.Collections.Generic;

namespace ARMeilleure.IntermediateRepresentation
{
    class Operand
    {
        public OperandKind Kind { get; private set; }
        public OperandType Type { get; private set; }

        public ulong Value { get; private set; }

        public bool Relocatable { get; private set; }
        public int? PtcIndex    { get; private set; }

        public List<Node> Assignments { get; }
        public List<Node> Uses        { get; }

        public Operand()
        {
            Assignments = new List<Node>();
            Uses        = new List<Node>();
        }

        public Operand(OperandKind kind, OperandType type = OperandType.None) : this()
        {
            Kind = kind;
            Type = type;
        }

        public Operand With(
            OperandKind kind,
            OperandType type = OperandType.None,
            ulong value = 0,
            bool relocatable = false,
            int? index = null)
        {
            Kind = kind;
            Type = type;

            Value = value;

            Relocatable = relocatable;
            PtcIndex    = index;

            Assignments.Clear();
            Uses.Clear();

            return this;
        }

        public Operand With(int value)
        {
            return With(OperandKind.Constant, OperandType.I32, (uint)value);
        }

        public Operand With(uint value)
        {
            return With(OperandKind.Constant, OperandType.I32, value);
        }

        public Operand With(long value, bool relocatable = false, int? index = null)
        {
            return With(OperandKind.Constant, OperandType.I64, (ulong)value, relocatable, index);
        }

        public Operand With(ulong value)
        {
            return With(OperandKind.Constant, OperandType.I64, value);
        }

        public Operand With(float value)
        {
            return With(OperandKind.Constant, OperandType.FP32, (ulong)BitConverter.SingleToInt32Bits(value));
        }

        public Operand With(double value)
        {
            return With(OperandKind.Constant, OperandType.FP64, (ulong)BitConverter.DoubleToInt64Bits(value));
        }

        public Operand With(int index, RegisterType regType, OperandType type)
        {
            return With(OperandKind.Register, type, (ulong)((int)regType << 24 | index));
        }

        public Register GetRegister()
        {
            return new Register((int)Value & 0xffffff, (RegisterType)(Value >> 24));
        }

        public byte AsByte()
        {
            return (byte)Value;
        }

        public short AsInt16()
        {
            return (short)Value;
        }

        public int AsInt32()
        {
            return (int)Value;
        }

        public long AsInt64()
        {
            return (long)Value;
        }

        public float AsFloat()
        {
            return BitConverter.Int32BitsToSingle((int)Value);
        }

        public double AsDouble()
        {
            return BitConverter.Int64BitsToDouble((long)Value);
        }

        internal void NumberLocal(int number)
        {
            if (Kind != OperandKind.LocalVariable)
            {
                throw new InvalidOperationException("The operand is not a local variable.");
            }

            Value = (ulong)number;
        }

        public override int GetHashCode()
        {
            if (Kind == OperandKind.LocalVariable)
            {
                return base.GetHashCode();
            }
            else
            {
                return (int)Value ^ ((int)Kind << 16) ^ ((int)Type << 20);
            }
        }
    }
}