using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitSimdHelper32;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    using Func2I = Func<Operand, Operand, Operand>;

    static partial class InstEmit32
    {
        public static void Vceq_V(ArmEmitterContext context)
        {
            EmitCmpOpF32(context, SoftFloat32.FPCompareEQFpscr, SoftFloat64.FPCompareEQFpscr, false);
        }

        public static void Vceq_I(ArmEmitterContext context)
        {
            EmitCmpOpI32(context, context.ICompareEqual, context.ICompareEqual, false, false);
        }

        public static void Vceq_Z(ArmEmitterContext context)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            if (op.F)
            {
                EmitCmpOpF32(context, SoftFloat32.FPCompareEQFpscr, SoftFloat64.FPCompareEQFpscr, true);
            }
            else
            {
                EmitCmpOpI32(context, context.ICompareEqual, context.ICompareEqual, true, false);
            }
        }

        public static void Vcge_V(ArmEmitterContext context)
        {
            EmitCmpOpF32(context, SoftFloat32.FPCompareGEFpscr, SoftFloat64.FPCompareGEFpscr, false);
        }

        public static void Vcge_I(ArmEmitterContext context)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            EmitCmpOpI32(context, context.ICompareGreaterOrEqual, context.ICompareGreaterOrEqualUI, false, !op.U);
        }

        public static void Vcge_Z(ArmEmitterContext context)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            if (op.F)
            {
                EmitCmpOpF32(context, SoftFloat32.FPCompareGEFpscr, SoftFloat64.FPCompareGEFpscr, true);
            } 
            else
            {
                EmitCmpOpI32(context, context.ICompareGreaterOrEqual, context.ICompareGreaterOrEqualUI, true, true);
            }
        }

        public static void Vcgt_V(ArmEmitterContext context)
        {
            EmitCmpOpF32(context, SoftFloat32.FPCompareGTFpscr, SoftFloat64.FPCompareGTFpscr, false);
        }

        public static void Vcgt_I(ArmEmitterContext context)
        {
            OpCode32SimdReg op = (OpCode32SimdReg)context.CurrOp;

            EmitCmpOpI32(context, context.ICompareGreater, context.ICompareGreaterUI, false, !op.U);
        }

        public static void Vcgt_Z(ArmEmitterContext context)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            if (op.F)
            {
                EmitCmpOpF32(context, SoftFloat32.FPCompareGTFpscr, SoftFloat64.FPCompareGTFpscr, true);
            }
            else
            {
                EmitCmpOpI32(context, context.ICompareGreater, context.ICompareGreaterUI, true, true);
            }
        }

        public static void Vcle_Z(ArmEmitterContext context)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            if (op.F)
            {
                EmitCmpOpF32(context, SoftFloat32.FPCompareLEFpscr, SoftFloat64.FPCompareLEFpscr, true);
            }
            else
            {
                EmitCmpOpI32(context, context.ICompareLessOrEqual, context.ICompareLessOrEqualUI, true, true);
            }
        }

        public static void Vclt_Z(ArmEmitterContext context)
        {
            OpCode32Simd op = (OpCode32Simd)context.CurrOp;

            if (op.F)
            {
                EmitCmpOpF32(context, SoftFloat32.FPCompareLTFpscr, SoftFloat64.FPCompareLTFpscr, true);
            }
            else
            {
                EmitCmpOpI32(context, context.ICompareLess, context.ICompareLessUI, true, true);
            }
        }

        private static void EmitCmpOpF32(
            ArmEmitterContext context,
            _F32_F32_F32_Bool f32,
            _F64_F64_F64_Bool f64,
            bool zero)
        {
            Operand one = Const(1);
            if (zero)
            {
                EmitVectorUnaryOpF32(context, (m) =>
                {
                    OperandType type = m.Type;

                    if (type == OperandType.FP64)
                    {
                        return context.Call(f64, m, ConstF(0.0), one);
                    }
                    else
                    {
                        return context.Call(f32, m, ConstF(0.0f), one);
                    }
                });
            }
            else
            {
                EmitVectorBinaryOpF32(context, (n, m) =>
                {
                    OperandType type = n.Type;

                    if (type == OperandType.FP64)
                    {
                        return context.Call(f64, n, m, one);
                    }
                    else
                    {
                        return context.Call(f32, n, m, one);
                    }
                });
            }
        }

        private static Operand ZerosOrOnes(ArmEmitterContext context, Operand fromBool, OperandType baseType)
        {
            var ones = (baseType == OperandType.I64) ? Const(-1L) : Const(-1);

            return context.ConditionalSelect(fromBool, ones, Const(baseType, 0L));
        }

        private static void EmitCmpOpI32(
            ArmEmitterContext context,
            Func2I signedOp,
            Func2I unsignedOp,
            bool zero,
            bool signed)
        {
            if (zero)
            {
                if (signed)
                {
                    EmitVectorUnaryOpSx32(context, (m) =>
                    {
                        OperandType type = m.Type;
                        Operand zeroV = (type == OperandType.I64) ? Const(0L) : Const(0);

                        return ZerosOrOnes(context, signedOp(m, zeroV), type);
                    });
                } 
                else
                {
                    EmitVectorUnaryOpZx32(context, (m) =>
                    {
                        OperandType type = m.Type;
                        Operand zeroV = (type == OperandType.I64) ? Const(0L) : Const(0);

                        return ZerosOrOnes(context, unsignedOp(m, zeroV), type);
                    });
                }
            }
            else
            {
                if (signed)
                {
                    EmitVectorBinaryOpSx32(context, (n, m) => ZerosOrOnes(context, signedOp(n, m), n.Type));
                } 
                else
                {
                    EmitVectorBinaryOpZx32(context, (n, m) => ZerosOrOnes(context, unsignedOp(n, m), n.Type));
                }
            }
        }

        public static void Vcmp(ArmEmitterContext context)
        {
            EmitVcmpOrVcmpe(context, false);
        }

        public static void Vcmpe(ArmEmitterContext context)
        {
            EmitVcmpOrVcmpe(context, true);
        }

        private static void EmitVcmpOrVcmpe(ArmEmitterContext context, bool signalNaNs)
        {
            OpCode32SimdS op = (OpCode32SimdS)context.CurrOp;

            bool cmpWithZero = (op.Opc & 2) != 0;
            {
                int fSize = op.Size & 1;
                OperandType type = fSize != 0 ? OperandType.FP64 : OperandType.FP32;

                Operand ne = ExtractScalar(context, type, op.Vd);
                Operand me;

                if (cmpWithZero)
                {
                    me = fSize == 0 ? ConstF(0f) : ConstF(0d);
                }
                else
                {
                    me = ExtractScalar(context, type, op.Vm);
                }

                Delegate dlg = fSize != 0
                    ? (Delegate)new _S32_F64_F64_Bool(SoftFloat64.FPCompare)
                    : (Delegate)new _S32_F32_F32_Bool(SoftFloat32.FPCompare);

                Operand nzcv = context.Call(dlg, ne, me, Const(signalNaNs));

                EmitSetFPSCRFlags(context, nzcv);
            }
        }

        private static void EmitSetFPSCRFlags(ArmEmitterContext context, Operand nzcv)
        {
            Operand Extract(Operand value, int bit)
            {
                if (bit != 0)
                {
                    value = context.ShiftRightUI(value, Const(bit));
                }

                value = context.BitwiseAnd(value, Const(1));

                return value;
            }

            SetFpFlag(context, FPState.VFlag, Extract(nzcv, 0));
            SetFpFlag(context, FPState.CFlag, Extract(nzcv, 1));
            SetFpFlag(context, FPState.ZFlag, Extract(nzcv, 2));
            SetFpFlag(context, FPState.NFlag, Extract(nzcv, 3));
        }
    }
}
