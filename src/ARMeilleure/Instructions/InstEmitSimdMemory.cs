using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System.Diagnostics;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitMemoryHelper;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
        public static void Ld__Vms(ArmEmitterContext context)
        {
            EmitSimdMemMs(context, isLoad: true);
        }

        public static void Ld__Vss(ArmEmitterContext context)
        {
            EmitSimdMemSs(context, isLoad: true);
        }

        public static void St__Vms(ArmEmitterContext context)
        {
            EmitSimdMemMs(context, isLoad: false);
        }

        public static void St__Vss(ArmEmitterContext context)
        {
            EmitSimdMemSs(context, isLoad: false);
        }

        private static void EmitSimdMemMs(ArmEmitterContext context, bool isLoad)
        {
            OpCodeSimdMemMs op = (OpCodeSimdMemMs)context.CurrOp;

            Operand n = GetIntOrSP(context, op.Rn);

            long offset = 0;

#pragma warning disable IDE0055 // Disable formatting
            for (int rep   = 0; rep   < op.Reps;   rep++)
            for (int elem  = 0; elem  < op.Elems;  elem++)
            for (int sElem = 0; sElem < op.SElems; sElem++)
            {
                int rtt = (op.Rt + rep + sElem) & 0x1f;

                Operand tt = GetVec(rtt);

                Operand address = context.Add(n, Const(offset));

                if (isLoad)
                {
                    EmitLoadSimd(context, address, tt, rtt, elem, op.Size);

                    if (op.RegisterSize == RegisterSize.Simd64 && elem == op.Elems - 1)
                    {
                        context.Copy(tt, context.VectorZeroUpper64(tt));
                    }
                }
                else
                {
                    EmitStoreSimd(context, address, rtt, elem, op.Size);
                }

                offset += 1 << op.Size;
            }
#pragma warning restore IDE0055

            if (op.WBack)
            {
                EmitSimdMemWBack(context, offset);
            }
        }

        private static void EmitSimdMemSs(ArmEmitterContext context, bool isLoad)
        {
            OpCodeSimdMemSs op = (OpCodeSimdMemSs)context.CurrOp;

            Operand n = GetIntOrSP(context, op.Rn);

            long offset = 0;

            if (op.Replicate)
            {
                // Only loads uses the replicate mode.
                Debug.Assert(isLoad, "Replicate mode is not valid for stores.");

                int elems = op.GetBytesCount() >> op.Size;

                for (int sElem = 0; sElem < op.SElems; sElem++)
                {
                    int rt = (op.Rt + sElem) & 0x1f;

                    Operand t = GetVec(rt);

                    Operand address = context.Add(n, Const(offset));

                    for (int index = 0; index < elems; index++)
                    {
                        EmitLoadSimd(context, address, t, rt, index, op.Size);
                    }

                    if (op.RegisterSize == RegisterSize.Simd64)
                    {
                        context.Copy(t, context.VectorZeroUpper64(t));
                    }

                    offset += 1 << op.Size;
                }
            }
            else
            {
                for (int sElem = 0; sElem < op.SElems; sElem++)
                {
                    int rt = (op.Rt + sElem) & 0x1f;

                    Operand t = GetVec(rt);

                    Operand address = context.Add(n, Const(offset));

                    if (isLoad)
                    {
                        EmitLoadSimd(context, address, t, rt, op.Index, op.Size);
                    }
                    else
                    {
                        EmitStoreSimd(context, address, rt, op.Index, op.Size);
                    }

                    offset += 1 << op.Size;
                }
            }

            if (op.WBack)
            {
                EmitSimdMemWBack(context, offset);
            }
        }

        private static void EmitSimdMemWBack(ArmEmitterContext context, long offset)
        {
            OpCodeMemReg op = (OpCodeMemReg)context.CurrOp;

            Operand n = GetIntOrSP(context, op.Rn);
            Operand m;

            if (op.Rm != RegisterAlias.Zr)
            {
                m = GetIntOrZR(context, op.Rm);
            }
            else
            {
                m = Const(offset);
            }

            context.Copy(n, context.Add(n, m));
        }
    }
}
