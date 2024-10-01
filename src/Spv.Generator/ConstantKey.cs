using System;
using System.Diagnostics.CodeAnalysis;

namespace Spv.Generator
{
    internal readonly struct ConstantKey : IEquatable<ConstantKey>
    {
        private readonly Instruction _constant;

        public ConstantKey(Instruction constant)
        {
            _constant = constant;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_constant.Opcode, _constant.GetHashCodeContent(), _constant.GetHashCodeResultType());
        }

        public bool Equals(ConstantKey other)
        {
            return _constant.Opcode == other._constant.Opcode && _constant.EqualsContent(other._constant) && _constant.EqualsResultType(other._constant);
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return obj is ConstantKey key && Equals(key);
        }
    }
}
