using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct ILOpCodeBranch : IILEmit
    {
        private OpCode   _ilOp;
        private ILLabel _label;

        public ILOpCodeBranch(OpCode ilOp, ILLabel label)
        {
            _ilOp  = ilOp;
            _label = label;
        }

        public void Emit(ILMethodBuilder context)
        {
            context.Generator.Emit(_ilOp, _label.GetLabel(context));
        }
    }
}