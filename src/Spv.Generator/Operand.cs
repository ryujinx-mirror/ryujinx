using System;
using System.IO;

namespace Spv.Generator
{
    public interface Operand : IEquatable<Operand>
    {
        OperandType Type { get; }

        ushort WordCount { get; }

        void WriteOperand(BinaryWriter writer);
    }
}
