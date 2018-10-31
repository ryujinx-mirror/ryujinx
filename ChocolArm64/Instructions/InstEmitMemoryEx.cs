using ChocolArm64.Decoders;
using ChocolArm64.Memory;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection.Emit;
using System.Threading;

using static ChocolArm64.Instructions.InstEmitMemoryHelper;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        [Flags]
        private enum AccessType
        {
            None      = 0,
            Ordered   = 1,
            Exclusive = 2,
            OrderedEx = Ordered | Exclusive
        }

        public static void Clrex(ILEmitterCtx context)
        {
            EmitMemoryCall(context, nameof(MemoryManager.ClearExclusive));
        }

        public static void Dmb(ILEmitterCtx context) => EmitBarrier(context);
        public static void Dsb(ILEmitterCtx context) => EmitBarrier(context);

        public static void Ldar(ILEmitterCtx context)  => EmitLdr(context, AccessType.Ordered);
        public static void Ldaxr(ILEmitterCtx context) => EmitLdr(context, AccessType.OrderedEx);
        public static void Ldxr(ILEmitterCtx context)  => EmitLdr(context, AccessType.Exclusive);
        public static void Ldxp(ILEmitterCtx context)  => EmitLdp(context, AccessType.Exclusive);
        public static void Ldaxp(ILEmitterCtx context) => EmitLdp(context, AccessType.OrderedEx);

        private static void EmitLdr(ILEmitterCtx context, AccessType accType)
        {
            EmitLoad(context, accType, false);
        }

        private static void EmitLdp(ILEmitterCtx context, AccessType accType)
        {
            EmitLoad(context, accType, true);
        }

        private static void EmitLoad(ILEmitterCtx context, AccessType accType, bool pair)
        {
            OpCodeMemEx64 op = (OpCodeMemEx64)context.CurrOp;

            bool ordered   = (accType & AccessType.Ordered)   != 0;
            bool exclusive = (accType & AccessType.Exclusive) != 0;

            if (ordered)
            {
                EmitBarrier(context);
            }

            if (exclusive)
            {
                EmitMemoryCall(context, nameof(MemoryManager.SetExclusive), op.Rn);
            }

            context.EmitLdint(op.Rn);
            context.EmitSttmp();

            context.EmitLdarg(TranslatedSub.MemoryArgIdx);
            context.EmitLdtmp();

            EmitReadZxCall(context, op.Size);

            context.EmitStintzr(op.Rt);

            if (pair)
            {
                context.EmitLdarg(TranslatedSub.MemoryArgIdx);
                context.EmitLdtmp();
                context.EmitLdc_I8(1 << op.Size);

                context.Emit(OpCodes.Add);

                EmitReadZxCall(context, op.Size);

                context.EmitStintzr(op.Rt2);
            }
        }

        public static void Pfrm(ILEmitterCtx context)
        {
            //Memory Prefetch, execute as no-op.
        }

        public static void Stlr(ILEmitterCtx context)  => EmitStr(context, AccessType.Ordered);
        public static void Stlxr(ILEmitterCtx context) => EmitStr(context, AccessType.OrderedEx);
        public static void Stxr(ILEmitterCtx context)  => EmitStr(context, AccessType.Exclusive);
        public static void Stxp(ILEmitterCtx context)  => EmitStp(context, AccessType.Exclusive);
        public static void Stlxp(ILEmitterCtx context) => EmitStp(context, AccessType.OrderedEx);

        private static void EmitStr(ILEmitterCtx context, AccessType accType)
        {
            EmitStore(context, accType, false);
        }

        private static void EmitStp(ILEmitterCtx context, AccessType accType)
        {
            EmitStore(context, accType, true);
        }

        private static void EmitStore(ILEmitterCtx context, AccessType accType, bool pair)
        {
            OpCodeMemEx64 op = (OpCodeMemEx64)context.CurrOp;

            bool ordered   = (accType & AccessType.Ordered)   != 0;
            bool exclusive = (accType & AccessType.Exclusive) != 0;

            if (ordered)
            {
                EmitBarrier(context);
            }

            ILLabel lblEx  = new ILLabel();
            ILLabel lblEnd = new ILLabel();

            if (exclusive)
            {
                EmitMemoryCall(context, nameof(MemoryManager.TestExclusive), op.Rn);

                context.Emit(OpCodes.Brtrue_S, lblEx);

                context.EmitLdc_I8(1);
                context.EmitStintzr(op.Rs);

                context.Emit(OpCodes.Br_S, lblEnd);
            }

            context.MarkLabel(lblEx);

            context.EmitLdarg(TranslatedSub.MemoryArgIdx);
            context.EmitLdint(op.Rn);
            context.EmitLdintzr(op.Rt);

            EmitWriteCall(context, op.Size);

            if (pair)
            {
                context.EmitLdarg(TranslatedSub.MemoryArgIdx);
                context.EmitLdint(op.Rn);
                context.EmitLdc_I8(1 << op.Size);

                context.Emit(OpCodes.Add);

                context.EmitLdintzr(op.Rt2);

                EmitWriteCall(context, op.Size);
            }

            if (exclusive)
            {
                context.EmitLdc_I8(0);
                context.EmitStintzr(op.Rs);

                EmitMemoryCall(context, nameof(MemoryManager.ClearExclusiveForStore));
            }

            context.MarkLabel(lblEnd);
        }

        private static void EmitMemoryCall(ILEmitterCtx context, string name, int rn = -1)
        {
            context.EmitLdarg(TranslatedSub.MemoryArgIdx);
            context.EmitLdarg(TranslatedSub.StateArgIdx);

            context.EmitCallPropGet(typeof(CpuThreadState), nameof(CpuThreadState.Core));

            if (rn != -1)
            {
                context.EmitLdint(rn);
            }

            context.EmitCall(typeof(MemoryManager), name);
        }

        private static void EmitBarrier(ILEmitterCtx context)
        {
            //Note: This barrier is most likely not necessary, and probably
            //doesn't make any difference since we need to do a ton of stuff
            //(software MMU emulation) to read or write anything anyway.
            context.EmitCall(typeof(Thread), nameof(Thread.MemoryBarrier));
        }
    }
}