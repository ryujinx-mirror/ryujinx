using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Tamper.Conditions;
using Ryujinx.HLE.HOS.Tamper.Operations;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Tamper.CodeEmitters
{
    /// <summary>
    /// Code type 2 marks the end of a conditional block (started by Code Type 1, Code Type 8 or Code Type C0).
    /// </summary>
    class EndConditionalBlock
    {
        public static void Emit(byte[] instruction, CompilationContext context)
        {
            // 20000000

            // Use the conditional begin instruction stored in the stack.
            instruction = context.CurrentBlock.BaseInstruction;
            CodeType codeType = InstructionHelper.GetCodeType(instruction);

            // Pop the current block of operations from the stack so control instructions
            // for the conditional can be emitted in the upper block.
            IEnumerable<IOperation> operations = context.CurrentOperations;
            context.BlockStack.Pop();

            ICondition condition;

            switch (codeType)
            {
                case CodeType.BeginMemoryConditionalBlock:
                    condition = MemoryConditional.Emit(instruction, context);
                    break;
                case CodeType.BeginKeypressConditionalBlock:
                    condition = KeyPressConditional.Emit(instruction, context);
                    break;
                case CodeType.BeginRegisterConditionalBlock:
                    condition = RegisterConditional.Emit(instruction, context);
                    break;
                default:
                    throw new TamperCompilationException($"Conditional end does not match code type {codeType} in Atmosphere cheat");
            }

            // Create a conditional block with the current operations and nest it in the upper
            // block of the stack.

            IfBlock block = new IfBlock(condition, operations);
            context.CurrentOperations.Add(block);
        }
    }
}
