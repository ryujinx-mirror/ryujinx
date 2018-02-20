using ChocolArm64.Decoder;
using ChocolArm64.Memory;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;
using System.Threading;

using static ChocolArm64.Instruction.AInstEmitMemoryHelper;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        [Flags]
        private enum AccessType
        {
            None      = 0,
            Ordered   = 1,
            Exclusive = 2,
            OrderedEx = Ordered | Exclusive
        }

        public static void Clrex(AILEmitterCtx Context)
        {
            EmitMemoryCall(Context, nameof(AMemory.ClearExclusive));
        }

        public static void Dmb(AILEmitterCtx Context) => EmitBarrier(Context);
        public static void Dsb(AILEmitterCtx Context) => EmitBarrier(Context);

        public static void Ldar(AILEmitterCtx Context)  => EmitLdr(Context, AccessType.Ordered);
        public static void Ldaxr(AILEmitterCtx Context) => EmitLdr(Context, AccessType.OrderedEx);
        public static void Ldxr(AILEmitterCtx Context)  => EmitLdr(Context, AccessType.Exclusive);
        public static void Ldxp(AILEmitterCtx Context)  => EmitLdp(Context, AccessType.Exclusive);
        public static void Ldaxp(AILEmitterCtx Context) => EmitLdp(Context, AccessType.OrderedEx);

        private static void EmitLdr(AILEmitterCtx Context, AccessType AccType)
        {
            EmitLoad(Context, AccType, false);
        }

        private static void EmitLdp(AILEmitterCtx Context, AccessType AccType)
        {
            EmitLoad(Context, AccType, true);
        }

        private static void EmitLoad(AILEmitterCtx Context, AccessType AccType, bool Pair)
        {
            AOpCodeMemEx Op = (AOpCodeMemEx)Context.CurrOp;

            Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
            Context.EmitLdint(Op.Rn);

            EmitReadZxCall(Context, Op.Size);

            Context.EmitStintzr(Op.Rt);

            if (Pair)
            {
                Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
                Context.EmitLdint(Op.Rn);
                Context.EmitLdc_I(8 << Op.Size);

                Context.Emit(OpCodes.Add);

                EmitReadZxCall(Context, Op.Size);

                Context.EmitStintzr(Op.Rt2);
            }

            if (AccType.HasFlag(AccessType.Exclusive))
            {
                EmitMemoryCall(Context, nameof(AMemory.SetExclusive), Op.Rn);
            }

            if (AccType.HasFlag(AccessType.Ordered))
            {
                EmitBarrier(Context);
            }
        }

        public static void Pfrm(AILEmitterCtx Context)
        {
            //Memory Prefetch, execute as no-op.
        }

        public static void Stlr(AILEmitterCtx Context)  => EmitStr(Context, AccessType.Ordered);
        public static void Stlxr(AILEmitterCtx Context) => EmitStr(Context, AccessType.OrderedEx);
        public static void Stxr(AILEmitterCtx Context)  => EmitStr(Context, AccessType.Exclusive);
        public static void Stxp(AILEmitterCtx Context)  => EmitStp(Context, AccessType.Exclusive);
        public static void Stlxp(AILEmitterCtx Context) => EmitStp(Context, AccessType.OrderedEx);

        private static void EmitStr(AILEmitterCtx Context, AccessType AccType)
        {
            EmitStore(Context, AccType, false);
        }

        private static void EmitStp(AILEmitterCtx Context, AccessType AccType)
        {
            EmitStore(Context, AccType, true);
        }

        private static void EmitStore(AILEmitterCtx Context, AccessType AccType, bool Pair)
        {
            AOpCodeMemEx Op = (AOpCodeMemEx)Context.CurrOp;

            if (AccType.HasFlag(AccessType.Ordered))
            {
                EmitBarrier(Context);
            }

            AILLabel LblEx  = new AILLabel();
            AILLabel LblEnd = new AILLabel();

            if (AccType.HasFlag(AccessType.Exclusive))
            {
                EmitMemoryCall(Context, nameof(AMemory.TestExclusive), Op.Rn);

                Context.Emit(OpCodes.Brtrue_S, LblEx);

                Context.EmitLdc_I8(1);
                Context.EmitStintzr(Op.Rs);

                Context.Emit(OpCodes.Br_S, LblEnd);
            }

            Context.MarkLabel(LblEx);

            Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
            Context.EmitLdint(Op.Rn);
            Context.EmitLdintzr(Op.Rt);

            EmitWriteCall(Context, Op.Size);

            if (Pair)
            {
                Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
                Context.EmitLdint(Op.Rn);
                Context.EmitLdc_I(8 << Op.Size);

                Context.Emit(OpCodes.Add);

                Context.EmitLdintzr(Op.Rt2);

                EmitWriteCall(Context, Op.Size);
            }

            if (AccType.HasFlag(AccessType.Exclusive))
            {
                Context.EmitLdc_I8(0);
                Context.EmitStintzr(Op.Rs);

                Clrex(Context);
            }

            Context.MarkLabel(LblEnd);
        }

        private static void EmitMemoryCall(AILEmitterCtx Context, string Name, int Rn = -1)
        {
            Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
            Context.EmitLdarg(ATranslatedSub.StateArgIdx);

            if (Rn != -1)
            {
                Context.EmitLdint(Rn);
            }

            Context.EmitCall(typeof(AMemory), Name);
        }

        private static void EmitBarrier(AILEmitterCtx Context)
        {
            //Note: This barrier is most likely not necessary, and probably
            //doesn't make any difference since we need to do a ton of stuff
            //(software MMU emulation) to read or write anything anyway.
            Context.EmitCall(typeof(Thread), nameof(Thread.MemoryBarrier));
        }
    }
}