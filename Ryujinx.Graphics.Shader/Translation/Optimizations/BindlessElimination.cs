using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Translation.Optimizations
{
    class BindlessElimination
    {
        public static void RunPass(BasicBlock block)
        {
            // We can turn a bindless into regular access by recognizing the pattern
            // produced by the compiler for separate texture and sampler.
            // We check for the following conditions:
            // - The handle is the result of a bitwise OR logical operation.
            // - Both sources of the OR operation comes from CB2 (used by NVN to hold texture handles).
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

                if (!(texOp.GetSource(0).AsgOp is Operation handleCombineOp))
                {
                    continue;
                }

                if (handleCombineOp.Inst != Instruction.BitwiseOr)
                {
                    continue;
                }

                Operand src0 = handleCombineOp.GetSource(0);
                Operand src1 = handleCombineOp.GetSource(1);

                if (src0.Type != OperandType.ConstantBuffer || src0.GetCbufSlot() != 2 ||
                    src1.Type != OperandType.ConstantBuffer || src1.GetCbufSlot() != 2)
                {
                    continue;
                }

                texOp.SetHandle(src0.GetCbufOffset() | (src1.GetCbufOffset() << 16));
            }
        }
    }
}
