using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Translation.Optimizations
{
    class BindlessElimination
    {
        private const int NvnTextureBufferSlot = 2;

        public static void RunPass(BasicBlock block, ShaderConfig config)
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

                if (texOp.Inst == Instruction.TextureSample)
                {
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

                    if (src0.Type != OperandType.ConstantBuffer || src0.GetCbufSlot() != NvnTextureBufferSlot ||
                        src1.Type != OperandType.ConstantBuffer || src1.GetCbufSlot() != NvnTextureBufferSlot)
                    {
                        continue;
                    }

                    texOp.SetHandle(src0.GetCbufOffset() | (src1.GetCbufOffset() << 16));
                }
                else if (texOp.Inst == Instruction.ImageLoad || texOp.Inst == Instruction.ImageStore)
                {
                    Operand src0 = texOp.GetSource(0);

                    if (src0.Type == OperandType.ConstantBuffer && src0.GetCbufSlot() == NvnTextureBufferSlot)
                    {
                        texOp.SetHandle(src0.GetCbufOffset());
                        texOp.Format = config.GetTextureFormat(texOp.Handle);
                    }
                }
            }
        }
    }
}
