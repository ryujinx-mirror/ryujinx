using ARMeilleure.IntermediateRepresentation;

using static ARMeilleure.IntermediateRepresentation.Operand.Factory;
using static ARMeilleure.IntermediateRepresentation.Operation.Factory;

namespace ARMeilleure.Translation
{
    static partial class Ssa
    {
        public static void Deconstruct(ControlFlowGraph cfg)
        {
            for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
            {
                Operation operation = block.Operations.First;

                while (operation != default && operation.Instruction == Instruction.Phi)
                {
                    Operation nextNode = operation.ListNext;

                    Operand local = Local(operation.Destination.Type);

                    PhiOperation phi = operation.AsPhi();

                    for (int index = 0; index < phi.SourcesCount; index++)
                    {
                        BasicBlock predecessor = phi.GetBlock(cfg, index);

                        Operand source = phi.GetSource(index);

                        predecessor.Append(Operation(Instruction.Copy, local, source));

                        phi.SetSource(index, default);
                    }

                    Operation copyOp = Operation(Instruction.Copy, operation.Destination, local);

                    block.Operations.AddBefore(operation, copyOp);

                    operation.Destination = default;

                    block.Operations.Remove(operation);

                    operation = nextNode;
                }
            }
        }
    }
}
