using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Tamper.Conditions;
using Ryujinx.HLE.HOS.Tamper.Operations;
using System;
using System.Globalization;

namespace Ryujinx.HLE.HOS.Tamper
{
    class InstructionHelper
    {
        private const int CodeTypeIndex = 0;

        public static void Emit(IOperation operation, CompilationContext context)
        {
            context.CurrentOperations.Add(operation);
        }

        public static void Emit(Type instruction, byte width, CompilationContext context, params Object[] operands)
        {
            Emit((IOperation)Create(instruction, width, operands), context);
        }

        public static void EmitMov(byte width, CompilationContext context, IOperand destination, IOperand source)
        {
            Emit(typeof(OpMov<>), width, context, destination, source);
        }

        public static ICondition CreateCondition(Comparison comparison, byte width, IOperand lhs, IOperand rhs)
        {
            ICondition Create(Type conditionType)
            {
                return (ICondition)InstructionHelper.Create(conditionType, width, lhs, rhs);
            }

            switch (comparison)
            {
                case Comparison.Greater       : return Create(typeof(CondGT<>));
                case Comparison.GreaterOrEqual: return Create(typeof(CondGE<>));
                case Comparison.Less          : return Create(typeof(CondLT<>));
                case Comparison.LessOrEqual   : return Create(typeof(CondLE<>));
                case Comparison.Equal         : return Create(typeof(CondEQ<>));
                case Comparison.NotEqual      : return Create(typeof(CondNE<>));
                default:
                    throw new TamperCompilationException($"Invalid comparison {comparison} in Atmosphere cheat");
            }
        }

        public static Object Create(Type instruction, byte width, params Object[] operands)
        {
            Type realType;

            switch (width)
            {
                case 1: realType = instruction.MakeGenericType(typeof(byte)); break;
                case 2: realType = instruction.MakeGenericType(typeof(ushort)); break;
                case 4: realType = instruction.MakeGenericType(typeof(uint)); break;
                case 8: realType = instruction.MakeGenericType(typeof(ulong)); break;
                default:
                    throw new TamperCompilationException($"Invalid instruction width {width} in Atmosphere cheat");
            }

            return Activator.CreateInstance(realType, operands);
        }

        public static ulong GetImmediate(byte[] instruction, int index, int nybbleCount)
        {
            ulong value = 0;

            for (int i = 0; i < nybbleCount; i++)
            {
                value <<= 4;
                value |= instruction[index + i];
            }

            return value;
        }

        public static CodeType GetCodeType(byte[] instruction)
        {
            int codeType = instruction[CodeTypeIndex];

            if (codeType >= 0xC)
            {
                byte extension = instruction[CodeTypeIndex + 1];
                codeType = (codeType << 4) | extension;

                if (extension == 0xF)
                {
                    extension = instruction[CodeTypeIndex + 2];
                    codeType = (codeType << 4) | extension;
                }
            }

            return (CodeType)codeType;
        }

        public static byte[] ParseRawInstruction(string rawInstruction)
        {
            const int wordSize = 2 * sizeof(uint);

            // Instructions are multi-word, with 32bit words. Split the raw instruction
            // and parse each word into individual nybbles of bits.

            var words = rawInstruction.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            byte[] instruction = new byte[wordSize * words.Length];

            if (words.Length == 0)
            {
                throw new TamperCompilationException("Empty instruction in Atmosphere cheat");
            }

            for (int wordIndex = 0; wordIndex < words.Length; wordIndex++)
            {
                string word = words[wordIndex];

                if (word.Length != wordSize)
                {
                    throw new TamperCompilationException($"Invalid word length for {word} in Atmosphere cheat");
                }

                for (int nybbleIndex = 0; nybbleIndex < wordSize; nybbleIndex++)
                {
                    int index = wordIndex * wordSize + nybbleIndex;
                    string byteData = word.Substring(nybbleIndex, 1);

                    instruction[index] = byte.Parse(byteData, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }
            }

            return instruction;
        }
    }
}
