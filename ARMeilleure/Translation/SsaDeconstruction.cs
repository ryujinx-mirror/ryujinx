using ARMeilleure.IntermediateRepresentation;

using static ARMeilleure.IntermediateRepresentation.OperandHelper;
using static ARMeilleure.IntermediateRepresentation.OperationHelper;

namespace ARMeilleure.Translation
{
    static partial class Ssa
    {
        public static void Deconstruct(ControlFlowGraph cfg)
        {
            for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
            {
                Node node = block.Operations.First;

                while (node is PhiNode phi)
                {
                    Node nextNode = node.ListNext;

                    Operand local = Local(phi.Destination.Type);

                    for (int index = 0; index < phi.SourcesCount; index++)
                    {
                        BasicBlock predecessor = phi.GetBlock(index);

                        Operand source = phi.GetSource(index);

                        predecessor.Append(Operation(Instruction.Copy, local, source));

                        phi.SetSource(index, null);
                    }

                    Operation copyOp = Operation(Instruction.Copy, phi.Destination, local);

                    block.Operations.AddBefore(node, copyOp);

                    phi.Destination = null;

                    block.Operations.Remove(node);

                    node = nextNode;
                }
            }
        }
    }
}