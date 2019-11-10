using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitSimdHelper;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    using Func2I = Func<Operand, Operand, Operand>;

    static partial class InstEmit
    {
        public static void Cmeq_S(ArmEmitterContext context)
        {
            EmitCmpOp(context, (op1, op2) => context.ICompareEqual(op1, op2), scalar: true);
        }

        public static void Cmeq_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                OpCodeSimd op = (OpCodeSimd)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m;

                if (op is OpCodeSimdReg binOp)
                {
                    m = GetVec(binOp.Rm);
                }
                else
                {
                    m = context.VectorZero();
                }

                Intrinsic cmpInst = X86PcmpeqInstruction[op.Size];

                Operand res = context.AddIntrinsic(cmpInst, n, m);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitCmpOp(context, (op1, op2) => context.ICompareEqual(op1, op2), scalar: false);
            }
        }

        public static void Cmge_S(ArmEmitterContext context)
        {
            EmitCmpOp(context, (op1, op2) => context.ICompareGreaterOrEqual(op1, op2), scalar: true);
        }

        public static void Cmge_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse42)
            {
                OpCodeSimd op = (OpCodeSimd)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m;

                if (op is OpCodeSimdReg binOp)
                {
                    m = GetVec(binOp.Rm);
                }
                else
                {
                    m = context.VectorZero();
                }

                Intrinsic cmpInst = X86PcmpgtInstruction[op.Size];

                Operand res = context.AddIntrinsic(cmpInst, m, n);

                Operand mask = X86GetAllElements(context, -1L);

                res = context.AddIntrinsic(Intrinsic.X86Pandn, res, mask);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitCmpOp(context, (op1, op2) => context.ICompareGreaterOrEqual(op1, op2), scalar: false);
            }
        }

        public static void Cmgt_S(ArmEmitterContext context)
        {
            EmitCmpOp(context, (op1, op2) => context.ICompareGreater(op1, op2), scalar: true);
        }

        public static void Cmgt_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse42)
            {
                OpCodeSimd op = (OpCodeSimd)context.CurrOp;

                Operand n = GetVec(op.Rn);
                Operand m;

                if (op is OpCodeSimdReg binOp)
                {
                    m = GetVec(binOp.Rm);
                }
                else
                {
                    m = context.VectorZero();
                }

                Intrinsic cmpInst = X86PcmpgtInstruction[op.Size];

                Operand res = context.AddIntrinsic(cmpInst, n, m);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitCmpOp(context, (op1, op2) => context.ICompareGreater(op1, op2), scalar: false);
            }
        }

        public static void Cmhi_S(ArmEmitterContext context)
        {
            EmitCmpOp(context, (op1, op2) => context.ICompareGreaterUI(op1, op2), scalar: true);
        }

        public static void Cmhi_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            if (Optimizations.UseSse41 && op.Size < 3)
            {
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Intrinsic maxInst = X86PmaxuInstruction[op.Size];

                Operand res = context.AddIntrinsic(maxInst, m, n);

                Intrinsic cmpInst = X86PcmpeqInstruction[op.Size];

                res = context.AddIntrinsic(cmpInst, res, m);

                Operand mask = X86GetAllElements(context, -1L);

                res = context.AddIntrinsic(Intrinsic.X86Pandn, res, mask);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitCmpOp(context, (op1, op2) => context.ICompareGreaterUI(op1, op2), scalar: false);
            }
        }

        public static void Cmhs_S(ArmEmitterContext context)
        {
            EmitCmpOp(context, (op1, op2) => context.ICompareGreaterOrEqualUI(op1, op2), scalar: true);
        }

        public static void Cmhs_V(ArmEmitterContext context)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            if (Optimizations.UseSse41 && op.Size < 3)
            {
                Operand n = GetVec(op.Rn);
                Operand m = GetVec(op.Rm);

                Intrinsic maxInst = X86PmaxuInstruction[op.Size];

                Operand res = context.AddIntrinsic(maxInst, n, m);

                Intrinsic cmpInst = X86PcmpeqInstruction[op.Size];

                res = context.AddIntrinsic(cmpInst, res, n);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitCmpOp(context, (op1, op2) => context.ICompareGreaterOrEqualUI(op1, op2), scalar: false);
            }
        }

        public static void Cmle_S(ArmEmitterContext context)
        {
            EmitCmpOp(context, (op1, op2) => context.ICompareLessOrEqual(op1, op2), scalar: true);
        }

        public static void Cmle_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse42)
            {
                OpCodeSimd op = (OpCodeSimd)context.CurrOp;

                Operand n = GetVec(op.Rn);

                Intrinsic cmpInst = X86PcmpgtInstruction[op.Size];

                Operand res = context.AddIntrinsic(cmpInst, n, context.VectorZero());

                Operand mask = X86GetAllElements(context, -1L);

                res = context.AddIntrinsic(Intrinsic.X86Pandn, res, mask);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitCmpOp(context, (op1, op2) => context.ICompareLessOrEqual(op1, op2), scalar: false);
            }
        }

        public static void Cmlt_S(ArmEmitterContext context)
        {
            EmitCmpOp(context, (op1, op2) => context.ICompareLess(op1, op2), scalar: true);
        }

        public static void Cmlt_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse42)
            {
                OpCodeSimd op = (OpCodeSimd)context.CurrOp;

                Operand n = GetVec(op.Rn);

                Intrinsic cmpInst = X86PcmpgtInstruction[op.Size];

                Operand res = context.AddIntrinsic(cmpInst, context.VectorZero(), n);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                EmitCmpOp(context, (op1, op2) => context.ICompareLess(op1, op2), scalar: false);
            }
        }

        public static void Cmtst_S(ArmEmitterContext context)
        {
            EmitCmtstOp(context, scalar: true);
        }

        public static void Cmtst_V(ArmEmitterContext context)
        {
            EmitCmtstOp(context, scalar: false);
        }

        public static void Fccmp_S(ArmEmitterContext context)
        {
            EmitFccmpOrFccmpe(context, signalNaNs: false);
        }

        public static void Fccmpe_S(ArmEmitterContext context)
        {
            EmitFccmpOrFccmpe(context, signalNaNs: true);
        }

        public static void Fcmeq_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitCmpSseOrSse2OpF(context, CmpCondition.Equal, scalar: true);
            }
            else
            {
                EmitCmpOpF(context, SoftFloat32.FPCompareEQ, SoftFloat64.FPCompareEQ, scalar: true);
            }
        }

        public static void Fcmeq_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitCmpSseOrSse2OpF(context, CmpCondition.Equal, scalar: false);
            }
            else
            {
                EmitCmpOpF(context, SoftFloat32.FPCompareEQ, SoftFloat64.FPCompareEQ, scalar: false);
            }
        }

        public static void Fcmge_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseAvx)
            {
                EmitCmpSseOrSse2OpF(context, CmpCondition.GreaterThanOrEqual, scalar: true);
            }
            else
            {
                EmitCmpOpF(context, SoftFloat32.FPCompareGE, SoftFloat64.FPCompareGE, scalar: true);
            }
        }

        public static void Fcmge_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseAvx)
            {
                EmitCmpSseOrSse2OpF(context, CmpCondition.GreaterThanOrEqual, scalar: false);
            }
            else
            {
                EmitCmpOpF(context, SoftFloat32.FPCompareGE, SoftFloat64.FPCompareGE, scalar: false);
            }
        }

        public static void Fcmgt_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseAvx)
            {
                EmitCmpSseOrSse2OpF(context, CmpCondition.GreaterThan, scalar: true);
            }
            else
            {
                EmitCmpOpF(context, SoftFloat32.FPCompareGT, SoftFloat64.FPCompareGT, scalar: true);
            }
        }

        public static void Fcmgt_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseAvx)
            {
                EmitCmpSseOrSse2OpF(context, CmpCondition.GreaterThan, scalar: false);
            }
            else
            {
                EmitCmpOpF(context, SoftFloat32.FPCompareGT, SoftFloat64.FPCompareGT, scalar: false);
            }
        }

        public static void Fcmle_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitCmpSseOrSse2OpF(context, CmpCondition.LessThanOrEqual, scalar: true);
            }
            else
            {
                EmitCmpOpF(context, SoftFloat32.FPCompareLE, SoftFloat64.FPCompareLE, scalar: true);
            }
        }

        public static void Fcmle_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitCmpSseOrSse2OpF(context, CmpCondition.LessThanOrEqual, scalar: false);
            }
            else
            {
                EmitCmpOpF(context, SoftFloat32.FPCompareLE, SoftFloat64.FPCompareLE, scalar: false);
            }
        }

        public static void Fcmlt_S(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitCmpSseOrSse2OpF(context, CmpCondition.LessThan, scalar: true);
            }
            else
            {
                EmitCmpOpF(context, SoftFloat32.FPCompareLT, SoftFloat64.FPCompareLT, scalar: true);
            }
        }

        public static void Fcmlt_V(ArmEmitterContext context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitCmpSseOrSse2OpF(context, CmpCondition.LessThan, scalar: false);
            }
            else
            {
                EmitCmpOpF(context, SoftFloat32.FPCompareLT, SoftFloat64.FPCompareLT, scalar: false);
            }
        }

        public static void Fcmp_S(ArmEmitterContext context)
        {
            EmitFcmpOrFcmpe(context, signalNaNs: false);
        }

        public static void Fcmpe_S(ArmEmitterContext context)
        {
            EmitFcmpOrFcmpe(context, signalNaNs: true);
        }

        private static void EmitFccmpOrFccmpe(ArmEmitterContext context, bool signalNaNs)
        {
            OpCodeSimdFcond op = (OpCodeSimdFcond)context.CurrOp;

            Operand lblTrue = Label();
            Operand lblEnd  = Label();

            context.BranchIfTrue(lblTrue, InstEmitFlowHelper.GetCondTrue(context, op.Cond));

            EmitSetNzcv(context, op.Nzcv);

            context.Branch(lblEnd);

            context.MarkLabel(lblTrue);

            EmitFcmpOrFcmpe(context, signalNaNs);

            context.MarkLabel(lblEnd);
        }

        private static void EmitSetNzcv(ArmEmitterContext context, int nzcv)
        {
            Operand Extract(int value, int bit)
            {
                if (bit != 0)
                {
                    value >>= bit;
                }

                value &= 1;

                return Const(value);
            }

            SetFlag(context, PState.VFlag, Extract(nzcv, 0));
            SetFlag(context, PState.CFlag, Extract(nzcv, 1));
            SetFlag(context, PState.ZFlag, Extract(nzcv, 2));
            SetFlag(context, PState.NFlag, Extract(nzcv, 3));
        }

        private static void EmitFcmpOrFcmpe(ArmEmitterContext context, bool signalNaNs)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            bool cmpWithZero = !(op is OpCodeSimdFcond) ? op.Bit3 : false;

            if (Optimizations.FastFP && (signalNaNs ? Optimizations.UseAvx : Optimizations.UseSse2))
            {
                Operand n = GetVec(op.Rn);
                Operand m = cmpWithZero ? context.VectorZero() : GetVec(op.Rm);

                CmpCondition cmpOrdered = signalNaNs ? CmpCondition.OrderedS : CmpCondition.OrderedQ;

                Operand lblNaN = Label();
                Operand lblEnd = Label();

                if (op.Size == 0)
                {
                    Operand ordMask = context.AddIntrinsic(Intrinsic.X86Cmpss, n, m, Const((int)cmpOrdered));

                    Operand isOrdered = context.AddIntrinsicInt(Intrinsic.X86Cvtsi2si, ordMask);

                    context.BranchIfFalse(lblNaN, isOrdered);

                    Operand cf = context.AddIntrinsicInt(Intrinsic.X86Comissge, n, m);
                    Operand zf = context.AddIntrinsicInt(Intrinsic.X86Comisseq, n, m);
                    Operand nf = context.AddIntrinsicInt(Intrinsic.X86Comisslt, n, m);

                    SetFlag(context, PState.VFlag, Const(0));
                    SetFlag(context, PState.CFlag, cf);
                    SetFlag(context, PState.ZFlag, zf);
                    SetFlag(context, PState.NFlag, nf);
                }
                else /* if (op.Size == 1) */
                {
                    Operand ordMask = context.AddIntrinsic(Intrinsic.X86Cmpsd, n, m, Const((int)cmpOrdered));

                    Operand isOrdered = context.AddIntrinsicLong(Intrinsic.X86Cvtsi2si, ordMask);

                    context.BranchIfFalse(lblNaN, isOrdered);

                    Operand cf = context.AddIntrinsicInt(Intrinsic.X86Comisdge, n, m);
                    Operand zf = context.AddIntrinsicInt(Intrinsic.X86Comisdeq, n, m);
                    Operand nf = context.AddIntrinsicInt(Intrinsic.X86Comisdlt, n, m);

                    SetFlag(context, PState.VFlag, Const(0));
                    SetFlag(context, PState.CFlag, cf);
                    SetFlag(context, PState.ZFlag, zf);
                    SetFlag(context, PState.NFlag, nf);
                }

                context.Branch(lblEnd);

                context.MarkLabel(lblNaN);

                SetFlag(context, PState.VFlag, Const(1));
                SetFlag(context, PState.CFlag, Const(1));
                SetFlag(context, PState.ZFlag, Const(0));
                SetFlag(context, PState.NFlag, Const(0));

                context.MarkLabel(lblEnd);
            }
            else
            {
                OperandType type = op.Size != 0 ? OperandType.FP64 : OperandType.FP32;

                Operand ne = context.VectorExtract(type, GetVec(op.Rn), 0);
                Operand me;

                if (cmpWithZero)
                {
                    me = op.Size == 0 ? ConstF(0f) : ConstF(0d);
                }
                else
                {
                    me = context.VectorExtract(type, GetVec(op.Rm), 0);
                }

                Delegate dlg = op.Size != 0
                    ? (Delegate)new _S32_F64_F64_Bool(SoftFloat64.FPCompare)
                    : (Delegate)new _S32_F32_F32_Bool(SoftFloat32.FPCompare);

                Operand nzcv = context.Call(dlg, ne, me, Const(signalNaNs));

                EmitSetNzcv(context, nzcv);
            }
        }

        private static void EmitSetNzcv(ArmEmitterContext context, Operand nzcv)
        {
            Operand Extract(Operand value, int bit)
            {
                if (bit != 0)
                {
                    value = context.ShiftRightUI(value, Const(bit));
                }

                value = context.BitwiseAnd(value, Const(1));

                return value;
            }

            SetFlag(context, PState.VFlag, Extract(nzcv, 0));
            SetFlag(context, PState.CFlag, Extract(nzcv, 1));
            SetFlag(context, PState.ZFlag, Extract(nzcv, 2));
            SetFlag(context, PState.NFlag, Extract(nzcv, 3));
        }

        private static void EmitCmpOp(ArmEmitterContext context, Func2I emitCmp, bool scalar)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = !scalar ? op.GetBytesCount() >> op.Size : 1;

            ulong szMask = ulong.MaxValue >> (64 - (8 << op.Size));

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtractSx(context, op.Rn, index, op.Size);
                Operand me;

                if (op is OpCodeSimdReg binOp)
                {
                    me = EmitVectorExtractSx(context, binOp.Rm, index, op.Size);
                }
                else
                {
                    me = Const(0L);
                }

                Operand isTrue = emitCmp(ne, me);

                Operand mask = context.ConditionalSelect(isTrue, Const(szMask), Const(0L));

                res = EmitVectorInsert(context, res, mask, index, op.Size);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        private static void EmitCmtstOp(ArmEmitterContext context, bool scalar)
        {
            OpCodeSimdReg op = (OpCodeSimdReg)context.CurrOp;

            Operand res = context.VectorZero();

            int elems = !scalar ? op.GetBytesCount() >> op.Size : 1;

            ulong szMask = ulong.MaxValue >> (64 - (8 << op.Size));

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorExtractZx(context, op.Rn, index, op.Size);
                Operand me = EmitVectorExtractZx(context, op.Rm, index, op.Size);

                Operand test = context.BitwiseAnd(ne, me);

                Operand isTrue = context.ICompareNotEqual(test, Const(0L));

                Operand mask = context.ConditionalSelect(isTrue, Const(szMask), Const(0L));

                res = EmitVectorInsert(context, res, mask, index, op.Size);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        private static void EmitCmpOpF(
            ArmEmitterContext context,
            _F32_F32_F32 f32,
            _F64_F64_F64 f64,
            bool scalar)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand res = context.VectorZero();

            int sizeF = op.Size & 1;

            OperandType type = sizeF != 0 ? OperandType.FP64 : OperandType.FP32;

            int elems = !scalar ? op.GetBytesCount() >> sizeF + 2 : 1;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = context.VectorExtract(type, GetVec(op.Rn), index);
                Operand me;

                if (op is OpCodeSimdReg binOp)
                {
                    me = context.VectorExtract(type, GetVec(binOp.Rm), index);
                }
                else
                {
                    me = sizeF == 0 ? ConstF(0f) : ConstF(0d);
                }

                Operand e = EmitSoftFloatCall(context, f32, f64, ne, me);

                res = context.VectorInsert(res, e, index);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        private static void EmitCmpSseOrSse2OpF(ArmEmitterContext context, CmpCondition cond, bool scalar)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);
            Operand m = op is OpCodeSimdReg binOp ? GetVec(binOp.Rm) : context.VectorZero();

            int sizeF = op.Size & 1;

            if (sizeF == 0)
            {
                Intrinsic inst = scalar ? Intrinsic.X86Cmpss : Intrinsic.X86Cmpps;

                Operand res = context.AddIntrinsic(inst, n, m, Const((int)cond));

                if (scalar)
                {
                    res = context.VectorZeroUpper96(res);
                }
                else if (op.RegisterSize == RegisterSize.Simd64)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
            else /* if (sizeF == 1) */
            {
                Intrinsic inst = scalar ? Intrinsic.X86Cmpsd : Intrinsic.X86Cmppd;

                Operand res = context.AddIntrinsic(inst, n, m, Const((int)cond));

                if (scalar)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
        }
    }
}
