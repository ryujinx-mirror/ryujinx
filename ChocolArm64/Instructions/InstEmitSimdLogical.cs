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
        public static void And_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse2)
            {
                EmitSse2Op(context, nameof(Sse2.And));
            }
            else
            {
                EmitVectorBinaryOpZx(context, () => context.Emit(OpCodes.And));
            }
        }

        public static void Bic_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                Type[] typesAndNot = new Type[] { typeof(Vector128<byte>), typeof(Vector128<byte>) };

                EmitLdvecWithUnsignedCast(context, op.Rm, 0);
                EmitLdvecWithUnsignedCast(context, op.Rn, 0);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.AndNot), typesAndNot));

                EmitStvecWithUnsignedCast(context, op.Rd, 0);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorBinaryOpZx(context, () =>
                {
                    context.Emit(OpCodes.Not);
                    context.Emit(OpCodes.And);
                });
            }
        }

        public static void Bic_Vi(ILEmitterCtx context)
        {
            EmitVectorImmBinaryOp(context, () =>
            {
                context.Emit(OpCodes.Not);
                context.Emit(OpCodes.And);
            });
        }

        public static void Bif_V(ILEmitterCtx context)
        {
            EmitBifBit(context, notRm: true);
        }

        public static void Bit_V(ILEmitterCtx context)
        {
            EmitBifBit(context, notRm: false);
        }

        private static void EmitBifBit(ILEmitterCtx context, bool notRm)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            if (Optimizations.UseSse2)
            {
                Type[] typesXorAndNot = new Type[] { typeof(Vector128<byte>), typeof(Vector128<byte>) };

                string nameAndNot = notRm ? nameof(Sse2.AndNot) : nameof(Sse2.And);

                EmitLdvecWithUnsignedCast(context, op.Rd, 0);
                EmitLdvecWithUnsignedCast(context, op.Rm, 0);
                EmitLdvecWithUnsignedCast(context, op.Rn, 0);
                EmitLdvecWithUnsignedCast(context, op.Rd, 0);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), typesXorAndNot));
                context.EmitCall(typeof(Sse2).GetMethod(nameAndNot,       typesXorAndNot));
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), typesXorAndNot));

                EmitStvecWithUnsignedCast(context, op.Rd, 0);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                int elems = op.RegisterSize == RegisterSize.Simd128 ? 2 : 1;

                for (int index = 0; index < elems; index++)
                {
                    EmitVectorExtractZx(context, op.Rd, index, 3);
                    context.Emit(OpCodes.Dup);

                    EmitVectorExtractZx(context, op.Rn, index, 3);

                    context.Emit(OpCodes.Xor);

                    EmitVectorExtractZx(context, op.Rm, index, 3);

                    if (notRm)
                    {
                        context.Emit(OpCodes.Not);
                    }

                    context.Emit(OpCodes.And);

                    context.Emit(OpCodes.Xor);

                    EmitVectorInsert(context, op.Rd, index, 3);
                }

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
        }

        public static void Bsl_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                Type[] typesXorAnd = new Type[] { typeof(Vector128<byte>), typeof(Vector128<byte>) };

                EmitLdvecWithUnsignedCast(context, op.Rm, 0);
                context.Emit(OpCodes.Dup);

                EmitLdvecWithUnsignedCast(context, op.Rn, 0);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), typesXorAnd));

                EmitLdvecWithUnsignedCast(context, op.Rd, 0);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.And), typesXorAnd));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), typesXorAnd));

                EmitStvecWithUnsignedCast(context, op.Rd, 0);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorTernaryOpZx(context, () =>
                {
                    context.EmitSttmp();
                    context.EmitLdtmp();

                    context.Emit(OpCodes.Xor);
                    context.Emit(OpCodes.And);

                    context.EmitLdtmp();

                    context.Emit(OpCodes.Xor);
                });
            }
        }

        public static void Eor_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse2)
            {
                EmitSse2Op(context, nameof(Sse2.Xor));
            }
            else
            {
                EmitVectorBinaryOpZx(context, () => context.Emit(OpCodes.Xor));
            }
        }

        public static void Not_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

                Type[] typesSav    = new Type[] { typeof(byte) };
                Type[] typesAndNot = new Type[] { typeof(Vector128<byte>), typeof(Vector128<byte>) };

                EmitLdvecWithUnsignedCast(context, op.Rn, 0);

                context.EmitLdc_I4(byte.MaxValue);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.AndNot), typesAndNot));

                EmitStvecWithUnsignedCast(context, op.Rd, 0);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorUnaryOpZx(context, () => context.Emit(OpCodes.Not));
            }
        }

        public static void Orn_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse2)
            {
                OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

                Type[] typesSav      = new Type[] { typeof(byte) };
                Type[] typesAndNotOr = new Type[] { typeof(Vector128<byte>), typeof(Vector128<byte>) };

                EmitLdvecWithUnsignedCast(context, op.Rn, 0);
                EmitLdvecWithUnsignedCast(context, op.Rm, 0);

                context.EmitLdc_I4(byte.MaxValue);
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetAllVector128), typesSav));

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.AndNot), typesAndNotOr));
                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Or),     typesAndNotOr));

                EmitStvecWithUnsignedCast(context, op.Rd, 0);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitVectorBinaryOpZx(context, () =>
                {
                    context.Emit(OpCodes.Not);
                    context.Emit(OpCodes.Or);
                });
            }
        }

        public static void Orr_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSse2)
            {
                EmitSse2Op(context, nameof(Sse2.Or));
            }
            else
            {
                EmitVectorBinaryOpZx(context, () => context.Emit(OpCodes.Or));
            }
        }

        public static void Orr_Vi(ILEmitterCtx context)
        {
            EmitVectorImmBinaryOp(context, () => context.Emit(OpCodes.Or));
        }

        public static void Rbit_V(ILEmitterCtx context)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int elems = op.RegisterSize == RegisterSize.Simd128 ? 16 : 8;

            for (int index = 0; index < elems; index++)
            {
                EmitVectorExtractZx(context, op.Rn, index, 0);

                context.Emit(OpCodes.Conv_U4);

                SoftFallback.EmitCall(context, nameof(SoftFallback.ReverseBits8));

                context.Emit(OpCodes.Conv_U8);

                EmitVectorInsert(context, op.Rd, index, 0);
            }

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }

        public static void Rev16_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSsse3)
            {
                OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

                Type[] typesSve = new Type[] { typeof(long), typeof(long) };
                Type[] typesSfl = new Type[] { typeof(Vector128<sbyte>), typeof(Vector128<sbyte>) };

                EmitLdvecWithSignedCast(context, op.Rn, 0); // value

                context.EmitLdc_I8(14L << 56 | 15L << 48 | 12L << 40 | 13L << 32 | 10L << 24 | 11L << 16 | 08L << 8 | 09L << 0); // maskE1
                context.EmitLdc_I8(06L << 56 | 07L << 48 | 04L << 40 | 05L << 32 | 02L << 24 | 03L << 16 | 00L << 8 | 01L << 0); // maskE0

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetVector128), typesSve));

                context.EmitCall(typeof(Ssse3).GetMethod(nameof(Ssse3.Shuffle), typesSfl));

                EmitStvecWithSignedCast(context, op.Rd, 0);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitRev_V(context, containerSize: 1);
            }
        }

        public static void Rev32_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSsse3)
            {
                OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

                Type[] typesSve = new Type[] { typeof(long), typeof(long) };
                Type[] typesSfl = new Type[] { typeof(Vector128<sbyte>), typeof(Vector128<sbyte>) };

                EmitLdvecWithSignedCast(context, op.Rn, op.Size); // value

                if (op.Size == 0)
                {
                    context.EmitLdc_I8(12L << 56 | 13L << 48 | 14L << 40 | 15L << 32 | 08L << 24 | 09L << 16 | 10L << 8 | 11L << 0); // maskE1
                    context.EmitLdc_I8(04L << 56 | 05L << 48 | 06L << 40 | 07L << 32 | 00L << 24 | 01L << 16 | 02L << 8 | 03L << 0); // maskE0
                }
                else /* if (op.Size == 1) */
                {
                    context.EmitLdc_I8(13L << 56 | 12L << 48 | 15L << 40 | 14L << 32 | 09L << 24 | 08L << 16 | 11L << 8 | 10L << 0); // maskE1
                    context.EmitLdc_I8(05L << 56 | 04L << 48 | 07L << 40 | 06L << 32 | 01L << 24 | 00L << 16 | 03L << 8 | 02L << 0); // maskE0
                }

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetVector128), typesSve));

                context.EmitCall(typeof(Ssse3).GetMethod(nameof(Ssse3.Shuffle), typesSfl));

                EmitStvecWithSignedCast(context, op.Rd, op.Size);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitRev_V(context, containerSize: 2);
            }
        }

        public static void Rev64_V(ILEmitterCtx context)
        {
            if (Optimizations.UseSsse3)
            {
                OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

                Type[] typesSve = new Type[] { typeof(long), typeof(long) };
                Type[] typesSfl = new Type[] { typeof(Vector128<sbyte>), typeof(Vector128<sbyte>) };

                EmitLdvecWithSignedCast(context, op.Rn, op.Size); // value

                if (op.Size == 0)
                {
                    context.EmitLdc_I8(08L << 56 | 09L << 48 | 10L << 40 | 11L << 32 | 12L << 24 | 13L << 16 | 14L << 8 | 15L << 0); // maskE1
                    context.EmitLdc_I8(00L << 56 | 01L << 48 | 02L << 40 | 03L << 32 | 04L << 24 | 05L << 16 | 06L << 8 | 07L << 0); // maskE0
                }
                else if (op.Size == 1)
                {
                    context.EmitLdc_I8(09L << 56 | 08L << 48 | 11L << 40 | 10L << 32 | 13L << 24 | 12L << 16 | 15L << 8 | 14L << 0); // maskE1
                    context.EmitLdc_I8(01L << 56 | 00L << 48 | 03L << 40 | 02L << 32 | 05L << 24 | 04L << 16 | 07L << 8 | 06L << 0); // maskE0
                }
                else /* if (op.Size == 2) */
                {
                    context.EmitLdc_I8(11L << 56 | 10L << 48 | 09L << 40 | 08L << 32 | 15L << 24 | 14L << 16 | 13L << 8 | 12L << 0); // maskE1
                    context.EmitLdc_I8(03L << 56 | 02L << 48 | 01L << 40 | 00L << 32 | 07L << 24 | 06L << 16 | 05L << 8 | 04L << 0); // maskE0
                }

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.SetVector128), typesSve));

                context.EmitCall(typeof(Ssse3).GetMethod(nameof(Ssse3.Shuffle), typesSfl));

                EmitStvecWithSignedCast(context, op.Rd, op.Size);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                EmitRev_V(context, containerSize: 3);
            }
        }

        private static void EmitRev_V(ILEmitterCtx context, int containerSize)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            int bytes = op.GetBitsCount() >> 3;
            int elems = bytes >> op.Size;

            int containerMask = (1 << (containerSize - op.Size)) - 1;

            for (int index = 0; index < elems; index++)
            {
                int revIndex = index ^ containerMask;

                EmitVectorExtractZx(context, op.Rn, revIndex, op.Size);

                EmitVectorInsertTmp(context, index, op.Size);
            }

            context.EmitLdvectmp();
            context.EmitStvec(op.Rd);

            if (op.RegisterSize == RegisterSize.Simd64)
            {
                EmitVectorZeroUpper(context, op.Rd);
            }
        }
    }
}
