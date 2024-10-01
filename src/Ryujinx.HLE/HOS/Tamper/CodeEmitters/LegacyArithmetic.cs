using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Tamper.Operations;
using System;

namespace Ryujinx.HLE.HOS.Tamper.CodeEmitters
{
    /// <summary>
    /// Code type 7 allows performing arithmetic on registers. However, it has been deprecated by Code
    /// type 9, and is only kept for backwards compatibility.
    /// </summary>
    class LegacyArithmetic
    {
        const int OperationWidthIndex = 1;
        const int DestinationRegisterIndex = 3;
        const int OperationTypeIndex = 4;
        const int ValueImmediateIndex = 8;

        const int ValueImmediateSize = 8;

        private const byte Add = 0; // reg += rhs
        private const byte Sub = 1; // reg -= rhs
        private const byte Mul = 2; // reg *= rhs
        private const byte Lsh = 3; // reg <<= rhs
        private const byte Rsh = 4; // reg >>= rhs

        public static void Emit(byte[] instruction, CompilationContext context)
        {
            // 7T0RC000 VVVVVVVV
            // T: Width of arithmetic operation(1, 2, 4, or 8 bytes).
            // R: Register to apply arithmetic to.
            // C: Arithmetic operation to apply, see below.
            // V: Value to use for arithmetic operation.

            byte operationWidth = instruction[OperationWidthIndex];
            Register register = context.GetRegister(instruction[DestinationRegisterIndex]);
            byte operation = instruction[OperationTypeIndex];
            ulong immediate = InstructionHelper.GetImmediate(instruction, ValueImmediateIndex, ValueImmediateSize);
            Value<ulong> rightHandSideValue = new(immediate);

            void Emit(Type operationType)
            {
                InstructionHelper.Emit(operationType, operationWidth, context, register, register, rightHandSideValue);
            }

            switch (operation)
            {
                case Add:
                    Emit(typeof(OpAdd<>));
                    break;
                case Sub:
                    Emit(typeof(OpSub<>));
                    break;
                case Mul:
                    Emit(typeof(OpMul<>));
                    break;
                case Lsh:
                    Emit(typeof(OpLsh<>));
                    break;
                case Rsh:
                    Emit(typeof(OpRsh<>));
                    break;
                default:
                    throw new TamperCompilationException($"Invalid arithmetic operation {operation} in Atmosphere cheat");
            }
        }
    }
}
