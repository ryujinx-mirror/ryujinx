using ARMeilleure.Translation;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.IntermediateRepresentation
{
    readonly struct PhiOperation
    {
        private readonly Operation _operation;

        public PhiOperation(Operation operation)
        {
            _operation = operation;
        }

        public int SourcesCount => _operation.SourcesCount / 2;

        public BasicBlock GetBlock(ControlFlowGraph cfg, int index)
        {
            return cfg.PostOrderBlocks[cfg.PostOrderMap[_operation.GetSource(index * 2).AsInt32()]];
        }

        public void SetBlock(int index, BasicBlock block)
        {
            _operation.SetSource(index * 2, Const(block.Index));
        }

        public Operand GetSource(int index)
        {
            return _operation.GetSource(index * 2 + 1);
        }

        public void SetSource(int index, Operand operand)
        {
            _operation.SetSource(index * 2 + 1, operand);
        }
    }
}
