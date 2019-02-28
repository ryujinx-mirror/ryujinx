using System.Reflection.Emit;

namespace ChocolArm64.Translation
{
    struct ILOpCodeBranch : IILEmit
    {
        public OpCode  ILOp  { get; }
        public ILLabel Label { get; }

        public ILOpCodeBranch(OpCode ilOp, ILLabel label)
        {
            ILOp  = ilOp;
            Label = label;
        }

        public void Emit(ILMethodBuilder context)
        {
            context.Generator.Emit(ILOp, Label.GetLabel(context));
        }
    }
}