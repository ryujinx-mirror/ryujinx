using System.Diagnostics;

namespace Ryujinx.Cpu.LightningJit.CodeGen
{
    readonly struct Operand
    {
        public readonly OperandKind Kind { get; }
        public readonly OperandType Type { get; }
        public readonly ulong Value { get; }

        public Operand(OperandKind kind, OperandType type, ulong value)
        {
            Kind = kind;
            Type = type;
            Value = value;
        }

        public Operand(int index, RegisterType regType, OperandType type) : this(OperandKind.Register, type, (ulong)((int)regType << 24 | index))
        {
        }

        public Operand(OperandType type, ulong value) : this(OperandKind.Constant, type, value)
        {
        }

        public readonly Register GetRegister()
        {
            Debug.Assert(Kind == OperandKind.Register);

            return new Register((int)Value & 0xffffff, (RegisterType)(Value >> 24));
        }

        public readonly int AsInt32()
        {
            return (int)Value;
        }
    }
}
