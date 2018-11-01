// https://github.com/intel/ARM_NEON_2_x86_SSE/blob/master/NEON_2_SSE.h

using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;

using static ChocolArm64.Instructions.InstEmitSimdHelper;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        public static void Rshrn_V(ILEmitterCtx context)
        {
            EmitVectorShrImmNarrowOpZx(context, round: true);
        }

        public static void Shl_S(ILEmitterCtx context)
        {
            OpCodeSimdShImm64 op = (OpCodeSimdShImm64)context.CurrOp;

            EmitScalarUnaryOpZx(context, () =>
            {
                context.EmitLdc_I4(GetImmShl(op));

                context.Emit(OpCodes.Shl);
            });
        }

        public static void Shl_V(ILEmitterCtx context)
        {
            OpCodeSimdShImm64 op = (OpCodeSimdShImm64)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0)
            {
                Type[] typesSll = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], typeof(byte) };

                EmitLdvecWithUnsignedCast(context, op.Rn, op.Size);

                context.EmitLdc_I4(GetImmShl(op));
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftLeftLogical), typesSll));

                EmitStvecWithUnsignedCast(context, op.Rd, op.Size);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorUnaryOpZx(context, () =>
                {
                    context.EmitLdc_I4(GetImmShl(op));

                    context.Emit(OpCodes.Shl);
                });
            }
        }

        public static void Shll_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int shift = 8 << op.Size;

            EmitVectorShImmWidenBinaryZx(context, () => context.Emit(OpCodes.Shl), shift);
        }

        public static void Shrn_V(ILEmitterCtx context)
        {
            EmitVectorShrImmNarrowOpZx(context, round: false);
        }

        public static void Sli_V(ILEmitterCtx context)
        {
            OpCodeSimdShImm64 op = (OpCodeSimdShImm64)context.CurrOp;

            int bytes = op.GetBitsCount() >> 3;
            int elems = bytes >> op.Size;

            int shift = GetImmShl(op);

            ulong mask = shift != 0 ? ulong.MaxValue >> (64 - shift) : 0;

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtractZx(context, op.Rn, index, op.Size);

                context.EmitLdc_I4(shift);

                context.Emit(OpCodes.Shl);

                EmitVectorExtractZx(context, op.Rd, index, op.Size);

                context.EmitLdc_I8((long)mask);

                context.Emit(OpCodes.And);
                context.Emit(OpCodes.Or);

                EmitVectorInsert(context, op.Rd, index, op.Size);
            }

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void Sqrshrn_S(ILEmitterCtx context)
        {
            EmitRoundShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.ScalarSxSx);
        }

        public static void Sqrshrn_V(ILEmitterCtx context)
        {
            EmitRoundShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.VectorSxSx);
        }

        public static void Sqrshrun_S(ILEmitterCtx context)
        {
            EmitRoundShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.ScalarSxZx);
        }

        public static void Sqrshrun_V(ILEmitterCtx context)
        {
            EmitRoundShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.VectorSxZx);
        }

        public static void Sqshrn_S(ILEmitterCtx context)
        {
            EmitShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.ScalarSxSx);
        }

        public static void Sqshrn_V(ILEmitterCtx context)
        {
            EmitShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.VectorSxSx);
        }

        public static void Sqshrun_S(ILEmitterCtx context)
        {
            EmitShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.ScalarSxZx);
        }

        public static void Sqshrun_V(ILEmitterCtx context)
        {
            EmitShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.VectorSxZx);
        }

        public static void Srshr_S(ILEmitterCtx context)
        {
            EmitScalarShrImmOpSx(context, ShrImmFlags.Round);
        }

        public static void Srshr_V(ILEmitterCtx context)
        {
            OpCodeSimdShImm64 op = (OpCodeSimdShImm64)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0
                                      && op.Size < 3)
            {
                Type[] typesShs = new Type[] { VectorIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesAdd = new Type[] { VectorIntTypesPerSizeLog2[op.Size], VectorIntTypesPerSizeLog2[op.Size] };

                int shift = GetImmShr(op);
                int eSize = 8 << op.Size;

                EmitLdvecWithSignedCast(context, op.Rn, op.Size);

                context.Emit(OpCodes.Dup);
                context.EmitStvectmp();

                context.EmitLdc_I4(eSize - shift);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftLeftLogical), typesShs));

                context.EmitLdc_I4(eSize - 1);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical), typesShs));

                context.EmitLdvectmp();

                context.EmitLdc_I4(shift);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightArithmetic), typesShs));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesAdd));

                EmitStvecWithSignedCast(context, op.Rd, op.Size);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorShrImmOpSx(context, ShrImmFlags.Round);
            }
        }

        public static void Srsra_S(ILEmitterCtx context)
        {
            EmitScalarShrImmOpSx(context, ShrImmFlags.Round | ShrImmFlags.Accumulate);
        }

        public static void Srsra_V(ILEmitterCtx context)
        {
            OpCodeSimdShImm64 op = (OpCodeSimdShImm64)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0
                                      && op.Size < 3)
            {
                Type[] typesShs = new Type[] { VectorIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesAdd = new Type[] { VectorIntTypesPerSizeLog2[op.Size], VectorIntTypesPerSizeLog2[op.Size] };

                int shift = GetImmShr(op);
                int eSize = 8 << op.Size;

                EmitLdvecWithSignedCast(context, op.Rd, op.Size);
                EmitLdvecWithSignedCast(context, op.Rn, op.Size);

                context.Emit(OpCodes.Dup);
                context.EmitStvectmp();

                context.EmitLdc_I4(eSize - shift);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftLeftLogical), typesShs));

                context.EmitLdc_I4(eSize - 1);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical), typesShs));

                context.EmitLdvectmp();

                context.EmitLdc_I4(shift);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightArithmetic), typesShs));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesAdd));
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesAdd));

                EmitStvecWithSignedCast(context, op.Rd, op.Size);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorShrImmOpSx(context, ShrImmFlags.Round | ShrImmFlags.Accumulate);
            }
        }

        public static void Sshl_V(ILEmitterCtx context)
        {
            EmitVectorShl(context, signed: true);
        }

        public static void Sshll_V(ILEmitterCtx context)
        {
            OpCodeSimdShImm64 op = (OpCodeSimdShImm64)context.CurrOp;

            EmitVectorShImmWidenBinarySx(context, () => context.Emit(OpCodes.Shl), GetImmShl(op));
        }

        public static void Sshr_S(ILEmitterCtx context)
        {
            EmitShrImmOp(context, ShrImmFlags.ScalarSx);
        }

        public static void Sshr_V(ILEmitterCtx context)
        {
            OpCodeSimdShImm64 op = (OpCodeSimdShImm64)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0
                                      && op.Size < 3)
            {
                Type[] typesSra = new Type[] { VectorIntTypesPerSizeLog2[op.Size], typeof(byte) };

                EmitLdvecWithSignedCast(context, op.Rn, op.Size);

                context.EmitLdc_I4(GetImmShr(op));
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightArithmetic), typesSra));

                EmitStvecWithSignedCast(context, op.Rd, op.Size);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitShrImmOp(context, ShrImmFlags.VectorSx);
            }
        }

        public static void Ssra_S(ILEmitterCtx context)
        {
            EmitScalarShrImmOpSx(context, ShrImmFlags.Accumulate);
        }

        public static void Ssra_V(ILEmitterCtx context)
        {
            OpCodeSimdShImm64 op = (OpCodeSimdShImm64)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0
                                      && op.Size < 3)
            {
                Type[] typesSra = new Type[] { VectorIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesAdd = new Type[] { VectorIntTypesPerSizeLog2[op.Size], VectorIntTypesPerSizeLog2[op.Size] };

                EmitLdvecWithSignedCast(context, op.Rd, op.Size);
                EmitLdvecWithSignedCast(context, op.Rn, op.Size);

                context.EmitLdc_I4(GetImmShr(op));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightArithmetic), typesSra));
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesAdd));

                EmitStvecWithSignedCast(context, op.Rd, op.Size);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorShrImmOpSx(context, ShrImmFlags.Accumulate);
            }
        }

        public static void Uqrshrn_S(ILEmitterCtx context)
        {
            EmitRoundShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.ScalarZxZx);
        }

        public static void Uqrshrn_V(ILEmitterCtx context)
        {
            EmitRoundShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.VectorZxZx);
        }

        public static void Uqshrn_S(ILEmitterCtx context)
        {
            EmitShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.ScalarZxZx);
        }

        public static void Uqshrn_V(ILEmitterCtx context)
        {
            EmitShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.VectorZxZx);
        }

        public static void Urshr_S(ILEmitterCtx context)
        {
            EmitScalarShrImmOpZx(context, ShrImmFlags.Round);
        }

        public static void Urshr_V(ILEmitterCtx context)
        {
            OpCodeSimdShImm64 op = (OpCodeSimdShImm64)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0)
            {
                Type[] typesShs = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesAdd = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], VectorUIntTypesPerSizeLog2[op.Size] };

                int shift = GetImmShr(op);
                int eSize = 8 << op.Size;

                EmitLdvecWithUnsignedCast(context, op.Rn, op.Size);

                context.Emit(OpCodes.Dup);
                context.EmitStvectmp();

                context.EmitLdc_I4(eSize - shift);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftLeftLogical), typesShs));

                context.EmitLdc_I4(eSize - 1);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical), typesShs));

                context.EmitLdvectmp();

                context.EmitLdc_I4(shift);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical), typesShs));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesAdd));

                EmitStvecWithUnsignedCast(context, op.Rd, op.Size);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorShrImmOpZx(context, ShrImmFlags.Round);
            }
        }

        public static void Ursra_S(ILEmitterCtx context)
        {
            EmitScalarShrImmOpZx(context, ShrImmFlags.Round | ShrImmFlags.Accumulate);
        }

        public static void Ursra_V(ILEmitterCtx context)
        {
            OpCodeSimdShImm64 op = (OpCodeSimdShImm64)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0)
            {
                Type[] typesShs = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesAdd = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], VectorUIntTypesPerSizeLog2[op.Size] };

                int shift = GetImmShr(op);
                int eSize = 8 << op.Size;

                EmitLdvecWithUnsignedCast(context, op.Rd, op.Size);
                EmitLdvecWithUnsignedCast(context, op.Rn, op.Size);

                context.Emit(OpCodes.Dup);
                context.EmitStvectmp();

                context.EmitLdc_I4(eSize - shift);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftLeftLogical), typesShs));

                context.EmitLdc_I4(eSize - 1);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical), typesShs));

                context.EmitLdvectmp();

                context.EmitLdc_I4(shift);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical), typesShs));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesAdd));
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesAdd));

                EmitStvecWithUnsignedCast(context, op.Rd, op.Size);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorShrImmOpZx(context, ShrImmFlags.Round | ShrImmFlags.Accumulate);
            }
        }

        public static void Ushl_V(ILEmitterCtx context)
        {
            EmitVectorShl(context, signed: false);
        }

        public static void Ushll_V(ILEmitterCtx context)
        {
            OpCodeSimdShImm64 op = (OpCodeSimdShImm64)context.CurrOp;

            EmitVectorShImmWidenBinaryZx(context, () => context.Emit(OpCodes.Shl), GetImmShl(op));
        }

        public static void Ushr_S(ILEmitterCtx context)
        {
            EmitShrImmOp(context, ShrImmFlags.ScalarZx);
        }

        public static void Ushr_V(ILEmitterCtx context)
        {
            OpCodeSimdShImm64 op = (OpCodeSimdShImm64)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0)
            {
                Type[] typesSrl = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], typeof(byte) };

                EmitLdvecWithUnsignedCast(context, op.Rn, op.Size);

                context.EmitLdc_I4(GetImmShr(op));
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical), typesSrl));

                EmitStvecWithUnsignedCast(context, op.Rd, op.Size);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitShrImmOp(context, ShrImmFlags.VectorZx);
            }
        }

        public static void Usra_S(ILEmitterCtx context)
        {
            EmitScalarShrImmOpZx(context, ShrImmFlags.Accumulate);
        }

        public static void Usra_V(ILEmitterCtx context)
        {
            OpCodeSimdShImm64 op = (OpCodeSimdShImm64)context.CurrOp;

            if (Optimizations.UseSse2 && op.Size > 0)
            {
                Type[] typesSrl = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], typeof(byte) };
                Type[] typesAdd = new Type[] { VectorUIntTypesPerSizeLog2[op.Size], VectorUIntTypesPerSizeLog2[op.Size] };

                EmitLdvecWithUnsignedCast(context, op.Rd, op.Size);
                EmitLdvecWithUnsignedCast(context, op.Rn, op.Size);

                context.EmitLdc_I4(GetImmShr(op));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.ShiftRightLogical), typesSrl));
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Add), typesAdd));

                EmitStvecWithUnsignedCast(context, op.Rd, op.Size);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorShrImmOpZx(context, ShrImmFlags.Accumulate);
            }
        }

        private static void EmitVectorShl(ILEmitterCtx context, bool signed)
        {
            //This instruction shifts the value on vector A by the number of bits
            //specified on the signed, lower 8 bits of vector B. If the shift value
            //is greater or equal to the data size of each lane, then the result is zero.
            //Additionally, negative shifts produces right shifts by the negated shift value.
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int maxShift = 8 << op.Size;

            Action emit = () =>
            {
                ILLabel lblShl  = new ILLabel();
                ILLabel lblZero = new ILLabel();
                ILLabel lblEnd  = new ILLabel();

                void EmitShift(OpCode ilOp)
                {
                    context.Emit(OpCodes.Dup);

                    context.EmitLdc_I4(maxShift);

                    context.Emit(OpCodes.Bge_S, lblZero);
                    context.Emit(ilOp);
                    context.Emit(OpCodes.Br_S, lblEnd);
                }

                context.Emit(OpCodes.Conv_I1);
                context.Emit(OpCodes.Dup);

                context.EmitLdc_I4(0);

                context.Emit(OpCodes.Bge_S, lblShl);
                context.Emit(OpCodes.Neg);

                EmitShift(signed
                    ? OpCodes.Shr
                    : OpCodes.Shr_Un);

                context.MarkLabel(lblShl);

                EmitShift(OpCodes.Shl);

                context.MarkLabel(lblZero);

                context.Emit(OpCodes.Pop);
                context.Emit(OpCodes.Pop);

                context.EmitLdc_I8(0);

                context.MarkLabel(lblEnd);
            };

            if (signed)
            {
                EmitVectorBinaryOpSx(context, emit);
            }
            else
            {
                EmitVectorBinaryOpZx(context, emit);
            }
        }

        [Flags]
        private enum ShrImmFlags
        {
            Scalar = 1 << 0,
            Signed = 1 << 1,

            Round      = 1 << 2,
            Accumulate = 1 << 3,

            ScalarSx = Scalar | Signed,
            ScalarZx = Scalar,

            VectorSx = Signed,
            VectorZx = 0
        }

        private static void EmitScalarShrImmOpSx(ILEmitterCtx context, ShrImmFlags flags)
        {
            EmitShrImmOp(context, ShrImmFlags.ScalarSx | flags);
        }

        private static void EmitScalarShrImmOpZx(ILEmitterCtx context, ShrImmFlags flags)
        {
            EmitShrImmOp(context, ShrImmFlags.ScalarZx | flags);
        }

        private static void EmitVectorShrImmOpSx(ILEmitterCtx context, ShrImmFlags flags)
        {
            EmitShrImmOp(context, ShrImmFlags.VectorSx | flags);
        }

        private static void EmitVectorShrImmOpZx(ILEmitterCtx context, ShrImmFlags flags)
        {
            EmitShrImmOp(context, ShrImmFlags.VectorZx | flags);
        }

        private static void EmitShrImmOp(ILEmitterCtx context, ShrImmFlags flags)
        {
            OpCodeSimdShImm64 op = (OpCodeSimdShImm64)context.CurrOp;

            bool scalar     = (flags & ShrImmFlags.Scalar)     != 0;
            bool signed     = (flags & ShrImmFlags.Signed)     != 0;
            bool round      = (flags & ShrImmFlags.Round)      != 0;
            bool accumulate = (flags & ShrImmFlags.Accumulate) != 0;

            int shift = GetImmShr(op);

            long roundConst = 1L << (shift - 1);

            int bytes = op.GetBitsCount() >> 3;
            int elems = !scalar ? bytes >> op.Size : 1;

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtract(context, op.Rn, index, op.Size, signed);

                if (op.Size <= 2)
                {
                    if (round)
                    {
                        context.EmitLdc_I8(roundConst);

                        context.Emit(OpCodes.Add);
                    }

                    context.EmitLdc_I4(shift);

                    context.Emit(signed ? OpCodes.Shr : OpCodes.Shr_Un);
                }
                else /* if (op.Size == 3) */
                {
                    EmitShrImm64(context, signed, round ? roundConst : 0L, shift);
                }

                if (accumulate)
                {
                    EmitVectorExtract(context, op.Rd, index, op.Size, signed);

                    context.Emit(OpCodes.Add);
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

        private static void EmitVectorShrImmNarrowOpZx(ILEmitterCtx context, bool round)
        {
            OpCodeSimdShImm64 op = (OpCodeSimdShImm64)context.CurrOp;

            int shift = GetImmShr(op);

            long roundConst = 1L << (shift - 1);

            int elems = 8 >> op.Size;

            int part = op.RegisterSize == RegisterSize.Simd128 ? elems : 0;

            if (part != 0)
            {
                context.EmitLdvec(op.Rd);
                context.EmitStvectmp();
            }

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtractZx(context, op.Rn, index, op.Size + 1);

                if (round)
                {
                    context.EmitLdc_I8(roundConst);

                    context.Emit(OpCodes.Add);
                }

                context.EmitLdc_I4(shift);

                context.Emit(OpCodes.Shr_Un);

                EmitVectorInsertTmp(context, part + index, op.Size);
            }

            context.EmitLdvectmp();
            context.EmitStvec(op.Rd);

            if (part == 0)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        [Flags]
        private enum ShrImmSaturatingNarrowFlags
        {
            Scalar    = 1 << 0,
            SignedSrc = 1 << 1,
            SignedDst = 1 << 2,

            Round = 1 << 3,

            ScalarSxSx = Scalar | SignedSrc | SignedDst,
            ScalarSxZx = Scalar | SignedSrc,
            ScalarZxZx = Scalar,

            VectorSxSx = SignedSrc | SignedDst,
            VectorSxZx = SignedSrc,
            VectorZxZx = 0
        }

        private static void EmitRoundShrImmSaturatingNarrowOp(ILEmitterCtx context, ShrImmSaturatingNarrowFlags flags)
        {
            EmitShrImmSaturatingNarrowOp(context, ShrImmSaturatingNarrowFlags.Round | flags);
        }

        private static void EmitShrImmSaturatingNarrowOp(ILEmitterCtx context, ShrImmSaturatingNarrowFlags flags)
        {
            OpCodeSimdShImm64 op = (OpCodeSimdShImm64)context.CurrOp;

            bool scalar    = (flags & ShrImmSaturatingNarrowFlags.Scalar)    != 0;
            bool signedSrc = (flags & ShrImmSaturatingNarrowFlags.SignedSrc) != 0;
            bool signedDst = (flags & ShrImmSaturatingNarrowFlags.SignedDst) != 0;
            bool round     = (flags & ShrImmSaturatingNarrowFlags.Round)     != 0;

            int shift = GetImmShr(op);

            long roundConst = 1L << (shift - 1);

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

                if (op.Size <= 1 || !round)
                {
                    if (round)
                    {
                        context.EmitLdc_I8(roundConst);

                        context.Emit(OpCodes.Add);
                    }

                    context.EmitLdc_I4(shift);

                    context.Emit(signedSrc ? OpCodes.Shr : OpCodes.Shr_Un);
                }
                else /* if (op.Size == 2 && round) */
                {
                    EmitShrImm64(context, signedSrc, roundConst, shift); // shift <= 32
                }

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

        // dst64 = (Int(src64, signed) + roundConst) >> shift;
        private static void EmitShrImm64(
            ILEmitterCtx context,
            bool signed,
            long roundConst,
            int  shift)
        {
            context.EmitLdc_I8(roundConst);
            context.EmitLdc_I4(shift);

            SoftFallback.EmitCall(context, signed
                ? nameof(SoftFallback.SignedShrImm64)
                : nameof(SoftFallback.UnsignedShrImm64));
        }

        private static void EmitVectorShImmWidenBinarySx(ILEmitterCtx context, Action emit, int imm)
        {
            EmitVectorShImmWidenBinaryOp(context, emit, imm, true);
        }

        private static void EmitVectorShImmWidenBinaryZx(ILEmitterCtx context, Action emit, int imm)
        {
            EmitVectorShImmWidenBinaryOp(context, emit, imm, false);
        }

        private static void EmitVectorShImmWidenBinaryOp(ILEmitterCtx context, Action emit, int imm, bool signed)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int elems = 8 >> op.Size;

            int part = op.RegisterSize == RegisterSize.Simd128 ? elems : 0;

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtract(context, op.Rn, part + index, op.Size, signed);

                context.EmitLdc_I4(imm);

                emit();

                EmitVectorInsertTmp(context, index, op.Size + 1);
            }

            context.EmitLdvectmp();
            context.EmitStvec(op.Rd);
        }
    }
}
