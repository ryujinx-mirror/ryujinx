using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;
using static Ryujinx.Graphics.Shader.Instructions.InstEmitAluHelper;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void F2F(EmitterContext context)
        {
            OpCodeFArith op = (OpCodeFArith)context.CurrOp;

            FPType dstType = (FPType)op.RawOpCode.Extract(8,  2);
            FPType srcType = (FPType)op.RawOpCode.Extract(10, 2);

            bool round     = op.RawOpCode.Extract(42);
            bool negateB   = op.RawOpCode.Extract(45);
            bool absoluteB = op.RawOpCode.Extract(49);

            Operand srcB = context.FPAbsNeg(GetSrcB(context, srcType), absoluteB, negateB, srcType.ToInstFPType());

            if (round && srcType == dstType)
            {
                switch (op.RoundingMode)
                {
                    case RoundingMode.ToNearest:
                        srcB = context.FPRound(srcB, srcType.ToInstFPType());
                        break;

                    case RoundingMode.TowardsNegativeInfinity:
                        srcB = context.FPFloor(srcB, srcType.ToInstFPType());
                        break;

                    case RoundingMode.TowardsPositiveInfinity:
                        srcB = context.FPCeiling(srcB, srcType.ToInstFPType());
                        break;

                    case RoundingMode.TowardsZero:
                        srcB = context.FPTruncate(srcB, srcType.ToInstFPType());
                        break;
                }
            }

            // We don't need to handle conversions between FP16 <-> FP32
            // since we do FP16 operations as FP32 directly.
            // FP16 <-> FP64 conversions are invalid.
            if (srcType == FPType.FP32 && dstType == FPType.FP64)
            {
                srcB = context.FP32ConvertToFP64(srcB);
            }
            else if (srcType == FPType.FP64 && dstType == FPType.FP32)
            {
                srcB = context.FP64ConvertToFP32(srcB);
            }

            srcB = context.FPSaturate(srcB, op.Saturate, dstType.ToInstFPType());

            WriteFP(context, dstType, srcB);

            // TODO: CC.
        }

        public static void F2I(EmitterContext context)
        {
            OpCodeFArith op = (OpCodeFArith)context.CurrOp;

            IntegerType intType = (IntegerType)op.RawOpCode.Extract(8, 2);

            if (intType == IntegerType.U64)
            {
                context.Config.GpuAccessor.Log("Unimplemented 64-bits F2I.");

                return;
            }

            bool isSmallInt = intType <= IntegerType.U16;

            FPType floatType = (FPType)op.RawOpCode.Extract(10, 2);

            bool isSignedInt = op.RawOpCode.Extract(12);
            bool negateB     = op.RawOpCode.Extract(45);
            bool absoluteB   = op.RawOpCode.Extract(49);

            if (isSignedInt)
            {
                intType |= IntegerType.S8;
            }

            Operand srcB = context.FPAbsNeg(GetSrcB(context, floatType), absoluteB, negateB);

            switch (op.RoundingMode)
            {
                case RoundingMode.ToNearest:
                    srcB = context.FPRound(srcB);
                    break;

                case RoundingMode.TowardsNegativeInfinity:
                    srcB = context.FPFloor(srcB);
                    break;

                case RoundingMode.TowardsPositiveInfinity:
                    srcB = context.FPCeiling(srcB);
                    break;

                case RoundingMode.TowardsZero:
                    srcB = context.FPTruncate(srcB);
                    break;
            }

            if (!isSignedInt)
            {
                // Negative float to uint cast is undefined, so we clamp
                // the value before conversion.
                srcB = context.FPMaximum(srcB, ConstF(0));
            }

            srcB = isSignedInt
                ? context.FPConvertToS32(srcB)
                : context.FPConvertToU32(srcB);

            if (isSmallInt)
            {
                int min = (int)GetIntMin(intType);
                int max = (int)GetIntMax(intType);

                srcB = isSignedInt
                    ? context.IClampS32(srcB, Const(min), Const(max))
                    : context.IClampU32(srcB, Const(min), Const(max));
            }

            Operand dest = GetDest(context);

            context.Copy(dest, srcB);

            // TODO: CC.
        }

        public static void I2F(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            FPType dstType = (FPType)op.RawOpCode.Extract(8, 2);

            IntegerType srcType = (IntegerType)op.RawOpCode.Extract(10, 2);

            // TODO: Handle S/U64.

            bool isSmallInt = srcType <= IntegerType.U16;

            bool isSignedInt = op.RawOpCode.Extract(13);
            bool negateB     = op.RawOpCode.Extract(45);
            bool absoluteB   = op.RawOpCode.Extract(49);

            Operand srcB = context.IAbsNeg(GetSrcB(context), absoluteB, negateB);

            if (isSmallInt)
            {
                int size = srcType == IntegerType.U16 ? 16 : 8;

                srcB = isSignedInt
                    ? context.BitfieldExtractS32(srcB, Const(op.ByteSelection * 8), Const(size))
                    : context.BitfieldExtractU32(srcB, Const(op.ByteSelection * 8), Const(size));
            }

            srcB = isSignedInt
                ? context.IConvertS32ToFP(srcB)
                : context.IConvertU32ToFP(srcB);

            WriteFP(context, dstType, srcB);

            // TODO: CC.
        }

        public static void I2I(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            IntegerType dstType = (IntegerType)op.RawOpCode.Extract(8,  2);
            IntegerType srcType = (IntegerType)op.RawOpCode.Extract(10, 2);

            if (srcType == IntegerType.U64 || dstType == IntegerType.U64)
            {
                context.Config.GpuAccessor.Log("Invalid I2I encoding.");

                return;
            }

            bool srcIsSmallInt = srcType <= IntegerType.U16;

            bool dstIsSignedInt = op.RawOpCode.Extract(12);
            bool srcIsSignedInt = op.RawOpCode.Extract(13);
            bool negateB        = op.RawOpCode.Extract(45);
            bool absoluteB      = op.RawOpCode.Extract(49);

            Operand srcB = GetSrcB(context);

            if (srcIsSmallInt)
            {
                int size = srcType == IntegerType.U16 ? 16 : 8;

                srcB = srcIsSignedInt
                    ? context.BitfieldExtractS32(srcB, Const(op.ByteSelection * 8), Const(size))
                    : context.BitfieldExtractU32(srcB, Const(op.ByteSelection * 8), Const(size));
            }

            srcB = context.IAbsNeg(srcB, absoluteB, negateB);

            if (op.Saturate)
            {
                if (dstIsSignedInt)
                {
                    dstType |= IntegerType.S8;
                }

                int min = (int)GetIntMin(dstType);
                int max = (int)GetIntMax(dstType);

                srcB = dstIsSignedInt
                    ? context.IClampS32(srcB, Const(min), Const(max))
                    : context.IClampU32(srcB, Const(min), Const(max));
            }

            context.Copy(GetDest(context), srcB);

            // TODO: CC.
        }

        private static void WriteFP(EmitterContext context, FPType type, Operand srcB)
        {
            Operand dest = GetDest(context);

            if (type == FPType.FP32)
            {
                context.Copy(dest, srcB);
            }
            else if (type == FPType.FP16)
            {
                context.Copy(dest, context.PackHalf2x16(srcB, ConstF(0)));
            }
            else /* if (type == FPType.FP64) */
            {
                Operand dest2 = GetDest2(context);

                context.Copy(dest, context.UnpackDouble2x32Low(srcB));
                context.Copy(dest2, context.UnpackDouble2x32High(srcB));
            }
        }
    }
}