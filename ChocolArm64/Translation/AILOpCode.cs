using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct AILOpCode : IAILEmit
    {
        private OpCode ILOp;

        public AILOpCode(OpCode ILOp)
        {
            this.ILOp = ILOp;
        }

        public void Emit(AILEmitter Context)
        {
            Context.Generator.Emit(ILOp);
        }
    }
}