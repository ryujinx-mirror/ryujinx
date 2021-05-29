using ARMeilleure.Translation.PTC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ARMeilleure.IntermediateRepresentation
{
    class Operand
    {
        public OperandKind Kind { get; private set; }
        public OperandType Type { get; private set; }

        public ulong Value { get; private set; }

        public List<Node> Assignments { get; }
        public List<Node> Uses        { get; }

        public Symbol Symbol { get; private set; }
        public bool Relocatable => Symbol.Type != SymbolType.None;

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
            Symbol symbol = default)
        {
            Kind = kind;
            Type = type;

            Value = value;

            Symbol = symbol;

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

        public Operand With(long value)
        {
            return With(OperandKind.Constant, OperandType.I64, (ulong)value);
        }

        public Operand With(long value, Symbol symbol)
        {
            return With(OperandKind.Constant, OperandType.I64, (ulong)value, symbol);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Register GetRegister()
        {
            return new Register((int)Value & 0xffffff, (RegisterType)(Value >> 24));
        }

        public int GetLocalNumber()
        {
            Debug.Assert(Kind == OperandKind.LocalVariable);

            return (int)Value;
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