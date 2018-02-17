using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;

using static ChocolArm64.Instruction.AInstEmitSimdHelper;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        [Flags]
        private enum ShrFlags
        {
            None       = 0,
            Signed     = 1 << 0,
            Rounding   = 1 << 1,
            Accumulate = 1 << 2
        }

        public static void Shl_S(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            EmitVectorExtractZx(Context, Op.Rn, 0, Op.Size);

            Context.EmitLdc_I4(GetImmShl(Op));

            Context.Emit(OpCodes.Shl);

            EmitScalarSet(Context, Op.Rd, Op.Size);
        }

        public static void Shl_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Shift = Op.Imm - (8 << Op.Size);

            EmitVectorShImmBinaryZx(Context, () => Context.Emit(OpCodes.Shl), Shift);
        }

        public static void Shrn_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Shift = (8 << (Op.Size + 1)) - Op.Imm;

            EmitVectorShImmNarrowBinaryZx(Context, () => Context.Emit(OpCodes.Shr_Un), Shift);
        }

        public static void Sshl_V(AILEmitterCtx Context)
        {
            EmitVectorShl(Context, Signed: true);
        }

        public static void Sshll_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Shift = Op.Imm - (8 << Op.Size);

            EmitVectorShImmWidenBinarySx(Context, () => Context.Emit(OpCodes.Shl), Shift);
        }

        public static void Sshr_S(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            EmitVectorExtractSx(Context, Op.Rn, 0, Op.Size);

            Context.EmitLdc_I4(GetImmShr(Op));

            Context.Emit(OpCodes.Shr);

            EmitScalarSet(Context, Op.Rd, Op.Size);
        }

        public static void Sshr_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Shift = (8 << (Op.Size + 1)) - Op.Imm;

            EmitVectorShImmBinarySx(Context, () => Context.Emit(OpCodes.Shr), Shift);
        }

        public static void Ushl_V(AILEmitterCtx Context)
        {
            EmitVectorShl(Context, Signed: false);
        }

        public static void Ushll_V(AILEmitterCtx Context)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Shift = Op.Imm - (8 << Op.Size);

            EmitVectorShImmWidenBinaryZx(Context, () => Context.Emit(OpCodes.Shl), Shift);
        }

        public static void Ushr_V(AILEmitterCtx Context)
        {
            EmitVectorShr(Context, ShrFlags.None);
        }

        public static void Usra_V(AILEmitterCtx Context)
        {
            EmitVectorShr(Context, ShrFlags.Accumulate);
        }

        private static void EmitVectorShl(AILEmitterCtx Context, bool Signed)
        {
            //This instruction shifts the value on vector A by the number of bits
            //specified on the signed, lower 8 bits of vector B. If the shift value
            //is greater or equal to the data size of each lane, then the result is zero.
            //Additionally, negative shifts produces right shifts by the negated shift value.
            AOpCodeSimd Op = (AOpCodeSimd)Context.CurrOp;

            int MaxShift = 8 << Op.Size;

            Action Emit = () =>
            {
                AILLabel LblShl  = new AILLabel();
                AILLabel LblZero = new AILLabel();
                AILLabel LblEnd  = new AILLabel();

                void EmitShift(OpCode ILOp)
                {
                    Context.Emit(OpCodes.Dup);

                    Context.EmitLdc_I4(MaxShift);

                    Context.Emit(OpCodes.Bge_S, LblZero);
                    Context.Emit(ILOp);
                    Context.Emit(OpCodes.Br_S, LblEnd);
                }

                Context.Emit(OpCodes.Conv_I1);
                Context.Emit(OpCodes.Dup);

                Context.EmitLdc_I4(0);

                Context.Emit(OpCodes.Bge_S, LblShl);
                Context.Emit(OpCodes.Neg);

                EmitShift(Signed
                    ? OpCodes.Shr
                    : OpCodes.Shr_Un);

                Context.MarkLabel(LblShl);

                EmitShift(OpCodes.Shl);

                Context.MarkLabel(LblZero);

                Context.Emit(OpCodes.Pop);
                Context.Emit(OpCodes.Pop);

                Context.EmitLdc_I8(0);

                Context.MarkLabel(LblEnd);
            };

            if (Signed)
            {
                EmitVectorBinaryOpSx(Context, Emit);
            }
            else
            {
                EmitVectorBinaryOpZx(Context, Emit);
            }
        }

        private static void EmitVectorShr(AILEmitterCtx Context, ShrFlags Flags)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Shift = (8 << (Op.Size + 1)) - Op.Imm;

            if (Flags.HasFlag(ShrFlags.Accumulate))
            {
                Action Emit = () =>
                {
                    Context.EmitLdc_I4(Shift);

                    Context.Emit(OpCodes.Shr_Un);
                    Context.Emit(OpCodes.Add);
                };

                EmitVectorOp(Context, Emit, OperFlags.RdRn, Signed: false);
            }
            else
            {
                EmitVectorUnaryOpZx(Context, () =>
                {
                    Context.EmitLdc_I4(Shift);

                    Context.Emit(OpCodes.Shr_Un);
                });
            }
        }

        private static void EmitVectorShImmBinarySx(AILEmitterCtx Context, Action Emit, int Imm)
        {
            EmitVectorShImmBinaryOp(Context, Emit, Imm, true);
        }

        private static void EmitVectorShImmBinaryZx(AILEmitterCtx Context, Action Emit, int Imm)
        {
            EmitVectorShImmBinaryOp(Context, Emit, Imm, false);
        }

        private static void EmitVectorShImmBinaryOp(AILEmitterCtx Context, Action Emit, int Imm, bool Signed)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Bytes = Context.CurrOp.GetBitsCount() >> 3;

            for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
            {
                EmitVectorExtract(Context, Op.Rn, Index, Op.Size, Signed);

                Context.EmitLdc_I4(Imm);

                Emit();

                EmitVectorInsert(Context, Op.Rd, Index, Op.Size);
            }

            if (Op.RegisterSize == ARegisterSize.SIMD64)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitVectorShImmNarrowBinarySx(AILEmitterCtx Context, Action Emit, int Imm)
        {
            EmitVectorShImmNarrowBinaryOp(Context, Emit, Imm, true);
        }

        private static void EmitVectorShImmNarrowBinaryZx(AILEmitterCtx Context, Action Emit, int Imm)
        {
            EmitVectorShImmNarrowBinaryOp(Context, Emit, Imm, false);
        }

        private static void EmitVectorShImmNarrowBinaryOp(AILEmitterCtx Context, Action Emit, int Imm, bool Signed)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Elems = 8 >> Op.Size;

            int Part = Op.RegisterSize == ARegisterSize.SIMD128 ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtract(Context, Op.Rn, Index, Op.Size + 1, Signed);

                Context.EmitLdc_I4(Imm);

                Emit();

                EmitVectorInsert(Context, Op.Rd, Part + Index, Op.Size);
            }

            if (Part == 0)
            {
                EmitVectorZeroUpper(Context, Op.Rd);
            }
        }

        private static void EmitVectorShImmWidenBinarySx(AILEmitterCtx Context, Action Emit, int Imm)
        {
            EmitVectorShImmWidenBinaryOp(Context, Emit, Imm, true);
        }

        private static void EmitVectorShImmWidenBinaryZx(AILEmitterCtx Context, Action Emit, int Imm)
        {
            EmitVectorShImmWidenBinaryOp(Context, Emit, Imm, false);
        }

        private static void EmitVectorShImmWidenBinaryOp(AILEmitterCtx Context, Action Emit, int Imm, bool Signed)
        {
            AOpCodeSimdShImm Op = (AOpCodeSimdShImm)Context.CurrOp;

            int Elems = 8 >> Op.Size;

            int Part = Op.RegisterSize == ARegisterSize.SIMD128 ? Elems : 0;

            for (int Index = 0; Index < Elems; Index++)
            {
                EmitVectorExtract(Context, Op.Rn, Part + Index, Op.Size, Signed);

                Context.EmitLdc_I4(Imm);

                Emit();

                EmitVectorInsertTmp(Context, Index, Op.Size + 1);
            }

            Context.EmitLdvectmp();
            Context.EmitStvec(Op.Rd);
        }
    }
}