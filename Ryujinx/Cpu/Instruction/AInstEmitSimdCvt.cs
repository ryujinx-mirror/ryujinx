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
        public static void Fcvt_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            EmitVectorExtractF(Context, Op.Rn, 0, Op.Size);

            EmitFloatCast(Context, Op.Opc);

            EmitScalarSetF(Context, Op.Rd, Op.Opc);
        }

        public static void Fcvtms_Gp(AILEmitterCtx Context)
        {
            EmitFcvt_s_Gp(Context, nameof(Math.Floor));
        }

        public static void Fcvtps_Gp(AILEmitterCtx Context)
        {
            EmitFcvt_s_Gp(Context, nameof(Math.Ceiling));
        }

        public static void Fcvtzs_Gp(AILEmitterCtx Context)
        {
            EmitFcvtz__Gp(Context, Signed: true);
        }

        public static void Fcvtzs_Gp_Fix(AILEmitterCtx Context)
        {
            EmitFcvtz__Gp_Fix(Context, Signed: true);
        }

        public static void Fcvtzs_V(AILEmitterCtx Context)
        {
            EmitVectorFcvt(Context, Signed: true);
        }

        public static void Fcvtzu_Gp(AILEmitterCtx Context)
        {
            EmitFcvtz__Gp(Context, Signed: false);
        }

        public static void Fcvtzu_Gp_Fix(AILEmitterCtx Context)
        {
            EmitFcvtz__Gp_Fix(Context, Signed: false);
        }

        public static void Fcvtzu_V(AILEmitterCtx Context)
        {
            EmitVectorFcvt(Context, Signed: false);
        }

        public static void Scvtf_Gp(AILEmitterCtx Context)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);

            if (Context.CurrOp.RegisterSize == ARegisterSize.Int32)
            {
                Context.Emit(OpCodes.Conv_U4);
            }

            EmitFloatCast(Context, Op.Size);

            EmitScalarSetF(Context, Op.Rd, Op.Size);
        }

        public static void Scvtf_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            EmitVectorExtractSx(Context, Op.Rd, 0, Op.Size + 2);

            EmitFloatCast(Context, Op.Size);

            EmitScalarSetF(Context, Op.Rd, Op.Size);
        }

        public static void Scvtf_V(AILEmitterCtx Context)
        {
            EmitVectorCvtf(Context, Signed: true);
        }

        public static void Ucvtf_Gp(AILEmitterCtx Context)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            Context.EmitLdintzr(Op.Rn);

            if (Context.CurrOp.RegisterSize == ARegisterSize.Int32)
            {
                Context.Emit(OpCodes.Conv_U4);
            }

            Context.Emit(OpCodes.Conv_R_Un);

            EmitFloatCast(Context, Op.Size);

            EmitScalarSetF(Context, Op.Rd, Op.Size);
        }

        public static void Ucvtf_S(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            EmitVectorExtractZx(Context, Op.Rn, 0, Op.Size + 2);

            Context.Emit(OpCodes.Conv_R_Un);

            EmitFloatCast(Context, Op.Size);

            EmitScalarSetF(Context, Op.Rd, Op.Size);
        }

        public static void Ucvtf_V(AILEmitterCtx Context)
        {
            EmitVectorCvtf(Context, Signed: false);
        }

        private static int GetFBits(AILEmitterCtx Context)
        {
            if (Context.CurrOp is AOpCodeSimdShImm Op)
            {
                return GetImmShr(Op);
            }

            return 0;
        }

        private static void EmitFloatCast(AILEmitterCtx Context, int Size)
        {
            if (Size == 0)
            {
                Context.Emit(OpCodes.Conv_R4);
            }
            else if (Size == 1)
            {
                Context.Emit(OpCodes.Conv_R8);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }
        }

        private static void EmitFcvt_s_Gp(AILEmitterCtx Context, string Name)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            EmitVectorExtractF(Context, Op.Rn, 0, Op.Size);

            EmitUnaryMathCall(Context, Name);

            EmitScalarFcvts(Context, Op.Size, 0);

            if (Context.CurrOp.RegisterSize == ARegisterSize.Int32)
            {
                Context.Emit(OpCodes.Conv_U8);
            }

            Context.EmitStintzr(Op.Rd);
        }

        private static void EmitFcvtz__Gp(AILEmitterCtx Context, bool Signed)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            EmitVectorExtractF(Context, Op.Rn, 0, Op.Size);

            if (Signed)
            {
                EmitScalarFcvts(Context, Op.Size, 0);
            }
            else
            {
                EmitScalarFcvtu(Context, Op.Size, 0);
            }

            if (Context.CurrOp.RegisterSize == ARegisterSize.Int32)
            {
                Context.Emit(OpCodes.Conv_U8);
            }

            Context.EmitStintzr(Op.Rd);
        }

        private static void EmitFcvtz__Gp_Fix(AILEmitterCtx Context, bool Signed)
        {
            AOpCodeSimdCvt Op = (AOpCodeSimdCvt)Context.CurrOp;

            EmitVectorExtractF(Context, Op.Rn, 0, Op.Size);

            if (Signed)
            {
                EmitScalarFcvts(Context, Op.Size, Op.FBits);
            }
            else
            {
                EmitScalarFcvtu(Context, Op.Size, Op.FBits);
            }

            Context.EmitStintzr(Op.Rd);
        }

        private static void EmitVectorCvtf(AILEmitterCtx Context, bool Signed)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;
            int SizeI = SizeF + 2;

            int FBits = GetFBits(Context);

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> SizeI); Index++)
            {
                EmitVectorExtract(Context, Op.Rn, Index, SizeI, Signed);

                if (!Signed)
                {
                    Context.Emit(OpCodes.Conv_R_Un);
                }

                Context.Emit(SizeF == 0
                    ? OpCodes.Conv_R4
                    : OpCodes.Conv_R8);

                EmitI2fFBitsMul(Context, SizeF, FBits);

                EmitVectorInsertF(Context, Op.Rd, Index, SizeF);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitVectorFcvt(AILEmitterCtx Context, bool Signed)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int SizeF = Op.Size & 1;
            int SizeI = SizeF + 2;

            int FBits = GetFBits(Context);

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> SizeI); Index++)
            {
                EmitVectorExtractF(Context, Op.Rn, Index, SizeF);

                EmitF2iFBitsMul(Context, SizeF, FBits);

                if (SizeF == 0)
                {
                    ASoftFallback.EmitCall(Context, Signed
                        ? nameof(ASoftFallback.SatF32ToS32)
                        : nameof(ASoftFallback.SatF32ToU32));
                }
                else /* if (SizeF == 1) */
                {
                    ASoftFallback.EmitCall(Context, Signed
                        ? nameof(ASoftFallback.SatF64ToS64)
                        : nameof(ASoftFallback.SatF64ToU64));
                }

                EmitVectorInsert(Context, Op.Rd, Index, SizeI);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitScalarFcvts(AILEmitterCtx Context, int Size, int FBits)
        {
            if (Size < 0 || Size > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            EmitF2iFBitsMul(Context, Size, FBits);

            if (Context.CurrOp.RegisterSize == ARegisterSize.Int32)
            {
                if (Size == 0)
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SatF32ToS32));
                }
                else /* if (Size == 1) */
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SatF64ToS32));
                }
            }
            else
            {
                if (Size == 0)
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SatF32ToS64));
                }
                else /* if (Size == 1) */
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SatF64ToS64));
                }
            }
        }

        private static void EmitScalarFcvtu(AILEmitterCtx Context, int Size, int FBits)
        {
            if (Size < 0 || Size > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Size));
            }

            EmitF2iFBitsMul(Context, Size, FBits);

            if (Context.CurrOp.RegisterSize == ARegisterSize.Int32)
            {
                if (Size == 0)
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SatF32ToU32));
                }
                else /* if (Size == 1) */
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SatF64ToU32));
                }
            }
            else
            {
                if (Size == 0)
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SatF32ToU64));
                }
                else /* if (Size == 1) */
                {
                    ASoftFallback.EmitCall(Context, nameof(ASoftFallback.SatF64ToU64));
                }
            }
        }

        private static void EmitF2iFBitsMul(AILEmitterCtx Context, int Size, int FBits)
        {
            if (FBits != 0)
            {
                if (Size == 0)
                {
                    Context.EmitLdc_R4(MathF.Pow(2, FBits));
                }
                else if (Size == 1)
                {
                    Context.EmitLdc_R8(Math.Pow(2, FBits));
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(Size));
                }

                Context.Emit(OpCodes.Mul);
            }
        }

        private static void EmitI2fFBitsMul(AILEmitterCtx Context, int Size, int FBits)
        {
            if (FBits != 0)
            {
                if (Size == 0)
                {
                    Context.EmitLdc_R4(1f / MathF.Pow(2, FBits));
                }
                else if (Size == 1)
                {
                    Context.EmitLdc_R8(1 / Math.Pow(2, FBits));
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(Size));
                }

                Context.Emit(OpCodes.Mul);
            }
        }
    }
}