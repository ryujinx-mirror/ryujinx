using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    static class PhiFunctions
    {
        public static void Remove(BasicBlock[] blocks)
        {
            for (int blkIndex = 0; blkIndex < blocks.Length; blkIndex++)
            {
                BasicBlock block = blocks[blkIndex];

                LinkedListNode<INode> node = block.Operations.First;

                while (node != null)
                {
                    LinkedListNode<INode> nextNode = node.Next;

                    if (!(node.Value is PhiNode phi))
                    {
                        node = nextNode;

                        continue;
                    }

                    for (int index = 0; index < phi.SourcesCount; index++)
                    {
                        Operand src = phi.GetSource(index);

                        BasicBlock srcBlock = phi.GetBlock(index);

                        Operation copyOp = new Operation(Instruction.Copy, phi.Dest, src);

                        AddBeforeBranch(srcBlock, copyOp);
                    }

                    block.Operations.Remove(node);

                    node = nextNode;
                }
            }
        }

        private static void AddBeforeBranch(BasicBlock block, INode node)
        {
            INode lastOp = block.GetLastOp();

            if (lastOp is Operation operation && IsControlFlowInst(operation.Inst))
            {
                block.Operations.AddBefore(block.Operations.Last, node);
            }
            else
            {
                block.Operations.AddLast(node);
            }
        }

        private static bool IsControlFlowInst(Instruction inst)
        {
            switch (inst)
            {
                case Instruction.Branch:
                case Instruction.BranchIfFalse:
                case Instruction.BranchIfTrue:
                case Instruction.Discard:
                case Instruction.Return:
                    return true;
            }

            return false;
        }
    }
}