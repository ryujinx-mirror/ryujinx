using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;
using System.Diagnostics;

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
            if (op1.Type == OperandType.FP64)
            {
                if (unsigned)
                {
                    return context.Call(new _U32_F64(SoftFallback.SatF64ToU32), op1);
                }
                else
                {
                    return context.Call(new _S32_F64(SoftFallback.SatF64ToS32), op1);
                }

            }
            else
            {
                if (unsigned)
                {
                    return context.Call(new _U32_F32(SoftFallback.SatF32ToU32), op1);
                }
                else
                {
                    return context.Call(new _S32_F32(SoftFallback.SatF32ToS32), op1);
                }
            }
        }

        public static void Vcvt_V(ArmEmitterContext context)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            bool unsigned = (op.Opc & 1) != 0;
            bool toInteger = (op.Opc & 2) != 0;
            OperandType floatSize = (op.Size == 2) ? OperandType.FP32 : OperandType.FP64;

            if (toInteger)
            {
                EmitVectorUnaryOpF32(context, (op1) =>
                {
                    return EmitSaturateFloatToInt(context, op1, unsigned);
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

        public static void Vcvt_FI(ArmEmitterContext context)
        {
            OpCode32SimdCvtFI op = (OpCode32SimdCvtFI)context.CurrOp;

            bool toInteger = (op.Opc2 & 0b100) != 0;

            OperandType floatSize = op.RegisterSize == RegisterSize.Int64 ? OperandType.FP64 : OperandType.FP32;

            if (toInteger)
            {
                bool unsigned = (op.Opc2 & 1) == 0;
                bool roundWithFpscr = op.Opc != 1;

                Operand toConvert = ExtractScalar(context, floatSize, op.Vm);

                Operand asInteger;

                // TODO: Fast Path.
                if (roundWithFpscr)
                {
                    // These need to get the FPSCR value, so it's worth noting we'd need to do a c# call at some point.
                    if (floatSize == OperandType.FP64)
                    {
                        if (unsigned)
                        {
                            asInteger = context.Call(new _U32_F64(SoftFallback.DoubleToUInt32), toConvert);
                        } 
                        else
                        {
                            asInteger = context.Call(new _S32_F64(SoftFallback.DoubleToInt32), toConvert);
                        }
                    } 
                    else
                    {
                        if (unsigned)
                        {
                            asInteger = context.Call(new _U32_F32(SoftFallback.FloatToUInt32), toConvert);
                        } 
                        else
                        {
                            asInteger = context.Call(new _S32_F32(SoftFallback.FloatToInt32), toConvert);
                        }
                    }
                } 
                else
                {
                    // Round towards zero.
                    asInteger = EmitSaturateFloatToInt(context, toConvert, unsigned);
                }

                InsertScalar(context, op.Vd, asInteger);
            } 
            else
            {
                bool unsigned = op.Opc == 0;

                Operand toConvert = ExtractScalar(context, OperandType.I32, op.Vm);

                Operand asFloat = EmitFPConvert(context, toConvert, floatSize, !unsigned);

                InsertScalar(context, op.Vd, asFloat);
            }
        }

        public static Operand EmitRoundMathCall(ArmEmitterContext context, MidpointRounding roundMode, Operand n)
        {
            IOpCode32Simd op = (IOpCode32Simd)context.CurrOp;

            Delegate dlg;

            if ((op.Size & 1) == 0)
            {
                dlg = new _F32_F32_MidpointRounding(MathF.Round);
            }
            else /* if ((op.Size & 1) == 1) */
            {
                dlg = new _F64_F64_MidpointRounding(Math.Round);
            }

            return context.Call(dlg, n, Const((int)roundMode));
        }

        public static void Vcvt_R(ArmEmitterContext context)
        {
            OpCode32SimdCvtFI op = (OpCode32SimdCvtFI)context.CurrOp;

            OperandType floatSize = op.RegisterSize == RegisterSize.Int64 ? OperandType.FP64 : OperandType.FP32;

            bool unsigned = (op.Opc & 1) == 0;

            Operand toConvert = ExtractScalar(context, floatSize, op.Vm);

            switch (op.Opc2)
            {
                case 0b00: // Away
                    toConvert = EmitRoundMathCall(context, MidpointRounding.AwayFromZero, toConvert);
                    break;
                case 0b01: // Nearest
                    toConvert = EmitRoundMathCall(context, MidpointRounding.ToEven, toConvert);
                    break;
                case 0b10: // Towards positive infinity
                    toConvert = EmitUnaryMathCall(context, MathF.Ceiling, Math.Ceiling, toConvert);
                    break;
                case 0b11: // Towards negative infinity
                    toConvert = EmitUnaryMathCall(context, MathF.Floor, Math.Floor, toConvert);
                    break;
            }

            Operand asInteger;

            asInteger = EmitSaturateFloatToInt(context, toConvert, unsigned);

            InsertScalar(context, op.Vd, asInteger);
        }

        public static void Vrint_RM(ArmEmitterContext context)
        {
            OpCode32SimdCvtFI op = (OpCode32SimdCvtFI)context.CurrOp;

            OperandType floatSize = op.RegisterSize == RegisterSize.Int64 ? OperandType.FP64 : OperandType.FP32;

            Operand toConvert = ExtractScalar(context, floatSize, op.Vm);

            switch (op.Opc2)
            {
                case 0b00: // Away
                    toConvert = EmitRoundMathCall(context, MidpointRounding.AwayFromZero, toConvert);
                    break;
                case 0b01: // Nearest
                    toConvert = EmitRoundMathCall(context, MidpointRounding.ToEven, toConvert);
                    break;
                case 0b10: // Towards positive infinity
                    toConvert = EmitUnaryMathCall(context, MathF.Ceiling, Math.Ceiling, toConvert);
                    break;
                case 0b11: // Towards negative infinity
                    toConvert = EmitUnaryMathCall(context, MathF.Floor, Math.Floor, toConvert);
                    break;
            }

            InsertScalar(context, op.Vd, toConvert);
        }

        public static void Vrint_Z(ArmEmitterContext context)
        {
            EmitScalarUnaryOpF32(context, (op1) => EmitUnaryMathCall(context, MathF.Truncate, Math.Truncate, op1));
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
    }
}
