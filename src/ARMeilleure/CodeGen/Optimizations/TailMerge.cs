using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using static ARMeilleure.IntermediateRepresentation.Operation.Factory;

namespace ARMeilleure.CodeGen.Optimizations
{
    static class TailMerge
    {
        public static void RunPass(in CompilerContext cctx)
        {
            ControlFlowGraph cfg = cctx.Cfg;

            BasicBlock mergedReturn = new(cfg.Blocks.Count);

            Operand returnValue;
            Operation returnOp;

            if (cctx.FuncReturnType == OperandType.None)
            {
                returnValue = default;
                returnOp = Operation(Instruction.Return, default);
            }
            else
            {
                returnValue = cfg.AllocateLocal(cctx.FuncReturnType);
                returnOp = Operation(Instruction.Return, default, returnValue);
            }

            mergedReturn.Frequency = BasicBlockFrequency.Cold;
            mergedReturn.Operations.AddLast(returnOp);

            for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
            {
                Operation op = block.Operations.Last;

                if (op != default && op.Instruction == Instruction.Return)
                {
                    block.Operations.Remove(op);

                    if (cctx.FuncReturnType == OperandType.None)
                    {
                        PrepareMerge(block, mergedReturn);
                    }
                    else
                    {
                        Operation copyOp = Operation(Instruction.Copy, returnValue, op.GetSource(0));

                        PrepareMerge(block, mergedReturn).Append(copyOp);
                    }
                }
            }

            cfg.Blocks.AddLast(mergedReturn);
            cfg.Update();
        }

        private static BasicBlock PrepareMerge(BasicBlock from, BasicBlock to)
        {
            BasicBlock fromPred = from.Predecessors.Count == 1 ? from.Predecessors[0] : null;

            // If the block is empty, we can try to append to the predecessor and avoid unnecessary jumps.
            if (from.Operations.Count == 0 && fromPred != null && fromPred.SuccessorsCount == 1)
            {
                for (int i = 0; i < fromPred.SuccessorsCount; i++)
                {
                    if (fromPred.GetSuccessor(i) == from)
                    {
                        fromPred.SetSuccessor(i, to);
                    }
                }

                // NOTE: `from` becomes unreachable and the call to `cfg.Update()` will remove it.
                return fromPred;
            }
            else
            {
                from.AddSuccessor(to);

                return from;
            }
        }
    }
}
