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
                EmitSse2Call(Context, nameof(Sse2.And));
            }
            else
            {
                EmitVectorBinaryOpZx(Context, () => Context.Emit(OpCodes.And));
            }
        }

        public static void Bic_V(AILEmitterCtx Context)
        {
            EmitVectorBinaryOpZx(Context, () =>
            {
                Context.Emit(OpCodes.Not);
                Context.Emit(OpCodes.And);
            });
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

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
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

        public static void Bsl_V(AILEmitterCtx Context)
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

        public static void Eor_V(AILEmitterCtx Context)
        {
            if (AOptimizations.UseSse2)
            {
                EmitSse2Call(Context, nameof(Sse2.Xor));
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
                EmitSse2Call(Context, nameof(Sse2.Or));
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

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            int Elems = Bytes >> Op.Size;

            if (Op.Size >= ContainerSize)
            {
                throw new InvalidOperationException();
            }

            int ContainerMask = (1 << (ContainerSize - Op.Size)) - 1;

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
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
