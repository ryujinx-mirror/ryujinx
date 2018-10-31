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

                EmitLdvecWithUnsignedCast(context, op.Rm, op.Size);
                EmitLdvecWithUnsignedCast(context, op.Rn, op.Size);

                Type[] types = new Type[]
                {
                    VectorUIntTypesPerSizeLog2[op.Size],
                    VectorUIntTypesPerSizeLog2[op.Size]
                };

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.AndNot), types));

                EmitStvecWithUnsignedCast(context, op.Rd, op.Size);

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
            EmitBitBif(context, true);
        }

        public static void Bit_V(ILEmitterCtx context)
        {
            EmitBitBif(context, false);
        }

        private static void EmitBitBif(ILEmitterCtx context, bool notRm)
        {
            OpCodeSimdReg64 op = (OpCodeSimdReg64)context.CurrOp;

            if (Optimizations.UseSse2)
            {
                Type[] types = new Type[]
                {
                    VectorUIntTypesPerSizeLog2[op.Size],
                    VectorUIntTypesPerSizeLog2[op.Size]
                };

                EmitLdvecWithUnsignedCast(context, op.Rm, op.Size);
                EmitLdvecWithUnsignedCast(context, op.Rd, op.Size);
                EmitLdvecWithUnsignedCast(context, op.Rn, op.Size);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), types));

                string name = notRm ? nameof(Sse2.AndNot) : nameof(Sse2.And);

                context.EmitCall(typeof(Sse2).GetMethod(name, types));

                EmitLdvecWithUnsignedCast(context, op.Rd, op.Size);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), types));

                EmitStvecWithUnsignedCast(context, op.Rd, op.Size);

                if (op.RegisterSize == RegisterSize.Simd64)
                {
                    EmitVectorZeroUpper(context, op.Rd);
                }
            }
            else
            {
                int bytes = op.GetBitsCount() >> 3;
                int elems = bytes >> op.Size;

                for (int index = 0; index < elems; index++)
                {
                    EmitVectorExtractZx(context, op.Rd, index, op.Size);
                    EmitVectorExtractZx(context, op.Rn, index, op.Size);

                    context.Emit(OpCodes.Xor);

                    EmitVectorExtractZx(context, op.Rm, index, op.Size);

                    if (notRm)
                    {
                        context.Emit(OpCodes.Not);
                    }

                    context.Emit(OpCodes.And);

                    EmitVectorExtractZx(context, op.Rd, index, op.Size);

                    context.Emit(OpCodes.Xor);

                    EmitVectorInsert(context, op.Rd, index, op.Size);
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

                Type[] types = new Type[]
                {
                    VectorUIntTypesPerSizeLog2[op.Size],
                    VectorUIntTypesPerSizeLog2[op.Size]
                };

                EmitLdvecWithUnsignedCast(context, op.Rn, op.Size);
                EmitLdvecWithUnsignedCast(context, op.Rm, op.Size);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), types));

                EmitLdvecWithUnsignedCast(context, op.Rd, op.Size);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.And), types));

                EmitLdvecWithUnsignedCast(context, op.Rm, op.Size);

                context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), types));

                EmitStvecWithUnsignedCast(context, op.Rd, op.Size);

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
            EmitVectorUnaryOpZx(context, () => context.Emit(OpCodes.Not));
        }

        public static void Orn_V(ILEmitterCtx context)
        {
            EmitVectorBinaryOpZx(context, () =>
            {
                context.Emit(OpCodes.Not);
                context.Emit(OpCodes.Or);
            });
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
            EmitRev_V(context, containerSize: 1);
        }

        public static void Rev32_V(ILEmitterCtx context)
        {
            EmitRev_V(context, containerSize: 2);
        }

        public static void Rev64_V(ILEmitterCtx context)
        {
            EmitRev_V(context, containerSize: 3);
        }

        private static void EmitRev_V(ILEmitterCtx context, int containerSize)
        {
            OpCodeSimd64 op = (OpCodeSimd64)context.CurrOp;

            if (op.Size >= containerSize)
            {
                throw new InvalidOperationException();
            }

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
