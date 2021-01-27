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
            context.Copy(GetDest(context), GetSrcB(context));
        }

        public static void R2p(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            bool isCC  = op.RawOpCode.Extract(40);
            int  shift = op.RawOpCode.Extract(41, 2) * 8;

            Operand value = GetSrcA(context);
            Operand mask  = GetSrcB(context);

            Operand Test(Operand value, int bit)
            {
                return context.ICompareNotEqual(context.BitwiseAnd(value, Const(1 << bit)), Const(0));
            }

            if (isCC)
            {
                // TODO: Support Register to condition code flags copy.
                context.Config.GpuAccessor.Log("R2P.CC not implemented.");
            }
            else
            {
                for (int bit = 0; bit < 7; bit++)
                {
                    Operand pred = Register(bit, RegisterType.Predicate);

                    Operand res = context.ConditionalSelect(Test(mask, bit), Test(value, bit + shift), pred);

                    context.Copy(pred, res);
                }
            }
        }

        public static void S2r(EmitterContext context)
        {
            // TODO: Better impl.
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            SystemRegister sysReg = (SystemRegister)op.RawOpCode.Extract(20, 8);

            Operand src;

            switch (sysReg)
            {
                case SystemRegister.LaneId: src = Attribute(AttributeConsts.LaneId); break;

                // TODO: Use value from Y direction GPU register.
                case SystemRegister.YDirection: src = ConstF(1); break;

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
                case SystemRegister.EqMask:    src = Attribute(AttributeConsts.EqMask);    break;
                case SystemRegister.LtMask:    src = Attribute(AttributeConsts.LtMask);    break;
                case SystemRegister.LeMask:    src = Attribute(AttributeConsts.LeMask);    break;
                case SystemRegister.GtMask:    src = Attribute(AttributeConsts.GtMask);    break;
                case SystemRegister.GeMask:    src = Attribute(AttributeConsts.GeMask);    break;

                default: src = Const(0); break;
            }

            context.Copy(GetDest(context), src);
        }

        public static void Sel(EmitterContext context)
        {
            Operand pred = GetPredicate39(context);

            Operand srcA = GetSrcA(context);
            Operand srcB = GetSrcB(context);

            Operand res = context.ConditionalSelect(pred, srcA, srcB);

            context.Copy(GetDest(context), res);
        }

        public static void Shfl(EmitterContext context)
        {
            OpCodeShuffle op = (OpCodeShuffle)context.CurrOp;

            Operand pred = Register(op.Predicate48);

            Operand srcA = GetSrcA(context);

            Operand srcB = op.IsBImmediate ? Const(op.ImmediateB) : Register(op.Rb);
            Operand srcC = op.IsCImmediate ? Const(op.ImmediateC) : Register(op.Rc);

            (Operand res, Operand valid) = op.ShuffleType switch
            {
                ShuffleType.Indexed   => context.Shuffle(srcA, srcB, srcC),
                ShuffleType.Up        => context.ShuffleUp(srcA, srcB, srcC),
                ShuffleType.Down      => context.ShuffleDown(srcA, srcB, srcC),
                ShuffleType.Butterfly => context.ShuffleXor(srcA, srcB, srcC),
                _                     => (null, null)
            };

            context.Copy(GetDest(context), res);
            context.Copy(pred, valid);
        }
    }
}