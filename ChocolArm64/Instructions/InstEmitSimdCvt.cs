using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

using static ChocolArm64.Instructions.InstEmitSimdHelper;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        public static void Fcvt_S(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            if (Optimizations.UseSse2)
            {
                if (op.Size == 1 && op.Opc == 0)
                {
                    //Double -> Single.
                    Type[] typesCvt = new Type[] { typeof(Vector128<float>), typeof(Vector128<double>) };

                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));
                    context.EmitLdvec(op.Rn);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertScalarToVector128Single), typesCvt));

                    context.EmitStvec(op.Rd);
                }
                else if (op.Size == 0 && op.Opc == 1)
                {
                    //Single -> Double.
                    Type[] typesCvt = new Type[] { typeof(Vector128<double>), typeof(Vector128<float>) };

                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));
                    context.EmitLdvec(op.Rn);

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertScalarToVector128Double), typesCvt));

                    context.EmitStvec(op.Rd);
                }
                else
                {
                    //Invalid encoding.
                    throw new InvalidOperationException();
                }
            }
            else
            {
                EmitVectorExtractF(context, op.Rn, 0, op.Size);

                EmitFloatCast(context, op.Opc);

                EmitScalarSetF(context, op.Rd, op.Opc);
            }
        }

        public static void Fcvtas_Gp(ILEmitterCtx context)
        {
            EmitFcvt_s_Gp(context, () => EmitRoundMathCall(context, MidpointRounding.AwayFromZero));
        }

        public static void Fcvtau_Gp(ILEmitterCtx context)
        {
            EmitFcvt_u_Gp(context, () => EmitRoundMathCall(context, MidpointRounding.AwayFromZero));
        }

        public static void Fcvtl_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 1)
            {
                Type[] typesCvt = new Type[] { typeof(Vector128<float>) };

                context.EmitLdvec(op.Rn);

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    context.EmitLdvec(op.Rn);

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.MoveHighToLow)));
                }

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertToVector128Double), typesCvt));

                context.EmitStvec(op.Rd);
            }
            else
            {
                int elems = 4 >> sizeF;

                int part = op.RegisterSize == RegisterSize.Simd128 ? elems : 0;

                for (int index = 0; index < elems; index++)
                {
                    if (sizeF == 0)
                    {
                        EmitVectorExtractZx(context, op.Rn, part + index, 1);
                        context.Emit(OpCodes.Conv_U2);

                        context.EmitLdarg(TranslatedSub.StateArgIdx);

                        context.EmitCall(typeof(SoftFloat16_32), nameof(SoftFloat16_32.FPConvert));
                    }
                    else /* if (sizeF == 1) */
                    {
                        EmitVectorExtractF(context, op.Rn, part + index, 0);

                        context.Emit(OpCodes.Conv_R8);
                    }

                    EmitVectorInsertTmpF(context, index, sizeF);
                }

                context.EmitLdvectmp();
                context.EmitStvec(op.Rd);
            }
        }

        public static void Fcvtms_Gp(ILEmitterCtx context)
        {
            EmitFcvt_s_Gp(context, () => EmitUnaryMathCall(context, nameof(Math.Floor)));
        }

        public static void Fcvtmu_Gp(ILEmitterCtx context)
        {
            EmitFcvt_u_Gp(context, () => EmitUnaryMathCall(context, nameof(Math.Floor)));
        }

        public static void Fcvtn_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 1)
            {
                Type[] typesCvt = new Type[] { typeof(Vector128<double>) };

                string nameMov = op.RegisterSize == RegisterSize.Simd128
                    ? nameof(Sse.MoveLowToHigh)
                    : nameof(Sse.MoveHighToLow);

                context.EmitLdvec(op.Rd);
                VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.MoveLowToHigh)));

                context.EmitLdvec(op.Rn);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertToVector128Single), typesCvt));
                context.Emit(OpCodes.Dup);

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.MoveLowToHigh)));

                context.EmitCall(typeof(Sse).GetMethod(nameMov));

                context.EmitStvec(op.Rd);
            }
            else
            {
                int elems = 4 >> sizeF;

                int part = op.RegisterSize == RegisterSize.Simd128 ? elems : 0;

                if (part != 0)
                {
                    context.EmitLdvec(op.Rd);
                    context.EmitStvectmp();
                }

                for (int index = 0; index < elems; index++)
                {
                    EmitVectorExtractF(context, op.Rn, index, sizeF);

                    if (sizeF == 0)
                    {
                        context.EmitLdarg(TranslatedSub.StateArgIdx);

                        context.EmitCall(typeof(SoftFloat32_16), nameof(SoftFloat32_16.FPConvert));

                        context.Emit(OpCodes.Conv_U8);
                        EmitVectorInsertTmp(context, part + index, 1);
                    }
                    else /* if (sizeF == 1) */
                    {
                        context.Emit(OpCodes.Conv_R4);

                        EmitVectorInsertTmpF(context, part + index, 0);
                    }
                }

                context.EmitLdvectmp();
                context.EmitStvec(op.Rd);

                if (part == 0)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
        }

        public static void Fcvtns_S(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvt_Signed(context, RoundMode.ToNearest, scalar: true);
            }
            else
            {
                EmitFcvtn(context, signed: true, scalar: true);
            }
        }

        public static void Fcvtns_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvt_Signed(context, RoundMode.ToNearest, scalar: false);
            }
            else
            {
                EmitFcvtn(context, signed: true, scalar: false);
            }
        }

        public static void Fcvtnu_S(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvt_Unsigned(context, RoundMode.ToNearest, scalar: true);
            }
            else
            {
                EmitFcvtn(context, signed: false, scalar: true);
            }
        }

        public static void Fcvtnu_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvt_Unsigned(context, RoundMode.ToNearest, scalar: false);
            }
            else
            {
                EmitFcvtn(context, signed: false, scalar: false);
            }
        }

        public static void Fcvtps_Gp(ILEmitterCtx context)
        {
            EmitFcvt_s_Gp(context, () => EmitUnaryMathCall(context, nameof(Math.Ceiling)));
        }

        public static void Fcvtpu_Gp(ILEmitterCtx context)
        {
            EmitFcvt_u_Gp(context, () => EmitUnaryMathCall(context, nameof(Math.Ceiling)));
        }

        public static void Fcvtzs_Gp(ILEmitterCtx context)
        {
            EmitFcvt_s_Gp(context, () => { });
        }

        public static void Fcvtzs_Gp_Fixed(ILEmitterCtx context)
        {
            EmitFcvtzs_Gp_Fixed(context);
        }

        public static void Fcvtzs_S(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvt_Signed(context, RoundMode.TowardsZero, scalar: true);
            }
            else
            {
                EmitFcvtz(context, signed: true, scalar: true);
            }
        }

        public static void Fcvtzs_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvt_Signed(context, RoundMode.TowardsZero, scalar: false);
            }
            else
            {
                EmitFcvtz(context, signed: true, scalar: false);
            }
        }

        public static void Fcvtzs_V_Fixed(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvt_Signed(context, RoundMode.TowardsZero, scalar: false);
            }
            else
            {
                EmitFcvtz(context, signed: true, scalar: false);
            }
        }

        public static void Fcvtzu_Gp(ILEmitterCtx context)
        {
            EmitFcvt_u_Gp(context, () => { });
        }

        public static void Fcvtzu_Gp_Fixed(ILEmitterCtx context)
        {
            EmitFcvtzu_Gp_Fixed(context);
        }

        public static void Fcvtzu_S(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvt_Unsigned(context, RoundMode.TowardsZero, scalar: true);
            }
            else
            {
                EmitFcvtz(context, signed: false, scalar: true);
            }
        }

        public static void Fcvtzu_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvt_Unsigned(context, RoundMode.TowardsZero, scalar: false);
            }
            else
            {
                EmitFcvtz(context, signed: false, scalar: false);
            }
        }

        public static void Fcvtzu_V_Fixed(ILEmitterCtx context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvt_Unsigned(context, RoundMode.TowardsZero, scalar: false);
            }
            else
            {
                EmitFcvtz(context, signed: false, scalar: false);
            }
        }

        public static void Scvtf_Gp(ILEmitterCtx context)
        {
            OpCodeSimdCvt64 op = (OpCodeSimdCvt64)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            if (context.CurrOp.RegisterSize == RegisterSize.Int32)
            {
                context.Emit(OpCodes.Conv_I4);
            }

            EmitFloatCast(context, op.Size);

            EmitScalarSetF(context, op.Rd, op.Size);
        }

        public static void Scvtf_Gp_Fixed(ILEmitterCtx context)
        {
            OpCodeSimdCvt64 op = (OpCodeSimdCvt64)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            if (context.CurrOp.RegisterSize == RegisterSize.Int32)
            {
                context.Emit(OpCodes.Conv_I4);
            }

            EmitFloatCast(context, op.Size);

            EmitI2fFBitsMul(context, op.Size, op.FBits);

            EmitScalarSetF(context, op.Rd, op.Size);
        }

        public static void Scvtf_S(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 0)
            {
                EmitSse2cvtF_Signed(context, scalar: true);
            }
            else
            {
                EmitVectorExtractSx(context, op.Rn, 0, sizeF + 2);

                EmitFloatCast(context, sizeF);

                EmitScalarSetF(context, op.Rd, sizeF);
            }
        }

        public static void Scvtf_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 0)
            {
                EmitSse2cvtF_Signed(context, scalar: false);
            }
            else
            {
                EmitVectorCvtf(context, signed: true);
            }
        }

        public static void Scvtf_V_Fixed(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            // sizeF == ((OpCodeSimdShImm64)op).Size - 2
            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 0)
            {
                EmitSse2cvtF_Signed(context, scalar: false);
            }
            else
            {
                EmitVectorCvtf(context, signed: true);
            }
        }

        public static void Ucvtf_Gp(ILEmitterCtx context)
        {
            OpCodeSimdCvt64 op = (OpCodeSimdCvt64)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            if (context.CurrOp.RegisterSize == RegisterSize.Int32)
            {
                context.Emit(OpCodes.Conv_U4);
            }

            context.Emit(OpCodes.Conv_R_Un);

            EmitFloatCast(context, op.Size);

            EmitScalarSetF(context, op.Rd, op.Size);
        }

        public static void Ucvtf_Gp_Fixed(ILEmitterCtx context)
        {
            OpCodeSimdCvt64 op = (OpCodeSimdCvt64)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            if (context.CurrOp.RegisterSize == RegisterSize.Int32)
            {
                context.Emit(OpCodes.Conv_U4);
            }

            context.Emit(OpCodes.Conv_R_Un);

            EmitFloatCast(context, op.Size);

            EmitI2fFBitsMul(context, op.Size, op.FBits);

            EmitScalarSetF(context, op.Rd, op.Size);
        }

        public static void Ucvtf_S(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 0)
            {
                EmitSse2cvtF_Unsigned(context, scalar: true);
            }
            else
            {
                EmitVectorExtractZx(context, op.Rn, 0, sizeF + 2);

                context.Emit(OpCodes.Conv_R_Un);

                EmitFloatCast(context, sizeF);

                EmitScalarSetF(context, op.Rd, sizeF);
            }
        }

        public static void Ucvtf_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 0)
            {
                EmitSse2cvtF_Unsigned(context, scalar: false);
            }
            else
            {
                EmitVectorCvtf(context, signed: false);
            }
        }

        public static void Ucvtf_V_Fixed(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            // sizeF == ((OpCodeSimdShImm64)op).Size - 2
            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 0)
            {
                EmitSse2cvtF_Unsigned(context, scalar: false);
            }
            else
            {
                EmitVectorCvtf(context, signed: false);
            }
        }

        private static void EmitFcvtn(ILEmitterCtx context, bool signed, bool scalar)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;
            int sizeI = sizeF + 2;

            int bytes = op.GetBitsCount() >> 3;
            int elems = !scalar ? bytes >> sizeI : 1;

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtractF(context, op.Rn, index, sizeF);

                EmitRoundMathCall(context, MidpointRounding.ToEven);

                if (sizeF == 0)
                {
                    VectorHelper.EmitCall(context, signed
                        ? nameof(VectorHelper.SatF32ToS32)
                        : nameof(VectorHelper.SatF32ToU32));

                    context.Emit(OpCodes.Conv_U8);
                }
                else /* if (sizeF == 1) */
                {
                    VectorHelper.EmitCall(context, signed
                        ? nameof(VectorHelper.SatF64ToS64)
                        : nameof(VectorHelper.SatF64ToU64));
                }

                if (scalar)
                {
                    EmitVectorZeroAll(context, op.Rd);
                }

                EmitVectorInsert(context, op.Rd, index, sizeI);
            }

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        private static void EmitFcvtz(ILEmitterCtx context, bool signed, bool scalar)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;
            int sizeI = sizeF + 2;

            int fBits = GetFBits(context);

            int bytes = op.GetBitsCount() >> 3;
            int elems = !scalar ? bytes >> sizeI : 1;

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtractF(context, op.Rn, index, sizeF);

                EmitF2iFBitsMul(context, sizeF, fBits);

                if (sizeF == 0)
                {
                    VectorHelper.EmitCall(context, signed
                        ? nameof(VectorHelper.SatF32ToS32)
                        : nameof(VectorHelper.SatF32ToU32));

                    context.Emit(OpCodes.Conv_U8);
                }
                else /* if (sizeF == 1) */
                {
                    VectorHelper.EmitCall(context, signed
                        ? nameof(VectorHelper.SatF64ToS64)
                        : nameof(VectorHelper.SatF64ToU64));
                }

                if (scalar)
                {
                    EmitVectorZeroAll(context, op.Rd);
                }

                EmitVectorInsert(context, op.Rd, index, sizeI);
            }

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        private static void EmitFcvt_s_Gp(ILEmitterCtx context, Action emit)
        {
            EmitFcvt___Gp(context, emit, true);
        }

        private static void EmitFcvt_u_Gp(ILEmitterCtx context, Action emit)
        {
            EmitFcvt___Gp(context, emit, false);
        }

        private static void EmitFcvt___Gp(ILEmitterCtx context, Action emit, bool signed)
        {
            OpCodeSimdCvt64 op = (OpCodeSimdCvt64)context.CurrOp;

            EmitVectorExtractF(context, op.Rn, 0, op.Size);

            emit();

            if (signed)
            {
                EmitScalarFcvts(context, op.Size, 0);
            }
            else
            {
                EmitScalarFcvtu(context, op.Size, 0);
            }

            if (context.CurrOp.RegisterSize == RegisterSize.Int32)
            {
                context.Emit(OpCodes.Conv_U8);
            }

            context.EmitStintzr(op.Rd);
        }

        private static void EmitFcvtzs_Gp_Fixed(ILEmitterCtx context)
        {
            EmitFcvtz__Gp_Fixed(context, true);
        }

        private static void EmitFcvtzu_Gp_Fixed(ILEmitterCtx context)
        {
            EmitFcvtz__Gp_Fixed(context, false);
        }

        private static void EmitFcvtz__Gp_Fixed(ILEmitterCtx context, bool signed)
        {
            OpCodeSimdCvt64 op = (OpCodeSimdCvt64)context.CurrOp;

            EmitVectorExtractF(context, op.Rn, 0, op.Size);

            if (signed)
            {
                EmitScalarFcvts(context, op.Size, op.FBits);
            }
            else
            {
                EmitScalarFcvtu(context, op.Size, op.FBits);
            }

            if (context.CurrOp.RegisterSize == RegisterSize.Int32)
            {
                context.Emit(OpCodes.Conv_U8);
            }

            context.EmitStintzr(op.Rd);
        }

        private static void EmitVectorCvtf(ILEmitterCtx context, bool signed)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;
            int sizeI = sizeF + 2;

            int fBits = GetFBits(context);

            int bytes = op.GetBitsCount() >> 3;
            int elems = bytes >> sizeI;

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtract(context, op.Rn, index, sizeI, signed);

                if (!signed)
                {
                    context.Emit(OpCodes.Conv_R_Un);
                }

                EmitFloatCast(context, sizeF);

                EmitI2fFBitsMul(context, sizeF, fBits);

                EmitVectorInsertF(context, op.Rd, index, sizeF);
            }

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        private static int GetFBits(ILEmitterCtx context)
        {
            if (context.CurrOp is OpCodeSimdShImm64 op)
            {
                return GetImmShr(op);
            }

            return 0;
        }

        private static void EmitFloatCast(ILEmitterCtx context, int size)
        {
            if (size == 0)
            {
                context.Emit(OpCodes.Conv_R4);
            }
            else if (size == 1)
            {
                context.Emit(OpCodes.Conv_R8);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
        }

        private static void EmitScalarFcvts(ILEmitterCtx context, int size, int fBits)
        {
            if (size < 0 || size > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            EmitF2iFBitsMul(context, size, fBits);

            if (context.CurrOp.RegisterSize == RegisterSize.Int32)
            {
                if (size == 0)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.SatF32ToS32));
                }
                else /* if (size == 1) */
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.SatF64ToS32));
                }
            }
            else
            {
                if (size == 0)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.SatF32ToS64));
                }
                else /* if (size == 1) */
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.SatF64ToS64));
                }
            }
        }

        private static void EmitScalarFcvtu(ILEmitterCtx context, int size, int fBits)
        {
            if (size < 0 || size > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            EmitF2iFBitsMul(context, size, fBits);

            if (context.CurrOp.RegisterSize == RegisterSize.Int32)
            {
                if (size == 0)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.SatF32ToU32));
                }
                else /* if (size == 1) */
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.SatF64ToU32));
                }
            }
            else
            {
                if (size == 0)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.SatF32ToU64));
                }
                else /* if (size == 1) */
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.SatF64ToU64));
                }
            }
        }

        private static void EmitF2iFBitsMul(ILEmitterCtx context, int size, int fBits)
        {
            if (fBits != 0)
            {
                if (size == 0)
                {
                    context.EmitLdc_R4(MathF.Pow(2f, fBits));
                }
                else if (size == 1)
                {
                    context.EmitLdc_R8(Math.Pow(2d, fBits));
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(size));
                }

                context.Emit(OpCodes.Mul);
            }
        }

        private static void EmitI2fFBitsMul(ILEmitterCtx context, int size, int fBits)
        {
            if (fBits != 0)
            {
                if (size == 0)
                {
                    context.EmitLdc_R4(1f / MathF.Pow(2f, fBits));
                }
                else if (size == 1)
                {
                    context.EmitLdc_R8(1d / Math.Pow(2d, fBits));
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(size));
                }

                context.Emit(OpCodes.Mul);
            }
        }

        private static void EmitSse41Fcvt_Signed(ILEmitterCtx context, RoundMode roundMode, bool scalar)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            // sizeF == ((OpCodeSimdShImm64)op).Size - 2
            int sizeF = op.Size & 1;

            if (sizeF == 0)
            {
                Type[] types       = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };
                Type[] typesRndCvt = new Type[] { typeof(Vector128<float>) };
                Type[] typesSav    = new Type[] { typeof(int) };

                context.EmitLdvec(op.Rn);
                context.EmitLdvec(op.Rn);

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.CompareOrdered), types));

                context.EmitLdvec(op.Rn);

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.And), types));

                if (op is OpCodeSimdShImm64 fixedOp)
                {
                    int fBits = GetImmShr(fixedOp);

                    // BitConverter.Int32BitsToSingle(fpScaled) == MathF.Pow(2f, fBits)
                    int fpScaled = 0x3F800000 + fBits * 0x800000;

                    context.EmitLdc_I4(fpScaled);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Multiply), types));
                }

                context.EmitCall(typeof(Sse41).GetMethod(GetVectorSse41NameRnd(roundMode), typesRndCvt));

                context.EmitStvectmp();
                context.EmitLdvectmp();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertToVector128Int32), typesRndCvt));

                context.EmitLdvectmp();

                context.EmitLdc_I4(0x4F000000); // 2.14748365E9f (2147483648)
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.CompareGreaterThanOrEqual), types));

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Xor), types));

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
                Type[] types       = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };
                Type[] typesRndCvt = new Type[] { typeof(Vector128<double>) };
                Type[] typesSv     = new Type[] { typeof(long), typeof(long) };
                Type[] typesSav    = new Type[] { typeof(long) };

                context.EmitLdvec(op.Rn);
                context.EmitLdvec(op.Rn);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.CompareOrdered), types));

                context.EmitLdvec(op.Rn);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.And), types));

                if (op is OpCodeSimdShImm64 fixedOp)
                {
                    int fBits = GetImmShr(fixedOp);

                    // BitConverter.Int64BitsToDouble(fpScaled) == Math.Pow(2d, fBits)
                    long fpScaled = 0x3FF0000000000000L + fBits * 0x10000000000000L;

                    context.EmitLdc_I8(fpScaled);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Multiply), types));
                }

                context.EmitCall(typeof(Sse41).GetMethod(GetVectorSse41NameRnd(roundMode), typesRndCvt));

                context.EmitStvectmp();

                if (!scalar)
                {
                    context.EmitLdvectmp();
                    context.EmitLdvectmp();

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.UnpackHigh), types));

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertToInt64), typesRndCvt));
                }
                else
                {
                    context.EmitLdc_I8(0L);
                }

                context.EmitLdvectmp();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertToInt64), typesRndCvt));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetVector128), typesSv));

                context.EmitLdvectmp();

                context.EmitLdc_I8(0x43E0000000000000L); // 9.2233720368547760E18d (9223372036854775808)
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.CompareGreaterThanOrEqual), types));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), types));

                context.EmitStvec(op.Rd);

                if (scalar)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
        }

        private static void EmitSse41Fcvt_Unsigned(ILEmitterCtx context, RoundMode roundMode, bool scalar)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            // sizeF == ((OpCodeSimdShImm64)op).Size - 2
            int sizeF = op.Size & 1;

            if (sizeF == 0)
            {
                Type[] types       = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };
                Type[] typesAdd    = new Type[] { typeof(Vector128<int>),   typeof(Vector128<int>) };
                Type[] typesRndCvt = new Type[] { typeof(Vector128<float>) };
                Type[] typesSav    = new Type[] { typeof(int) };

                context.EmitLdvec(op.Rn);
                context.EmitLdvec(op.Rn);

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.CompareOrdered), types));

                context.EmitLdvec(op.Rn);

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.And), types));

                if (op is OpCodeSimdShImm64 fixedOp)
                {
                    int fBits = GetImmShr(fixedOp);

                    // BitConverter.Int32BitsToSingle(fpScaled) == MathF.Pow(2f, fBits)
                    int fpScaled = 0x3F800000 + fBits * 0x800000;

                    context.EmitLdc_I4(fpScaled);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                    context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Multiply), types));
                }

                context.EmitCall(typeof(Sse41).GetMethod(GetVectorSse41NameRnd(roundMode), typesRndCvt));

                context.Emit(OpCodes.Dup);

                VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.CompareGreaterThan), types));

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.And), types));

                context.EmitStvectmp();
                context.EmitLdvectmp();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertToVector128Int32), typesRndCvt));

                context.EmitLdvectmp();

                context.EmitLdc_I4(0x4F000000); // 2.14748365E9f (2147483648)
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                context.EmitStvectmp2();
                context.EmitLdvectmp2();

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Subtract), types));

                context.Emit(OpCodes.Dup);

                VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.CompareGreaterThan), types));

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.And), types));

                context.EmitStvectmp();
                context.EmitLdvectmp();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertToVector128Int32), typesRndCvt));

                context.EmitLdvectmp();
                context.EmitLdvectmp2();

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.CompareGreaterThanOrEqual), types));

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Xor), types));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesAdd));

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
                Type[] types       = new Type[] { typeof(Vector128<double>), typeof(Vector128<double>) };
                Type[] typesAdd    = new Type[] { typeof(Vector128<long>),   typeof(Vector128<long>) };
                Type[] typesRndCvt = new Type[] { typeof(Vector128<double>) };
                Type[] typesSv     = new Type[] { typeof(long), typeof(long) };
                Type[] typesSav    = new Type[] { typeof(long) };

                context.EmitLdvec(op.Rn);
                context.EmitLdvec(op.Rn);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.CompareOrdered), types));

                context.EmitLdvec(op.Rn);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.And), types));

                if (op is OpCodeSimdShImm64 fixedOp)
                {
                    int fBits = GetImmShr(fixedOp);

                    // BitConverter.Int64BitsToDouble(fpScaled) == Math.Pow(2d, fBits)
                    long fpScaled = 0x3FF0000000000000L + fBits * 0x10000000000000L;

                    context.EmitLdc_I8(fpScaled);
                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Multiply), types));
                }

                context.EmitCall(typeof(Sse41).GetMethod(GetVectorSse41NameRnd(roundMode), typesRndCvt));

                context.Emit(OpCodes.Dup);

                VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.CompareGreaterThan), types));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.And), types));

                context.EmitStvectmp();

                if (!scalar)
                {
                    context.EmitLdvectmp();
                    context.EmitLdvectmp();

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.UnpackHigh), types));

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertToInt64), typesRndCvt));
                }
                else
                {
                    context.EmitLdc_I8(0L);
                }

                context.EmitLdvectmp();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertToInt64), typesRndCvt));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetVector128), typesSv));

                context.EmitLdvectmp();

                context.EmitLdc_I8(0x43E0000000000000L); // 9.2233720368547760E18d (9223372036854775808)
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                context.EmitStvectmp2();
                context.EmitLdvectmp2();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Subtract), types));

                context.Emit(OpCodes.Dup);

                VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.CompareGreaterThan), types));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.And), types));

                context.EmitStvectmp();

                if (!scalar)
                {
                    context.EmitLdvectmp();
                    context.EmitLdvectmp();

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.UnpackHigh), types));

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertToInt64), typesRndCvt));
                }
                else
                {
                    context.EmitLdc_I8(0L);
                }

                context.EmitLdvectmp();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertToInt64), typesRndCvt));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetVector128), typesSv));

                context.EmitLdvectmp();
                context.EmitLdvectmp2();

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.CompareGreaterThanOrEqual), types));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), types));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesAdd));

                context.EmitStvec(op.Rd);

                if (scalar)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
        }

        private static void EmitSse2cvtF_Signed(ILEmitterCtx context, bool scalar)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            Type[] typesMul = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };
            Type[] typesCvt = new Type[] { typeof(Vector128<int>) };
            Type[] typesSav = new Type[] { typeof(int) };

            context.EmitLdvec(op.Rn);

            context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertToVector128Single), typesCvt));

            if (op is OpCodeSimdShImm64 fixedOp)
            {
                int fBits = GetImmShr(fixedOp);

                // BitConverter.Int32BitsToSingle(fpScaled) == 1f / MathF.Pow(2f, fBits)
                int fpScaled = 0x3F800000 - fBits * 0x800000;

                context.EmitLdc_I4(fpScaled);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Multiply), typesMul));
            }

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

        private static void EmitSse2cvtF_Unsigned(ILEmitterCtx context, bool scalar)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            Type[] typesMulAdd = new Type[] { typeof(Vector128<float>), typeof(Vector128<float>) };
            Type[] typesSrlSll = new Type[] { typeof(Vector128<int>),   typeof(byte) };
            Type[] typesCvt    = new Type[] { typeof(Vector128<int>) };
            Type[] typesSav    = new Type[] { typeof(int) };

            context.EmitLdvec(op.Rn);

            context.EmitLdc_I4(16);
            context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical), typesSrlSll));

            context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertToVector128Single), typesCvt));

            context.EmitLdc_I4(0x47800000); // 65536.0f (1 << 16)
            context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

            context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Multiply), typesMulAdd));

            context.EmitLdvec(op.Rn);

            context.EmitLdc_I4(16);
            context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftLeftLogical), typesSrlSll));

            context.EmitLdc_I4(16);
            context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical), typesSrlSll));

            context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertToVector128Single), typesCvt));

            context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Add), typesMulAdd));

            if (op is OpCodeSimdShImm64 fixedOp)
            {
                int fBits = GetImmShr(fixedOp);

                // BitConverter.Int32BitsToSingle(fpScaled) == 1f / MathF.Pow(2f, fBits)
                int fpScaled = 0x3F800000 - fBits * 0x800000;

                context.EmitLdc_I4(fpScaled);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.Multiply), typesMulAdd));
            }

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

        private static string GetScalarSse41NameRnd(RoundMode roundMode)
        {
            switch (roundMode)
            {
                case RoundMode.ToNearest:
                    return nameof(Sse41.RoundToNearestIntegerScalar); // even

                case RoundMode.TowardsPlusInfinity:
                    return nameof(Sse41.RoundToPositiveInfinityScalar);

                case RoundMode.TowardsMinusInfinity:
                    return nameof(Sse41.RoundToNegativeInfinityScalar);

                default: /* case RoundMode.TowardsZero: */
                    return nameof(Sse41.RoundToZeroScalar);
            }
        }

        private static string GetVectorSse41NameRnd(RoundMode roundMode)
        {
            switch (roundMode)
            {
                case RoundMode.ToNearest:
                    return nameof(Sse41.RoundToNearestInteger); // even

                case RoundMode.TowardsPlusInfinity:
                    return nameof(Sse41.RoundToPositiveInfinity);

                case RoundMode.TowardsMinusInfinity:
                    return nameof(Sse41.RoundToNegativeInfinity);

                default: /* case RoundMode.TowardsZero: */
                    return nameof(Sse41.RoundToZero);
            }
        }
    }
}
