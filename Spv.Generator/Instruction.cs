using System;
using System.Diagnostics;
using System.IO;

namespace Spv.Generator
{
    public sealed class Instruction : Operand, IEquatable<Instruction>
    {
        public const uint InvalidId = uint.MaxValue;

        public Specification.Op Opcode { get; private set; }
        private Instruction _resultType;
        private InstructionOperands _operands;

        public uint Id { get; set; }

        public Instruction() { }

        public void Set(Specification.Op opcode, uint id = InvalidId, Instruction resultType = null)
        {
            Opcode = opcode;
            Id = id;
            _resultType = resultType;

            _operands = new InstructionOperands();
        }

        public void SetId(uint id)
        {
            Id = id;
        }

        public OperandType Type => OperandType.Instruction;

        public ushort GetTotalWordCount()
        {
            ushort result = WordCount;

            if (Id != InvalidId)
            {
                result++;
            }

            if (_resultType != null)
            {
                result += _resultType.WordCount;
            }

            Span<Operand> operands = _operands.ToSpan();
            for (int i = 0; i < operands.Length; i++)
            {
                result += operands[i].WordCount;
            }

            return result;
        }

        public ushort WordCount => 1;

        public void AddOperand(Operand value)
        {
            Debug.Assert(value != null);
            _operands.Add(value);
        }

        public void AddOperand(Operand[] value)
        {
            foreach (Operand instruction in value)
            {
                AddOperand(instruction);
            }
        }

        public void AddOperand(LiteralInteger[] value)
        {
            foreach (LiteralInteger instruction in value)
            {
                AddOperand(instruction);
            }
        }

        public void AddOperand(LiteralInteger value)
        {
            AddOperand((Operand)value);
        }

        public void AddOperand(Instruction[] value)
        {
            foreach (Instruction instruction in value)
            {
                AddOperand(instruction);
            }
        }

        public void AddOperand(Instruction value)
        {
            AddOperand((Operand)value);
        }

        public void AddOperand(string value)
        {
            AddOperand(new LiteralString(value));
        }

        public void AddOperand<T>(T value) where T: Enum
        {
            AddOperand(LiteralInteger.CreateForEnum(value));
        }

        public void Write(BinaryWriter writer)
        {
            // Word 0
            writer.Write((ushort)Opcode);
            writer.Write(GetTotalWordCount());

            _resultType?.WriteOperand(writer);

            if (Id != InvalidId)
            {
                writer.Write(Id);
            }

            Span<Operand> operands = _operands.ToSpan();
            for (int i = 0; i < operands.Length; i++)
            {
                operands[i].WriteOperand(writer);
            }
        }

        public void WriteOperand(BinaryWriter writer)
        {
            Debug.Assert(Id != InvalidId);

            if (Id == InvalidId)
            {
                string methodToCall;

                if (Opcode == Specification.Op.OpVariable)
                {
                    methodToCall = "AddLocalVariable or AddGlobalVariable";
                }
                else if (Opcode == Specification.Op.OpLabel)
                {
                    methodToCall = "AddLabel";
                }
                else
                {
                    throw new InvalidOperationException("Internal error");
                }

                throw new InvalidOperationException($"Id wasn't bound to the module, please make sure to call {methodToCall}");
            }

            writer.Write(Id);
        }

        public override bool Equals(object obj)
        {
            return obj is Instruction instruction && Equals(instruction);
        }

        public bool Equals(Instruction cmpObj)
        {
            bool result = Type == cmpObj.Type && Id == cmpObj.Id;

            if (result)
            {
                if (_resultType != null && cmpObj._resultType != null)
                {
                    result &= _resultType.Equals(cmpObj._resultType);
                }
                else if (_resultType != null || cmpObj._resultType != null)
                {
                    return false;
                }
            }

            if (result)
            {
                result &= EqualsContent(cmpObj);
            }

            return result;
        }

        public bool EqualsContent(Instruction cmpObj)
        {
            Span<Operand> thisOperands = _operands.ToSpan();
            Span<Operand> cmpOperands = cmpObj._operands.ToSpan();

            if (thisOperands.Length != cmpOperands.Length)
            {
                return false;
            }

            for (int i = 0; i < thisOperands.Length; i++)
            {
                if (!thisOperands[i].Equals(cmpOperands[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool EqualsResultType(Instruction cmpObj)
        {
            return _resultType.Opcode == cmpObj._resultType.Opcode && _resultType.EqualsContent(cmpObj._resultType);
        }

        public int GetHashCodeContent()
        {
            return DeterministicHashCode.Combine<Operand>(_operands.ToSpan());
        }

        public int GetHashCodeResultType()
        {
            return DeterministicHashCode.Combine(_resultType.Opcode, _resultType.GetHashCodeContent());
        }

        public override int GetHashCode()
        {
            return DeterministicHashCode.Combine(Opcode, Id, _resultType, DeterministicHashCode.Combine<Operand>(_operands.ToSpan()));
        }

        public bool Equals(Operand obj)
        {
            return obj is Instruction instruction && Equals(instruction);
        }
    }
}
