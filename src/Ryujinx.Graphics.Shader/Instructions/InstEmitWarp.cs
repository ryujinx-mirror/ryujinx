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
            Operand res = EmitVote(context, op.VoteMode, pred);

            if (res != null)
            {
                context.Copy(Register(op.VpDest, RegisterType.Predicate), res);
            }
            else
            {
                context.TranslatorContext.GpuAccessor.Log($"Invalid vote operation: {op.VoteMode}.");
            }

            if (op.Dest != RegisterConsts.RegisterZeroIndex)
            {
                context.Copy(GetDest(op.Dest), EmitBallot(context, pred));
            }
        }

        private static Operand EmitVote(EmitterContext context, VoteMode voteMode, Operand pred)
        {
            int subgroupSize = context.TranslatorContext.GpuAccessor.QueryHostSubgroupSize();

            if (subgroupSize <= 32)
            {
                return voteMode switch
                {
                    VoteMode.All => context.VoteAll(pred),
                    VoteMode.Any => context.VoteAny(pred),
                    VoteMode.Eq => context.VoteAllEqual(pred),
                    _ => null,
                };
            }

            // Emulate vote with ballot masks.
            // We do that when the GPU thread count is not 32,
            // since the shader code assumes it is 32.
            // allInvocations => ballot(pred) == ballot(true),
            // anyInvocation => ballot(pred) != 0,
            // allInvocationsEqual => ballot(pred) == balot(true) || ballot(pred) == 0
            Operand ballotMask = EmitBallot(context, pred);

            Operand AllTrue() => context.ICompareEqual(ballotMask, EmitBallot(context, Const(IrConsts.True)));

            return voteMode switch
            {
                VoteMode.All => AllTrue(),
                VoteMode.Any => context.ICompareNotEqual(ballotMask, Const(0)),
                VoteMode.Eq => context.BitwiseOr(AllTrue(), context.ICompareEqual(ballotMask, Const(0))),
                _ => null,
            };
        }

        private static Operand EmitBallot(EmitterContext context, Operand pred)
        {
            int subgroupSize = context.TranslatorContext.GpuAccessor.QueryHostSubgroupSize();

            if (subgroupSize <= 32)
            {
                return context.Ballot(pred, 0);
            }
            else if (subgroupSize == 64)
            {
                // TODO: Add support for vector destination and do that with a single operation.

                Operand laneId = context.Load(StorageKind.Input, IoVariable.SubgroupLaneId);
                Operand low = context.Ballot(pred, 0);
                Operand high = context.Ballot(pred, 1);

                return context.ConditionalSelect(context.BitwiseAnd(laneId, Const(32)), high, low);
            }
            else
            {
                // TODO: Add support for vector destination and do that with a single operation.

                Operand laneId = context.Load(StorageKind.Input, IoVariable.SubgroupLaneId);
                Operand element = context.ShiftRightU32(laneId, Const(5));

                Operand res = context.Ballot(pred, 0);
                res = context.ConditionalSelect(
                    context.ICompareEqual(element, Const(1)),
                    context.Ballot(pred, 1), res);
                res = context.ConditionalSelect(
                    context.ICompareEqual(element, Const(2)),
                    context.Ballot(pred, 2), res);
                res = context.ConditionalSelect(
                    context.ICompareEqual(element, Const(3)),
                    context.Ballot(pred, 3), res);

                return res;
            }
        }
    }
}
