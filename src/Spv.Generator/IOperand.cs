using System;
using System.IO;

namespace Spv.Generator
{
    public interface IOperand : IEquatable<IOperand>
    {
        OperandType Type { get; }

        ushort WordCount { get; }

        void WriteOperand(BinaryWriter writer);
    }
}
