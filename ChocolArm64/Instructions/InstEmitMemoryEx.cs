using ChocolArm64.Decoders;
using ChocolArm64.IntermediateRepresentation;
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
            context.EmitLdarg(TranslatedSub.StateArgIdx);

            context.EmitPrivateCall(typeof(CpuThreadState), nameof(CpuThreadState.ClearExclusiveAddress));
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
            EmitLoad(context, accType, pair: false);
        }

        private static void EmitLdp(ILEmitterCtx context, AccessType accType)
        {
            EmitLoad(context, accType, pair: true);
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

            context.EmitLdint(op.Rn);
            context.EmitSttmp();

            if (exclusive)
            {
                context.EmitLdarg(TranslatedSub.StateArgIdx);
                context.EmitLdtmp();

                context.EmitPrivateCall(typeof(CpuThreadState), nameof(CpuThreadState.SetExclusiveAddress));
            }

            void WriteExclusiveValue(string propName)
            {
                context.Emit(OpCodes.Dup);

                if (op.Size < 3)
                {
                    context.Emit(OpCodes.Conv_U8);
                }

                context.EmitSttmp2();
                context.EmitLdarg(TranslatedSub.StateArgIdx);
                context.EmitLdtmp2();

                context.EmitCallPrivatePropSet(typeof(CpuThreadState), propName);
            }

            if (pair)
            {
                // Exclusive loads should be atomic. For pairwise loads, we need to
                // read all the data at once. For a 32-bits pairwise load, we do a
                // simple 64-bits load, for a 128-bits load, we need to call a special
                // method to read 128-bits atomically.
                if (op.Size == 2)
                {
                    context.EmitLdtmp();

                    EmitReadZxCall(context, 3);

                    context.Emit(OpCodes.Dup);

                    // Mask low half.
                    context.Emit(OpCodes.Conv_U4);

                    if (exclusive)
                    {
                        WriteExclusiveValue(nameof(CpuThreadState.ExclusiveValueLow));
                    }

                    context.EmitStintzr(op.Rt);

                    // Shift high half.
                    context.EmitLsr(32);
                    context.Emit(OpCodes.Conv_U4);

                    if (exclusive)
                    {
                        WriteExclusiveValue(nameof(CpuThreadState.ExclusiveValueHigh));
                    }

                    context.EmitStintzr(op.Rt2);
                }
                else if (op.Size == 3)
                {
                    context.EmitLdarg(TranslatedSub.MemoryArgIdx);
                    context.EmitLdtmp();

                    context.EmitPrivateCall(typeof(MemoryManager), nameof(MemoryManager.AtomicReadInt128));

                    context.Emit(OpCodes.Dup);

                    // Load low part of the vector.
                    context.EmitLdc_I4(0);
                    context.EmitLdc_I4(3);

                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorExtractIntZx));

                    if (exclusive)
                    {
                        WriteExclusiveValue(nameof(CpuThreadState.ExclusiveValueLow));
                    }

                    context.EmitStintzr(op.Rt);

                    // Load high part of the vector.
                    context.EmitLdc_I4(1);
                    context.EmitLdc_I4(3);

                    VectorHelper.EmitCall(context, nameof(VectorHelper.VectorExtractIntZx));

                    if (exclusive)
                    {
                        WriteExclusiveValue(nameof(CpuThreadState.ExclusiveValueHigh));
                    }

                    context.EmitStintzr(op.Rt2);
                }
                else
                {
                    throw new InvalidOperationException($"Invalid load size of {1 << op.Size} bytes.");
                }
            }
            else
            {
                // 8, 16, 32 or 64-bits (non-pairwise) load.
                context.EmitLdtmp();

                EmitReadZxCall(context, op.Size);

                if (exclusive)
                {
                    WriteExclusiveValue(nameof(CpuThreadState.ExclusiveValueLow));
                }

                context.EmitStintzr(op.Rt);
            }
        }

        public static void Pfrm(ILEmitterCtx context)
        {
            // Memory Prefetch, execute as no-op.
        }

        public static void Stlr(ILEmitterCtx context)  => EmitStr(context, AccessType.Ordered);
        public static void Stlxr(ILEmitterCtx context) => EmitStr(context, AccessType.OrderedEx);
        public static void Stxr(ILEmitterCtx context)  => EmitStr(context, AccessType.Exclusive);
        public static void Stxp(ILEmitterCtx context)  => EmitStp(context, AccessType.Exclusive);
        public static void Stlxp(ILEmitterCtx context) => EmitStp(context, AccessType.OrderedEx);

        private static void EmitStr(ILEmitterCtx context, AccessType accType)
        {
            EmitStore(context, accType, pair: false);
        }

        private static void EmitStp(ILEmitterCtx context, AccessType accType)
        {
            EmitStore(context, accType, pair: true);
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

            if (exclusive)
            {
                ILLabel lblEx  = new ILLabel();
                ILLabel lblEnd = new ILLabel();

                context.EmitLdarg(TranslatedSub.StateArgIdx);
                context.EmitLdint(op.Rn);

                context.EmitPrivateCall(typeof(CpuThreadState), nameof(CpuThreadState.CheckExclusiveAddress));

                context.Emit(OpCodes.Brtrue_S, lblEx);

                // Address check failed, set error right away and do not store anything.
                context.EmitLdc_I4(1);
                context.EmitStintzr(op.Rs);

                context.Emit(OpCodes.Br, lblEnd);

                // Address check passed.
                context.MarkLabel(lblEx);

                context.EmitLdarg(TranslatedSub.MemoryArgIdx);
                context.EmitLdint(op.Rn);

                context.EmitLdarg(TranslatedSub.StateArgIdx);

                context.EmitCallPrivatePropGet(typeof(CpuThreadState), nameof(CpuThreadState.ExclusiveValueLow));

                void EmitCast()
                {
                    // The input should be always int64.
                    switch (op.Size)
                    {
                        case 0: context.Emit(OpCodes.Conv_U1); break;
                        case 1: context.Emit(OpCodes.Conv_U2); break;
                        case 2: context.Emit(OpCodes.Conv_U4); break;
                    }
                }

                EmitCast();

                if (pair)
                {
                    context.EmitLdarg(TranslatedSub.StateArgIdx);

                    context.EmitCallPrivatePropGet(typeof(CpuThreadState), nameof(CpuThreadState.ExclusiveValueHigh));

                    EmitCast();

                    context.EmitLdintzr(op.Rt);

                    EmitCast();

                    context.EmitLdintzr(op.Rt2);

                    EmitCast();

                    switch (op.Size)
                    {
                        case 2: context.EmitPrivateCall(typeof(MemoryManager), nameof(MemoryManager.AtomicCompareExchange2xInt32)); break;
                        case 3: context.EmitPrivateCall(typeof(MemoryManager), nameof(MemoryManager.AtomicCompareExchangeInt128));  break;

                        default: throw new InvalidOperationException($"Invalid store size of {1 << op.Size} bytes.");
                    }
                }
                else
                {
                    context.EmitLdintzr(op.Rt);

                    EmitCast();

                    switch (op.Size)
                    {
                        case 0: context.EmitCall(typeof(MemoryManager), nameof(MemoryManager.AtomicCompareExchangeByte));  break;
                        case 1: context.EmitCall(typeof(MemoryManager), nameof(MemoryManager.AtomicCompareExchangeInt16)); break;
                        case 2: context.EmitCall(typeof(MemoryManager), nameof(MemoryManager.AtomicCompareExchangeInt32)); break;
                        case 3: context.EmitCall(typeof(MemoryManager), nameof(MemoryManager.AtomicCompareExchangeInt64)); break;

                        default: throw new InvalidOperationException($"Invalid store size of {1 << op.Size} bytes.");
                    }
                }

                // The value returned is a bool, true if the values compared
                // were equal and the new value was written, false otherwise.
                // We need to invert this result, as on ARM 1 indicates failure,
                // and 0 success on those instructions.
                context.EmitLdc_I4(1);

                context.Emit(OpCodes.Xor);
                context.Emit(OpCodes.Dup);
                context.Emit(OpCodes.Conv_U8);

                context.EmitStintzr(op.Rs);

                // Only clear the exclusive monitor if the store was successful (Rs = false).
                context.Emit(OpCodes.Brtrue_S, lblEnd);

                Clrex(context);

                context.MarkLabel(lblEnd);
            }
            else
            {
                void EmitWriteCall(int rt, long offset)
                {
                    context.EmitLdint(op.Rn);

                    if (offset != 0)
                    {
                        context.EmitLdc_I8(offset);

                        context.Emit(OpCodes.Add);
                    }

                    context.EmitLdintzr(rt);

                    InstEmitMemoryHelper.EmitWriteCall(context, op.Size);
                }

                EmitWriteCall(op.Rt, 0);

                if (pair)
                {
                    EmitWriteCall(op.Rt2, 1 << op.Size);
                }
            }
        }

        private static void EmitBarrier(ILEmitterCtx context)
        {
            // Note: This barrier is most likely not necessary, and probably
            // doesn't make any difference since we need to do a ton of stuff
            // (software MMU emulation) to read or write anything anyway.
            context.EmitCall(typeof(Thread), nameof(Thread.MemoryBarrier));
        }
    }
}