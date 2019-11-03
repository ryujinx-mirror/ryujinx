using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Translation.Optimizations
{
    static class BindlessToIndexed
    {
        private const int StorageDescsBaseOffset = 0x44; // In words.

        private const int UbeStorageDescsBaseOffset = 0x84; // In words.
        private const int UbeStorageMaxCount        = 14;

        private const int StorageDescSize = 4; // In words.
        private const int StorageMaxCount = 16;

        private const int StorageDescsSize  = StorageDescSize * StorageMaxCount;

        public static void RunPass(BasicBlock block)
        {
            // We can turn a bindless texture access into a indexed access,
            // as long the following conditions are true:
            // - The handle is loaded using a LDC instruction.
            // - The handle is loaded from the constant buffer with the handles (CB2 for NVN).
            // - The load has a constant offset.
            // The base offset of the array of handles on the constant buffer is the constant offset.
            for (LinkedListNode<INode> node = block.Operations.First; node != null; node = node.Next)
            {
                if (!(node.Value is TextureOperation texOp))
                {
                    continue;
                }

                if ((texOp.Flags & TextureFlags.Bindless) == 0)
                {
                    continue;
                }

                if (!(texOp.GetSource(0).AsgOp is Operation handleAsgOp))
                {
                    continue;
                }

                if (handleAsgOp.Inst != Instruction.LoadConstant)
                {
                    continue;
                }

                Operand ldcSrc0 = handleAsgOp.GetSource(0);
                Operand ldcSrc1 = handleAsgOp.GetSource(1);

                if (ldcSrc0.Type != OperandType.Constant || ldcSrc0.Value != 2)
                {
                    continue;
                }

                if (!(ldcSrc1.AsgOp is Operation addOp))
                {
                    continue;
                }

                Operand addSrc1 = addOp.GetSource(1);

                if (addSrc1.Type != OperandType.Constant)
                {
                    continue;
                }

                texOp.TurnIntoIndexed(addSrc1.Value);

                Operand index = Local();

                Operand source = addOp.GetSource(0);

                Operation shrBy1 = new Operation(Instruction.ShiftRightU32, index, source, Const(1));

                block.Operations.AddBefore(node, shrBy1);

                texOp.SetSource(0, index);
            }
        }
    }
}