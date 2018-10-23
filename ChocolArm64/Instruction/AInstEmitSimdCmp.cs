using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;

using static ChocolArm64.Instruction.AInstEmitAluHelper;
using static ChocolArm64.Instruction.AInstEmitSimdHelper;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Cmeq_S(AILEmitterCtx Context)
        {
            EmitCmp(Context, OpCodes.Beq_S, Scalar: true);
        }

        public static void Cmeq_V(AILEmitterCtx Context)
        {
            if (Context.CurrOp is AOpCodeSimdReg Op)
            {
                if (Op.Size < 3 && AOptimizations.UseSse2)
                {
                    EmitSse2Op(Context, nameof(Sse2.CompareEqual));
                }
                else if (Op.Size == 3 && AOptimizations.UseSse41)
                {
                    EmitSse41Op(Context, nameof(Sse41.CompareEqual));
                }
                else
                {
                    EmitCmp(Context, OpCodes.Beq_S, Scalar: false);
                }
            }
            else
            {
                EmitCmp(Context, OpCodes.Beq_S, Scalar: false);
            }
        }

        public static void Cmge_S(AILEmitterCtx Context)
        {
            EmitCmp(Context, OpCodes.Bge_S, Scalar: true);
        }

        public static void Cmge_V(AILEmitterCtx Context)
        {
            EmitCmp(Context, OpCodes.Bge_S, Scalar: false);
        }

        public static void Cmgt_S(AILEmitterCtx Context)
        {
            EmitCmp(Context, OpCodes.Bgt_S, Scalar: true);
        }

        public static void Cmgt_V(AILEmitterCtx Context)
        {
            if (Context.CurrOp is AOpCodeSimdReg Op)
            {
                if (Op.Size < 3 && AOptimizations.UseSse2)
                {
                    EmitSse2Op(Context, nameof(Sse2.CompareGreaterThan));
                }
                else if (Op.Size == 3 && AOptimizations.UseSse42)
                {
                    EmitSse42Op(Context, nameof(Sse42.CompareGreaterThan));
                }
                else
                {
                    EmitCmp(Context, OpCodes.Bgt_S, Scalar: false);
                }
            }
            else
            {
                EmitCmp(Context, OpCodes.Bgt_S, Scalar: false);
            }
        }

        public static void Cmhi_S(AILEmitterCtx Context)
        {
            EmitCmp(Context, OpCodes.Bgt_Un_S, Scalar: true);
        }

        public static void Cmhi_V(AILEmitterCtx Context)
        {
            EmitCmp(Context, OpCodes.Bgt_Un_S, Scalar: false);
        }

        public static void Cmhs_S(AILEmitterCtx Context)
        {
            EmitCmp(Context, OpCodes.Bge_Un_S, Scalar: true);
        }

        public static void Cmhs_V(AILEmitterCtx Context)
        {
            EmitCmp(Context, OpCodes.Bge_Un_S, Scalar: false);
        }

        public static void Cmle_S(AILEmitterCtx Context)
        {
            EmitCmp(Context, OpCodes.Ble_S, Scalar: true);
        }

        public static void Cmle_V(AILEmitterCtx Context)
        {
            EmitCmp(Context, OpCodes.Ble_S, Scalar: false);
        }

        public static void Cmlt_S(AILEmitterCtx Context)
        {
            EmitCmp(Context, OpCodes.Blt_S, Scalar: true);
        }

        public static void Cmlt_V(AILEmitterCtx Context)
        {
            EmitCmp(Context, OpCodes.Blt_S, Scalar: false);
        }

        public static void Cmtst_S(AILEmitterCtx Context)
        {
            EmitCmtst(Context, Scalar: true);
        }

        public static void Cmtst_V(AILEmitterCtx Context)
        {
            EmitCmtst(Context, Scalar: false);
        }

        public static void Fccmp_S(AILEmitterCtx Context)
        {
            AOpCodeSimdFcond Op = (AOpCodeSimdFcond)Context.CurrOp;

            AILLabel LblTrue = new AILLabel();
            AILLabel LblEnd  = new AILLabel();

            Context.EmitCondBranch(LblTrue, Op.Cond);

            EmitSetNZCV(Context, Op.NZCV);

            Context.Emit(OpCodes.Br, LblEnd);

            Context.MarkLabel(LblTrue);

            Fcmp_S(Context);

            Context.MarkLabel(LblEnd);
        }

        public static void Fccmpe_S(AILEmitterCtx Context)
        {
            Fccmp_S(Context);
        }

        public static void Fcmeq_S(AILEmitterCtx Context)
        {
            if (Context.CurrOp is AOpCodeSimdReg && AOptimizations.UseSse
                                                 && AOptimizations.UseSse2)
            {
                EmitScalarSseOrSse2OpF(Context, nameof(Sse.CompareEqualScalar));
            }
            else
            {
                EmitScalarFcmp(Context, OpCodes.Beq_S);
            }
        }

        public static void Fcmeq_V(AILEmitterCtx Context)
        {
            if (Context.CurrOp is AOpCodeSimdReg && AOptimizations.UseSse
                                                 && AOptimizations.UseSse2)
            {
                EmitVectorSseOrSse2OpF(Context, nameof(Sse.CompareEqual));
            }
            else
            {
                EmitVectorFcmp(Context, OpCodes.Beq_S);
            }
        }

        public static void Fcmge_S(AILEmitterCtx Context)
        {
            if (Context.CurrOp is AOpCodeSimdReg && AOptimizations.UseSse
                                                 && AOptimizations.UseSse2)
            {
                EmitScalarSseOrSse2OpF(Context, nameof(Sse.CompareGreaterThanOrEqualScalar));
            }
            else
            {
                EmitScalarFcmp(Context, OpCodes.Bge_S);
            }
        }

        public static void Fcmge_V(AILEmitterCtx Context)
        {
            if (Context.CurrOp is AOpCodeSimdReg && AOptimizations.UseSse
                                                 && AOptimizations.UseSse2)
            {
                EmitVectorSseOrSse2OpF(Context, nameof(Sse.CompareGreaterThanOrEqual));
            }
            else
            {
                EmitVectorFcmp(Context, OpCodes.Bge_S);
            }
        }

        public static void Fcmgt_S(AILEmitterCtx Context)
        {
            if (Context.CurrOp is AOpCodeSimdReg && AOptimizations.UseSse
                                                 && AOptimizations.UseSse2)
            {
                EmitScalarSseOrSse2OpF(Context, nameof(Sse.CompareGreaterThanScalar));
            }
            else
            {
                EmitScalarFcmp(Context, OpCodes.Bgt_S);
            }
        }

        public static void Fcmgt_V(AILEmitterCtx Context)
        {
            if (Context.CurrOp is AOpCodeSimdReg && AOptimizations.UseSse
                                                 && AOptimizations.UseSse2)
            {
                EmitVectorSseOrSse2OpF(Context, nameof(Sse.CompareGreaterThan));
            }
            else
            {
                EmitVectorFcmp(Context, OpCodes.Bgt_S);
            }
        }

        public static void Fcmle_S(AILEmitterCtx Context)
        {
            EmitScalarFcmp(Context, OpCodes.Ble_S);
        }

        public static void Fcmle_V(AILEmitterCtx Context)
        {
            EmitVectorFcmp(Context, OpCodes.Ble_S);
        }

        public static void Fcmlt_S(AILEmitterCtx Context)
        {
            EmitScalarFcmp(Context, OpCodes.Blt_S);
        }

        public static void Fcmlt_V(AILEmitterCtx Context)
        {
            EmitVectorFcmp(Context, OpCodes.Blt_S);
        }

        public static void Fcmp_S(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            bool CmpWithZero = !(Op is AOpCodeSimdFcond) ? Op.Bit3 : false;

            //Handle NaN case.
            //If any number is NaN, then NZCV = 0011.
            if (CmpWithZero)
            {
                EmitNaNCheck(Context, Op.Rn);
            }
            else
            {
                EmitNaNCheck(Context, Op.Rn);
                EmitNaNCheck(Context, Op.Rm);

                Context.Emit(OpCodes.Or);
            }

            AILLabel LblNaN = new AILLabel();
            AILLabel LblEnd = new AILLabel();

            Context.Emit(OpCodes.Brtrue_S, LblNaN);

            void EmitLoadOpers()
            {
                EmitVectorExtractF(Context, Op.Rn, 0, Op.Size);

                if (CmpWithZero)
                {
                    if (Op.Size == 0)
                    {
                        Context.EmitLdc_R4(0f);
                    }
                    else /* if (Op.Size == 1) */
                    {
                        Context.EmitLdc_R8(0d);
                    }
                }
                else
                {
                    EmitVectorExtractF(Context, Op.Rm, 0, Op.Size);
                }
            }

            //Z = Rn == Rm
            EmitLoadOpers();

            Context.Emit(OpCodes.Ceq);
            Context.Emit(OpCodes.Dup);

            Context.EmitStflg((int)APState.ZBit);

            //C = Rn >= Rm
            EmitLoadOpers();

            Context.Emit(OpCodes.Cgt);
            Context.Emit(OpCodes.Or);

            Context.EmitStflg((int)APState.CBit);

            //N = Rn < Rm
            EmitLoadOpers();

            Context.Emit(OpCodes.Clt);

            Context.EmitStflg((int)APState.NBit);

            //V = 0
            Context.EmitLdc_I4(0);

            Context.EmitStflg((int)APState.VBit);

            Context.Emit(OpCodes.Br_S, LblEnd);

            Context.MarkLabel(LblNaN);

            EmitSetNZCV(Context, 0b0011);

            Context.MarkLabel(LblEnd);
        }

        public static void Fcmpe_S(AILEmitterCtx Context)
        {
            Fcmp_S(Context);
        }

        private static void EmitNaNCheck(AILEmitterCtx Context, int Reg)
        {
            IAOpCodeSimd Op = (IAOpCodeSimd)Context.CurrOp;

            EmitVectorExtractF(Context, Reg, 0, Op.Size);

            if (Op.Size == 0)
            {
                Context.EmitCall(typeof(float), nameof(float.IsNaN));
            }
            else if (Op.Size == 1)
            {
                Context.EmitCall(typeof(double), nameof(double.IsNaN));
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static void EmitCmp(AILEmitterCtx Context, OpCode ILOp, bool Scalar)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = !Scalar ? Bytes >> Op.Size : 1;

            ulong SzMask = ulong.MaxValue >> (64 - (8 << Op.Size));

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtractSx(Context, Op.Rn, Index, Op.Size);

                if (Op is AOpCodeSimdReg BinOp)
                {
                    EmitVectorExtractSx(Context, BinOp.Rm, Index, Op.Size);
                }
                else
                {
                    Context.EmitLdc_I8(0L);
                }

                AILLabel LblTrue = new AILLabel();
                AILLabel LblEnd  = new AILLabel();

                Context.Emit(ILOp, LblTrue);

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size, 0);

                Context.Emit(OpCodes.Br_S, LblEnd);

                Context.MarkLabel(LblTrue);

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size, (long)SzMask);

                Context.MarkLabel(LblEnd);
            }

            if ((Op.RegisterSize == ARegisterSize.SIMD64) || Scalar)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitCmtst(AILEmitterCtx Context, bool Scalar)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = !Scalar ? Bytes >> Op.Size : 1;

            ulong SzMask = ulong.MaxValue >> (64 - (8 << Op.Size));

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, Op.Size);
                EmitVectorExtractZx(Context, Op.Rm, Index, Op.Size);

                AILLabel LblTrue = new AILLabel();
                AILLabel LblEnd  = new AILLabel();

                Context.Emit(OpCodes.And);

                Context.EmitLdc_I8(0L);

                Context.Emit(OpCodes.Bne_Un_S, LblTrue);

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size, 0);

                Context.Emit(OpCodes.Br_S, LblEnd);

                Context.MarkLabel(LblTrue);

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size, (long)SzMask);

                Context.MarkLabel(LblEnd);
            }

            if ((Op.RegisterSize == ARegisterSize.SIMD64) || Scalar)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitScalarFcmp(AILEmitterCtx Context, OpCode ILOp)
        {
            EmitFcmp(Context, ILOp, 0, Scalar: true);
        }

        private static void EmitVectorFcmp(AILEmitterCtx Context, OpCode ILOp)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = Bytes >> SizeF + 2;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitFcmp(Context, ILOp, Index, Scalar: false);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitFcmp(AILEmitterCtx Context, OpCode ILOp, int Index, bool Scalar)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;

            ulong SzMask = ulong.MaxValue >> (64 - (32 << SizeF));

            EmitVectorExtractF(Context, Op.Rn, Index, SizeF);

            if (Op is AOpCodeSimdReg BinOp)
            {
                EmitVectorExtractF(Context, BinOp.Rm, Index, SizeF);
            }
            else if (SizeF == 0)
            {
                Context.EmitLdc_R4(0f);
            }
            else /* if (SizeF == 1) */
            {
                Context.EmitLdc_R8(0d);
            }

            AILLabel LblTrue = new AILLabel();
            AILLabel LblEnd  = new AILLabel();

            Context.Emit(ILOp, LblTrue);

            if (Scalar)
            {
                EmitVectorZeroAll(Context, Op.Rd);
            }
            else
            {
                EmitVectorInsert(Context, Op.Rd, Index, SizeF + 2, 0);
            }

            Context.Emit(OpCodes.Br_S, LblEnd);

            Context.MarkLabel(LblTrue);

            if (Scalar)
            {
                EmitVectorInsert(Context, Op.Rd, Index, 3, (long)SzMask);

                EmitVectorZeroUpper(Context, Op.Rd);
            }
            else
            {
                EmitVectorInsert(Context, Op.Rd, Index, SizeF + 2, (long)SzMask);
            }

            Context.MarkLabel(LblEnd);
        }
    }
}
