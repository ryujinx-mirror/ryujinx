using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.Graphics.Shader.Translation.Optimizations
{
    static class Optimizer
    {
        public static void Optimize(BasicBlock[] blocks)
        {
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

                        ConstantFolding.Fold(operation);

                        Simplification.Simplify(operation);

                        if (DestIsLocalVar(operation))
                        {
                            if (operation.Inst == Instruction.Copy)
                            {
                                PropagateCopy(operation);

                                RemoveNode(block, node);

                                modified = true;
                            }
                            else if (operation.Inst == Instruction.PackHalf2x16 && PropagatePack(operation))
                            {
                                if (operation.Dest.UseOps.Count == 0)
                                {
                                    RemoveNode(block, node);
                                }

                                modified = true;
                            }
                        }

                        node = nextNode;
                    }

                    if (BranchElimination.Eliminate(block))
                    {
                        RemoveNode(block, block.Operations.Last);

                        modified = true;
                    }
                }
            }
            while (modified);
        }

        private static void PropagateCopy(Operation copyOp)
        {
            //Propagate copy source operand to all uses of
            //the destination operand.
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
            //Propagate pack source operands to uses by unpack
            //instruction. The source depends on the unpack instruction.
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
                    operation.TurnIntoCopy(operation.ComponentIndex == 1 ? src1 : src0);

                    modified = true;
                }
            }

            return modified;
        }

        private static void RemoveNode(BasicBlock block, LinkedListNode<INode> llNode)
        {
            //Remove a node from the nodes list, and also remove itself
            //from all the use lists on the operands that this node uses.
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
                        nodes.Enqueue(src.AsgOp);
                    }
                }
            }
        }

        private static bool IsUnused(INode node)
        {
            return DestIsLocalVar(node) && node.Dest.UseOps.Count == 0;
        }

        private static bool DestIsLocalVar(INode node)
        {
            return node.Dest != null && node.Dest.Type == OperandType.LocalVariable;
        }
    }
}