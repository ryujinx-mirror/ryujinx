using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System;
using System.Diagnostics;
using System.Reflection;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitSimdHelper;
using static ARMeilleure.Instructions.InstEmitSimdHelper32;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit32
    {
        private static int FlipVdBits(int vd, bool lowBit)
        {
            if (lowBit)
            {
                // Move the low bit to the top.
                return ((vd & 0x1) << 4) | (vd >> 1);
            }
            else
            {
                // Move the high bit to the bottom.
                return ((vd & 0xf) << 1) | (vd >> 4);
            }
        }

        private static Operand EmitSaturateFloatToInt(ArmEmitterContext context, Operand op1, bool unsigned)
        {
            MethodInfo info;

            if (op1.Type == OperandType.FP64)
            {
                info = unsigned
                    ? typeof(SoftFallback).GetMethod(nameof(SoftFallback.SatF64ToU32))
                    : typeof(SoftFallback).GetMethod(nameof(SoftFallback.SatF64ToS32));
            }
            else
            {
                info = unsigned
                    ? typeof(SoftFallback).GetMethod(nameof(SoftFallback.SatF32ToU32))
                    : typeof(SoftFallback).GetMethod(nameof(SoftFallback.SatF32ToS32));
            }

            return context.Call(info, op1);
        }

        public static void Vcvt_V(ArmEmitterContext context)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            bool unsigned = (op.Opc & 1) != 0;
            bool toInteger = (op.Opc & 2) != 0;
            OperandType floatSize = (op.Size == 2) ? OperandType.FP32 : OperandType.FP64;

            if (toInteger)
            {
                if (Optimizations.UseSse41)
                {
                    EmitSse41ConvertVector32(context, FPRoundingMode.TowardsZero, !unsigned);
                }
                else
                {
                    EmitVectorUnaryOpF32(context, (op1) =>
                    {
                        return EmitSaturateFloatToInt(context, op1, unsigned);
                    });
                }
            }
            else
            {
                if (Optimizations.UseSse2)
                {
                    EmitVectorUnaryOpSimd32(context, (n) =>
                    {
                        if (unsigned)
                        {
                            Operand mask = X86GetAllElements(context, 0x47800000);

                            Operand res = context.AddIntrinsic(Intrinsic.X86Psrld, n, Const(16));
                            res = context.AddIntrinsic(Intrinsic.X86Cvtdq2ps, res);
                            res = context.AddIntrinsic(Intrinsic.X86Mulps, res, mask);

                            Operand res2 = context.AddIntrinsic(Intrinsic.X86Pslld, n, Const(16));
                            res2 = context.AddIntrinsic(Intrinsic.X86Psrld, res2, Const(16));
                            res2 = context.AddIntrinsic(Intrinsic.X86Cvtdq2ps, res2);

                            return context.AddIntrinsic(Intrinsic.X86Addps, res, res2);
                        }
                        else
                        {
                            return context.AddIntrinsic(Intrinsic.X86Cvtdq2ps, n);
                        }
                    });
                }
                else
                {
                    if (unsigned)
                    {
                        EmitVectorUnaryOpZx32(context, (op1) => EmitFPConvert(context, op1, floatSize, false));
                    }
                    else
                    {
                        EmitVectorUnaryOpSx32(context, (op1) => EmitFPConvert(context, op1, floatSize, true));
                    }
                }
            }
        }

        public static void Vcvt_FD(ArmEmitterContext context)
        {
            OpCode32SimdS op = (OpCode32SimdS)context.CurrOp;

            int vm = op.Vm;
            int vd;
            if (op.Size == 3)
            {
                vd = FlipVdBits(op.Vd, false);
                // Double to single.
                Operand fp = ExtractScalar(context, OperandType.FP64, vm);

                Operand res = context.ConvertToFP(OperandType.FP32, fp);

                InsertScalar(context, vd, res);
            }
            else
            {
                vd = FlipVdBits(op.Vd, true);
                // Single to double.
                Operand fp = ExtractScalar(context, OperandType.FP32, vm);

                Operand res = context.ConvertToFP(OperandType.FP64, fp);

                InsertScalar(context, vd, res);
            }
        }

        // VCVT (floating-point to integer, floating-point) | VCVT (integer to floating-point, floating-point).
        public static void Vcvt_FI(ArmEmitterContext context)
        {
            OpCode32SimdCvtFI op = (OpCode32SimdCvtFI)context.CurrOp;

            bool toInteger = (op.Opc2 & 0b100) != 0;

            OperandType floatSize = op.RegisterSize == RegisterSize.Int64 ? OperandType.FP64 : OperandType.FP32;

            if (toInteger)
            {
                bool unsigned = (op.Opc2 & 1) == 0;
                bool roundWithFpscr = op.Opc != 1;

                if (!roundWithFpscr && Optimizations.UseSse41)
                {
                    EmitSse41ConvertInt32(context, FPRoundingMode.TowardsZero, !unsigned);
                }
                else
                {
                    Operand toConvert = ExtractScalar(context, floatSize, op.Vm);

                    Operand asInteger;

                    // TODO: Fast Path.
                    if (roundWithFpscr)
                    {
                        MethodInfo info;

                        if (floatSize == OperandType.FP64)
                        {
                            info = unsigned
                                ? typeof(SoftFallback).GetMethod(nameof(SoftFallback.DoubleToUInt32))
                                : typeof(SoftFallback).GetMethod(nameof(SoftFallback.DoubleToInt32));
                        }
                        else
                        {
                            info = unsigned
                                ? typeof(SoftFallback).GetMethod(nameof(SoftFallback.FloatToUInt32))
                                : typeof(SoftFallback).GetMethod(nameof(SoftFallback.FloatToInt32));
                        }

                        asInteger = context.Call(info, toConvert);
                    }
                    else
                    {
                        // Round towards zero.
                        asInteger = EmitSaturateFloatToInt(context, toConvert, unsigned);
                    }

                    InsertScalar(context, op.Vd, asInteger);
                }
            }
            else
            {
                bool unsigned = op.Opc == 0;

                Operand toConvert = ExtractScalar(context, OperandType.I32, op.Vm);

                Operand asFloat = EmitFPConvert(context, toConvert, floatSize, !unsigned);

                InsertScalar(context, op.Vd, asFloat);
            }
        }

        private static Operand EmitRoundMathCall(ArmEmitterContext context, MidpointRounding roundMode, Operand n)
        {
            IOpCode32Simd op = (IOpCode32Simd)context.CurrOp;

            string name = nameof(Math.Round);

            MethodInfo info = (op.Size & 1) == 0
                ? typeof(MathF).GetMethod(name, new Type[] { typeof(float),  typeof(MidpointRounding) })
                : typeof(Math). GetMethod(name, new Type[] { typeof(double), typeof(MidpointRounding) });

            return context.Call(info, n, Const((int)roundMode));
        }

        private static FPRoundingMode RMToRoundMode(int rm)
        {
            FPRoundingMode roundMode;
            switch (rm)
            {
                case 0b01:
                    roundMode = FPRoundingMode.ToNearest;
                    break;
                case 0b10:
                    roundMode = FPRoundingMode.TowardsPlusInfinity;
                    break;
                case 0b11:
                    roundMode = FPRoundingMode.TowardsMinusInfinity;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rm));
            }
            return roundMode;
        }

        // VCVTA/M/N/P (floating-point).
        public static void Vcvt_RM(ArmEmitterContext context)
        {
            OpCode32SimdCvtFI op = (OpCode32SimdCvtFI)context.CurrOp; // toInteger == true (opCode<18> == 1 => Opc2<2> == 1).

            OperandType floatSize = op.RegisterSize == RegisterSize.Int64 ? OperandType.FP64 : OperandType.FP32;

            bool unsigned = op.Opc == 0;
            int rm = op.Opc2 & 3;

            if (Optimizations.UseSse41 && rm != 0b00)
            {
                EmitSse41ConvertInt32(context, RMToRoundMode(rm), !unsigned);
            }
            else
            {
                Operand toConvert = ExtractScalar(context, floatSize, op.Vm);

                switch (rm)
                {
                    case 0b00: // Away
                        toConvert = EmitRoundMathCall(context, MidpointRounding.AwayFromZero, toConvert);
                        break;
                    case 0b01: // Nearest
                        toConvert = EmitRoundMathCall(context, MidpointRounding.ToEven, toConvert);
                        break;
                    case 0b10: // Towards positive infinity
                        toConvert = EmitUnaryMathCall(context, nameof(Math.Ceiling), toConvert);
                        break;
                    case 0b11: // Towards negative infinity
                        toConvert = EmitUnaryMathCall(context, nameof(Math.Floor), toConvert);
                        break;
                }

                Operand asInteger;

                asInteger = EmitSaturateFloatToInt(context, toConvert, unsigned);

                InsertScalar(context, op.Vd, asInteger);
            }
        }

        // VRINTA/M/N/P (floating-point).
        public static void Vrint_RM(ArmEmitterContext context)
        {
            OpCode32SimdS op = (OpCode32SimdS)context.CurrOp;

            OperandType floatSize = op.RegisterSize == RegisterSize.Int64 ? OperandType.FP64 : OperandType.FP32;

            int rm = op.Opc2 & 3;

            if (Optimizations.UseSse2 && rm != 0b00)
            {
                EmitScalarUnaryOpSimd32(context, (m) =>
                {
                    Intrinsic inst = (op.Size & 1) == 0 ? Intrinsic.X86Roundss : Intrinsic.X86Roundsd;

                    FPRoundingMode roundMode = RMToRoundMode(rm);

                    return context.AddIntrinsic(inst, m, Const(X86GetRoundControl(roundMode)));
                });
            }
            else
            {
                Operand toConvert = ExtractScalar(context, floatSize, op.Vm);

                switch (rm)
                {
                    case 0b00: // Away
                        toConvert = EmitRoundMathCall(context, MidpointRounding.AwayFromZero, toConvert);
                        break;
                    case 0b01: // Nearest
                        toConvert = EmitRoundMathCall(context, MidpointRounding.ToEven, toConvert);
                        break;
                    case 0b10: // Towards positive infinity
                        toConvert = EmitUnaryMathCall(context, nameof(Math.Ceiling), toConvert);
                        break;
                    case 0b11: // Towards negative infinity
                        toConvert = EmitUnaryMathCall(context, nameof(Math.Floor), toConvert);
                        break;
                }

                InsertScalar(context, op.Vd, toConvert);
            }
        }

        // VRINTZ (floating-point).
        public static void Vrint_Z(ArmEmitterContext context)
        {
            OpCode32SimdS op = (OpCode32SimdS)context.CurrOp;

            if (Optimizations.UseSse2)
            {
                EmitScalarUnaryOpSimd32(context, (m) =>
                {
                    Intrinsic inst = (op.Size & 1) == 0 ? Intrinsic.X86Roundss : Intrinsic.X86Roundsd;
                    return context.AddIntrinsic(inst, m, Const(X86GetRoundControl(FPRoundingMode.TowardsZero)));
                });
            }
            else
            {
                EmitScalarUnaryOpF32(context, (op1) => EmitUnaryMathCall(context, nameof(Math.Truncate), op1));
            }
        }

        // VRINTX (floating-point).
        public static void Vrintx_S(ArmEmitterContext context)
        {
            OpCode32SimdS op = (OpCode32SimdS)context.CurrOp;

            bool doubleSize = (op.Size & 1) == 1;
            string methodName = doubleSize ? nameof(SoftFallback.Round) : nameof(SoftFallback.RoundF);

            EmitScalarUnaryOpF32(context, (op1) =>
            {
                MethodInfo info = typeof(SoftFallback).GetMethod(methodName);
                return context.Call(info, op1);
            });
        }

        private static Operand EmitFPConvert(ArmEmitterContext context, Operand value, OperandType type, bool signed)
        {
            Debug.Assert(value.Type == OperandType.I32 || value.Type == OperandType.I64);

            if (signed)
            {
                return context.ConvertToFP(type, value);
            }
            else
            {
                return context.ConvertToFPUI(type, value);
            }
        }

        private static void EmitSse41ConvertInt32(ArmEmitterContext context, FPRoundingMode roundMode, bool signed)
        {
            // A port of the similar round function in InstEmitSimdCvt.
            OpCode32SimdCvtFI op = (OpCode32SimdCvtFI)context.CurrOp;

            bool doubleSize = (op.Size & 1) != 0;
            int shift = doubleSize ? 1 : 2;
            Operand n = GetVecA32(op.Vm >> shift);
            n = EmitSwapScalar(context, n, op.Vm, doubleSize);

            if (!doubleSize)
            {
                Operand nRes = context.AddIntrinsic(Intrinsic.X86Cmpss, n, n, Const((int)CmpCondition.OrderedQ));
                nRes = context.AddIntrinsic(Intrinsic.X86Pand, nRes, n);

                nRes = context.AddIntrinsic(Intrinsic.X86Roundss, nRes, Const(X86GetRoundControl(roundMode)));

                Operand zero = context.VectorZero();

                Operand nCmp;
                Operand nIntOrLong2 = null;
                if (!signed)
                {
                    nCmp = context.AddIntrinsic(Intrinsic.X86Cmpss, nRes, zero, Const((int)CmpCondition.NotLessThanOrEqual));
                    nRes = context.AddIntrinsic(Intrinsic.X86Pand, nRes, nCmp);
                }

                int fpMaxVal = 0x4F000000; // 2.14748365E9f (2147483648)

                Operand fpMaxValMask = X86GetScalar(context, fpMaxVal);

                Operand nIntOrLong = context.AddIntrinsicInt(Intrinsic.X86Cvtss2si, nRes);

                if (!signed)
                {
                    nRes = context.AddIntrinsic(Intrinsic.X86Subss, nRes, fpMaxValMask);

                    nCmp = context.AddIntrinsic(Intrinsic.X86Cmpss, nRes, zero, Const((int)CmpCondition.NotLessThanOrEqual));
                    nRes = context.AddIntrinsic(Intrinsic.X86Pand, nRes, nCmp);

                    nIntOrLong2 = context.AddIntrinsicInt(Intrinsic.X86Cvtss2si, nRes);
                }

                nRes = context.AddIntrinsic(Intrinsic.X86Cmpss, nRes, fpMaxValMask, Const((int)CmpCondition.NotLessThan));

                Operand nInt = context.AddIntrinsicInt(Intrinsic.X86Cvtsi2si, nRes);

                Operand dRes;
                if (signed)
                {
                    dRes = context.BitwiseExclusiveOr(nIntOrLong, nInt);
                }
                else
                {
                    dRes = context.BitwiseExclusiveOr(nIntOrLong2, nInt);
                    dRes = context.Add(dRes, nIntOrLong);
                }

                InsertScalar(context, op.Vd, dRes);
            }
            else
            {
                Operand nRes = context.AddIntrinsic(Intrinsic.X86Cmpsd, n, n, Const((int)CmpCondition.OrderedQ));
                nRes = context.AddIntrinsic(Intrinsic.X86Pand, nRes, n);

                nRes = context.AddIntrinsic(Intrinsic.X86Roundsd, nRes, Const(X86GetRoundControl(roundMode)));

                Operand zero = context.VectorZero();

                Operand nCmp;
                Operand nIntOrLong2 = null;
                if (!signed)
                {
                    nCmp = context.AddIntrinsic(Intrinsic.X86Cmpsd, nRes, zero, Const((int)CmpCondition.NotLessThanOrEqual));
                    nRes = context.AddIntrinsic(Intrinsic.X86Pand, nRes, nCmp);
                }

                long fpMaxVal = 0x41E0000000000000L; // 2147483648.0000000d (2147483648)

                Operand fpMaxValMask = X86GetScalar(context, fpMaxVal);

                Operand nIntOrLong = context.AddIntrinsicInt(Intrinsic.X86Cvtsd2si, nRes);

                if (!signed)
                {
                    nRes = context.AddIntrinsic(Intrinsic.X86Subsd, nRes, fpMaxValMask);

                    nCmp = context.AddIntrinsic(Intrinsic.X86Cmpsd, nRes, zero, Const((int)CmpCondition.NotLessThanOrEqual));
                    nRes = context.AddIntrinsic(Intrinsic.X86Pand, nRes, nCmp);

                    nIntOrLong2 = context.AddIntrinsicInt(Intrinsic.X86Cvtsd2si, nRes);
                }

                nRes = context.AddIntrinsic(Intrinsic.X86Cmpsd, nRes, fpMaxValMask, Const((int)CmpCondition.NotLessThan));

                Operand nLong = context.AddIntrinsicLong(Intrinsic.X86Cvtsi2si, nRes);
                nLong = context.ConvertI64ToI32(nLong);

                Operand dRes;
                if (signed)
                {
                    dRes = context.BitwiseExclusiveOr(nIntOrLong, nLong);
                }
                else
                {
                    dRes = context.BitwiseExclusiveOr(nIntOrLong2, nLong);
                    dRes = context.Add(dRes, nIntOrLong);
                }

                InsertScalar(context, op.Vd, dRes);
            }
        }

        private static void EmitSse41ConvertVector32(ArmEmitterContext context, FPRoundingMode roundMode, bool signed)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            EmitVectorUnaryOpSimd32(context, (n) =>
            {
                int sizeF = op.Size & 1;

                if (sizeF == 0)
                {
                    Operand nRes = context.AddIntrinsic(Intrinsic.X86Cmpps, n, n, Const((int)CmpCondition.OrderedQ));
                    nRes = context.AddIntrinsic(Intrinsic.X86Pand, nRes, n);

                    nRes = context.AddIntrinsic(Intrinsic.X86Roundps, nRes, Const(X86GetRoundControl(roundMode)));

                    Operand zero = context.VectorZero();
                    Operand nCmp;
                    if (!signed)
                    {
                        nCmp = context.AddIntrinsic(Intrinsic.X86Cmpps, nRes, zero, Const((int)CmpCondition.NotLessThanOrEqual));
                        nRes = context.AddIntrinsic(Intrinsic.X86Pand, nRes, nCmp);
                    }

                    Operand fpMaxValMask = X86GetAllElements(context, 0x4F000000); // 2.14748365E9f (2147483648)

                    Operand nInt = context.AddIntrinsic(Intrinsic.X86Cvtps2dq, nRes);
                    Operand nInt2 = null;
                    if (!signed)
                    {
                        nRes = context.AddIntrinsic(Intrinsic.X86Subps, nRes, fpMaxValMask);

                        nCmp = context.AddIntrinsic(Intrinsic.X86Cmpps, nRes, zero, Const((int)CmpCondition.NotLessThanOrEqual));
                        nRes = context.AddIntrinsic(Intrinsic.X86Pand, nRes, nCmp);

                        nInt2 = context.AddIntrinsic(Intrinsic.X86Cvtps2dq, nRes);
                    }

                    nRes = context.AddIntrinsic(Intrinsic.X86Cmpps, nRes, fpMaxValMask, Const((int)CmpCondition.NotLessThan));

                    if (signed)
                    {
                        return context.AddIntrinsic(Intrinsic.X86Pxor, nInt, nRes);
                    }
                    else
                    {
                        Operand dRes = context.AddIntrinsic(Intrinsic.X86Pxor, nInt2, nRes);
                        return context.AddIntrinsic(Intrinsic.X86Paddd, dRes, nInt);
                    }
                }
                else /* if (sizeF == 1) */
                {
                    Operand nRes = context.AddIntrinsic(Intrinsic.X86Cmppd, n, n, Const((int)CmpCondition.OrderedQ));
                    nRes = context.AddIntrinsic(Intrinsic.X86Pand, nRes, n);

                    nRes = context.AddIntrinsic(Intrinsic.X86Roundpd, nRes, Const(X86GetRoundControl(roundMode)));

                    Operand zero = context.VectorZero();
                    Operand nCmp;
                    if (!signed)
                    {
                        nCmp = context.AddIntrinsic(Intrinsic.X86Cmppd, nRes, zero, Const((int)CmpCondition.NotLessThanOrEqual));
                        nRes = context.AddIntrinsic(Intrinsic.X86Pand, nRes, nCmp);
                    }

                    Operand fpMaxValMask = X86GetAllElements(context, 0x43E0000000000000L); // 9.2233720368547760E18d (9223372036854775808)

                    Operand nLong = InstEmit.EmitSse2CvtDoubleToInt64OpF(context, nRes, false);
                    Operand nLong2 = null;
                    if (!signed)
                    {
                        nRes = context.AddIntrinsic(Intrinsic.X86Subpd, nRes, fpMaxValMask);

                        nCmp = context.AddIntrinsic(Intrinsic.X86Cmppd, nRes, zero, Const((int)CmpCondition.NotLessThanOrEqual));
                        nRes = context.AddIntrinsic(Intrinsic.X86Pand, nRes, nCmp);

                        nLong2 = InstEmit.EmitSse2CvtDoubleToInt64OpF(context, nRes, false);
                    }

                    nRes = context.AddIntrinsic(Intrinsic.X86Cmppd, nRes, fpMaxValMask, Const((int)CmpCondition.NotLessThan));

                    if (signed)
                    {
                        return context.AddIntrinsic(Intrinsic.X86Pxor, nLong, nRes);
                    }
                    else
                    {
                        Operand dRes = context.AddIntrinsic(Intrinsic.X86Pxor, nLong2, nRes);
                        return context.AddIntrinsic(Intrinsic.X86Paddq, dRes, nLong);
                    }
                }
            });
        }
    }
}
