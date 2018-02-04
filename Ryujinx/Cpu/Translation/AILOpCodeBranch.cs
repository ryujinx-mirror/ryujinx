using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct AILOpCodeBranch : IAILEmit
    {
        private OpCode   ILOp;
        private AILLabel Label;

        public AILOpCodeBranch(OpCode ILOp, AILLabel Label)
        {
            this.ILOp  = ILOp;
            this.Label = Label;
        }

        public void Emit(AILEmitter Context)
        {
            Context.Generator.Emit(ILOp, Label.GetLabel(Context));
        }
    }
}