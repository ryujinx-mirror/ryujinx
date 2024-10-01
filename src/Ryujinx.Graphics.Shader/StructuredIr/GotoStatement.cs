using Ryujinx.Graphics.Shader.IntermediateRepresentation;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    class GotoStatement
    {
        public AstOperation Goto { get; }
        public AstAssignment Label { get; }

        public IAstNode Condition => Label.Destination;

        public bool IsLoop { get; set; }

        public bool IsUnconditional => Goto.Inst == Instruction.Branch;

        public GotoStatement(AstOperation branch, AstAssignment label, bool isLoop)
        {
            Goto = branch;
            Label = label;
            IsLoop = isLoop;
        }
    }
}
