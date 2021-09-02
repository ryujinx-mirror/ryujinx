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
                    Operand bindlessHandle = Utils.FindLastOperation(texOp.GetSource(0), block);
                    bool rewriteSamplerType = texOp.Inst == Instruction.TextureSize;

                    if (bindlessHandle.Type == OperandType.ConstantBuffer)
                    {
                        SetHandle(config, texOp, bindlessHandle.GetCbufOffset(), bindlessHandle.GetCbufSlot(), rewriteSamplerType);
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

                    Operand src0 = Utils.FindLastOperation(handleCombineOp.GetSource(0), block);
                    Operand src1 = Utils.FindLastOperation(handleCombineOp.GetSource(1), block);

                    if (src0.Type != OperandType.ConstantBuffer || src1.Type != OperandType.ConstantBuffer)
                    {
                        continue;
                    }

                    SetHandle(
                        config,
                        texOp,
                        src0.GetCbufOffset() | ((src1.GetCbufOffset() + 1) << 16),
                        src0.GetCbufSlot() | ((src1.GetCbufSlot() + 1) << 16),
                        rewriteSamplerType);
                }
                else if (texOp.Inst == Instruction.ImageLoad ||
                         texOp.Inst == Instruction.ImageStore ||
                         texOp.Inst == Instruction.ImageAtomic)
                {
                    Operand src0 = Utils.FindLastOperation(texOp.GetSource(0), block);

                    if (src0.Type == OperandType.ConstantBuffer)
                    {
                        int cbufOffset = src0.GetCbufOffset();
                        int cbufSlot = src0.GetCbufSlot();

                        if (texOp.Inst == Instruction.ImageAtomic)
                        {
                            texOp.Format = config.GetTextureFormatAtomic(cbufOffset, cbufSlot);
                        }
                        else
                        {
                            texOp.Format = config.GetTextureFormat(cbufOffset, cbufSlot);
                        }

                        SetHandle(config, texOp, cbufOffset, cbufSlot, false);
                    }
                }
            }
        }

        private static void SetHandle(ShaderConfig config, TextureOperation texOp, int cbufOffset, int cbufSlot, bool rewriteSamplerType)
        {
            texOp.SetHandle(cbufOffset, cbufSlot);
            
            if (rewriteSamplerType)
            {
                texOp.Type = config.GpuAccessor.QuerySamplerType(cbufOffset, cbufSlot);
            }

            config.SetUsedTexture(texOp.Inst, texOp.Type, texOp.Format, texOp.Flags, cbufSlot, cbufOffset);
        }
    }
}
