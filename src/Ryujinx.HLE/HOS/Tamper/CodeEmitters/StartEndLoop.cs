using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Tamper.Operations;

namespace Ryujinx.HLE.HOS.Tamper.CodeEmitters
{
    /// <summary>
    /// Code type 3 allows for iterating in a loop a fixed number of times.
    /// </summary>
    class StartEndLoop
    {
        private const int StartOrEndIndex = 1;
        private const int IterationRegisterIndex = 3;
        private const int IterationsImmediateIndex = 8;

        private const int IterationsImmediateSize = 8;

        private const byte LoopBegin = 0;
        private const byte LoopEnd = 1;

        public static void Emit(byte[] instruction, CompilationContext context)
        {
            // 300R0000 VVVVVVVV
            // R: Register to use as loop counter.
            // V: Number of iterations to loop.

            // 310R0000

            byte mode = instruction[StartOrEndIndex];
            byte iterationRegisterIndex = instruction[IterationRegisterIndex];

            switch (mode)
            {
                case LoopBegin:
                    // Just start a new compilation block and parse the instruction itself at the end.
                    context.BlockStack.Push(new OperationBlock(instruction));
                    return;
                case LoopEnd:
                    break;
                default:
                    throw new TamperCompilationException($"Invalid loop {mode} in Atmosphere cheat");
            }

            // Use the loop begin instruction stored in the stack.
            instruction = context.CurrentBlock.BaseInstruction;
            CodeType codeType = InstructionHelper.GetCodeType(instruction);

            if (codeType != CodeType.StartEndLoop)
            {
                throw new TamperCompilationException($"Loop end does not match code type {codeType} in Atmosphere cheat");
            }

            // Validate if the register in the beginning and end are the same.

            byte oldIterationRegisterIndex = instruction[IterationRegisterIndex];

            if (iterationRegisterIndex != oldIterationRegisterIndex)
            {
                throw new TamperCompilationException($"The register used for the loop changed from {oldIterationRegisterIndex} to {iterationRegisterIndex} in Atmosphere cheat");
            }

            Register iterationRegister = context.GetRegister(iterationRegisterIndex);
            ulong immediate = InstructionHelper.GetImmediate(instruction, IterationsImmediateIndex, IterationsImmediateSize);

            // Create a loop block with the current operations and nest it in the upper
            // block of the stack.

            ForBlock block = new(immediate, iterationRegister, context.CurrentOperations);
            context.BlockStack.Pop();
            context.CurrentOperations.Add(block);
        }
    }
}
