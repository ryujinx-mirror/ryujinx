using System;

namespace Ryujinx.Graphics.Shader.Decoders
{
    struct Register : IEquatable<Register>
    {
        public int Index { get; }

        public RegisterType Type { get; }

        public bool IsRZ => Type == RegisterType.Gpr       && Index == RegisterConsts.RegisterZeroIndex;
        public bool IsPT => Type == RegisterType.Predicate && Index == RegisterConsts.PredicateTrueIndex;

        public Register(int index, RegisterType type)
        {
            Index = index;
            Type  = type;
        }

        public override int GetHashCode()
        {
            return (ushort)Index | ((ushort)Type << 16);
        }

        public override bool Equals(object obj)
        {
            return obj is Register reg && Equals(reg);
        }

        public bool Equals(Register other)
        {
            return other.Index == Index &&
                   other.Type  == Type;
        }
    }
}