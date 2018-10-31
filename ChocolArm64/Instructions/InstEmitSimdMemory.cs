using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;

using static ChocolArm64.Instructions.InstEmitMemoryHelper;
using static ChocolArm64.Instructions.InstEmitSimdHelper;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        public static void Ld__Vms(ILEmitterCtx context)
        {
            EmitSimdMemMs(context, isLoad: true);
        }

        public static void Ld__Vss(ILEmitterCtx context)
        {
            EmitSimdMemSs(context, isLoad: true);
        }

        public static void St__Vms(ILEmitterCtx context)
        {
            EmitSimdMemMs(context, isLoad: false);
        }

        public static void St__Vss(ILEmitterCtx context)
        {
            EmitSimdMemSs(context, isLoad: false);
        }

        private static void EmitSimdMemMs(ILEmitterCtx context, bool isLoad)
        {
            OpCodeSimdMemMs64 op = (OpCodeSimdMemMs64)context.CurrOp;

            int offset = 0;

            for (int rep   = 0; rep   < op.Reps;   rep++)
            for (int elem  = 0; elem  < op.Elems;  elem++)
            for (int sElem = 0; sElem < op.SElems; sElem++)
            {
                int rtt = (op.Rt + rep + sElem) & 0x1f;

                if (isLoad)
                {
                    context.EmitLdarg(TranslatedSub.MemoryArgIdx);
                    context.EmitLdint(op.Rn);
                    context.EmitLdc_I8(offset);

                    context.Emit(OpCodes.Add);

                    EmitReadZxCall(context, op.Size);

                    EmitVectorInsert(context, rtt, elem, op.Size);

                    if (op.RegisterSize == RegisterSize.Simd64 && elem == op.Elems - 1)
                    {
                        EmitVectorZeroUpper(context, rtt);
                    }
                }
                else
                {
                    context.EmitLdarg(TranslatedSub.MemoryArgIdx);
                    context.EmitLdint(op.Rn);
                    context.EmitLdc_I8(offset);

                    context.Emit(OpCodes.Add);

                    EmitVectorExtractZx(context, rtt, elem, op.Size);

                    EmitWriteCall(context, op.Size);
                }

                offset += 1 << op.Size;
            }

            if (op.WBack)
            {
                EmitSimdMemWBack(context, offset);
            }
        }

        private static void EmitSimdMemSs(ILEmitterCtx context, bool isLoad)
        {
            OpCodeSimdMemSs64 op = (OpCodeSimdMemSs64)context.CurrOp;

            int offset = 0;

            void EmitMemAddress()
            {
                context.EmitLdarg(TranslatedSub.MemoryArgIdx);
                context.EmitLdint(op.Rn);
                context.EmitLdc_I8(offset);

                context.Emit(OpCodes.Add);
            }

            if (op.Replicate)
            {
                //Only loads uses the replicate mode.
                if (!isLoad)
                {
                    throw new InvalidOperationException();
                }

                int bytes = op.GetBitsCount() >> 3;
                int elems = bytes >> op.Size;

                for (int sElem = 0; sElem < op.SElems; sElem++)
                {
                    int rt = (op.Rt + sElem) & 0x1f;

                    for (int index = 0; index < elems; index++)
                    {
                        EmitMemAddress();

                        EmitReadZxCall(context, op.Size);

                        EmitVectorInsert(context, rt, index, op.Size);
                    }

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        EmitVectorZeroUpper(context, rt);
                    }

                    offset += 1 << op.Size;
                }
            }
            else
            {
                for (int sElem = 0; sElem < op.SElems; sElem++)
                {
                    int rt = (op.Rt + sElem) & 0x1f;

                    if (isLoad)
                    {
                        EmitMemAddress();

                        EmitReadZxCall(context, op.Size);

                        EmitVectorInsert(context, rt, op.Index, op.Size);
                    }
                    else
                    {
                        EmitMemAddress();

                        EmitVectorExtractZx(context, rt, op.Index, op.Size);

                        EmitWriteCall(context, op.Size);
                    }

                    offset += 1 << op.Size;
                }
            }

            if (op.WBack)
            {
                EmitSimdMemWBack(context, offset);
            }
        }

        private static void EmitSimdMemWBack(ILEmitterCtx context, int offset)
        {
            OpCodeMemReg64 op = (OpCodeMemReg64)context.CurrOp;

            context.EmitLdint(op.Rn);

            if (op.Rm != CpuThreadState.ZrIndex)
            {
                context.EmitLdint(op.Rm);
            }
            else
            {
                context.EmitLdc_I8(offset);
            }

            context.Emit(OpCodes.Add);

            context.EmitStint(op.Rn);
        }
    }
}