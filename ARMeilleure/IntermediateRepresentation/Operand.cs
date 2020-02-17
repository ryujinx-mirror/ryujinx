using System;
using System.Collections.Generic;

namespace ARMeilleure.IntermediateRepresentation
{
    class Operand
    {
        public OperandKind Kind { get; }

        public OperandType Type { get; }

        public ulong Value { get; private set; }

        public List<Node> Assignments { get; }
        public List<Node> Uses        { get; }

        private Operand()
        {
            Assignments = new List<Node>();
            Uses        = new List<Node>();
        }

        public Operand(OperandKind kind, OperandType type = OperandType.None) : this()
        {
            Kind = kind;
            Type = type;
        }

        public Operand(int value) : this(OperandKind.Constant, OperandType.I32)
        {
            Value = (uint)value;
        }

        public Operand(uint value) : this(OperandKind.Constant, OperandType.I32)
        {
            Value = (uint)value;
        }

        public Operand(long value) : this(OperandKind.Constant, OperandType.I64)
        {
            Value = (ulong)value;
        }

        public Operand(ulong value) : this(OperandKind.Constant, OperandType.I64)
        {
            Value = value;
        }

        public Operand(float value) : this(OperandKind.Constant, OperandType.FP32)
        {
            Value = (ulong)BitConverter.SingleToInt32Bits(value);
        }

        public Operand(double value) : this(OperandKind.Constant, OperandType.FP64)
        {
            Value = (ulong)BitConverter.DoubleToInt64Bits(value);
        }

        public Operand(int index, RegisterType regType, OperandType type) : this()
        {
            Kind = OperandKind.Register;
            Type = type;

            Value = (ulong)((int)regType << 24 | index);
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