using System;
using System.Diagnostics.CodeAnalysis;

namespace Spv.Generator
{
    internal struct TypeDeclarationKey : IEquatable<TypeDeclarationKey>
    {
        private Instruction _typeDeclaration;

        public TypeDeclarationKey(Instruction typeDeclaration)
        {
            _typeDeclaration = typeDeclaration;
        }

        public override int GetHashCode()
        {
            return DeterministicHashCode.Combine(_typeDeclaration.Opcode, _typeDeclaration.GetHashCodeContent());
        }

        public bool Equals(TypeDeclarationKey other)
        {
            return _typeDeclaration.Opcode == other._typeDeclaration.Opcode && _typeDeclaration.EqualsContent(other._typeDeclaration);
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return obj is TypeDeclarationKey && Equals((TypeDeclarationKey)obj);
        }
    }
}
