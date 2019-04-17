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

            FPType srcType = (FPType)op.RawOpCode.Extract(8,  2);
            FPType dstType = (FPType)op.RawOpCode.Extract(10, 2);

            bool pass      = op.RawOpCode.Extract(40);
            bool negateB   = op.RawOpCode.Extract(45);
            bool absoluteB = op.RawOpCode.Extract(49);

            pass &= op.RoundingMode == RoundingMode.TowardsNegativeInfinity;

            Operand srcB = context.FPAbsNeg(GetSrcB(context, srcType), absoluteB, negateB);

            if (!pass)
            {
                switch (op.RoundingMode)
                {
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
            }

            srcB = context.FPSaturate(srcB, op.Saturate);

            WriteFP(context, dstType, srcB);

            //TODO: CC.
        }

        public static void F2I(EmitterContext context)
        {
            OpCodeFArith op = (OpCodeFArith)context.CurrOp;

            IntegerType intType = (IntegerType)op.RawOpCode.Extract(8, 2);

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

            srcB = context.FPConvertToS32(srcB);

            //TODO: S/U64, conversion overflow handling.
            if (intType != IntegerType.S32)
            {
                int min = GetIntMin(intType);
                int max = GetIntMax(intType);

                srcB = isSignedInt
                    ? context.IClampS32(srcB, Const(min), Const(max))
                    : context.IClampU32(srcB, Const(min), Const(max));
            }

            Operand dest = GetDest(context);

            context.Copy(dest, srcB);

            //TODO: CC.
        }

        public static void I2F(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            FPType dstType = (FPType)op.RawOpCode.Extract(8, 2);

            IntegerType srcType = (IntegerType)op.RawOpCode.Extract(10, 2);

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

            //TODO: CC.
        }

        public static void I2I(EmitterContext context)
        {
            OpCodeAlu op = (OpCodeAlu)context.CurrOp;

            IntegerType dstType = (IntegerType)op.RawOpCode.Extract(8,  2);
            IntegerType srcType = (IntegerType)op.RawOpCode.Extract(10, 2);

            if (srcType == IntegerType.U64 || dstType == IntegerType.U64)
            {
                //TODO: Warning. This instruction doesn't support 64-bits integers
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

                int min = GetIntMin(dstType);
                int max = GetIntMax(dstType);

                srcB = dstIsSignedInt
                    ? context.IClampS32(srcB, Const(min), Const(max))
                    : context.IClampU32(srcB, Const(min), Const(max));
            }

            context.Copy(GetDest(context), srcB);

            //TODO: CC.
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
            else
            {
                //TODO.
            }
        }
    }
}