using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Mov(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            context.Copy(GetDest(context), GetSrcB(context));
        }

        public static void S2r(EmitterContext context)
        {
            // TODO: Better impl.
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            SystemRegister sysReg = (SystemRegister)op.RawOpCode.Extract(20, 8);

            Operand src;

            switch (sysReg)
            {
                case SystemRegister.ThreadId:
                {
                    Operand tidX = Attribute(AttributeConsts.ThreadIdX);
                    Operand tidY = Attribute(AttributeConsts.ThreadIdY);
                    Operand tidZ = Attribute(AttributeConsts.ThreadIdZ);

                    tidY = context.ShiftLeft(tidY, Const(16));
                    tidZ = context.ShiftLeft(tidZ, Const(26));

                    src = context.BitwiseOr(tidX, context.BitwiseOr(tidY, tidZ));

                    break;
                }

                case SystemRegister.ThreadIdX: src = Attribute(AttributeConsts.ThreadIdX); break;
                case SystemRegister.ThreadIdY: src = Attribute(AttributeConsts.ThreadIdY); break;
                case SystemRegister.ThreadIdZ: src = Attribute(AttributeConsts.ThreadIdZ); break;
                case SystemRegister.CtaIdX:    src = Attribute(AttributeConsts.CtaIdX);    break;
                case SystemRegister.CtaIdY:    src = Attribute(AttributeConsts.CtaIdY);    break;
                case SystemRegister.CtaIdZ:    src = Attribute(AttributeConsts.CtaIdZ);    break;

                default: src = Const(0); break;
            }

            context.Copy(GetDest(context), src);
        }

        public static void Sel(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            Operand pred = GetPredicate39(context);

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);

            Operand res = context.ConditionalSelect(pred, srcA, srcB);

            context.Copy(GetDest(context), res);
        }
    }
}