using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct ILOpCode : IILEmit
    {
        private OpCode _ilOp;

        public ILOpCode(OpCode ilOp)
        {
            _ilOp = ilOp;
        }

        public void Emit(ILEmitter context)
        {
            context.Generator.Emit(_ilOp);
        }
    }
}