using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using static ChocolArm64.Instructions.InstEmitAluHelper;
using static ChocolArm64.Instructions.InstEmitSimdHelper;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        public static void Cmeq_S(ILEmitterCtx context)
        {
            EmitCmpOp(context, OpCodes.Beq_S, scalar: true);
        }

        public static void Cmeq_V(ILEmitterCtx context)
        {
            if (context.CurrOp is OpCodeSimdReg64 op)
            {
                if (op.Size < 3 && Optimizations.UseSse2)
                {
                    EmitSse2Op(context, nameof(Sse2.CompareEqual));
                }
                else if (op.Size == 3 && Optimizations.UseSse41)
                {
                    EmitSse41Op(context, nameof(Sse41.CompareEqual));
                }
                else
                {
                    EmitCmpOp(context, OpCodes.Beq_S, scalar: false);
                }
            }
            else
            {
                EmitCmpOp(context, OpCodes.Beq_S, scalar: false);
            }
        }

        public static void Cmge_S(ILEmitterCtx context)
        {
            EmitCmpOp(context, OpCodes.Bge_S, scalar: true);
        }

        public static void Cmge_V(ILEmitterCtx context)
        {
            EmitCmpOp(context, OpCodes.Bge_S, scalar: false);
        }

        public static void Cmgt_S(ILEmitterCtx context)
        {
            EmitCmpOp(context, OpCodes.Bgt_S, scalar: true);
        }

        public static void Cmgt_V(ILEmitterCtx context)
        {
            if (context.CurrOp is OpCodeSimdReg64 op)
            {
                if (op.Size < 3 && Optimizations.UseSse2)
                {
                    EmitSse2Op(context, nameof(Sse2.CompareGreaterThan));
                }
                else if (op.Size == 3 && Optimizations.UseSse42)
                {
                    EmitSse42Op(context, nameof(Sse42.CompareGreaterThan));
                }
                else
                {
                    EmitCmpOp(context, OpCodes.Bgt_S, scalar: false);
                }
            }
            else
            {
                EmitCmpOp(context, OpCodes.Bgt_S, scalar: false);
            }
        }

        public static void Cmhi_S(ILEmitterCtx context)
        {
            EmitCmpOp(context, OpCodes.Bgt_Un_S, scalar: true);
        }

        public static void Cmhi_V(ILEmitterCtx context)
        {
            EmitCmpOp(context, OpCodes.Bgt_Un_S, scalar: false);
        }

        public static void Cmhs_S(ILEmitterCtx context)
        {
            EmitCmpOp(context, OpCodes.Bge_Un_S, scalar: true);
        }

        public static void Cmhs_V(ILEmitterCtx context)
        {
            EmitCmpOp(context, OpCodes.Bge_Un_S, scalar: false);
        }

        public static void Cmle_S(ILEmitterCtx context)
        {
            EmitCmpOp(context, OpCodes.Ble_S, scalar: true);
        }

        public static void Cmle_V(ILEmitterCtx context)
        {
            EmitCmpOp(context, OpCodes.Ble_S, scalar: false);
        }

        public static void Cmlt_S(ILEmitterCtx context)
        {
            EmitCmpOp(context, OpCodes.Blt_S, scalar: true);
        }

        public static void Cmlt_V(ILEmitterCtx context)
        {
            EmitCmpOp(context, OpCodes.Blt_S, scalar: false);
        }

        public static void Cmtst_S(ILEmitterCtx context)
        {
            EmitCmtstOp(context, scalar: true);
        }

        public static void Cmtst_V(ILEmitterCtx context)
        {
            EmitCmtstOp(context, scalar: false);
        }

        public static void Fccmp_S(ILEmitterCtx context)
        {
            OpCodeSimdFcond64 op = (OpCodeSimdFcond64)context.CurrOp;

            ILLabel lblTrue = new ILLabel();
            ILLabel lblEnd  = new ILLabel();

            context.EmitCondBranch(lblTrue, op.Cond);

            context.EmitLdc_I4(op.Nzcv);
            EmitSetNzcv(context);

            context.Emit(OpCodes.Br, lblEnd);

            context.MarkLabel(lblTrue);

            EmitFcmpOrFcmpe(context, signalNaNs: false);

            context.MarkLabel(lblEnd);
        }

        public static void Fccmpe_S(ILEmitterCtx context)
        {
            OpCodeSimdFcond64 op = (OpCodeSimdFcond64)context.CurrOp;

            ILLabel lblTrue = new ILLabel();
            ILLabel lblEnd  = new ILLabel();

            context.EmitCondBranch(lblTrue, op.Cond);

            context.EmitLdc_I4(op.Nzcv);
            EmitSetNzcv(context);

            context.Emit(OpCodes.Br, lblEnd);

            context.MarkLabel(lblTrue);

            EmitFcmpOrFcmpe(context, signalNaNs: true);

            context.MarkLabel(lblEnd);
        }

        public static void Fcmeq_S(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitCmpSseOrSse2OpF(context, nameof(Sse.CompareEqualScalar), scalar: true);
            }
            else
            {
                EmitCmpOpF(context, nameof(SoftFloat32.FPCompareEQ), scalar: true);
            }
        }

        public static void Fcmeq_V(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitCmpSseOrSse2OpF(context, nameof(Sse.CompareEqual), scalar: false);
            }
            else
            {
                EmitCmpOpF(context, nameof(SoftFloat32.FPCompareEQ), scalar: false);
            }
        }

        public static void Fcmge_S(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitCmpSseOrSse2OpF(context, nameof(Sse.CompareGreaterThanOrEqualScalar), scalar: true);
            }
            else
            {
                EmitCmpOpF(context, nameof(SoftFloat32.FPCompareGE), scalar: true);
            }
        }

        public static void Fcmge_V(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitCmpSseOrSse2OpF(context, nameof(Sse.CompareGreaterThanOrEqual), scalar: false);
            }
            else
            {
                EmitCmpOpF(context, nameof(SoftFloat32.FPCompareGE), scalar: false);
            }
        }

        public static void Fcmgt_S(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitCmpSseOrSse2OpF(context, nameof(Sse.CompareGreaterThanScalar), scalar: true);
            }
            else
            {
                EmitCmpOpF(context, nameof(SoftFloat32.FPCompareGT), scalar: true);
            }
        }

        public static void Fcmgt_V(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitCmpSseOrSse2OpF(context, nameof(Sse.CompareGreaterThan), scalar: false);
            }
            else
            {
                EmitCmpOpF(context, nameof(SoftFloat32.FPCompareGT), scalar: false);
            }
        }

        public static void Fcmle_S(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitCmpSseOrSse2OpF(context, nameof(Sse.CompareGreaterThanOrEqualScalar), scalar: true, isLeOrLt: true);
            }
            else
            {
                EmitCmpOpF(context, nameof(SoftFloat32.FPCompareLE), scalar: true);
            }
        }

        public static void Fcmle_V(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitCmpSseOrSse2OpF(context, nameof(Sse.CompareGreaterThanOrEqual), scalar: false, isLeOrLt: true);
            }
            else
            {
                EmitCmpOpF(context, nameof(SoftFloat32.FPCompareLE), scalar: false);
            }
        }

        public static void Fcmlt_S(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitCmpSseOrSse2OpF(context, nameof(Sse.CompareGreaterThanScalar), scalar: true, isLeOrLt: true);
            }
            else
            {
                EmitCmpOpF(context, nameof(SoftFloat32.FPCompareLT), scalar: true);
            }
        }

        public static void Fcmlt_V(ILEmitterCtx context)
        {
            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                EmitCmpSseOrSse2OpF(context, nameof(Sse.CompareGreaterThan), scalar: false, isLeOrLt: true);
            }
            else
            {
                EmitCmpOpF(context, nameof(SoftFloat32.FPCompareLT), scalar: false);
            }
        }

        public static void Fcmp_S(ILEmitterCtx context)
        {
            EmitFcmpOrFcmpe(context, signalNaNs: false);
        }

        public static void Fcmpe_S(ILEmitterCtx context)
        {
            EmitFcmpOrFcmpe(context, signalNaNs: true);
        }

        private static void EmitFcmpOrFcmpe(ILEmitterCtx context, bool signalNaNs)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            bool cmpWithZero = !(op is OpCodeSimdFcond64) ? op.Bit3 : false;

            if (Optimizations.FastFP && Optimizations.UseSse2)
            {
                if (op.Size == 0)
                {
                    Type[] typesCmp = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                    ILLabel lblNaN = new ILLabel();
                    ILLabel lblEnd = new ILLabel();

                    context.EmitLdvec(op.Rn);

                    context.Emit(OpCodes.Dup);
                    context.EmitStvectmp();

                    if (cmpWithZero)
                    {
                        VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));
                    }
                    else
                    {
                        context.EmitLdvec(op.Rm);
                    }

                    context.Emit(OpCodes.Dup);
                    context.EmitStvectmp2();

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.CompareOrderedScalar), typesCmp));
                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.CompareEqualOrderedScalar), typesCmp));

                    context.Emit(OpCodes.Brtrue_S, lblNaN);

                    context.EmitLdc_I4(0);

                    context.EmitLdvectmp();
                    context.EmitLdvectmp2();
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.CompareGreaterThanOrEqualOrderedScalar), typesCmp));

                    context.EmitLdvectmp();
                    context.EmitLdvectmp2();
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.CompareEqualOrderedScalar), typesCmp));

                    context.EmitLdvectmp();
                    context.EmitLdvectmp2();
                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.CompareLessThanOrderedScalar), typesCmp));

                    context.EmitStflg((int)PState.NBit);
                    context.EmitStflg((int)PState.ZBit);
                    context.EmitStflg((int)PState.CBit);
                    context.EmitStflg((int)PState.VBit);

                    context.Emit(OpCodes.Br_S, lblEnd);

                    context.MarkLabel(lblNaN);

                    context.EmitLdc_I4(1);
                    context.Emit(OpCodes.Dup);
                    context.EmitLdc_I4(0);
                    context.Emit(OpCodes.Dup);

                    context.EmitStflg((int)PState.NBit);
                    context.EmitStflg((int)PState.ZBit);
                    context.EmitStflg((int)PState.CBit);
                    context.EmitStflg((int)PState.VBit);

                    context.MarkLabel(lblEnd);
                }
                else /* if (op.Size == 1) */
                {
                    Type[] typesCmp = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                    ILLabel lblNaN = new ILLabel();
                    ILLabel lblEnd = new ILLabel();

                    context.EmitLdvec(op.Rn);

                    context.Emit(OpCodes.Dup);
                    context.EmitStvectmp();

                    if (cmpWithZero)
                    {
                        VectorHelper.EmitCall(context, nameof(VectorHelper.VectorDoubleZero));
                    }
                    else
                    {
                        context.EmitLdvec(op.Rm);
                    }

                    context.Emit(OpCodes.Dup);
                    context.EmitStvectmp2();

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.CompareOrderedScalar), typesCmp));
                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorDoubleZero));

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.CompareEqualOrderedScalar), typesCmp));

                    context.Emit(OpCodes.Brtrue_S, lblNaN);

                    context.EmitLdc_I4(0);

                    context.EmitLdvectmp();
                    context.EmitLdvectmp2();
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.CompareGreaterThanOrEqualOrderedScalar), typesCmp));

                    context.EmitLdvectmp();
                    context.EmitLdvectmp2();
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.CompareEqualOrderedScalar), typesCmp));

                    context.EmitLdvectmp();
                    context.EmitLdvectmp2();
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.CompareLessThanOrderedScalar), typesCmp));

                    context.EmitStflg((int)PState.NBit);
                    context.EmitStflg((int)PState.ZBit);
                    context.EmitStflg((int)PState.CBit);
                    context.EmitStflg((int)PState.VBit);

                    context.Emit(OpCodes.Br_S, lblEnd);

                    context.MarkLabel(lblNaN);

                    context.EmitLdc_I4(1);
                    context.Emit(OpCodes.Dup);
                    context.EmitLdc_I4(0);
                    context.Emit(OpCodes.Dup);

                    context.EmitStflg((int)PState.NBit);
                    context.EmitStflg((int)PState.ZBit);
                    context.EmitStflg((int)PState.CBit);
                    context.EmitStflg((int)PState.VBit);

                    context.MarkLabel(lblEnd);
                }
            }
            else
            {
                EmitVectorExtractF(context, op.Rn, 0, op.Size);

                if (cmpWithZero)
                {
                    if (op.Size == 0)
                    {
                        context.EmitLdc_R4(0f);
                    }
                    else /* if (op.Size == 1) */
                    {
                        context.EmitLdc_R8(0d);
                    }
                }
                else
                {
                    EmitVectorExtractF(context, op.Rm, 0, op.Size);
                }

                context.EmitLdc_I4(!signalNaNs ? 0 : 1);

                EmitSoftFloatCall(context, nameof(SoftFloat32.FPCompare));

                EmitSetNzcv(context);
            }
        }

        private static void EmitCmpOp(ILEmitterCtx context, OpCode ilOp, bool scalar)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int bytes = op.GetBitsCount() >> 3;
            int elems = !scalar ? bytes >> op.Size : 1;

            ulong szMask = ulong.MaxValue >> (64 - (8 << op.Size));

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtractSx(context, op.Rn, index, op.Size);

                if (op is OpCodeSimdReg64 binOp)
                {
                    EmitVectorExtractSx(context, binOp.Rm, index, op.Size);
                }
                else
                {
                    context.EmitLdc_I8(0L);
                }

                ILLabel lblTrue = new ILLabel();
                ILLabel lblEnd  = new ILLabel();

                context.Emit(ilOp, lblTrue);

                EmitVectorInsert(context, op.Rd, index, op.Size, 0);

                context.Emit(OpCodes.Br_S, lblEnd);

                context.MarkLabel(lblTrue);

                EmitVectorInsert(context, op.Rd, index, op.Size, (long)szMask);

                context.MarkLabel(lblEnd);
            }

            if ((op.RegisterSize == RegisterSize.Simd64) || scalar)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        private static void EmitCmtstOp(ILEmitterCtx context, bool scalar)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            int bytes = op.GetBitsCount() >> 3;
            int elems = !scalar ? bytes >> op.Size : 1;

            ulong szMask = ulong.MaxValue >> (64 - (8 << op.Size));

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtractZx(context, op.Rn, index, op.Size);
                EmitVectorExtractZx(context, op.Rm, index, op.Size);

                ILLabel lblTrue = new ILLabel();
                ILLabel lblEnd  = new ILLabel();

                context.Emit(OpCodes.And);

                context.EmitLdc_I8(0L);

                context.Emit(OpCodes.Bne_Un_S, lblTrue);

                EmitVectorInsert(context, op.Rd, index, op.Size, 0);

                context.Emit(OpCodes.Br_S, lblEnd);

                context.MarkLabel(lblTrue);

                EmitVectorInsert(context, op.Rd, index, op.Size, (long)szMask);

                context.MarkLabel(lblEnd);
            }

            if ((op.RegisterSize == RegisterSize.Simd64) || scalar)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        private static void EmitCmpOpF(ILEmitterCtx context, string name, bool scalar)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            int bytes = op.GetBitsCount() >> 3;
            int elems = !scalar ? bytes >> sizeF + 2 : 1;

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtractF(context, op.Rn, index, sizeF);

                if (op is OpCodeSimdReg64 binOp)
                {
                    EmitVectorExtractF(context, binOp.Rm, index, sizeF);
                }
                else
                {
                    if (sizeF == 0)
                    {
                        context.EmitLdc_R4(0f);
                    }
                    else /* if (sizeF == 1) */
                    {
                        context.EmitLdc_R8(0d);
                    }
                }

                EmitSoftFloatCall(context, name);

                EmitVectorInsertF(context, op.Rd, index, sizeF);
            }

            if (!scalar)
            {
                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                if (sizeF == 0)
                {
                    EmitVectorZero32_128(context, op.Rd);
                }
                else /* if (sizeF == 1) */
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
        }

        private static void EmitCmpSseOrSse2OpF(ILEmitterCtx context, string name, bool scalar, bool isLeOrLt = false)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            if (sizeF == 0)
            {
                Type[] types = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };

                if (!isLeOrLt)
                {
                    context.EmitLdvec(op.Rn);
                }

                if (op is OpCodeSimdReg64 binOp)
                {
                    context.EmitLdvec(binOp.Rm);
                }
                else
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));
                }

                if (isLeOrLt)
                {
                    context.EmitLdvec(op.Rn);
                }

                context.EmitCall(typeof(Sse).GetMethod(name, types));

                context.EmitStvec(op.Rd);

                if (scalar)
                {
                    EmitVectorZero32_128(context, op.Rd);
                }
                else if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else /* if (sizeF == 1) */
            {
                Type[] types = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };

                if (!isLeOrLt)
                {
                    context.EmitLdvec(op.Rn);
                }

                if (op is OpCodeSimdReg64 binOp)
                {
                    context.EmitLdvec(binOp.Rm);
                }
                else
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorDoubleZero));
                }

                if (isLeOrLt)
                {
                    context.EmitLdvec(op.Rn);
                }

                context.EmitCall(typeof(Sse2).GetMethod(name, types));

                context.EmitStvec(op.Rd);

                if (scalar)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
        }
    }
}
