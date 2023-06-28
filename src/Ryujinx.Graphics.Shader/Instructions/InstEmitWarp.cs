using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;
using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Fswzadd(EmitterContext context)
        {
            InstFswzadd op = context.GetOp<InstFswzadd>();

            Operand srcA = GetSrcReg(context, op.SrcA);
            Operand srcB = GetSrcReg(context, op.SrcB);
            Operand dest = GetDest(op.Dest);

            context.Copy(dest, context.FPSwizzleAdd(srcA, srcB, op.PnWord));

            InstEmitAluHelper.SetFPZnFlags(context, dest, op.WriteCC);
        }

        public static void Shfl(EmitterContext context)
        {
            InstShfl op = context.GetOp<InstShfl>();

            Operand pred = Register(op.DestPred, RegisterType.Predicate);

            Operand srcA = GetSrcReg(context, op.SrcA);

            Operand srcB = op.BFixShfl ? Const(op.SrcBImm) : GetSrcReg(context, op.SrcB);
            Operand srcC = op.CFixShfl ? Const(op.SrcCImm) : GetSrcReg(context, op.SrcC);

            (Operand res, Operand valid) = op.ShflMode switch
            {
                ShflMode.Idx => context.Shuffle(srcA, srcB, srcC),
                ShflMode.Up => context.ShuffleUp(srcA, srcB, srcC),
                ShflMode.Down => context.ShuffleDown(srcA, srcB, srcC),
                ShflMode.Bfly => context.ShuffleXor(srcA, srcB, srcC),
                _ => (null, null),
            };

            context.Copy(GetDest(op.Dest), res);
            context.Copy(pred, valid);
        }

        public static void Vote(EmitterContext context)
        {
            InstVote op = context.GetOp<InstVote>();

            Operand pred = GetPredicate(context, op.SrcPred, op.SrcPredInv);
            Operand res = null;

            switch (op.VoteMode)
            {
                case VoteMode.All:
                    res = context.VoteAll(pred);
                    break;
                case VoteMode.Any:
                    res = context.VoteAny(pred);
                    break;
                case VoteMode.Eq:
                    res = context.VoteAllEqual(pred);
                    break;
            }

            if (res != null)
            {
                context.Copy(Register(op.VpDest, RegisterType.Predicate), res);
            }
            else
            {
                context.Config.GpuAccessor.Log($"Invalid vote operation: {op.VoteMode}.");
            }

            if (op.Dest != RegisterConsts.RegisterZeroIndex)
            {
                context.Copy(GetDest(op.Dest), context.Ballot(pred));
            }
        }
    }
}
