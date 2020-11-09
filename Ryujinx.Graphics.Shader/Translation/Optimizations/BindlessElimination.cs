using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Translation.Optimizations
{
    class BindlessElimination
    {
        public static void RunPass(BasicBlock block, ShaderConfig config)
        {
            // We can turn a bindless into regular access by recognizing the pattern
            // produced by the compiler for separate texture and sampler.
            // We check for the following conditions:
            // - The handle is a constant buffer value.
            // - The handle is the result of a bitwise OR logical operation.
            // - Both sources of the OR operation comes from a constant buffer.
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

                if (texOp.Inst == Instruction.Lod ||
                    texOp.Inst == Instruction.TextureSample ||
                    texOp.Inst == Instruction.TextureSize)
                {
                    Operand bindlessHandle = texOp.GetSource(0);

                    if (bindlessHandle.Type == OperandType.ConstantBuffer)
                    {
                        texOp.SetHandle(bindlessHandle.GetCbufOffset(), bindlessHandle.GetCbufSlot());
                        continue;
                    }

                    if (!(bindlessHandle.AsgOp is Operation handleCombineOp))
                    {
                        continue;
                    }

                    if (handleCombineOp.Inst != Instruction.BitwiseOr)
                    {
                        continue;
                    }

                    Operand src0 = handleCombineOp.GetSource(0);
                    Operand src1 = handleCombineOp.GetSource(1);

                    if (src0.Type != OperandType.ConstantBuffer ||
                        src1.Type != OperandType.ConstantBuffer || src0.GetCbufSlot() != src1.GetCbufSlot())
                    {
                        continue;
                    }

                    texOp.SetHandle(src0.GetCbufOffset() | (src1.GetCbufOffset() << 16), src0.GetCbufSlot());
                }
                else if (texOp.Inst == Instruction.ImageLoad || texOp.Inst == Instruction.ImageStore)
                {
                    Operand src0 = texOp.GetSource(0);

                    if (src0.Type == OperandType.ConstantBuffer)
                    {
                        texOp.SetHandle(src0.GetCbufOffset(), src0.GetCbufSlot());
                        texOp.Format = config.GetTextureFormat(texOp.Handle);
                    }
                }
            }
        }
    }
}
