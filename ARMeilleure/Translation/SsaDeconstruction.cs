using ARMeilleure.IntermediateRepresentation;
using System.Collections.Generic;

using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Translation
{
    static partial class Ssa
    {
        public static void Deconstruct(ControlFlowGraph cfg)
        {
            foreach (BasicBlock block in cfg.Blocks)
            {
                LinkedListNode<Node> node = block.Operations.First;

                while (node?.Value is PhiNode phi)
                {
                    LinkedListNode<Node> nextNode = node.Next;

                    Operand local = Local(phi.Destination.Type);

                    for (int index = 0; index < phi.SourcesCount; index++)
                    {
                        BasicBlock predecessor = phi.GetBlock(index);

                        Operand source = phi.GetSource(index);

                        predecessor.Append(new Operation(Instruction.Copy, local, source));

                        phi.SetSource(index, null);
                    }

                    Operation copyOp = new Operation(Instruction.Copy, phi.Destination, local);

                    block.Operations.AddBefore(node, copyOp);

                    phi.Destination = null;

                    block.Operations.Remove(node);

                    node = nextNode;
                }
            }
        }
    }
}