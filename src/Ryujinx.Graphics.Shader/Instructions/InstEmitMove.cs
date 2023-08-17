using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void MovR(EmitterContext context)
        {
            InstMovR op = context.GetOp<InstMovR>();

            context.Copy(GetDest(op.Dest), GetSrcReg(context, op.SrcA));
        }

        public static void MovI(EmitterContext context)
        {
            InstMovI op = context.GetOp<InstMovI>();

            context.Copy(GetDest(op.Dest), GetSrcImm(context, op.Imm20));
        }

        public static void MovC(EmitterContext context)
        {
            InstMovC op = context.GetOp<InstMovC>();

            context.Copy(GetDest(op.Dest), GetSrcCbuf(context, op.CbufSlot, op.CbufOffset));
        }

        public static void Mov32i(EmitterContext context)
        {
            InstMov32i op = context.GetOp<InstMov32i>();

            context.Copy(GetDest(op.Dest), GetSrcImm(context, op.Imm32));
        }

        public static void R2pR(EmitterContext context)
        {
            InstR2pR op = context.GetOp<InstR2pR>();

            Operand value = GetSrcReg(context, op.SrcA);
            Operand mask = GetSrcReg(context, op.SrcB);

            EmitR2p(context, value, mask, op.ByteSel, op.Ccpr);
        }

        public static void R2pI(EmitterContext context)
        {
            InstR2pI op = context.GetOp<InstR2pI>();

            Operand value = GetSrcReg(context, op.SrcA);
            Operand mask = GetSrcImm(context, Imm20ToSInt(op.Imm20));

            EmitR2p(context, value, mask, op.ByteSel, op.Ccpr);
        }

        public static void R2pC(EmitterContext context)
        {
            InstR2pC op = context.GetOp<InstR2pC>();

            Operand value = GetSrcReg(context, op.SrcA);
            Operand mask = GetSrcCbuf(context, op.CbufSlot, op.CbufOffset);

            EmitR2p(context, value, mask, op.ByteSel, op.Ccpr);
        }

        public static void S2r(EmitterContext context)
        {
            InstS2r op = context.GetOp<InstS2r>();

            Operand src;

            switch (op.SReg)
            {
                case SReg.LaneId:
                    src = EmitLoadSubgroupLaneId(context);
                    break;

                case SReg.InvocationId:
                    src = context.Load(StorageKind.Input, IoVariable.InvocationId);
                    break;

                case SReg.YDirection:
                    src = ConstF(1); // TODO: Use value from Y direction GPU register.
                    break;

                case SReg.ThreadKill:
                    src = context.TranslatorContext.Definitions.Stage == ShaderStage.Fragment ? context.Load(StorageKind.Input, IoVariable.ThreadKill) : Const(0);
                    break;

                case SReg.InvocationInfo:
                    if (context.TranslatorContext.Definitions.Stage != ShaderStage.Compute && context.TranslatorContext.Definitions.Stage != ShaderStage.Fragment)
                    {
                        // Note: Lowest 8-bits seems to contain some primitive index,
                        // but it seems to be NVIDIA implementation specific as it's only used
                        // to calculate ISBE offsets, so we can just keep it as zero.

                        if (context.TranslatorContext.Definitions.Stage == ShaderStage.TessellationControl ||
                            context.TranslatorContext.Definitions.Stage == ShaderStage.TessellationEvaluation)
                        {
                            src = context.ShiftLeft(context.Load(StorageKind.Input, IoVariable.PatchVertices), Const(16));
                        }
                        else
                        {
                            src = Const(context.TranslatorContext.Definitions.InputTopology.ToInputVertices() << 16);
                        }
                    }
                    else
                    {
                        src = Const(0);
                    }
                    break;

                case SReg.TId:
                    Operand tidX = context.Load(StorageKind.Input, IoVariable.ThreadId, null, Const(0));
                    Operand tidY = context.Load(StorageKind.Input, IoVariable.ThreadId, null, Const(1));
                    Operand tidZ = context.Load(StorageKind.Input, IoVariable.ThreadId, null, Const(2));

                    tidY = context.ShiftLeft(tidY, Const(16));
                    tidZ = context.ShiftLeft(tidZ, Const(26));

                    src = context.BitwiseOr(tidX, context.BitwiseOr(tidY, tidZ));
                    break;

                case SReg.TIdX:
                    src = context.Load(StorageKind.Input, IoVariable.ThreadId, null, Const(0));
                    break;
                case SReg.TIdY:
                    src = context.Load(StorageKind.Input, IoVariable.ThreadId, null, Const(1));
                    break;
                case SReg.TIdZ:
                    src = context.Load(StorageKind.Input, IoVariable.ThreadId, null, Const(2));
                    break;

                case SReg.CtaIdX:
                    src = context.Load(StorageKind.Input, IoVariable.CtaId, null, Const(0));
                    break;
                case SReg.CtaIdY:
                    src = context.Load(StorageKind.Input, IoVariable.CtaId, null, Const(1));
                    break;
                case SReg.CtaIdZ:
                    src = context.Load(StorageKind.Input, IoVariable.CtaId, null, Const(2));
                    break;

                case SReg.EqMask:
                    src = EmitLoadSubgroupMask(context, IoVariable.SubgroupEqMask);
                    break;
                case SReg.LtMask:
                    src = EmitLoadSubgroupMask(context, IoVariable.SubgroupLtMask);
                    break;
                case SReg.LeMask:
                    src = EmitLoadSubgroupMask(context, IoVariable.SubgroupLeMask);
                    break;
                case SReg.GtMask:
                    src = EmitLoadSubgroupMask(context, IoVariable.SubgroupGtMask);
                    break;
                case SReg.GeMask:
                    src = EmitLoadSubgroupMask(context, IoVariable.SubgroupGeMask);
                    break;

                default:
                    src = Const(0);
                    break;
            }

            context.Copy(GetDest(op.Dest), src);
        }

        private static Operand EmitLoadSubgroupLaneId(EmitterContext context)
        {
            if (context.TranslatorContext.GpuAccessor.QueryHostSubgroupSize() <= 32)
            {
                return context.Load(StorageKind.Input, IoVariable.SubgroupLaneId);
            }

            return context.BitwiseAnd(context.Load(StorageKind.Input, IoVariable.SubgroupLaneId), Const(0x1f));
        }

        private static Operand EmitLoadSubgroupMask(EmitterContext context, IoVariable ioVariable)
        {
            int subgroupSize = context.TranslatorContext.GpuAccessor.QueryHostSubgroupSize();

            if (subgroupSize <= 32)
            {
                return context.Load(StorageKind.Input, ioVariable, null, Const(0));
            }
            else if (subgroupSize == 64)
            {
                Operand laneId = context.Load(StorageKind.Input, IoVariable.SubgroupLaneId);
                Operand low = context.Load(StorageKind.Input, ioVariable, null, Const(0));
                Operand high = context.Load(StorageKind.Input, ioVariable, null, Const(1));

                return context.ConditionalSelect(context.BitwiseAnd(laneId, Const(32)), high, low);
            }
            else
            {
                Operand laneId = context.Load(StorageKind.Input, IoVariable.SubgroupLaneId);
                Operand element = context.ShiftRightU32(laneId, Const(5));

                Operand res = context.Load(StorageKind.Input, ioVariable, null, Const(0));
                res = context.ConditionalSelect(
                    context.ICompareEqual(element, Const(1)),
                    context.Load(StorageKind.Input, ioVariable, null, Const(1)), res);
                res = context.ConditionalSelect(
                    context.ICompareEqual(element, Const(2)),
                    context.Load(StorageKind.Input, ioVariable, null, Const(2)), res);
                res = context.ConditionalSelect(
                    context.ICompareEqual(element, Const(3)),
                    context.Load(StorageKind.Input, ioVariable, null, Const(3)), res);

                return res;
            }
        }

        public static void SelR(EmitterContext context)
        {
            InstSelR op = context.GetOp<InstSelR>();

            Operand srcA = GetSrcReg(context, op.SrcA);
            Operand srcB = GetSrcReg(context, op.SrcB);
            Operand srcPred = GetPredicate(context, op.SrcPred, op.SrcPredInv);

            EmitSel(context, srcA, srcB, srcPred, op.Dest);
        }

        public static void SelI(EmitterContext context)
        {
            InstSelI op = context.GetOp<InstSelI>();

            Operand srcA = GetSrcReg(context, op.SrcA);
            Operand srcB = GetSrcImm(context, Imm20ToSInt(op.Imm20));
            Operand srcPred = GetPredicate(context, op.SrcPred, op.SrcPredInv);

            EmitSel(context, srcA, srcB, srcPred, op.Dest);
        }

        public static void SelC(EmitterContext context)
        {
            InstSelC op = context.GetOp<InstSelC>();

            Operand srcA = GetSrcReg(context, op.SrcA);
            Operand srcB = GetSrcCbuf(context, op.CbufSlot, op.CbufOffset);
            Operand srcPred = GetPredicate(context, op.SrcPred, op.SrcPredInv);

            EmitSel(context, srcA, srcB, srcPred, op.Dest);
        }

        private static void EmitR2p(EmitterContext context, Operand value, Operand mask, ByteSel byteSel, bool ccpr)
        {
            Operand Test(Operand value, int bit)
            {
                return context.ICompareNotEqual(context.BitwiseAnd(value, Const(1 << bit)), Const(0));
            }

            int count = ccpr ? RegisterConsts.FlagsCount : RegisterConsts.PredsCount;
            RegisterType type = ccpr ? RegisterType.Flag : RegisterType.Predicate;
            int shift = (int)byteSel * 8;

            for (int bit = 0; bit < count; bit++)
            {
                Operand flag = Register(bit, type);
                Operand res = context.ConditionalSelect(Test(mask, bit), Test(value, bit + shift), flag);
                context.Copy(flag, res);
            }
        }

        private static void EmitSel(EmitterContext context, Operand srcA, Operand srcB, Operand srcPred, int rd)
        {
            Operand res = context.ConditionalSelect(srcPred, srcA, srcB);

            context.Copy(GetDest(rd), res);
        }
    }
}
