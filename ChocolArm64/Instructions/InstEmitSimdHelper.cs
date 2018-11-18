using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace ChocolArm64.Instructions
{
    static class InstEmitSimdHelper
    {
        public static readonly Type[] IntTypesPerSizeLog2 = new Type[]
        {
            typeof(sbyte),
            typeof(short),
            typeof(int),
            typeof(long)
        };

        public static readonly Type[] UIntTypesPerSizeLog2 = new Type[]
        {
            typeof(byte),
            typeof(ushort),
            typeof(uint),
            typeof(ulong)
        };

        public static readonly Type[] VectorIntTypesPerSizeLog2 = new Type[]
        {
            typeof(Vector128<sbyte>),
            typeof(Vector128<short>),
            typeof(Vector128<int>),
            typeof(Vector128<long>)
        };

        public static readonly Type[] VectorUIntTypesPerSizeLog2 = new Type[]
        {
            typeof(Vector128<byte>),
            typeof(Vector128<ushort>),
            typeof(Vector128<uint>),
            typeof(Vector128<ulong>)
        };

        [Flags]
        public enum OperFlags
        {
            Rd = 1 << 0,
            Rn = 1 << 1,
            Rm = 1 << 2,
            Ra = 1 << 3,

            RnRm   = Rn | Rm,
            RdRn   = Rd | Rn,
            RaRnRm = Ra | Rn | Rm,
            RdRnRm = Rd | Rn | Rm
        }

        public static int GetImmShl(OpCodeSimdShImm64 op)
        {
            return op.Imm - (8 << op.Size);
        }

        public static int GetImmShr(OpCodeSimdShImm64 op)
        {
            return (8 << (op.Size + 1)) - op.Imm;
        }

        public static void EmitSse2Op(ILEmitterCtx context, string name)
        {
            EmitSseOp(context, name, typeof(Sse2));
        }

        public static void EmitSse41Op(ILEmitterCtx context, string name)
        {
            EmitSseOp(context, name, typeof(Sse41));
        }

        public static void EmitSse42Op(ILEmitterCtx context, string name)
        {
            EmitSseOp(context, name, typeof(Sse42));
        }

        private static void EmitSseOp(ILEmitterCtx context, string name, Type type)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            EmitLdvecWithSignedCast(context, op.Rn, op.Size);

            Type baseType = VectorIntTypesPerSizeLog2[op.Size];

            if (op is OpCodeSimdReg64 binOp)
            {
                EmitLdvecWithSignedCast(context, binOp.Rm, op.Size);

                context.EmitCall(type.GetMethod(name, new Type[] { baseType, baseType }));
            }
            else
            {
                context.EmitCall(type.GetMethod(name, new Type[] { baseType }));
            }

            EmitStvecWithSignedCast(context, op.Rd, op.Size);

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void EmitLdvecWithSignedCast(ILEmitterCtx context, int reg, int size)
        {
            context.EmitLdvec(reg);

            switch (size)
            {
                case 0: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleToSByte)); break;
                case 1: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleToInt16)); break;
                case 2: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleToInt32)); break;
                case 3: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleToInt64)); break;

                default: throw new ArgumentOutOfRangeException(nameof(size));
            }
        }

        public static void EmitLdvecWithCastToDouble(ILEmitterCtx context, int reg)
        {
            context.EmitLdvec(reg);

            VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleToDouble));
        }

        public static void EmitStvecWithCastFromDouble(ILEmitterCtx context, int reg)
        {
            VectorHelper.EmitCall(context, nameof(VectorHelper.VectorDoubleToSingle));

            context.EmitStvec(reg);
        }

        public static void EmitLdvecWithUnsignedCast(ILEmitterCtx context, int reg, int size)
        {
            context.EmitLdvec(reg);

            switch (size)
            {
                case 0: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleToByte));   break;
                case 1: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleToUInt16)); break;
                case 2: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleToUInt32)); break;
                case 3: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleToUInt64)); break;

                default: throw new ArgumentOutOfRangeException(nameof(size));
            }
        }

        public static void EmitStvecWithSignedCast(ILEmitterCtx context, int reg, int size)
        {
            switch (size)
            {
                case 0: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSByteToSingle)); break;
                case 1: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorInt16ToSingle)); break;
                case 2: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorInt32ToSingle)); break;
                case 3: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorInt64ToSingle)); break;

                default: throw new ArgumentOutOfRangeException(nameof(size));
            }

            context.EmitStvec(reg);
        }

        public static void EmitStvecWithUnsignedCast(ILEmitterCtx context, int reg, int size)
        {
            switch (size)
            {
                case 0: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorByteToSingle));   break;
                case 1: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorUInt16ToSingle)); break;
                case 2: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorUInt32ToSingle)); break;
                case 3: VectorHelper.EmitCall(context, nameof(VectorHelper.VectorUInt64ToSingle)); break;

                default: throw new ArgumentOutOfRangeException(nameof(size));
            }

            context.EmitStvec(reg);
        }

        public static void EmitScalarSseOrSse2OpF(ILEmitterCtx context, string name)
        {
            EmitSseOrSse2OpF(context, name, true);
        }

        public static void EmitVectorSseOrSse2OpF(ILEmitterCtx context, string name)
        {
            EmitSseOrSse2OpF(context, name, false);
        }

        public static void EmitSseOrSse2OpF(ILEmitterCtx context, string name, bool scalar)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            void Ldvec(int reg)
            {
                context.EmitLdvec(reg);

                if (sizeF == 1)
                {
                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleToDouble));
                }
            }

            Ldvec(op.Rn);

            Type type;
            Type baseType;

            if (sizeF == 0)
            {
                type     = typeof(Sse);
                baseType = typeof(Vector128<float>);
            }
            else /* if (sizeF == 1) */
            {
                type     = typeof(Sse2);
                baseType = typeof(Vector128<double>);
            }

            if (op is OpCodeSimdReg64 binOp)
            {
                Ldvec(binOp.Rm);

                context.EmitCall(type.GetMethod(name, new Type[] { baseType, baseType }));
            }
            else
            {
                context.EmitCall(type.GetMethod(name, new Type[] { baseType }));
            }

            if (sizeF == 1)
            {
                VectorHelper.EmitCall(context, nameof(VectorHelper.VectorDoubleToSingle));
            }

            context.EmitStvec(op.Rd);

            if (scalar)
            {
                if (sizeF == 0)
                {
                    EmitVectorZero32_128(context, op.Rd);
                }
                else /* if (sizeF == 1) */
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void EmitUnaryMathCall(ILEmitterCtx context, string name)
        {
            IOpCodeSimd64 op = (IOpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            MethodInfo mthdInfo;

            if (sizeF == 0)
            {
                mthdInfo = typeof(MathF).GetMethod(name, new Type[] { typeof(float) });
            }
            else /* if (sizeF == 1) */
            {
                mthdInfo = typeof(Math).GetMethod(name, new Type[] { typeof(double) });
            }

            context.EmitCall(mthdInfo);
        }

        public static void EmitBinaryMathCall(ILEmitterCtx context, string name)
        {
            IOpCodeSimd64 op = (IOpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            MethodInfo mthdInfo;

            if (sizeF == 0)
            {
                mthdInfo = typeof(MathF).GetMethod(name, new Type[] { typeof(float), typeof(float) });
            }
            else /* if (sizeF == 1) */
            {
                mthdInfo = typeof(Math).GetMethod(name, new Type[] { typeof(double), typeof(double) });
            }

            context.EmitCall(mthdInfo);
        }

        public static void EmitRoundMathCall(ILEmitterCtx context, MidpointRounding roundMode)
        {
            IOpCodeSimd64 op = (IOpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            MethodInfo mthdInfo;

            if (sizeF == 0)
            {
                mthdInfo = typeof(MathF).GetMethod(nameof(MathF.Round), new Type[] { typeof(float), typeof(MidpointRounding) });
            }
            else /* if (sizeF == 1) */
            {
                mthdInfo = typeof(Math).GetMethod(nameof(Math.Round), new Type[] { typeof(double), typeof(MidpointRounding) });
            }

            context.EmitLdc_I4((int)roundMode);

            context.EmitCall(mthdInfo);
        }

        public static void EmitUnarySoftFloatCall(ILEmitterCtx context, string name)
        {
            IOpCodeSimd64 op = (IOpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            MethodInfo mthdInfo;

            if (sizeF == 0)
            {
                mthdInfo = typeof(SoftFloat).GetMethod(name, new Type[] { typeof(float) });
            }
            else /* if (sizeF == 1) */
            {
                mthdInfo = typeof(SoftFloat).GetMethod(name, new Type[] { typeof(double) });
            }

            context.EmitCall(mthdInfo);
        }

        public static void EmitSoftFloatCall(ILEmitterCtx context, string name)
        {
            IOpCodeSimd64 op = (IOpCodeSimd64)context.CurrOp;

            Type type = (op.Size & 1) == 0
                ? typeof(SoftFloat32)
                : typeof(SoftFloat64);

            context.EmitLdarg(TranslatedSub.StateArgIdx);

            context.EmitCall(type, name);
        }

        public static void EmitScalarBinaryOpByElemF(ILEmitterCtx context, Action emit)
        {
            OpCodeSimdRegElemF64 op = (OpCodeSimdRegElemF64)context.CurrOp;

            EmitScalarOpByElemF(context, emit, op.Index, ternary: false);
        }

        public static void EmitScalarTernaryOpByElemF(ILEmitterCtx context, Action emit)
        {
            OpCodeSimdRegElemF64 op = (OpCodeSimdRegElemF64)context.CurrOp;

            EmitScalarOpByElemF(context, emit, op.Index, ternary: true);
        }

        public static void EmitScalarOpByElemF(ILEmitterCtx context, Action emit, int elem, bool ternary)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            int sizeF = op.Size & 1;

            if (ternary)
            {
                EmitVectorExtractF(context, op.Rd, 0, sizeF);
            }

            EmitVectorExtractF(context, op.Rn, 0,    sizeF);
            EmitVectorExtractF(context, op.Rm, elem, sizeF);

            emit();

            EmitScalarSetF(context, op.Rd, sizeF);
        }

        public static void EmitScalarUnaryOpSx(ILEmitterCtx context, Action emit)
        {
            EmitScalarOp(context, emit, OperFlags.Rn, true);
        }

        public static void EmitScalarBinaryOpSx(ILEmitterCtx context, Action emit)
        {
            EmitScalarOp(context, emit, OperFlags.RnRm, true);
        }

        public static void EmitScalarUnaryOpZx(ILEmitterCtx context, Action emit)
        {
            EmitScalarOp(context, emit, OperFlags.Rn, false);
        }

        public static void EmitScalarBinaryOpZx(ILEmitterCtx context, Action emit)
        {
            EmitScalarOp(context, emit, OperFlags.RnRm, false);
        }

        public static void EmitScalarTernaryOpZx(ILEmitterCtx context, Action emit)
        {
            EmitScalarOp(context, emit, OperFlags.RdRnRm, false);
        }

        public static void EmitScalarOp(ILEmitterCtx context, Action emit, OperFlags opers, bool signed)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            bool rd = (opers & OperFlags.Rd) != 0;
            bool rn = (opers & OperFlags.Rn) != 0;
            bool rm = (opers & OperFlags.Rm) != 0;

            if (rd)
            {
                EmitVectorExtract(context, op.Rd, 0, op.Size, signed);
            }

            if (rn)
            {
                EmitVectorExtract(context, op.Rn, 0, op.Size, signed);
            }

            if (rm)
            {
                EmitVectorExtract(context, ((OpCodeSimdReg64)op).Rm, 0, op.Size, signed);
            }

            emit();

            EmitScalarSet(context, op.Rd, op.Size);
        }

        public static void EmitScalarUnaryOpF(ILEmitterCtx context, Action emit)
        {
            EmitScalarOpF(context, emit, OperFlags.Rn);
        }

        public static void EmitScalarBinaryOpF(ILEmitterCtx context, Action emit)
        {
            EmitScalarOpF(context, emit, OperFlags.RnRm);
        }

        public static void EmitScalarTernaryRaOpF(ILEmitterCtx context, Action emit)
        {
            EmitScalarOpF(context, emit, OperFlags.RaRnRm);
        }

        public static void EmitScalarOpF(ILEmitterCtx context, Action emit, OperFlags opers)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            bool ra = (opers & OperFlags.Ra) != 0;
            bool rn = (opers & OperFlags.Rn) != 0;
            bool rm = (opers & OperFlags.Rm) != 0;

            if (ra)
            {
                EmitVectorExtractF(context, ((OpCodeSimdReg64)op).Ra, 0, sizeF);
            }

            if (rn)
            {
                EmitVectorExtractF(context, op.Rn, 0, sizeF);
            }

            if (rm)
            {
                EmitVectorExtractF(context, ((OpCodeSimdReg64)op).Rm, 0, sizeF);
            }

            emit();

            EmitScalarSetF(context, op.Rd, sizeF);
        }

        public static void EmitVectorUnaryOpF(ILEmitterCtx context, Action emit)
        {
            EmitVectorOpF(context, emit, OperFlags.Rn);
        }

        public static void EmitVectorBinaryOpF(ILEmitterCtx context, Action emit)
        {
            EmitVectorOpF(context, emit, OperFlags.RnRm);
        }

        public static void EmitVectorTernaryOpF(ILEmitterCtx context, Action emit)
        {
            EmitVectorOpF(context, emit, OperFlags.RdRnRm);
        }

        public static void EmitVectorOpF(ILEmitterCtx context, Action emit, OperFlags opers)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int sizeF = op.Size & 1;

            int bytes = op.GetBitsCount() >> 3;
            int elems = bytes >> sizeF + 2;

            bool rd = (opers & OperFlags.Rd) != 0;
            bool rn = (opers & OperFlags.Rn) != 0;
            bool rm = (opers & OperFlags.Rm) != 0;

            for (int index = 0; index < elems; index++)
            {
                if (rd)
                {
                    EmitVectorExtractF(context, op.Rd, index, sizeF);
                }

                if (rn)
                {
                    EmitVectorExtractF(context, op.Rn, index, sizeF);
                }

                if (rm)
                {
                    EmitVectorExtractF(context, ((OpCodeSimdReg64)op).Rm, index, sizeF);
                }

                emit();

                EmitVectorInsertF(context, op.Rd, index, sizeF);
            }

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void EmitVectorBinaryOpByElemF(ILEmitterCtx context, Action emit)
        {
            OpCodeSimdRegElemF64 op = (OpCodeSimdRegElemF64)context.CurrOp;

            EmitVectorOpByElemF(context, emit, op.Index, ternary: false);
        }

        public static void EmitVectorTernaryOpByElemF(ILEmitterCtx context, Action emit)
        {
            OpCodeSimdRegElemF64 op = (OpCodeSimdRegElemF64)context.CurrOp;

            EmitVectorOpByElemF(context, emit, op.Index, ternary: true);
        }

        public static void EmitVectorOpByElemF(ILEmitterCtx context, Action emit, int elem, bool ternary)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            int sizeF = op.Size & 1;

            int bytes = op.GetBitsCount() >> 3;
            int elems = bytes >> sizeF + 2;

            for (int index = 0; index < elems; index++)
            {
                if (ternary)
                {
                    EmitVectorExtractF(context, op.Rd, index, sizeF);
                }

                EmitVectorExtractF(context, op.Rn, index, sizeF);
                EmitVectorExtractF(context, op.Rm, elem,  sizeF);

                emit();

                EmitVectorInsertTmpF(context, index, sizeF);
            }

            context.EmitLdvectmp();
            context.EmitStvec(op.Rd);

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void EmitVectorUnaryOpSx(ILEmitterCtx context, Action emit)
        {
            EmitVectorOp(context, emit, OperFlags.Rn, true);
        }

        public static void EmitVectorBinaryOpSx(ILEmitterCtx context, Action emit)
        {
            EmitVectorOp(context, emit, OperFlags.RnRm, true);
        }

        public static void EmitVectorTernaryOpSx(ILEmitterCtx context, Action emit)
        {
            EmitVectorOp(context, emit, OperFlags.RdRnRm, true);
        }

        public static void EmitVectorUnaryOpZx(ILEmitterCtx context, Action emit)
        {
            EmitVectorOp(context, emit, OperFlags.Rn, false);
        }

        public static void EmitVectorBinaryOpZx(ILEmitterCtx context, Action emit)
        {
            EmitVectorOp(context, emit, OperFlags.RnRm, false);
        }

        public static void EmitVectorTernaryOpZx(ILEmitterCtx context, Action emit)
        {
            EmitVectorOp(context, emit, OperFlags.RdRnRm, false);
        }

        public static void EmitVectorOp(ILEmitterCtx context, Action emit, OperFlags opers, bool signed)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int bytes = op.GetBitsCount() >> 3;
            int elems = bytes >> op.Size;

            bool rd = (opers & OperFlags.Rd) != 0;
            bool rn = (opers & OperFlags.Rn) != 0;
            bool rm = (opers & OperFlags.Rm) != 0;

            for (int index = 0; index < elems; index++)
            {
                if (rd)
                {
                    EmitVectorExtract(context, op.Rd, index, op.Size, signed);
                }

                if (rn)
                {
                    EmitVectorExtract(context, op.Rn, index, op.Size, signed);
                }

                if (rm)
                {
                    EmitVectorExtract(context, ((OpCodeSimdReg64)op).Rm, index, op.Size, signed);
                }

                emit();

                EmitVectorInsert(context, op.Rd, index, op.Size);
            }

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void EmitVectorBinaryOpByElemSx(ILEmitterCtx context, Action emit)
        {
            OpCodeSimdRegElem64 op = (OpCodeSimdRegElem64)context.CurrOp;

            EmitVectorOpByElem(context, emit, op.Index, false, true);
        }

        public static void EmitVectorBinaryOpByElemZx(ILEmitterCtx context, Action emit)
        {
            OpCodeSimdRegElem64 op = (OpCodeSimdRegElem64)context.CurrOp;

            EmitVectorOpByElem(context, emit, op.Index, false, false);
        }

        public static void EmitVectorTernaryOpByElemZx(ILEmitterCtx context, Action emit)
        {
            OpCodeSimdRegElem64 op = (OpCodeSimdRegElem64)context.CurrOp;

            EmitVectorOpByElem(context, emit, op.Index, true, false);
        }

        public static void EmitVectorOpByElem(ILEmitterCtx context, Action emit, int elem, bool ternary, bool signed)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            int bytes = op.GetBitsCount() >> 3;
            int elems = bytes >> op.Size;

            EmitVectorExtract(context, op.Rm, elem, op.Size, signed);
            context.EmitSttmp();

            for (int index = 0; index < elems; index++)
            {
                if (ternary)
                {
                    EmitVectorExtract(context, op.Rd, index, op.Size, signed);
                }

                EmitVectorExtract(context, op.Rn, index, op.Size, signed);
                context.EmitLdtmp();

                emit();

                EmitVectorInsertTmp(context, index, op.Size);
            }

            context.EmitLdvectmp();
            context.EmitStvec(op.Rd);

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void EmitVectorImmUnaryOp(ILEmitterCtx context, Action emit)
        {
            EmitVectorImmOp(context, emit, false);
        }

        public static void EmitVectorImmBinaryOp(ILEmitterCtx context, Action emit)
        {
            EmitVectorImmOp(context, emit, true);
        }

        public static void EmitVectorImmOp(ILEmitterCtx context, Action emit, bool binary)
        {
            OpCodeSimdImm64 op = (OpCodeSimdImm64)context.CurrOp;

            int bytes = op.GetBitsCount() >> 3;
            int elems = bytes >> op.Size;

            for (int index = 0; index < elems; index++)
            {
                if (binary)
                {
                    EmitVectorExtractZx(context, op.Rd, index, op.Size);
                }

                context.EmitLdc_I8(op.Imm);

                emit();

                EmitVectorInsert(context, op.Rd, index, op.Size);
            }

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void EmitVectorWidenRmBinaryOpSx(ILEmitterCtx context, Action emit)
        {
            EmitVectorWidenRmBinaryOp(context, emit, true);
        }

        public static void EmitVectorWidenRmBinaryOpZx(ILEmitterCtx context, Action emit)
        {
            EmitVectorWidenRmBinaryOp(context, emit, false);
        }

        public static void EmitVectorWidenRmBinaryOp(ILEmitterCtx context, Action emit, bool signed)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            int elems = 8 >> op.Size;

            int part = op.RegisterSize == RegisterSize.Simd128 ? elems : 0;

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtract(context, op.Rn,        index, op.Size + 1, signed);
                EmitVectorExtract(context, op.Rm, part + index, op.Size,     signed);

                emit();

                EmitVectorInsertTmp(context, index, op.Size + 1);
            }

            context.EmitLdvectmp();
            context.EmitStvec(op.Rd);
        }

        public static void EmitVectorWidenRnRmBinaryOpSx(ILEmitterCtx context, Action emit)
        {
            EmitVectorWidenRnRmOp(context, emit, false, true);
        }

        public static void EmitVectorWidenRnRmBinaryOpZx(ILEmitterCtx context, Action emit)
        {
            EmitVectorWidenRnRmOp(context, emit, false, false);
        }

        public static void EmitVectorWidenRnRmTernaryOpSx(ILEmitterCtx context, Action emit)
        {
            EmitVectorWidenRnRmOp(context, emit, true, true);
        }

        public static void EmitVectorWidenRnRmTernaryOpZx(ILEmitterCtx context, Action emit)
        {
            EmitVectorWidenRnRmOp(context, emit, true, false);
        }

        public static void EmitVectorWidenRnRmOp(ILEmitterCtx context, Action emit, bool ternary, bool signed)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            int elems = 8 >> op.Size;

            int part = op.RegisterSize == RegisterSize.Simd128 ? elems : 0;

            for (int index = 0; index < elems; index++)
            {
                if (ternary)
                {
                    EmitVectorExtract(context, op.Rd, index, op.Size + 1, signed);
                }

                EmitVectorExtract(context, op.Rn, part + index, op.Size, signed);
                EmitVectorExtract(context, op.Rm, part + index, op.Size, signed);

                emit();

                EmitVectorInsertTmp(context, index, op.Size + 1);
            }

            context.EmitLdvectmp();
            context.EmitStvec(op.Rd);
        }

        public static void EmitVectorPairwiseOpSx(ILEmitterCtx context, Action emit)
        {
            EmitVectorPairwiseOp(context, emit, true);
        }

        public static void EmitVectorPairwiseOpZx(ILEmitterCtx context, Action emit)
        {
            EmitVectorPairwiseOp(context, emit, false);
        }

        public static void EmitVectorPairwiseOp(ILEmitterCtx context, Action emit, bool signed)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            int words = op.GetBitsCount() >> 4;
            int pairs = words >> op.Size;

            for (int index = 0; index < pairs; index++)
            {
                int idx = index << 1;

                EmitVectorExtract(context, op.Rn, idx,     op.Size, signed);
                EmitVectorExtract(context, op.Rn, idx + 1, op.Size, signed);

                emit();

                EmitVectorExtract(context, op.Rm, idx,     op.Size, signed);
                EmitVectorExtract(context, op.Rm, idx + 1, op.Size, signed);

                emit();

                EmitVectorInsertTmp(context, pairs + index, op.Size);
                EmitVectorInsertTmp(context,         index, op.Size);
            }

            context.EmitLdvectmp();
            context.EmitStvec(op.Rd);

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void EmitVectorPairwiseOpF(ILEmitterCtx context, Action emit)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            int sizeF = op.Size & 1;

            int words = op.GetBitsCount() >> 4;
            int pairs = words >> sizeF + 2;

            for (int index = 0; index < pairs; index++)
            {
                int idx = index << 1;

                EmitVectorExtractF(context, op.Rn, idx,     sizeF);
                EmitVectorExtractF(context, op.Rn, idx + 1, sizeF);

                emit();

                EmitVectorExtractF(context, op.Rm, idx,     sizeF);
                EmitVectorExtractF(context, op.Rm, idx + 1, sizeF);

                emit();

                EmitVectorInsertTmpF(context, pairs + index, sizeF);
                EmitVectorInsertTmpF(context,         index, sizeF);
            }

            context.EmitLdvectmp();
            context.EmitStvec(op.Rd);

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        [Flags]
        public enum SaturatingFlags
        {
            Scalar = 1 << 0,
            Signed = 1 << 1,

            Add = 1 << 2,
            Sub = 1 << 3,

            Accumulate = 1 << 4,

            ScalarSx = Scalar | Signed,
            ScalarZx = Scalar,

            VectorSx = Signed,
            VectorZx = 0
        }

        public static void EmitScalarSaturatingUnaryOpSx(ILEmitterCtx context, Action emit)
        {
            EmitSaturatingUnaryOpSx(context, emit, SaturatingFlags.ScalarSx);
        }

        public static void EmitVectorSaturatingUnaryOpSx(ILEmitterCtx context, Action emit)
        {
            EmitSaturatingUnaryOpSx(context, emit, SaturatingFlags.VectorSx);
        }

        public static void EmitSaturatingUnaryOpSx(ILEmitterCtx context, Action emit, SaturatingFlags flags)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            bool scalar = (flags & SaturatingFlags.Scalar) != 0;

            int bytes = op.GetBitsCount() >> 3;
            int elems = !scalar ? bytes >> op.Size : 1;

            if (scalar)
            {
                EmitVectorZeroLowerTmp(context);
            }

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtractSx(context, op.Rn, index, op.Size);

                emit();

                if (op.Size <= 2)
                {
                    EmitSatQ(context, op.Size, true, true);
                }
                else /* if (op.Size == 3) */
                {
                    EmitUnarySignedSatQAbsOrNeg(context);
                }

                EmitVectorInsertTmp(context, index, op.Size);
            }

            context.EmitLdvectmp();
            context.EmitStvec(op.Rd);

            if ((op.RegisterSize == RegisterSize.Simd64) || scalar)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void EmitScalarSaturatingBinaryOpSx(ILEmitterCtx context, SaturatingFlags flags)
        {
            EmitSaturatingBinaryOp(context, () => { }, SaturatingFlags.ScalarSx | flags);
        }

        public static void EmitScalarSaturatingBinaryOpZx(ILEmitterCtx context, SaturatingFlags flags)
        {
            EmitSaturatingBinaryOp(context, () => { }, SaturatingFlags.ScalarZx | flags);
        }

        public static void EmitVectorSaturatingBinaryOpSx(ILEmitterCtx context, SaturatingFlags flags)
        {
            EmitSaturatingBinaryOp(context, () => { }, SaturatingFlags.VectorSx | flags);
        }

        public static void EmitVectorSaturatingBinaryOpZx(ILEmitterCtx context, SaturatingFlags flags)
        {
            EmitSaturatingBinaryOp(context, () => { }, SaturatingFlags.VectorZx | flags);
        }

        public static void EmitSaturatingBinaryOp(ILEmitterCtx context, Action emit, SaturatingFlags flags)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            bool scalar = (flags & SaturatingFlags.Scalar) != 0;
            bool signed = (flags & SaturatingFlags.Signed) != 0;

            bool add = (flags & SaturatingFlags.Add) != 0;
            bool sub = (flags & SaturatingFlags.Sub) != 0;

            bool accumulate = (flags & SaturatingFlags.Accumulate) != 0;

            int bytes = op.GetBitsCount() >> 3;
            int elems = !scalar ? bytes >> op.Size : 1;

            if (scalar)
            {
                EmitVectorZeroLowerTmp(context);
            }

            if (add || sub)
            {
                for (int index = 0; index < elems; index++)
                {
                    EmitVectorExtract(context,                   op.Rn,  index, op.Size, signed);
                    EmitVectorExtract(context, ((OpCodeSimdReg64)op).Rm, index, op.Size, signed);

                    if (op.Size <= 2)
                    {
                        context.Emit(add ? OpCodes.Add : OpCodes.Sub);

                        EmitSatQ(context, op.Size, true, signed);
                    }
                    else /* if (op.Size == 3) */
                    {
                        if (add)
                        {
                            EmitBinarySatQAdd(context, signed);
                        }
                        else /* if (sub) */
                        {
                            EmitBinarySatQSub(context, signed);
                        }
                    }

                    EmitVectorInsertTmp(context, index, op.Size);
                }
            }
            else if (accumulate)
            {
                for (int index = 0; index < elems; index++)
                {
                    EmitVectorExtract(context, op.Rn, index, op.Size, !signed);
                    EmitVectorExtract(context, op.Rd, index, op.Size,  signed);

                    if (op.Size <= 2)
                    {
                        context.Emit(OpCodes.Add);

                        EmitSatQ(context, op.Size, true, signed);
                    }
                    else /* if (op.Size == 3) */
                    {
                        EmitBinarySatQAccumulate(context, signed);
                    }

                    EmitVectorInsertTmp(context, index, op.Size);
                }
            }
            else
            {
                for (int index = 0; index < elems; index++)
                {
                    EmitVectorExtract(context,                   op.Rn,  index, op.Size, signed);
                    EmitVectorExtract(context, ((OpCodeSimdReg64)op).Rm, index, op.Size, signed);

                    emit();

                    EmitSatQ(context, op.Size, true, signed);

                    EmitVectorInsertTmp(context, index, op.Size);
                }
            }

            context.EmitLdvectmp();
            context.EmitStvec(op.Rd);

            if ((op.RegisterSize == RegisterSize.Simd64) || scalar)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        [Flags]
        public enum SaturatingNarrowFlags
        {
            Scalar    = 1 << 0,
            SignedSrc = 1 << 1,
            SignedDst = 1 << 2,

            ScalarSxSx = Scalar | SignedSrc | SignedDst,
            ScalarSxZx = Scalar | SignedSrc,
            ScalarZxZx = Scalar,

            VectorSxSx = SignedSrc | SignedDst,
            VectorSxZx = SignedSrc,
            VectorZxZx = 0
        }

        public static void EmitSaturatingNarrowOp(ILEmitterCtx context, SaturatingNarrowFlags flags)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            bool scalar    = (flags & SaturatingNarrowFlags.Scalar)    != 0;
            bool signedSrc = (flags & SaturatingNarrowFlags.SignedSrc) != 0;
            bool signedDst = (flags & SaturatingNarrowFlags.SignedDst) != 0;

            int elems = !scalar ? 8 >> op.Size : 1;

            int part = !scalar && (op.RegisterSize == RegisterSize.Simd128) ? elems : 0;

            if (scalar)
            {
                EmitVectorZeroLowerTmp(context);
            }

            if (part != 0)
            {
                context.EmitLdvec(op.Rd);
                context.EmitStvectmp();
            }

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtract(context, op.Rn, index, op.Size + 1, signedSrc);

                EmitSatQ(context, op.Size, signedSrc, signedDst);

                EmitVectorInsertTmp(context, part + index, op.Size);
            }

            context.EmitLdvectmp();
            context.EmitStvec(op.Rd);

            if (part == 0)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        // TSrc (16bit, 32bit, 64bit; signed, unsigned) > TDst (8bit, 16bit, 32bit; signed, unsigned).
        public static void EmitSatQ(
            ILEmitterCtx context,
            int  sizeDst,
            bool signedSrc,
            bool signedDst)
        {
            if (sizeDst > 2)
            {
                throw new ArgumentOutOfRangeException(nameof(sizeDst));
            }

            context.EmitLdc_I4(sizeDst);
            context.EmitLdarg(TranslatedSub.StateArgIdx);

            if (signedSrc)
            {
                SoftFallback.EmitCall(context, signedDst
                    ? nameof(SoftFallback.SignedSrcSignedDstSatQ)
                    : nameof(SoftFallback.SignedSrcUnsignedDstSatQ));
            }
            else
            {
                SoftFallback.EmitCall(context, signedDst
                    ? nameof(SoftFallback.UnsignedSrcSignedDstSatQ)
                    : nameof(SoftFallback.UnsignedSrcUnsignedDstSatQ));
            }
        }

        // TSrc (64bit) == TDst (64bit); signed.
        public static void EmitUnarySignedSatQAbsOrNeg(ILEmitterCtx context)
        {
            if (((OpCodeSimd64)context.CurrOp).Size < 3)
            {
                throw new InvalidOperationException();
            }

            context.EmitLdarg(TranslatedSub.StateArgIdx);

            SoftFallback.EmitCall(context, nameof(SoftFallback.UnarySignedSatQAbsOrNeg));
        }

        // TSrcs (64bit) == TDst (64bit); signed, unsigned.
        public static void EmitBinarySatQAdd(ILEmitterCtx context, bool signed)
        {
            if (((OpCodeSimdReg64)context.CurrOp).Size < 3)
            {
                throw new InvalidOperationException();
            }

            context.EmitLdarg(TranslatedSub.StateArgIdx);

            SoftFallback.EmitCall(context, signed
                ? nameof(SoftFallback.BinarySignedSatQAdd)
                : nameof(SoftFallback.BinaryUnsignedSatQAdd));
        }

        // TSrcs (64bit) == TDst (64bit); signed, unsigned.
        public static void EmitBinarySatQSub(ILEmitterCtx context, bool signed)
        {
            if (((OpCodeSimdReg64)context.CurrOp).Size < 3)
            {
                throw new InvalidOperationException();
            }

            context.EmitLdarg(TranslatedSub.StateArgIdx);

            SoftFallback.EmitCall(context, signed
                ? nameof(SoftFallback.BinarySignedSatQSub)
                : nameof(SoftFallback.BinaryUnsignedSatQSub));
        }

        // TSrcs (64bit) == TDst (64bit); signed, unsigned.
        public static void EmitBinarySatQAccumulate(ILEmitterCtx context, bool signed)
        {
            if (((OpCodeSimd64)context.CurrOp).Size < 3)
            {
                throw new InvalidOperationException();
            }

            context.EmitLdarg(TranslatedSub.StateArgIdx);

            SoftFallback.EmitCall(context, signed
                ? nameof(SoftFallback.BinarySignedSatQAcc)
                : nameof(SoftFallback.BinaryUnsignedSatQAcc));
        }

        public static void EmitScalarSet(ILEmitterCtx context, int reg, int size)
        {
            EmitVectorZeroAll(context, reg);
            EmitVectorInsert(context, reg, 0, size);
        }

        public static void EmitScalarSetF(ILEmitterCtx context, int reg, int size)
        {
            if (Optimizations.UseSse41 && size == 0)
            {
                //If the type is float, we can perform insertion and
                //zero the upper bits with a single instruction (INSERTPS);
                context.EmitLdvec(reg);

                VectorHelper.EmitCall(context, nameof(VectorHelper.Sse41VectorInsertScalarSingle));

                context.EmitStvec(reg);
            }
            else
            {
                EmitVectorZeroAll(context, reg);
                EmitVectorInsertF(context, reg, 0, size);
            }
        }

        public static void EmitVectorExtractSx(ILEmitterCtx context, int reg, int index, int size)
        {
            EmitVectorExtract(context, reg, index, size, true);
        }

        public static void EmitVectorExtractZx(ILEmitterCtx context, int reg, int index, int size)
        {
            EmitVectorExtract(context, reg, index, size, false);
        }

        public static void EmitVectorExtract(ILEmitterCtx context, int reg, int index, int size, bool signed)
        {
            ThrowIfInvalid(index, size);

            context.EmitLdvec(reg);
            context.EmitLdc_I4(index);
            context.EmitLdc_I4(size);

            VectorHelper.EmitCall(context, signed
                ? nameof(VectorHelper.VectorExtractIntSx)
                : nameof(VectorHelper.VectorExtractIntZx));
        }

        public static void EmitVectorExtractF(ILEmitterCtx context, int reg, int index, int size)
        {
            ThrowIfInvalidF(index, size);

            context.EmitLdvec(reg);
            context.EmitLdc_I4(index);

            if (size == 0)
            {
                VectorHelper.EmitCall(context, nameof(VectorHelper.VectorExtractSingle));
            }
            else if (size == 1)
            {
                VectorHelper.EmitCall(context, nameof(VectorHelper.VectorExtractDouble));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
        }

        public static void EmitVectorZeroAll(ILEmitterCtx context, int reg)
        {
            if (Optimizations.UseSse)
            {
                VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));

                context.EmitStvec(reg);
            }
            else
            {
                EmitVectorZeroLower(context, reg);
                EmitVectorZeroUpper(context, reg);
            }
        }

        public static void EmitVectorZeroLower(ILEmitterCtx context, int reg)
        {
            EmitVectorInsert(context, reg, 0, 3, 0);
        }

        public static void EmitVectorZeroLowerTmp(ILEmitterCtx context)
        {
            if (Optimizations.UseSse)
            {
                context.EmitLdvectmp();
                VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.MoveHighToLow)));

                context.EmitStvectmp();
            }
            else
            {
                EmitVectorInsertTmp(context, 0, 3, 0);
            }
        }

        public static void EmitVectorZeroUpper(ILEmitterCtx context, int reg)
        {
            if (Optimizations.UseSse)
            {
                //TODO: Use Sse2.MoveScalar once it is fixed,
                //as of the time of writing it just crashes the JIT (SDK 2.1.500).

                /*Type[] typesMov = new Type[] { typeof(Vector128<ulong>) };

                EmitLdvecWithUnsignedCast(context, reg, 3);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.MoveScalar), typesMov));

                EmitStvecWithUnsignedCast(context, reg, 3);*/

                context.EmitLdvec(reg);
                VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));

                context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.MoveLowToHigh)));

                context.EmitStvec(reg);
            }
            else
            {
                EmitVectorInsert(context, reg, 1, 3, 0);
            }
        }

        public static void EmitVectorZero32_128(ILEmitterCtx context, int reg)
        {
            if (!Sse.IsSupported)
            {
                throw new PlatformNotSupportedException();
            }

            VectorHelper.EmitCall(context, nameof(VectorHelper.VectorSingleZero));
            context.EmitLdvec(reg);

            context.EmitCall(typeof(Sse).GetMethod(nameof(Sse.MoveScalar)));

            context.EmitStvec(reg);
        }

        public static void EmitVectorInsert(ILEmitterCtx context, int reg, int index, int size)
        {
            ThrowIfInvalid(index, size);

            context.EmitLdvec(reg);
            context.EmitLdc_I4(index);
            context.EmitLdc_I4(size);

            VectorHelper.EmitCall(context, nameof(VectorHelper.VectorInsertInt));

            context.EmitStvec(reg);
        }

        public static void EmitVectorInsertTmp(ILEmitterCtx context, int index, int size)
        {
            ThrowIfInvalid(index, size);

            context.EmitLdvectmp();
            context.EmitLdc_I4(index);
            context.EmitLdc_I4(size);

            VectorHelper.EmitCall(context, nameof(VectorHelper.VectorInsertInt));

            context.EmitStvectmp();
        }

        public static void EmitVectorInsert(ILEmitterCtx context, int reg, int index, int size, long value)
        {
            ThrowIfInvalid(index, size);

            context.EmitLdc_I8(value);
            context.EmitLdvec(reg);
            context.EmitLdc_I4(index);
            context.EmitLdc_I4(size);

            VectorHelper.EmitCall(context, nameof(VectorHelper.VectorInsertInt));

            context.EmitStvec(reg);
        }

        public static void EmitVectorInsertTmp(ILEmitterCtx context, int index, int size, long value)
        {
            ThrowIfInvalid(index, size);

            context.EmitLdc_I8(value);
            context.EmitLdvectmp();
            context.EmitLdc_I4(index);
            context.EmitLdc_I4(size);

            VectorHelper.EmitCall(context, nameof(VectorHelper.VectorInsertInt));

            context.EmitStvectmp();
        }

        public static void EmitVectorInsertF(ILEmitterCtx context, int reg, int index, int size)
        {
            ThrowIfInvalidF(index, size);

            context.EmitLdvec(reg);
            context.EmitLdc_I4(index);

            if (size == 0)
            {
                VectorHelper.EmitCall(context, nameof(VectorHelper.VectorInsertSingle));
            }
            else if (size == 1)
            {
                VectorHelper.EmitCall(context, nameof(VectorHelper.VectorInsertDouble));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            context.EmitStvec(reg);
        }

        public static void EmitVectorInsertTmpF(ILEmitterCtx context, int index, int size)
        {
            ThrowIfInvalidF(index, size);

            context.EmitLdvectmp();
            context.EmitLdc_I4(index);

            if (size == 0)
            {
                VectorHelper.EmitCall(context, nameof(VectorHelper.VectorInsertSingle));
            }
            else if (size == 1)
            {
                VectorHelper.EmitCall(context, nameof(VectorHelper.VectorInsertDouble));
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            context.EmitStvectmp();
        }

        private static void ThrowIfInvalid(int index, int size)
        {
            if ((uint)size > 3u)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            if ((uint)index >= 16u >> size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        private static void ThrowIfInvalidF(int index, int size)
        {
            if ((uint)size > 1u)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            if ((uint)index >= 4u >> size)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}
