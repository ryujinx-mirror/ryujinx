using ARMeilleure.State;
using System;

namespace ARMeilleure.IntermediateRepresentation
{
    static class OperandHelper
    {
        public static Operand Const(OperandType type, long value)
        {
            return type == OperandType.I32 ? new Operand((int)value) : new Operand(value);
        }

        public static Operand Const(bool value)
        {
            return new Operand(value ? 1 : 0);
        }

        public static Operand Const(int value)
        {
            return new Operand(value);
        }

        public static Operand Const(uint value)
        {
            return new Operand(value);
        }

        public static Operand Const(long value)
        {
            return new Operand(value);
        }

        public static Operand Const(ulong value)
        {
            return new Operand(value);
        }

        public static Operand ConstF(float value)
        {
            return new Operand(value);
        }

        public static Operand ConstF(double value)
        {
            return new Operand(value);
        }

        public static Operand Label()
        {
            return new Operand(OperandKind.Label);
        }

        public static Operand Local(OperandType type)
        {
            return new Operand(OperandKind.LocalVariable, type);
        }

        public static Operand Register(int index, RegisterType regType, OperandType type)
        {
            return new Operand(index, regType, type);
        }

        public static Operand Undef()
        {
            return new Operand(OperandKind.Undefined);
        }
    }
}