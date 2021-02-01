using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ryujinx.Graphics.Shader.Translation.Optimizations
{
    static class Optimizer
    {
        public static void RunPass(BasicBlock[] blocks, ShaderConfig config)
        {
            for (int blkIndex = 0; blkIndex < blocks.Length; blkIndex++)
            {
                GlobalToStorage.RunPass(blocks[blkIndex], config);
            }

            bool modified;

            do
            {
                modified = false;

                for (int blkIndex = 0; blkIndex < blocks.Length; blkIndex++)
                {
                    BasicBlock block = blocks[blkIndex];

                    LinkedListNode<INode> node = block.Operations.First;

                    while (node != null)
                    {
                        LinkedListNode<INode> nextNode = node.Next;

                        bool isUnused = IsUnused(node.Value);

                        if (!(node.Value is Operation operation) || isUnused)
                        {
                            if (isUnused)
                            {
                                RemoveNode(block, node);

                                modified = true;
                            }

                            node = nextNode;

                            continue;
                        }

                        ConstantFolding.RunPass(operation);

                        Simplification.RunPass(operation);

                        if (DestIsLocalVar(operation))
                        {
                            if (operation.Inst == Instruction.Copy)
                            {
                                PropagateCopy(operation);

                                RemoveNode(block, node);

                                modified = true;
                            }
                            else if ((operation.Inst == Instruction.PackHalf2x16 && PropagatePack(operation)) ||
                                     (operation.Inst == Instruction.ShuffleXor   && MatchDdxOrDdy(operation)))
                            {
                                if (DestHasNoUses(operation))
                                {
                                    RemoveNode(block, node);
                                }

                                modified = true;
                            }
                        }

                        node = nextNode;
                    }

                    if (BranchElimination.RunPass(block))
                    {
                        RemoveNode(block, block.Operations.Last);

                        modified = true;
                    }
                }
            }
            while (modified);

            for (int blkIndex = 0; blkIndex < blocks.Length; blkIndex++)
            {
                BindlessToIndexed.RunPass(blocks[blkIndex]);
                BindlessElimination.RunPass(blocks[blkIndex], config);

                // Try to eliminate any operations that are now unused.
                LinkedListNode<INode> node = blocks[blkIndex].Operations.First;

                while (node != null)
                {
                    LinkedListNode<INode> nextNode = node.Next;

                    if (IsUnused(node.Value))
                    {
                        RemoveNode(blocks[blkIndex], node);
                    }

                    node = nextNode;
                }
            }
        }

        private static void PropagateCopy(Operation copyOp)
        {
            // Propagate copy source operand to all uses of
            // the destination operand.
            Operand dest = copyOp.Dest;
            Operand src  = copyOp.GetSource(0);

            INode[] uses = dest.UseOps.ToArray();

            foreach (INode useNode in uses)
            {
                for (int index = 0; index < useNode.SourcesCount; index++)
                {
                    if (useNode.GetSource(index) == dest)
                    {
                        useNode.SetSource(index, src);
                    }
                }
            }
        }

        private static bool PropagatePack(Operation packOp)
        {
            // Propagate pack source operands to uses by unpack
            // instruction. The source depends on the unpack instruction.
            bool modified = false;

            Operand dest = packOp.Dest;
            Operand src0 = packOp.GetSource(0);
            Operand src1 = packOp.GetSource(1);

            INode[] uses = dest.UseOps.ToArray();

            foreach (INode useNode in uses)
            {
                if (!(useNode is Operation operation) || operation.Inst != Instruction.UnpackHalf2x16)
                {
                    continue;
                }

                if (operation.GetSource(0) == dest)
                {
                    operation.TurnIntoCopy(operation.Index == 1 ? src1 : src0);

                    modified = true;
                }
            }

            return modified;
        }

        public static bool MatchDdxOrDdy(Operation operation)
        {
            // It's assumed that "operation.Inst" is ShuffleXor,
            // that should be checked before calling this method.
            Debug.Assert(operation.Inst == Instruction.ShuffleXor);

            bool modified = false;

            Operand src2 = operation.GetSource(1);
            Operand src3 = operation.GetSource(2);

            if (src2.Type != OperandType.Constant || (src2.Value != 1 && src2.Value != 2))
            {
                return false;
            }

            if (src3.Type != OperandType.Constant || src3.Value != 0x1c03)
            {
                return false;
            }

            bool isDdy = src2.Value == 2;
            bool isDdx = !isDdy;

            // We can replace any use by a FSWZADD with DDX/DDY, when
            // the following conditions are true:
            // - The mask should be 0b10100101 for DDY, or 0b10011001 for DDX.
            // - The first source operand must be the shuffle output.
            // - The second source operand must be the shuffle first source operand.
            INode[] uses = operation.Dest.UseOps.ToArray();

            foreach (INode use in uses)
            {
                if (!(use is Operation test))
                {
                    continue;
                }

                if (!(use is Operation useOp) || useOp.Inst != Instruction.SwizzleAdd)
                {
                    continue;
                }

                Operand fswzaddSrc1 = useOp.GetSource(0);
                Operand fswzaddSrc2 = useOp.GetSource(1);
                Operand fswzaddSrc3 = useOp.GetSource(2);

                if (fswzaddSrc1 != operation.Dest)
                {
                    continue;
                }

                if (fswzaddSrc2 != operation.GetSource(0))
                {
                    continue;
                }

                if (fswzaddSrc3.Type != OperandType.Constant)
                {
                    continue;
                }

                int mask = fswzaddSrc3.Value;

                if ((isDdx && mask != 0b10011001) ||
                    (isDdy && mask != 0b10100101))
                {
                    continue;
                }

                useOp.TurnInto(isDdx ? Instruction.Ddx : Instruction.Ddy, fswzaddSrc2);

                modified = true;
            }

            return modified;
        }

        private static void RemoveNode(BasicBlock block, LinkedListNode<INode> llNode)
        {
            // Remove a node from the nodes list, and also remove itself
            // from all the use lists on the operands that this node uses.
            block.Operations.Remove(llNode);

            Queue<INode> nodes = new Queue<INode>();

            nodes.Enqueue(llNode.Value);

            while (nodes.TryDequeue(out INode node))
            {
                for (int index = 0; index < node.SourcesCount; index++)
                {
                    Operand src = node.GetSource(index);

                    if (src.Type != OperandType.LocalVariable)
                    {
                        continue;
                    }

                    if (src.UseOps.Remove(node) && src.UseOps.Count == 0)
                    {
                        Debug.Assert(src.AsgOp != null);
                        nodes.Enqueue(src.AsgOp);
                    }
                }
            }
        }

        private static bool IsUnused(INode node)
        {
            return !HasSideEffects(node) && DestIsLocalVar(node) && DestHasNoUses(node);
        }

        private static bool HasSideEffects(INode node)
        {
            if (node is Operation operation)
            {
                switch (operation.Inst & Instruction.Mask)
                {
                    case Instruction.AtomicAdd:
                    case Instruction.AtomicAnd:
                    case Instruction.AtomicCompareAndSwap:
                    case Instruction.AtomicMaxS32:
                    case Instruction.AtomicMaxU32:
                    case Instruction.AtomicMinS32:
                    case Instruction.AtomicMinU32:
                    case Instruction.AtomicOr:
                    case Instruction.AtomicSwap:
                    case Instruction.AtomicXor:
                    case Instruction.Call:
                        return true;
                }
            }

            return false;
        }

        private static bool DestIsLocalVar(INode node)
        {
            if (node.DestsCount == 0)
            {
                return false;
            }

            for (int index = 0; index < node.DestsCount; index++)
            {
                Operand dest = node.GetDest(index);

                if (dest != null && dest.Type != OperandType.LocalVariable)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool DestHasNoUses(INode node)
        {
            for (int index = 0; index < node.DestsCount; index++)
            {
                Operand dest = node.GetDest(index);

                if (dest != null && dest.UseOps.Count != 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}