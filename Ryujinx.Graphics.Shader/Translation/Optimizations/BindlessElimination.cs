using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.Translation.Optimizations
{
    class BindlessElimination
    {
        private static Operation FindBranchSource(BasicBlock block)
        {
            foreach (BasicBlock sourceBlock in block.Predecessors)
            {
                if (sourceBlock.Operations.Count > 0)
                {
                    Operation lastOp = sourceBlock.Operations.Last.Value as Operation;

                    if (lastOp != null &&
                        ((sourceBlock.Next == block && lastOp.Inst == Instruction.BranchIfFalse) ||
                        (sourceBlock.Branch == block && lastOp.Inst == Instruction.BranchIfTrue)))
                    {
                        return lastOp;
                    }
                }
            }

            return null;
        }

        private static bool BlockConditionsMatch(BasicBlock currentBlock, BasicBlock queryBlock)
        {
            // Check if all the conditions for the query block are satisfied by the current block.
            // Just checks the top-most conditional for now.

            Operation currentBranch = FindBranchSource(currentBlock);
            Operation queryBranch = FindBranchSource(queryBlock);

            Operand currentCondition = currentBranch?.GetSource(0);
            Operand queryCondition = queryBranch?.GetSource(0);

            // The condition should be the same operand instance.

            return currentBranch != null && queryBranch != null &&
                   currentBranch.Inst == queryBranch.Inst &&
                   currentCondition == queryCondition;
        }

        private static Operand FindLastOperation(Operand source, BasicBlock block)
        {
            if (source.AsgOp is PhiNode phiNode)
            {
                // This source can have a different value depending on a previous branch.
                // Ensure that conditions met for that branch are also met for the current one.
                // Prefer the latest sources for the phi node.

                for (int i = phiNode.SourcesCount - 1; i >= 0; i--)
                {
                    BasicBlock phiBlock = phiNode.GetBlock(i);

                    if (BlockConditionsMatch(block, phiBlock))
                    {
                        return phiNode.GetSource(i);
                    }
                }
            }

            return source;
        }

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
                    Operand bindlessHandle = FindLastOperation(texOp.GetSource(0), block);

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

                    Operand src0 = FindLastOperation(handleCombineOp.GetSource(0), block);
                    Operand src1 = FindLastOperation(handleCombineOp.GetSource(1), block);

                    if (src0.Type != OperandType.ConstantBuffer ||
                        src1.Type != OperandType.ConstantBuffer || src0.GetCbufSlot() != src1.GetCbufSlot())
                    {
                        continue;
                    }

                    texOp.SetHandle(src0.GetCbufOffset() | (src1.GetCbufOffset() << 16), src0.GetCbufSlot());
                }
                else if (texOp.Inst == Instruction.ImageLoad || texOp.Inst == Instruction.ImageStore)
                {
                    Operand src0 = FindLastOperation(texOp.GetSource(0), block);

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
