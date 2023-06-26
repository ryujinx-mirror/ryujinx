using ARMeilleure.IntermediateRepresentation;

namespace ARMeilleure.Translation
{
    readonly struct CompilerContext
    {
        public ControlFlowGraph Cfg { get; }

        public OperandType[] FuncArgTypes { get; }
        public OperandType FuncReturnType { get; }

        public CompilerOptions Options { get; }

        public CompilerContext(
            ControlFlowGraph cfg,
            OperandType[] funcArgTypes,
            OperandType funcReturnType,
            CompilerOptions options)
        {
            Cfg = cfg;
            FuncArgTypes = funcArgTypes;
            FuncReturnType = funcReturnType;
            Options = options;
        }
    }
}
