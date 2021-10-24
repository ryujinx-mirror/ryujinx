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

                    TextureHandleType handleType = TextureHandleType.SeparateSamplerHandle;

                    // Try to match masked pattern:
                    // - samplerHandle = samplerHandle & 0xFFF00000;
                    // - textureHandle = textureHandle & 0xFFFFF;
                    // - combinedHandle = samplerHandle | textureHandle;
                    // where samplerHandle and textureHandle comes from a constant buffer, and shifted pattern:
                    // - samplerHandle = samplerId << 20;
                    // - combinedHandle = samplerHandle | textureHandle;
                    // where samplerId and textureHandle comes from a constant buffer.
                    if (src0.AsgOp is Operation src0AsgOp)
                    {
                        if (src1.AsgOp is Operation src1AsgOp &&
                            src0AsgOp.Inst == Instruction.BitwiseAnd &&
                            src1AsgOp.Inst == Instruction.BitwiseAnd)
                        {
                            src0 = GetSourceForMaskedHandle(src0AsgOp, 0xFFFFF);
                            src1 = GetSourceForMaskedHandle(src1AsgOp, 0xFFF00000);

                            // The OR operation is commutative, so we can also try to swap the operands to get a match.
                            if (src0 == null || src1 == null)
                            {
                                src0 = GetSourceForMaskedHandle(src1AsgOp, 0xFFFFF);
                                src1 = GetSourceForMaskedHandle(src0AsgOp, 0xFFF00000);
                            }

                            if (src0 == null || src1 == null)
                            {
                                continue;
                            }
                        }
                        else if (src0AsgOp.Inst == Instruction.ShiftLeft)
                        {
                            Operand shift = src0AsgOp.GetSource(1);

                            if (shift.Type == OperandType.Constant && shift.Value == 20)
                            {
                                src0 = src1;
                                src1 = src0AsgOp.GetSource(0);
                                handleType = TextureHandleType.SeparateSamplerId;
                            }
                        }
                    }
                    else if (src1.AsgOp is Operation src1AsgOp && src1AsgOp.Inst == Instruction.ShiftLeft)
                    {
                        Operand shift = src1AsgOp.GetSource(1);

                        if (shift.Type == OperandType.Constant && shift.Value == 20)
                        {
                            src1 = src1AsgOp.GetSource(0);
                            handleType = TextureHandleType.SeparateSamplerId;
                        }
                    }

                    if (src0.Type != OperandType.ConstantBuffer || src1.Type != OperandType.ConstantBuffer)
                    {
                        continue;
                    }

                    SetHandle(
                        config,
                        texOp,
                        TextureHandle.PackOffsets(src0.GetCbufOffset(), src1.GetCbufOffset(), handleType),
                        TextureHandle.PackSlots(src0.GetCbufSlot(), src1.GetCbufSlot()),
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

                        if (texOp.Format == TextureFormat.Unknown)
                        {
                            if (texOp.Inst == Instruction.ImageAtomic)
                            {
                                texOp.Format = config.GetTextureFormatAtomic(cbufOffset, cbufSlot);
                            }
                            else
                            {
                                texOp.Format = config.GetTextureFormat(cbufOffset, cbufSlot);
                            }
                        }

                        SetHandle(config, texOp, cbufOffset, cbufSlot, false);
                    }
                }
            }
        }

        private static Operand GetSourceForMaskedHandle(Operation asgOp, uint mask)
        {
            // Assume it was already checked that the operation is bitwise AND.
            Operand src0 = asgOp.GetSource(0);
            Operand src1 = asgOp.GetSource(1);

            if (src0.Type == OperandType.ConstantBuffer && src1.Type == OperandType.ConstantBuffer)
            {
                // We can't check if the mask matches here as both operands are from a constant buffer.
                // Be optimistic and assume it matches. Avoid constant buffer 1 as official drivers
                // uses this one to store compiler constants.
                return src0.GetCbufSlot() == 1 ? src1 : src0;
            }
            else if (src0.Type == OperandType.ConstantBuffer && src1.Type == OperandType.Constant)
            {
                if ((uint)src1.Value == mask)
                {
                    return src0;
                }
            }
            else if (src0.Type == OperandType.Constant && src1.Type == OperandType.ConstantBuffer)
            {
                if ((uint)src0.Value == mask)
                {
                    return src1;
                }
            }

            return null;
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
