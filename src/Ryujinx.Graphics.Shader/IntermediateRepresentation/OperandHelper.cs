using Ryujinx.Graphics.Shader.Decoders;
using System;

namespace Ryujinx.Graphics.Shader.IntermediateRepresentation
{
    static class OperandHelper
    {
        public static Operand Argument(int value)
        {
            return new Operand(OperandType.Argument, value);
        }

        public static Operand Cbuf(int slot, int offset)
        {
            return new Operand(slot, offset);
        }

        public static Operand Const(int value)
        {
            return new Operand(OperandType.Constant, value);
        }

        public static Operand ConstF(float value)
        {
            return new Operand(OperandType.Constant, BitConverter.SingleToInt32Bits(value));
        }

        public static Operand Label()
        {
            return new Operand(OperandType.Label);
        }

        public static Operand Local()
        {
            return new Operand(OperandType.LocalVariable);
        }

        public static Operand Register(int index, RegisterType type)
        {
            return Register(new Register(index, type));
        }

        public static Operand Register(Register reg)
        {
            if (reg.IsRZ)
            {
                return Const(0);
            }
            else if (reg.IsPT)
            {
                return Const(IrConsts.True);
            }

            return new Operand(reg);
        }

        public static Operand Undef()
        {
            return new Operand(OperandType.Undefined);
        }
    }
}
