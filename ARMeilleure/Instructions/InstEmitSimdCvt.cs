using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System;
using System.Diagnostics;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitSimdHelper;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    using Func1I = Func<Operand, Operand>;

    static partial class InstEmit
    {
        public static void Fcvt_S(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            if (op.Size == 0 && op.Opc == 1) // Single -> Double.
            {
                if (Optimizations.UseSse2)
                {
                    Operand n = GetVec(op.Rn);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Cvtss2sd, context.VectorZero(), n);

                    context.Copy(GetVec(op.Rd), res);
                }
                else
                {
                    Operand ne = context.VectorExtract(OperandType.FP32, GetVec(op.Rn), 0);

                    Operand res = context.ConvertToFP(OperandType.FP64, ne);

                    context.Copy(GetVec(op.Rd), context.VectorInsert(context.VectorZero(), res, 0));
                }
            }
            else if (op.Size == 1 && op.Opc == 0) // Double -> Single.
            {
                if (Optimizations.UseSse2)
                {
                    Operand n = GetVec(op.Rn);

                    Operand res = context.AddIntrinsic(Intrinsic.X86Cvtsd2ss, context.VectorZero(), n);

                    context.Copy(GetVec(op.Rd), res);
                }
                else
                {
                    Operand ne = context.VectorExtract(OperandType.FP64, GetVec(op.Rn), 0);

                    Operand res = context.ConvertToFP(OperandType.FP32, ne);

                    context.Copy(GetVec(op.Rd), context.VectorInsert(context.VectorZero(), res, 0));
                }
            }
            else if (op.Size == 0 && op.Opc == 3) // Single -> Half.
            {
                Operand ne = context.VectorExtract(OperandType.FP32, GetVec(op.Rn), 0);

                Delegate dlg = new _U16_F32(SoftFloat32_16.FPConvert);

                Operand res = context.Call(dlg, ne);

                res = context.ZeroExtend16(OperandType.I64, res);

                context.Copy(GetVec(op.Rd), EmitVectorInsert(context, context.VectorZero(), res, 0, 1));
            }
            else if (op.Size == 3 && op.Opc == 0) // Half -> Single.
            {
                Operand ne = EmitVectorExtractZx(context, op.Rn, 0, 1);

                Delegate dlg = new _F32_U16(SoftFloat16_32.FPConvert);

                Operand res = context.Call(dlg, ne);

                context.Copy(GetVec(op.Rd), context.VectorInsert(context.VectorZero(), res, 0));
            }
            else if (op.Size == 1 && op.Opc == 3) // Double -> Half.
            {
                throw new NotImplementedException("Double-precision to half-precision.");
            }
            else if (op.Size == 3 && op.Opc == 1) // Double -> Half.
            {
                throw new NotImplementedException("Half-precision to double-precision.");
            }
            else // Invalid encoding.
            {
                Debug.Assert(false, $"type == {op.Size} && opc == {op.Opc}");
            }
        }

        public static void Fcvtas_Gp(ArmEmitterContext context)
        {
            EmitFcvt_s_Gp(context, (op1) => EmitRoundMathCall(context, MidpointRounding.AwayFromZero, op1));
        }

        public static void Fcvtau_Gp(ArmEmitterContext context)
        {
            EmitFcvt_u_Gp(context, (op1) => EmitRoundMathCall(context, MidpointRounding.AwayFromZero, op1));
        }

        public static void Fcvtl_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 1)
            {
                Operand n = GetVec(op.Rn);
                Operand res;

                if (op.RegisterSize == RegisterSize.Simd128)
                {
                    res = context.AddIntrinsic(Intrinsic.X86Movhlps, n, n);
                }
                else
                {
                    res = n;
                }

                res = context.AddIntrinsic(Intrinsic.X86Cvtps2pd, res);

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                Operand res = context.VectorZero();

                int elems = 4 >> sizeF;

                int part = op.RegisterSize == RegisterSize.Simd128 ? elems : 0;

                for (int index = 0; index < elems; index++)
                {
                    if (sizeF == 0)
                    {
                        Operand ne = EmitVectorExtractZx(context, op.Rn, part + index, 1);

                        Delegate dlg = new _F32_U16(SoftFloat16_32.FPConvert);

                        Operand e = context.Call(dlg, ne);

                        res = context.VectorInsert(res, e, index);
                    }
                    else /* if (sizeF == 1) */
                    {
                        Operand ne = context.VectorExtract(OperandType.FP32, GetVec(op.Rn), part + index);

                        Operand e = context.ConvertToFP(OperandType.FP64, ne);

                        res = context.VectorInsert(res, e, index);
                    }
                }

                context.Copy(GetVec(op.Rd), res);
            }
        }

        public static void Fcvtms_Gp(ArmEmitterContext context)
        {
            EmitFcvt_s_Gp(context, (op1) => EmitUnaryMathCall(context, MathF.Floor, Math.Floor, op1));
        }

        public static void Fcvtmu_Gp(ArmEmitterContext context)
        {
            EmitFcvt_u_Gp(context, (op1) => EmitUnaryMathCall(context, MathF.Floor, Math.Floor, op1));
        }

        public static void Fcvtn_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 1)
            {
                Operand d = GetVec(op.Rd);
                Operand n = GetVec(op.Rn);

                Operand res = context.AddIntrinsic(Intrinsic.X86Movlhps, d, context.VectorZero());

                Operand nInt = context.AddIntrinsic(Intrinsic.X86Cvtpd2ps, n);

                nInt = context.AddIntrinsic(Intrinsic.X86Movlhps, nInt, nInt);

                Intrinsic movInst = op.RegisterSize == RegisterSize.Simd128
                    ? Intrinsic.X86Movlhps
                    : Intrinsic.X86Movhlps;

                res = context.AddIntrinsic(movInst, res, nInt);

                context.Copy(GetVec(op.Rd), res);
            }
            else
            {
                OperandType type = sizeF == 0 ? OperandType.FP32 : OperandType.FP64;

                int elems = 4 >> sizeF;

                int part = op.RegisterSize == RegisterSize.Simd128 ? elems : 0;

                Operand res = part == 0 ? context.VectorZero() : context.Copy(GetVec(op.Rd));

                for (int index = 0; index < elems; index++)
                {
                    Operand ne = context.VectorExtract(type, GetVec(op.Rn), 0);

                    if (sizeF == 0)
                    {
                        Delegate dlg = new _U16_F32(SoftFloat32_16.FPConvert);

                        Operand e = context.Call(dlg, ne);

                        e = context.ZeroExtend16(OperandType.I64, e);

                        res = EmitVectorInsert(context, res, e, part + index, 1);
                    }
                    else /* if (sizeF == 1) */
                    {
                        Operand e = context.ConvertToFP(OperandType.FP32, ne);

                        res = context.VectorInsert(res, e, part + index);
                    }
                }

                context.Copy(GetVec(op.Rd), res);
            }
        }

        public static void Fcvtns_S(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvts(context, FPRoundingMode.ToNearest, scalar: true);
            }
            else
            {
                EmitFcvtn(context, signed: true, scalar: true);
            }
        }

        public static void Fcvtns_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvts(context, FPRoundingMode.ToNearest, scalar: false);
            }
            else
            {
                EmitFcvtn(context, signed: true, scalar: false);
            }
        }

        public static void Fcvtnu_S(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvtu(context, FPRoundingMode.ToNearest, scalar: true);
            }
            else
            {
                EmitFcvtn(context, signed: false, scalar: true);
            }
        }

        public static void Fcvtnu_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvtu(context, FPRoundingMode.ToNearest, scalar: false);
            }
            else
            {
                EmitFcvtn(context, signed: false, scalar: false);
            }
        }

        public static void Fcvtps_Gp(ArmEmitterContext context)
        {
            EmitFcvt_s_Gp(context, (op1) => EmitUnaryMathCall(context, MathF.Ceiling, Math.Ceiling, op1));
        }

        public static void Fcvtpu_Gp(ArmEmitterContext context)
        {
            EmitFcvt_u_Gp(context, (op1) => EmitUnaryMathCall(context, MathF.Ceiling, Math.Ceiling, op1));
        }

        public static void Fcvtzs_Gp(ArmEmitterContext context)
        {
            EmitFcvt_s_Gp(context, (op1) => op1);
        }

        public static void Fcvtzs_Gp_Fixed(ArmEmitterContext context)
        {
            EmitFcvtzs_Gp_Fixed(context);
        }

        public static void Fcvtzs_S(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvts(context, FPRoundingMode.TowardsZero, scalar: true);
            }
            else
            {
                EmitFcvtz(context, signed: true, scalar: true);
            }
        }

        public static void Fcvtzs_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvts(context, FPRoundingMode.TowardsZero, scalar: false);
            }
            else
            {
                EmitFcvtz(context, signed: true, scalar: false);
            }
        }

        public static void Fcvtzs_V_Fixed(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvts(context, FPRoundingMode.TowardsZero, scalar: false);
            }
            else
            {
                EmitFcvtz(context, signed: true, scalar: false);
            }
        }

        public static void Fcvtzu_Gp(ArmEmitterContext context)
        {
            EmitFcvt_u_Gp(context, (op1) => op1);
        }

        public static void Fcvtzu_Gp_Fixed(ArmEmitterContext context)
        {
            EmitFcvtzu_Gp_Fixed(context);
        }

        public static void Fcvtzu_S(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvtu(context, FPRoundingMode.TowardsZero, scalar: true);
            }
            else
            {
                EmitFcvtz(context, signed: false, scalar: true);
            }
        }

        public static void Fcvtzu_V(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvtu(context, FPRoundingMode.TowardsZero, scalar: false);
            }
            else
            {
                EmitFcvtz(context, signed: false, scalar: false);
            }
        }

        public static void Fcvtzu_V_Fixed(ArmEmitterContext context)
        {
            if (Optimizations.UseSse41)
            {
                EmitSse41Fcvtu(context, FPRoundingMode.TowardsZero, scalar: false);
            }
            else
            {
                EmitFcvtz(context, signed: false, scalar: false);
            }
        }

        public static void Scvtf_Gp(ArmEmitterContext context)
        {
            OpCodeSimdCvt op = (OpCodeSimdCvt)context.CurrOp;

            Operand res = GetIntOrZR(context, op.Rn);

            if (op.RegisterSize == RegisterSize.Int32)
            {
                res = context.SignExtend32(OperandType.I64, res);
            }

            res = EmitFPConvert(context, res, op.Size, signed: true);

            context.Copy(GetVec(op.Rd), context.VectorInsert(context.VectorZero(), res, 0));
        }

        public static void Scvtf_Gp_Fixed(ArmEmitterContext context)
        {
            OpCodeSimdCvt op = (OpCodeSimdCvt)context.CurrOp;

            Operand res = GetIntOrZR(context, op.Rn);

            if (op.RegisterSize == RegisterSize.Int32)
            {
                res = context.SignExtend32(OperandType.I64, res);
            }

            res = EmitFPConvert(context, res, op.Size, signed: true);

            res = EmitI2fFBitsMul(context, res, op.FBits);

            context.Copy(GetVec(op.Rd), context.VectorInsert(context.VectorZero(), res, 0));
        }

        public static void Scvtf_S(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 0)
            {
                EmitSse2Scvtf(context, scalar: true);
            }
            else
            {
                Operand res = EmitVectorLongExtract(context, op.Rn, 0, sizeF + 2);

                res = EmitFPConvert(context, res, op.Size, signed: true);

                context.Copy(GetVec(op.Rd), context.VectorInsert(context.VectorZero(), res, 0));
            }
        }

        public static void Scvtf_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 0)
            {
                EmitSse2Scvtf(context, scalar: false);
            }
            else
            {
                EmitVectorCvtf(context, signed: true);
            }
        }

        public static void Scvtf_V_Fixed(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            // sizeF == ((OpCodeSimdShImm64)op).Size - 2
            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 0)
            {
                EmitSse2Scvtf(context, scalar: false);
            }
            else
            {
                EmitVectorCvtf(context, signed: true);
            }
        }

        public static void Ucvtf_Gp(ArmEmitterContext context)
        {
            OpCodeSimdCvt op = (OpCodeSimdCvt)context.CurrOp;

            Operand res = GetIntOrZR(context, op.Rn);

            res = EmitFPConvert(context, res, op.Size, signed: false);

            context.Copy(GetVec(op.Rd), context.VectorInsert(context.VectorZero(), res, 0));
        }

        public static void Ucvtf_Gp_Fixed(ArmEmitterContext context)
        {
            OpCodeSimdCvt op = (OpCodeSimdCvt)context.CurrOp;

            Operand res = GetIntOrZR(context, op.Rn);

            res = EmitFPConvert(context, res, op.Size, signed: false);

            res = EmitI2fFBitsMul(context, res, op.FBits);

            context.Copy(GetVec(op.Rd), context.VectorInsert(context.VectorZero(), res, 0));
        }

        public static void Ucvtf_S(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 0)
            {
                EmitSse2Ucvtf(context, scalar: true);
            }
            else
            {
                Operand ne = EmitVectorLongExtract(context, op.Rn, 0, sizeF + 2);

                Operand res = EmitFPConvert(context, ne, sizeF, signed: false);

                context.Copy(GetVec(op.Rd), context.VectorInsert(context.VectorZero(), res, 0));
            }
        }

        public static void Ucvtf_V(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 0)
            {
                EmitSse2Ucvtf(context, scalar: false);
            }
            else
            {
                EmitVectorCvtf(context, signed: false);
            }
        }

        public static void Ucvtf_V_Fixed(ArmEmitterContext context)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            // sizeF == ((OpCodeSimdShImm)op).Size - 2
            int sizeF = op.Size & 1;

            if (Optimizations.UseSse2 && sizeF == 0)
            {
                EmitSse2Ucvtf(context, scalar: false);
            }
            else
            {
                EmitVectorCvtf(context, signed: false);
            }
        }

        private static void EmitFcvtn(ArmEmitterContext context, bool signed, bool scalar)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand res = context.VectorZero();

            Operand n = GetVec(op.Rn);

            int sizeF = op.Size & 1;
            int sizeI = sizeF + 2;

            OperandType type = sizeF == 0 ? OperandType.FP32 : OperandType.FP64;

            int elems = !scalar ? op.GetBytesCount() >> sizeI : 1;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = context.VectorExtract(type, n, index);

                Operand e = EmitRoundMathCall(context, MidpointRounding.ToEven, ne);

                if (sizeF == 0)
                {
                    Delegate dlg = signed
                        ? (Delegate)new _S32_F32(SoftFallback.SatF32ToS32)
                        : (Delegate)new _U32_F32(SoftFallback.SatF32ToU32);

                    e = context.Call(dlg, e);

                    e = context.ZeroExtend32(OperandType.I64, e);
                }
                else /* if (sizeF == 1) */
                {
                    Delegate dlg = signed
                        ? (Delegate)new _S64_F64(SoftFallback.SatF64ToS64)
                        : (Delegate)new _U64_F64(SoftFallback.SatF64ToU64);

                    e = context.Call(dlg, e);
                }

                res = EmitVectorInsert(context, res, e, index, sizeI);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        private static void EmitFcvtz(ArmEmitterContext context, bool signed, bool scalar)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand res = context.VectorZero();

            Operand n = GetVec(op.Rn);

            int sizeF = op.Size & 1;
            int sizeI = sizeF + 2;

            OperandType type = sizeF == 0 ? OperandType.FP32 : OperandType.FP64;

            int fBits = GetFBits(context);

            int elems = !scalar ? op.GetBytesCount() >> sizeI : 1;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = context.VectorExtract(type, n, index);

                Operand e = EmitF2iFBitsMul(context, ne, fBits);

                if (sizeF == 0)
                {
                    Delegate dlg = signed
                        ? (Delegate)new _S32_F32(SoftFallback.SatF32ToS32)
                        : (Delegate)new _U32_F32(SoftFallback.SatF32ToU32);

                    e = context.Call(dlg, e);

                    e = context.ZeroExtend32(OperandType.I64, e);
                }
                else /* if (sizeF == 1) */
                {
                    Delegate dlg = signed
                        ? (Delegate)new _S64_F64(SoftFallback.SatF64ToS64)
                        : (Delegate)new _U64_F64(SoftFallback.SatF64ToU64);

                    e = context.Call(dlg, e);
                }

                res = EmitVectorInsert(context, res, e, index, sizeI);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        private static void EmitFcvt_s_Gp(ArmEmitterContext context, Func1I emit)
        {
            EmitFcvt___Gp(context, emit, signed: true);
        }

        private static void EmitFcvt_u_Gp(ArmEmitterContext context, Func1I emit)
        {
            EmitFcvt___Gp(context, emit, signed: false);
        }

        private static void EmitFcvt___Gp(ArmEmitterContext context, Func1I emit, bool signed)
        {
            OpCodeSimdCvt op = (OpCodeSimdCvt)context.CurrOp;

            OperandType type = op.Size == 0 ? OperandType.FP32 : OperandType.FP64;

            Operand ne = context.VectorExtract(type, GetVec(op.Rn), 0);

            Operand res = signed
                ? EmitScalarFcvts(context, emit(ne), 0)
                : EmitScalarFcvtu(context, emit(ne), 0);

            SetIntOrZR(context, op.Rd, res);
        }

        private static void EmitFcvtzs_Gp_Fixed(ArmEmitterContext context)
        {
            EmitFcvtz__Gp_Fixed(context, signed: true);
        }

        private static void EmitFcvtzu_Gp_Fixed(ArmEmitterContext context)
        {
            EmitFcvtz__Gp_Fixed(context, signed: false);
        }

        private static void EmitFcvtz__Gp_Fixed(ArmEmitterContext context, bool signed)
        {
            OpCodeSimdCvt op = (OpCodeSimdCvt)context.CurrOp;

            OperandType type = op.Size == 0 ? OperandType.FP32 : OperandType.FP64;

            Operand ne = context.VectorExtract(type, GetVec(op.Rn), 0);

            Operand res = signed
                ? EmitScalarFcvts(context, ne, op.FBits)
                : EmitScalarFcvtu(context, ne, op.FBits);

            SetIntOrZR(context, op.Rd, res);
        }

        private static void EmitVectorCvtf(ArmEmitterContext context, bool signed)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand res = context.VectorZero();

            int sizeF = op.Size & 1;
            int sizeI = sizeF + 2;

            int fBits = GetFBits(context);

            int elems = op.GetBytesCount() >> sizeI;

            for (int index = 0; index < elems; index++)
            {
                Operand ne = EmitVectorLongExtract(context, op.Rn, index, sizeI);

                Operand e = EmitFPConvert(context, ne, sizeF, signed);

                e = EmitI2fFBitsMul(context, e, fBits);

                res = context.VectorInsert(res, e, index);
            }

            context.Copy(GetVec(op.Rd), res);
        }

        private static int GetFBits(ArmEmitterContext context)
        {
            if (context.CurrOp is OpCodeSimdShImm op)
            {
                return GetImmShr(op);
            }

            return 0;
        }

        private static Operand EmitFPConvert(ArmEmitterContext context, Operand value, int size, bool signed)
        {
            Debug.Assert(value.Type == OperandType.I32 || value.Type == OperandType.I64);
            Debug.Assert((uint)size < 2);

            OperandType type = size == 0 ? OperandType.FP32 : OperandType.FP64;

            if (signed)
            {
                return context.ConvertToFP(type, value);
            }
            else
            {
                return context.ConvertToFPUI(type, value);
            }
        }

        private static Operand EmitScalarFcvts(ArmEmitterContext context, Operand value, int fBits)
        {
            Debug.Assert(value.Type == OperandType.FP32 || value.Type == OperandType.FP64);

            value = EmitF2iFBitsMul(context, value, fBits);

            if (context.CurrOp.RegisterSize == RegisterSize.Int32)
            {
                Delegate dlg = value.Type == OperandType.FP32
                    ? (Delegate)new _S32_F32(SoftFallback.SatF32ToS32)
                    : (Delegate)new _S32_F64(SoftFallback.SatF64ToS32);

                return context.Call(dlg, value);
            }
            else
            {
                Delegate dlg = value.Type == OperandType.FP32
                    ? (Delegate)new _S64_F32(SoftFallback.SatF32ToS64)
                    : (Delegate)new _S64_F64(SoftFallback.SatF64ToS64);

                return context.Call(dlg, value);
            }
        }

        private static Operand EmitScalarFcvtu(ArmEmitterContext context, Operand value, int fBits)
        {
            Debug.Assert(value.Type == OperandType.FP32 || value.Type == OperandType.FP64);

            value = EmitF2iFBitsMul(context, value, fBits);

            if (context.CurrOp.RegisterSize == RegisterSize.Int32)
            {
                Delegate dlg = value.Type == OperandType.FP32
                    ? (Delegate)new _U32_F32(SoftFallback.SatF32ToU32)
                    : (Delegate)new _U32_F64(SoftFallback.SatF64ToU32);

                return context.Call(dlg, value);
            }
            else
            {
                Delegate dlg = value.Type == OperandType.FP32
                    ? (Delegate)new _U64_F32(SoftFallback.SatF32ToU64)
                    : (Delegate)new _U64_F64(SoftFallback.SatF64ToU64);

                return context.Call(dlg, value);
            }
        }

        private static Operand EmitF2iFBitsMul(ArmEmitterContext context, Operand value, int fBits)
        {
            Debug.Assert(value.Type == OperandType.FP32 || value.Type == OperandType.FP64);

            if (fBits == 0)
            {
                return value;
            }

            if (value.Type == OperandType.FP32)
            {
                return context.Multiply(value, ConstF(MathF.Pow(2f, fBits)));
            }
            else /* if (value.Type == OperandType.FP64) */
            {
                return context.Multiply(value, ConstF(Math.Pow(2d, fBits)));
            }
        }

        private static Operand EmitI2fFBitsMul(ArmEmitterContext context, Operand value, int fBits)
        {
            Debug.Assert(value.Type == OperandType.FP32 || value.Type == OperandType.FP64);

            if (fBits == 0)
            {
                return value;
            }

            if (value.Type == OperandType.FP32)
            {
                return context.Multiply(value, ConstF(1f / MathF.Pow(2f, fBits)));
            }
            else /* if (value.Type == OperandType.FP64) */
            {
                return context.Multiply(value, ConstF(1d / Math.Pow(2d, fBits)));
            }
        }

        private static void EmitSse41Fcvts(ArmEmitterContext context, FPRoundingMode roundMode, bool scalar)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            // sizeF == ((OpCodeSimdShImm64)op).Size - 2
            int sizeF = op.Size & 1;

            if (sizeF == 0)
            {
                Operand nMask = context.AddIntrinsic(Intrinsic.X86Cmpps, n, n, Const((int)CmpCondition.OrderedQ));

                Operand nScaled = context.AddIntrinsic(Intrinsic.X86Pand, nMask, n);

                if (op is OpCodeSimdShImm fixedOp)
                {
                    int fBits = GetImmShr(fixedOp);

                    // BitConverter.Int32BitsToSingle(fpScaled) == MathF.Pow(2f, fBits)
                    int fpScaled = 0x3F800000 + fBits * 0x800000;

                    Operand scale = X86GetAllElements(context, fpScaled);

                    nScaled = context.AddIntrinsic(Intrinsic.X86Mulps, nScaled, scale);
                }

                Operand nRnd = context.AddIntrinsic(Intrinsic.X86Roundps, nScaled, Const(X86GetRoundControl(roundMode)));

                Operand nInt = context.AddIntrinsic(Intrinsic.X86Cvtps2dq, nRnd);

                Operand mask = X86GetAllElements(context, 0x4F000000); // 2.14748365E9f (2147483648)

                Operand mask2 = context.AddIntrinsic(Intrinsic.X86Cmpps, nRnd, mask, Const((int)CmpCondition.NotLessThan));

                Operand res = context.AddIntrinsic(Intrinsic.X86Pxor, nInt, mask2);

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
                Operand nMask = context.AddIntrinsic(Intrinsic.X86Cmppd, n, n, Const((int)CmpCondition.OrderedQ));

                Operand nScaled = context.AddIntrinsic(Intrinsic.X86Pand, nMask, n);

                if (op is OpCodeSimdShImm fixedOp)
                {
                    int fBits = GetImmShr(fixedOp);

                    // BitConverter.Int64BitsToDouble(fpScaled) == Math.Pow(2d, fBits)
                    long fpScaled = 0x3FF0000000000000L + fBits * 0x10000000000000L;

                    Operand scale = X86GetAllElements(context, fpScaled);

                    nScaled = context.AddIntrinsic(Intrinsic.X86Mulpd, nScaled, scale);
                }

                Operand nRnd = context.AddIntrinsic(Intrinsic.X86Roundpd, nScaled, Const(X86GetRoundControl(roundMode)));

                Operand high;

                if (!scalar)
                {
                    high = context.AddIntrinsic(Intrinsic.X86Unpckhpd, nRnd, nRnd);
                    high = context.AddIntrinsicLong(Intrinsic.X86Cvtsd2si, high);
                }
                else
                {
                    high = Const(0L);
                }

                Operand low = context.AddIntrinsicLong(Intrinsic.X86Cvtsd2si, nRnd);

                Operand nInt = EmitVectorLongCreate(context, low, high);

                Operand mask = X86GetAllElements(context, 0x43E0000000000000L); // 9.2233720368547760E18d (9223372036854775808)

                Operand mask2 = context.AddIntrinsic(Intrinsic.X86Cmppd, nRnd, mask, Const((int)CmpCondition.NotLessThan));

                Operand res = context.AddIntrinsic(Intrinsic.X86Pxor, nInt, mask2);

                if (scalar)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
        }

        private static void EmitSse41Fcvtu(ArmEmitterContext context, FPRoundingMode roundMode, bool scalar)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            // sizeF == ((OpCodeSimdShImm)op).Size - 2
            int sizeF = op.Size & 1;

            if (sizeF == 0)
            {
                Operand nMask = context.AddIntrinsic(Intrinsic.X86Cmpps, n, n, Const((int)CmpCondition.OrderedQ));

                Operand nScaled = context.AddIntrinsic(Intrinsic.X86Pand, nMask, n);

                if (op is OpCodeSimdShImm fixedOp)
                {
                    int fBits = GetImmShr(fixedOp);

                    // BitConverter.Int32BitsToSingle(fpScaled) == MathF.Pow(2f, fBits)
                    int fpScaled = 0x3F800000 + fBits * 0x800000;

                    Operand scale = X86GetAllElements(context, fpScaled);

                    nScaled = context.AddIntrinsic(Intrinsic.X86Mulps, nScaled, scale);
                }

                Operand nRnd = context.AddIntrinsic(Intrinsic.X86Roundps, nScaled, Const(X86GetRoundControl(roundMode)));

                Operand nRndMask = context.AddIntrinsic(Intrinsic.X86Cmpps, nRnd, context.VectorZero(), Const((int)CmpCondition.NotLessThanOrEqual));

                Operand nRndMasked = context.AddIntrinsic(Intrinsic.X86Pand, nRnd, nRndMask);

                Operand nInt = context.AddIntrinsic(Intrinsic.X86Cvtps2dq, nRndMasked);

                Operand mask = X86GetAllElements(context, 0x4F000000); // 2.14748365E9f (2147483648)

                Operand res = context.AddIntrinsic(Intrinsic.X86Subps, nRndMasked, mask);

                Operand mask2 = context.AddIntrinsic(Intrinsic.X86Cmpps, res, context.VectorZero(), Const((int)CmpCondition.NotLessThanOrEqual));

                Operand resMasked = context.AddIntrinsic(Intrinsic.X86Pand, res, mask2);

                res = context.AddIntrinsic(Intrinsic.X86Cvtps2dq, resMasked);

                Operand mask3 = context.AddIntrinsic(Intrinsic.X86Cmpps, resMasked, mask, Const((int)CmpCondition.NotLessThan));

                res = context.AddIntrinsic(Intrinsic.X86Pxor, res, mask3);
                res = context.AddIntrinsic(Intrinsic.X86Paddd, res, nInt);

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
                Operand nMask = context.AddIntrinsic(Intrinsic.X86Cmppd, n, n, Const((int)CmpCondition.OrderedQ));

                Operand nScaled = context.AddIntrinsic(Intrinsic.X86Pand, nMask, n);

                if (op is OpCodeSimdShImm fixedOp)
                {
                    int fBits = GetImmShr(fixedOp);

                    // BitConverter.Int64BitsToDouble(fpScaled) == Math.Pow(2d, fBits)
                    long fpScaled = 0x3FF0000000000000L + fBits * 0x10000000000000L;

                    Operand scale = X86GetAllElements(context, fpScaled);

                    nScaled = context.AddIntrinsic(Intrinsic.X86Mulpd, nScaled, scale);
                }

                Operand nRnd = context.AddIntrinsic(Intrinsic.X86Roundpd, nScaled, Const(X86GetRoundControl(roundMode)));

                Operand nRndMask = context.AddIntrinsic(Intrinsic.X86Cmppd, nRnd, context.VectorZero(), Const((int)CmpCondition.NotLessThanOrEqual));

                Operand nRndMasked = context.AddIntrinsic(Intrinsic.X86Pand, nRnd, nRndMask);

                Operand high;

                if (!scalar)
                {
                    high = context.AddIntrinsic(Intrinsic.X86Unpckhpd, nRndMasked, nRndMasked);
                    high = context.AddIntrinsicLong(Intrinsic.X86Cvtsd2si, high);
                }
                else
                {
                    high = Const(0L);
                }

                Operand low = context.AddIntrinsicLong(Intrinsic.X86Cvtsd2si, nRndMasked);

                Operand nInt = EmitVectorLongCreate(context, low, high);

                Operand mask = X86GetAllElements(context, 0x43E0000000000000L); // 9.2233720368547760E18d (9223372036854775808)

                Operand res = context.AddIntrinsic(Intrinsic.X86Subpd, nRndMasked, mask);

                Operand mask2 = context.AddIntrinsic(Intrinsic.X86Cmppd, res, context.VectorZero(), Const((int)CmpCondition.NotLessThanOrEqual));

                Operand resMasked = context.AddIntrinsic(Intrinsic.X86Pand, res, mask2);

                if (!scalar)
                {
                    high = context.AddIntrinsic(Intrinsic.X86Unpckhpd, resMasked, resMasked);
                    high = context.AddIntrinsicLong(Intrinsic.X86Cvtsd2si, high);
                }

                low = context.AddIntrinsicLong(Intrinsic.X86Cvtsd2si, resMasked);

                res = EmitVectorLongCreate(context, low, high);

                Operand mask3 = context.AddIntrinsic(Intrinsic.X86Cmppd, resMasked, mask, Const((int)CmpCondition.NotLessThan));

                res = context.AddIntrinsic(Intrinsic.X86Pxor, res, mask3);
                res = context.AddIntrinsic(Intrinsic.X86Paddq, res, nInt);

                if (scalar)
                {
                    res = context.VectorZeroUpper64(res);
                }

                context.Copy(GetVec(op.Rd), res);
            }
        }

        private static void EmitSse2Scvtf(ArmEmitterContext context, bool scalar)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            Operand res = context.AddIntrinsic(Intrinsic.X86Cvtdq2ps, n);

            if (op is OpCodeSimdShImm fixedOp)
            {
                int fBits = GetImmShr(fixedOp);

                // BitConverter.Int32BitsToSingle(fpScaled) == 1f / MathF.Pow(2f, fBits)
                int fpScaled = 0x3F800000 - fBits * 0x800000;

                Operand scale = X86GetAllElements(context, fpScaled);

                res = context.AddIntrinsic(Intrinsic.X86Mulps, res, scale);
            }

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

        private static void EmitSse2Ucvtf(ArmEmitterContext context, bool scalar)
        {
            OpCodeSimd op = (OpCodeSimd)context.CurrOp;

            Operand n = GetVec(op.Rn);

            Operand res = context.AddIntrinsic(Intrinsic.X86Psrld, n, Const(16));

            res = context.AddIntrinsic(Intrinsic.X86Cvtdq2ps, res);

            Operand mask = X86GetAllElements(context, 0x47800000); // 65536.0f (1 << 16)

            res = context.AddIntrinsic(Intrinsic.X86Mulps, res, mask);

            Operand res2 = context.AddIntrinsic(Intrinsic.X86Pslld, n, Const(16));

            res2 = context.AddIntrinsic(Intrinsic.X86Psrld, res2, Const(16));
            res2 = context.AddIntrinsic(Intrinsic.X86Cvtdq2ps, res2);

            res = context.AddIntrinsic(Intrinsic.X86Addps, res, res2);

            if (op is OpCodeSimdShImm fixedOp)
            {
                int fBits = GetImmShr(fixedOp);

                // BitConverter.Int32BitsToSingle(fpScaled) == 1f / MathF.Pow(2f, fBits)
                int fpScaled = 0x3F800000 - fBits * 0x800000;

                Operand scale = X86GetAllElements(context, fpScaled);

                res = context.AddIntrinsic(Intrinsic.X86Mulps, res, scale);
            }

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

        private static Operand EmitVectorLongExtract(ArmEmitterContext context, int reg, int index, int size)
        {
            OperandType type = size == 3 ? OperandType.I64 : OperandType.I32;

            return context.VectorExtract(type, GetVec(reg), index);
        }

        private static Operand EmitVectorLongCreate(ArmEmitterContext context, Operand low, Operand high)
        {
            Operand vector = context.VectorCreateScalar(low);

            vector = context.VectorInsert(vector, high, 1);

            return vector;
        }
    }
}
