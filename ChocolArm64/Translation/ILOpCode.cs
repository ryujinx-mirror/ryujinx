using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct ILOpCode : IILEmit
    {
        public OpCode ILOp { get; }

        public ILOpCode(OpCode ilOp)
        {
            ILOp = ilOp;
        }

        public void Emit(ILMethodBuilder context)
        {
            context.Generator.Emit(ILOp);
        }
    }
}