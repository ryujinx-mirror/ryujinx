using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;

using static ChocolArm64.Instruction.AInstEmitAluHelper;
using static ChocolArm64.Instruction.AInstEmitSimdHelper;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Cmeq_V(AILEmitterCtx Context)
        {
            EmitVectorCmp(Context, OpCodes.Beq_S);
        }

        public static void Cmge_V(AILEmitterCtx Context)
        {
            EmitVectorCmp(Context, OpCodes.Bge_S);
        }

        public static void Cmgt_V(AILEmitterCtx Context)
        {
            EmitVectorCmp(Context, OpCodes.Bgt_S);
        }

        public static void Cmhi_V(AILEmitterCtx Context)
        {
            EmitVectorCmp(Context, OpCodes.Bgt_Un_S);
        }

        public static void Cmhs_V(AILEmitterCtx Context)
        {
            EmitVectorCmp(Context, OpCodes.Bge_Un_S);
        }

        public static void Cmle_V(AILEmitterCtx Context)
        {
            EmitVectorCmp(Context, OpCodes.Ble_S);
        }

        public static void Cmlt_V(AILEmitterCtx Context)
        {
            EmitVectorCmp(Context, OpCodes.Blt_S);
        }

        public static void Cmtst_V(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            ulong SzMask = ulong.MaxValue >> (64 - (8 << Op.Size));

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, Op.Size);
                EmitVectorExtractZx(Context, Op.Rm, Index, Op.Size);

                AILLabel LblTrue = new AILLabel();
                AILLabel LblEnd  = new AILLabel();

                Context.Emit(OpCodes.And);

                Context.EmitLdc_I4(0);

                Context.Emit(OpCodes.Bne_Un_S, LblTrue);

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size, 0);

                Context.Emit(OpCodes.Br_S, LblEnd);

                Context.MarkLabel(LblTrue);

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size, (long)SzMask);

                Context.MarkLabel(LblEnd);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
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

        public static void Fcmp_S(AILEmitterCtx Context)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            bool CmpWithZero = !(Op is AOpCodeSimdFcond) ? Op.Bit3 : false;

            //Handle NaN case. If any number is NaN, then NZCV = 0011.
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
                    EmitLdcImmF(Context, 0, Op.Size);
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

        private static void EmitLdcImmF(AILEmitterCtx Context, double ImmF, int Size)
        {
            if (Size == 0)
            {
                Context.EmitLdc_R4((float)ImmF);
            }
            else if (Size == 1)
            {
                Context.EmitLdc_R8(ImmF);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }
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

        private static void EmitVectorCmp(AILEmitterCtx Context, OpCode ILOp)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            ulong SzMask = ulong.MaxValue >> (64 - (8 << Op.Size));

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
            {
                EmitVectorExtractSx(Context, Op.Rn, Index, Op.Size);

                if (Op is AOpCodeSimdReg BinOp)
                {
                    EmitVectorExtractSx(Context, BinOp.Rm, Index, Op.Size);
                }
                else
                {
                    Context.EmitLdc_I8(0);
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

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }
    }
}