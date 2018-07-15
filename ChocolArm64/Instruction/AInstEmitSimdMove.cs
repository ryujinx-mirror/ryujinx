using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;

using static ChocolArm64.Instruction.AInstEmitSimdHelper;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Dup_Gp(AILEmitterCtx Context)
        {
            AOpCodeSimdIns Op = (AOpCodeSimdIns)Context.CurrOp;

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = Bytes >> Op.Size;

            for (int Index = 0; Index < Elems; Index++)
            {
                Context.EmitLdintzr(Op.Rn);

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void Dup_S(AILEmitterCtx Context)
        {
            AOpCodeSimdIns Op = (AOpCodeSimdIns)Context.CurrOp;

            EmitVectorExtractZx(Context, Op.Rn, Op.DstIndex, Op.Size);

            EmitScalarSet(Context, Op.Rd, Op.Size);
        }

        public static void Dup_V(AILEmitterCtx Context)
        {
            AOpCodeSimdIns Op = (AOpCodeSimdIns)Context.CurrOp;

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = Bytes >> Op.Size;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Op.DstIndex, Op.Size);

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void Ext_V(AILEmitterCtx Context)
        {
            AOpCodeSimdExt Op = (AOpCodeSimdExt)Context.CurrOp;

            Context.EmitLdvec(Op.Rd);
            Context.EmitStvectmp();

            int Bytes = Op.GetBitsCount() >> 3;

            int Position = Op.Imm4;

            for (int Index = 0; Index < Bytes; Index++)
            {
                int Reg = Op.Imm4 + Index < Bytes ? Op.Rn : Op.Rm;

                if (Position == Bytes)
                {
                    Position = 0;
                }

                EmitVectorExtractZx(Context, Reg, Position++, 0);
                EmitVectorInsertTmp(Context, Index, 0);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void Fcsel_S(AILEmitterCtx Context)
        {
            AOpCodeSimdFcond Op = (AOpCodeSimdFcond)Context.CurrOp;

            AILLabel LblTrue = new AILLabel();
            AILLabel LblEnd  = new AILLabel();

            Context.EmitCondBranch(LblTrue, Op.Cond);

            EmitVectorExtractF(Context, Op.Rm, 0, Op.Size);

            Context.Emit(OpCodes.Br_S, LblEnd);

            Context.MarkLabel(LblTrue);

            EmitVectorExtractF(Context, Op.Rn, 0, Op.Size);

            Context.MarkLabel(LblEnd);

            EmitScalarSetF(Context, Op.Rd, Op.Size);
        }

        public static void Fmov_Ftoi(AILEmitterCtx Context)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            EmitVectorExtractZx(Context, Op.Rn, 0, 3);

            EmitIntZeroUpperIfNeeded(Context);

            Context.EmitStintzr(Op.Rd);
        }

        public static void Fmov_Ftoi1(AILEmitterCtx Context)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            EmitVectorExtractZx(Context, Op.Rn, 1, 3);

            EmitIntZeroUpperIfNeeded(Context);

            Context.EmitStintzr(Op.Rd);
        }

        public static void Fmov_Itof(AILEmitterCtx Context)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);

            EmitIntZeroUpperIfNeeded(Context);

            EmitScalarSet(Context, Op.Rd, 3);
        }

        public static void Fmov_Itof1(AILEmitterCtx Context)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);

            EmitIntZeroUpperIfNeeded(Context);

            EmitVectorInsert(Context, Op.Rd, 1, 3);
        }

        public static void Fmov_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            EmitVectorExtractF(Context, Op.Rn, 0, Op.Size);

            EmitScalarSetF(Context, Op.Rd, Op.Size);
        }

        public static void Fmov_Si(AILEmitterCtx Context)
        {
            AOpCodeSimdFmov Op = (AOpCodeSimdFmov)Context.CurrOp;

            Context.EmitLdc_I8(Op.Imm);

            EmitScalarSet(Context, Op.Rd, Op.Size + 2);
        }

        public static void Fmov_V(AILEmitterCtx Context)
        {
            AOpCodeSimdImm Op = (AOpCodeSimdImm)Context.CurrOp;

            int Elems = Op.RegisterSize == ARegisterSize.SIMD128 ? 4 : 2;

            for (int Index = 0; Index < (Elems >> Op.Size); Index++)
            {
                Context.EmitLdc_I8(Op.Imm);

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size + 2);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void Ins_Gp(AILEmitterCtx Context)
        {
            AOpCodeSimdIns Op = (AOpCodeSimdIns)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);

            EmitVectorInsert(Context, Op.Rd, Op.DstIndex, Op.Size);
        }

        public static void Ins_V(AILEmitterCtx Context)
        {
            AOpCodeSimdIns Op = (AOpCodeSimdIns)Context.CurrOp;

            EmitVectorExtractZx(Context, Op.Rn, Op.SrcIndex, Op.Size);

            EmitVectorInsert(Context, Op.Rd, Op.DstIndex, Op.Size);
        }

        public static void Movi_V(AILEmitterCtx Context)
        {
            EmitVectorImmUnaryOp(Context, () => { });
        }

        public static void Mvni_V(AILEmitterCtx Context)
        {
            EmitVectorImmUnaryOp(Context, () => Context.Emit(OpCodes.Not));
        }

        public static void Tbl_V(AILEmitterCtx Context)
        {
            AOpCodeSimdTbl Op = (AOpCodeSimdTbl)Context.CurrOp;

            Context.EmitLdvec(Op.Rm);

            for (int Index = 0; Index < Op.Size; Index++)
            {
                Context.EmitLdvec((Op.Rn + Index) & 0x1f);
            }

            switch (Op.Size)
            {
                case 1: AVectorHelper.EmitCall(Context,
                    nameof(AVectorHelper.Tbl1_V64),
                    nameof(AVectorHelper.Tbl1_V128)); break;

                case 2: AVectorHelper.EmitCall(Context,
                    nameof(AVectorHelper.Tbl2_V64),
                    nameof(AVectorHelper.Tbl2_V128)); break;

                case 3: AVectorHelper.EmitCall(Context,
                    nameof(AVectorHelper.Tbl3_V64),
                    nameof(AVectorHelper.Tbl3_V128)); break;

                case 4: AVectorHelper.EmitCall(Context,
                    nameof(AVectorHelper.Tbl4_V64),
                    nameof(AVectorHelper.Tbl4_V128)); break;

                default: throw new InvalidOperationException();
            }

            Context.EmitStvec(Op.Rd);
        }

        public static void Trn1_V(AILEmitterCtx Context)
        {
            EmitVectorTranspose(Context, Part: 0);
        }

        public static void Trn2_V(AILEmitterCtx Context)
        {
            EmitVectorTranspose(Context, Part: 1);
        }

        public static void Umov_S(AILEmitterCtx Context)
        {
            AOpCodeSimdIns Op = (AOpCodeSimdIns)Context.CurrOp;

            EmitVectorExtractZx(Context, Op.Rn, Op.DstIndex, Op.Size);

            Context.EmitStintzr(Op.Rd);
        }

        public static void Uzp1_V(AILEmitterCtx Context)
        {
            EmitVectorUnzip(Context, Part: 0);
        }

        public static void Uzp2_V(AILEmitterCtx Context)
        {
            EmitVectorUnzip(Context, Part: 1);
        }

        public static void Xtn_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Elems = 8 >> Op.Size;

            int Part = Op.RegisterSize == ARegisterSize.SIMD128 ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, Op.Size + 1);

                EmitVectorInsert(Context, Op.Rd, Part + Index, Op.Size);
            }

            if (Part == 0)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void Zip1_V(AILEmitterCtx Context)
        {
            EmitVectorZip(Context, Part: 0);
        }

        public static void Zip2_V(AILEmitterCtx Context)
        {
            EmitVectorZip(Context, Part: 1);
        }

        private static void EmitIntZeroUpperIfNeeded(AILEmitterCtx Context)
        {
            if (Context.CurrOp.RegisterSize == ARegisterSize.Int32)
            {
                Context.Emit(OpCodes.Conv_U4);
                Context.Emit(OpCodes.Conv_U8);
            }
        }

        private static void EmitVectorTranspose(AILEmitterCtx Context, int Part)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int Words = Op.GetBitsCount() >> 4;
            int Pairs = Words >> Op.Size;

            for (int Index = 0; Index < Pairs; Index++)
            {
                int Idx = Index << 1;

                EmitVectorExtractZx(Context, Op.Rn, Idx + Part, Op.Size);
                EmitVectorExtractZx(Context, Op.Rm, Idx + Part, Op.Size);

                EmitVectorInsertTmp(Context, Idx + 1, Op.Size);
                EmitVectorInsertTmp(Context, Idx    , Op.Size);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitVectorUnzip(AILEmitterCtx Context, int Part)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int Words = Op.GetBitsCount() >> 4;
            int Pairs = Words >> Op.Size;

            for (int Index = 0; Index < Pairs; Index++)
            {
                int Idx = Index << 1;

                EmitVectorExtractZx(Context, Op.Rn, Idx + Part, Op.Size);
                EmitVectorExtractZx(Context, Op.Rm, Idx + Part, Op.Size);

                EmitVectorInsertTmp(Context, Pairs + Index, Op.Size);
                EmitVectorInsertTmp(Context,         Index, Op.Size);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitVectorZip(AILEmitterCtx Context, int Part)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int Words = Op.GetBitsCount() >> 4;
            int Pairs = Words >> Op.Size;

            int Base = Part != 0 ? Pairs : 0;

            for (int Index = 0; Index < Pairs; Index++)
            {
                int Idx = Index << 1;

                EmitVectorExtractZx(Context, Op.Rn, Base + Index, Op.Size);
                EmitVectorExtractZx(Context, Op.Rm, Base + Index, Op.Size);

                EmitVectorInsertTmp(Context, Idx + 1, Op.Size);
                EmitVectorInsertTmp(Context, Idx    , Op.Size);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }
    }
}
