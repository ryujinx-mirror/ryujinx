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
                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));

                    EmitLdvecWithCastToDouble(context, op.Rn);

                    Type[] types = new Type[] { typeof(Vector128<float>), typeof(Vector128<double>) };

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertScalarToVector128Single), types));

                    context.EmitStvec(op.Rd);
                }
                else if (op.Size == 0 && op.Opc == 1)
                {
                    //Single -> Double.
                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorDoubleZero));

                    context.EmitLdvec(op.Rn);

                    Type[] types = new Type[] { typeof(Vector128<double>), typeof(Vector128<float>) };

                    context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertScalarToVector128Double), types));

                    EmitStvecWithCastFromDouble(context, op.Rd);
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

                string nameMov = op.RegisterSize == RegisterSize.Simd128
                    ? nameof(Sse.MoveHighToLow)
                    : nameof(Sse.MoveLowToHigh);

                context.EmitLdvec(op.Rn);
                context.Emit(OpCodes.Dup);

                context.EmitCall(typeof(Sse).GetMethod(nameMov));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertToVector128Double), typesCvt));

                EmitStvecWithCastFromDouble(context, op.Rd);
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

                EmitLdvecWithCastToDouble(context, op.Rn);
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
            EmitFcvtn(context, signed: true, scalar: true);
        }

        public static void Fcvtns_V(ILEmitterCtx context)
        {
            EmitFcvtn(context, signed: true, scalar: false);
        }

        public static void Fcvtnu_S(ILEmitterCtx context)
        {
            EmitFcvtn(context, signed: false, scalar: true);
        }

        public static void Fcvtnu_V(ILEmitterCtx context)
        {
            EmitFcvtn(context, signed: false, scalar: false);
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
            EmitScalarFcvtzs(context);
        }

        public static void Fcvtzs_V(ILEmitterCtx context)
        {
            EmitVectorFcvtzs(context);
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
            EmitScalarFcvtzu(context);
        }

        public static void Fcvtzu_V(ILEmitterCtx context)
        {
            EmitVectorFcvtzu(context);
        }

        public static void Scvtf_Gp(ILEmitterCtx context)
        {
            OpCodeSimdCvt64 op = (OpCodeSimdCvt64)context.CurrOp;

            context.EmitLdintzr(op.Rn);

            if (context.CurrOp.RegisterSize == RegisterSize.Int32)
            {
                context.Emit(OpCodes.Conv_U4);
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

            EmitVectorExtractSx(context, op.Rn, 0, op.Size + 2);

            EmitFloatCast(context, op.Size);

            EmitScalarSetF(context, op.Rd, op.Size);
        }

        public static void Scvtf_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 0)
            {
                Type[] typesCvt = new Type[] { typeof(Vector128<int>) };

                EmitLdvecWithSignedCast(context, op.Rn, 2);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ConvertToVector128Single), typesCvt));

                context.EmitStvec(op.Rd);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
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

            EmitVectorExtractZx(context, op.Rn, 0, op.Size + 2);

            context.Emit(OpCodes.Conv_R_Un);

            EmitFloatCast(context, op.Size);

            EmitScalarSetF(context, op.Rd, op.Size);
        }

        public static void Ucvtf_V(ILEmitterCtx context)
        {
            EmitVectorCvtf(context, signed: false);
        }

        private static void EmitFcvtn(ILEmitterCtx context, bool signed, bool scalar)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;
            int sizeI = sizeF + 2;

            int bytes = op.GetBitsCount() >> 3;
            int elems = !scalar ? bytes >> sizeI : 1;

            if (scalar && (sizeF == 0))
            {
                EmitVectorZeroLowerTmp(context);
            }

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

                EmitVectorInsertTmp(context, index, sizeI);
            }

            context.EmitLdvectmp();
            context.EmitStvec(op.Rd);

            if ((op.RegisterSize == RegisterSize.Simd64) || scalar)
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

        private static void EmitScalarFcvtzs(ILEmitterCtx context)
        {
            EmitScalarFcvtz(context, true);
        }

        private static void EmitScalarFcvtzu(ILEmitterCtx context)
        {
            EmitScalarFcvtz(context, false);
        }

        private static void EmitScalarFcvtz(ILEmitterCtx context, bool signed)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;
            int sizeI = sizeF + 2;

            int fBits = GetFBits(context);

            EmitVectorExtractF(context, op.Rn, 0, sizeF);

            EmitF2iFBitsMul(context, sizeF, fBits);

            if (sizeF == 0)
            {
                VectorHelper.EmitCall(context, signed
                    ? nameof(VectorHelper.SatF32ToS32)
                    : nameof(VectorHelper.SatF32ToU32));
            }
            else /* if (sizeF == 1) */
            {
                VectorHelper.EmitCall(context, signed
                    ? nameof(VectorHelper.SatF64ToS64)
                    : nameof(VectorHelper.SatF64ToU64));
            }

            if (sizeF == 0)
            {
                context.Emit(OpCodes.Conv_U8);
            }

            EmitScalarSet(context, op.Rd, sizeI);
        }

        private static void EmitVectorFcvtzs(ILEmitterCtx context)
        {
            EmitVectorFcvtz(context, true);
        }

        private static void EmitVectorFcvtzu(ILEmitterCtx context)
        {
            EmitVectorFcvtz(context, false);
        }

        private static void EmitVectorFcvtz(ILEmitterCtx context, bool signed)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;
            int sizeI = sizeF + 2;

            int fBits = GetFBits(context);

            int bytes = op.GetBitsCount() >> 3;
            int elems = bytes >> sizeI;

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtractF(context, op.Rn, index, sizeF);

                EmitF2iFBitsMul(context, sizeF, fBits);

                if (sizeF == 0)
                {
                    VectorHelper.EmitCall(context, signed
                        ? nameof(VectorHelper.SatF32ToS32)
                        : nameof(VectorHelper.SatF32ToU32));
                }
                else /* if (sizeF == 1) */
                {
                    VectorHelper.EmitCall(context, signed
                        ? nameof(VectorHelper.SatF64ToS64)
                        : nameof(VectorHelper.SatF64ToU64));
                }

                if (sizeF == 0)
                {
                    context.Emit(OpCodes.Conv_U8);
                }

                EmitVectorInsert(context, op.Rd, index, sizeI);
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
    }
}
