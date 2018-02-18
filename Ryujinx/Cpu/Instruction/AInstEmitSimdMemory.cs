using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;

using static ChocolArm64.Instruction.AInstEmitMemoryHelper;
using static ChocolArm64.Instruction.AInstEmitSimdHelper;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Ld__Vms(AILEmitterCtx Context)
        {
            EmitSimdMemMs(Context, IsLoad: true);
        }

        public static void Ld__Vss(AILEmitterCtx Context)
        {
            EmitSimdMemSs(Context, IsLoad: true);
        }

        public static void St__Vms(AILEmitterCtx Context)
        {
            EmitSimdMemMs(Context, IsLoad: false);
        }

        public static void St__Vss(AILEmitterCtx Context)
        {
            EmitSimdMemSs(Context, IsLoad: false);
        }

        private static void EmitSimdMemMs(AILEmitterCtx Context, bool IsLoad)
        {
            AOpCodeSimdMemMs Op = (AOpCodeSimdMemMs)Context.CurrOp;

            int Offset = 0;

            for (int Rep   = 0; Rep   < Op.Reps;   Rep++)
            for (int Elem  = 0; Elem  < Op.Elems;  Elem++)
            for (int SElem = 0; SElem < Op.SElems; SElem++)
            {
                int Rtt = (Op.Rt + Rep + SElem) & 0x1f;

                if (IsLoad)
                {
                    Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
                    Context.EmitLdint(Op.Rn);
                    Context.EmitLdc_I8(Offset);

                    Context.Emit(OpCodes.Add);

                    EmitReadZxCall(Context, Op.Size);

                    EmitVectorInsert(Context, Rtt, Elem, Op.Size);

                    if (Op.RegisterSize == ARegisterSize.SIMD64 && Elem == Op.Elems - 1)
                    {
                        EmitVectorZeroUpper(Context, Rtt);
                    }
                }
                else
                {
                    Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
                    Context.EmitLdint(Op.Rn);
                    Context.EmitLdc_I8(Offset);

                    Context.Emit(OpCodes.Add);

                    EmitVectorExtractZx(Context, Rtt, Elem, Op.Size);

                    EmitWriteCall(Context, Op.Size);
                }

                Offset += 1 << Op.Size;
            }

            if (Op.WBack)
            {
                EmitSimdMemWBack(Context, Offset);
            }
        }

        private static void EmitSimdMemSs(AILEmitterCtx Context, bool IsLoad)
        {
            AOpCodeSimdMemSs Op = (AOpCodeSimdMemSs)Context.CurrOp;

            int Offset = 0;

            void EmitMemAddress()
            {
                Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
                Context.EmitLdint(Op.Rn);
                Context.EmitLdc_I8(Offset);

                Context.Emit(OpCodes.Add);
            }

            if (Op.Replicate)
            {
                //Only loads uses the replicate mode.
                if (!IsLoad)
                {
                    throw new InvalidOperationException();
                }

                int Bytes = Context.CurrOp.GetBitsCount() >> 3;

                for (int SElem = 0; SElem < Op.SElems; SElem++)
                {
                    int Rt = (Op.Rt + SElem) & 0x1f;

                    for (int Index = 0; Index < (Bytes >> Op.Size); Index++)
                    {
                        EmitMemAddress();

                        EmitReadZxCall(Context, Op.Size);

                        EmitVectorInsert(Context, Rt, Index, Op.Size);
                    }

                    if (Op.RegisterSize == ARegisterSize.SIMD64)
                    {
                        EmitVectorZeroUpper(Context, Rt);
                    }

                    Offset += 1 << Op.Size;
                }
            }
            else
            {
                for (int SElem = 0; SElem < Op.SElems; SElem++)
                {
                    int Rt = (Op.Rt + SElem) & 0x1f;

                    if (IsLoad)
                    {
                        EmitMemAddress();

                        EmitReadZxCall(Context, Op.Size);

                        EmitVectorInsert(Context, Rt, Op.Index, Op.Size);
                    }
                    else
                    {
                        EmitMemAddress();

                        EmitVectorExtractZx(Context, Rt, Op.Index, Op.Size);

                        EmitWriteCall(Context, Op.Size);
                    }

                    Offset += 1 << Op.Size;
                }
            }

            if (Op.WBack)
            {
                EmitSimdMemWBack(Context, Offset);
            }
        }

        private static void EmitSimdMemWBack(AILEmitterCtx Context, int Offset)
        {
            AOpCodeMemReg Op = (AOpCodeMemReg)Context.CurrOp;

            Context.EmitLdint(Op.Rn);

            if (Op.Rm != ARegisters.ZRIndex)
            {
                Context.EmitLdint(Op.Rm);
            }
            else
            {
                Context.EmitLdc_I8(Offset);
            }

            Context.Emit(OpCodes.Add);

            Context.EmitStint(Op.Rn);
        }
    }
}