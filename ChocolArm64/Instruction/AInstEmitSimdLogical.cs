using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;

using static ChocolArm64.Instruction.AInstEmitSimdHelper;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void And_V(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse2)
            {
                EmitSse2Op(Context, nameof(Sse2.And));
            }
            else
            {
                EmitVectorBinaryOpZx(Context, () => Context.Emit(OpCodes.And));
            }
        }

        public static void Bic_V(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse2)
            {
                AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

                EmitLdvecWithUnsignedCast(Context, Op.Rm, Op.Size);
                EmitLdvecWithUnsignedCast(Context, Op.Rn, Op.Size);

                Type[] Types = new Type[]
                {
                    VectorUIntTypesPerSizeLog2[Op.Size],
                    VectorUIntTypesPerSizeLog2[Op.Size]
                };

                Context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.AndNot), Types));

                EmitStvecWithUnsignedCast(Context, Op.Rd, Op.Size);

                if (Op.RegisterSize == ARegisterSize.SIMD64)
                {
                    EmitVectorZeroUpper(Context, Op.Rd);
                }
            }
            else
            {
                EmitVectorBinaryOpZx(Context, () =>
                {
                    Context.Emit(OpCodes.Not);
                    Context.Emit(OpCodes.And);
                });
            }
        }

        public static void Bic_Vi(AILEmitterCtx Context)
        {
            EmitVectorImmBinaryOp(Context, () =>
            {
                Context.Emit(OpCodes.Not);
                Context.Emit(OpCodes.And);
            });
        }

        public static void Bif_V(AILEmitterCtx Context)
        {
            EmitBitBif(Context, true);
        }

        public static void Bit_V(AILEmitterCtx Context)
        {
            EmitBitBif(Context, false);
        }

        private static void EmitBitBif(AILEmitterCtx Context, bool NotRm)
        {
            AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

            if (AOptimizations.UseSse2)
            {
                Type[] Types = new Type[]
                {
                    VectorUIntTypesPerSizeLog2[Op.Size],
                    VectorUIntTypesPerSizeLog2[Op.Size]
                };

                EmitLdvecWithUnsignedCast(Context, Op.Rm, Op.Size);
                EmitLdvecWithUnsignedCast(Context, Op.Rd, Op.Size);
                EmitLdvecWithUnsignedCast(Context, Op.Rn, Op.Size);

                Context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), Types));

                string Name = NotRm ? nameof(Sse2.AndNot) : nameof(Sse2.And);

                Context.EmitCall(typeof(Sse2).GetMethod(Name, Types));

                EmitLdvecWithUnsignedCast(Context, Op.Rd, Op.Size);

                Context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), Types));

                EmitStvecWithUnsignedCast(Context, Op.Rd, Op.Size);

                if (Op.RegisterSize == ARegisterSize.SIMD64)
                {
                    EmitVectorZeroUpper(Context, Op.Rd);
                }
            }
            else
            {
                int Bytes = Op.GetBitsCount() >> 3;
                int Elems = Bytes >> Op.Size;

                for (int Index = 0; Index < Elems; Index++)
                {
                    EmitVectorExtractZx(Context, Op.Rd, Index, Op.Size);
                    EmitVectorExtractZx(Context, Op.Rn, Index, Op.Size);

                    Context.Emit(OpCodes.Xor);

                    EmitVectorExtractZx(Context, Op.Rm, Index, Op.Size);

                    if (NotRm)
                    {
                        Context.Emit(OpCodes.Not);
                    }

                    Context.Emit(OpCodes.And);

                    EmitVectorExtractZx(Context, Op.Rd, Index, Op.Size);

                    Context.Emit(OpCodes.Xor);

                    EmitVectorInsert(Context, Op.Rd, Index, Op.Size);
                }

                if (Op.RegisterSize == ARegisterSize.SIMD64)
                {
                    EmitVectorZeroUpper(Context, Op.Rd);
                }
            }
        }

        public static void Bsl_V(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse2)
            {
                AOpCodeSimdReg Op = (AOpCodeSimdReg)Context.CurrOp;

                Type[] Types = new Type[]
                {
                    VectorUIntTypesPerSizeLog2[Op.Size],
                    VectorUIntTypesPerSizeLog2[Op.Size]
                };

                EmitLdvecWithUnsignedCast(Context, Op.Rn, Op.Size);
                EmitLdvecWithUnsignedCast(Context, Op.Rm, Op.Size);

                Context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), Types));

                EmitLdvecWithUnsignedCast(Context, Op.Rd, Op.Size);

                Context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.And), Types));

                EmitLdvecWithUnsignedCast(Context, Op.Rm, Op.Size);

                Context.EmitCall(typeof(Sse2).GetMethod(nameof(Sse2.Xor), Types));

                EmitStvecWithUnsignedCast(Context, Op.Rd, Op.Size);

                if (Op.RegisterSize == ARegisterSize.SIMD64)
                {
                    EmitVectorZeroUpper(Context, Op.Rd);
                }
            }
            else
            {
                EmitVectorTernaryOpZx(Context, () =>
                {
                    Context.EmitSttmp();
                    Context.EmitLdtmp();

                    Context.Emit(OpCodes.Xor);
                    Context.Emit(OpCodes.And);

                    Context.EmitLdtmp();

                    Context.Emit(OpCodes.Xor);
                });
            }
        }

        public static void Eor_V(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse2)
            {
                EmitSse2Op(Context, nameof(Sse2.Xor));
            }
            else
            {
                EmitVectorBinaryOpZx(Context, () => Context.Emit(OpCodes.Xor));
            }
        }

        public static void Not_V(AILEmitterCtx Context)
        {
            EmitVectorUnaryOpZx(Context, () => Context.Emit(OpCodes.Not));
        }

        public static void Orn_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpZx(Context, () =>
            {
                Context.Emit(OpCodes.Not);
                Context.Emit(OpCodes.Or);
            });
        }

        public static void Orr_V(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse2)
            {
                EmitSse2Op(Context, nameof(Sse2.Or));
            }
            else
            {
                EmitVectorBinaryOpZx(Context, () => Context.Emit(OpCodes.Or));
            }
        }

        public static void Orr_Vi(AILEmitterCtx Context)
        {
            EmitVectorImmBinaryOp(Context, () => Context.Emit(OpCodes.Or));
        }

        public static void Rbit_V(AILEmitterCtx Context)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int Elems = Op.RegisterSize == ARegisterSize.SIMD128 ? 16 : 8;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtractZx(Context, Op.Rn, Index, 0);

                Context.Emit(OpCodes.Conv_U4);

                ASoftFallback.EmitCall(Context, nameof(ASoftFallback.ReverseBits8));

                Context.Emit(OpCodes.Conv_U8);

                EmitVectorInsert(Context, Op.Rd, Index, 0);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        public static void Rev16_V(AILEmitterCtx Context)
        {
            EmitRev_V(Context, ContainerSize: 1);
        }

        public static void Rev32_V(AILEmitterCtx Context)
        {
            EmitRev_V(Context, ContainerSize: 2);
        }

        public static void Rev64_V(AILEmitterCtx Context)
        {
            EmitRev_V(Context, ContainerSize: 3);
        }

        private static void EmitRev_V(AILEmitterCtx Context, int ContainerSize)
        {
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            if (Op.Size >= ContainerSize)
            {
                throw new InvalidOperationException();
            }

            int Bytes = Op.GetBitsCount() >> 3;
            int Elems = Bytes >> Op.Size;

            int ContainerMask = (1 << (ContainerSize - Op.Size)) - 1;

            for (int Index = 0; Index < Elems; Index++)
            {
                int RevIndex = Index ^ ContainerMask;

                EmitVectorExtractZx(Context, Op.Rn, RevIndex, Op.Size);

                EmitVectorInsertTmp(Context, Index, Op.Size);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }
    }
}
