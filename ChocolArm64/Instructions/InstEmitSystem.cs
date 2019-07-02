using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        public static void Hint(ILEmitterCtx context)
        {
            // Execute as no-op.
        }

        public static void Isb(ILEmitterCtx context)
        {
            // Execute as no-op.
        }

        public static void Mrs(ILEmitterCtx context)
        {
            OpCodeSystem64 op = (OpCodeSystem64)context.CurrOp;

            context.EmitLdarg(TranslatedSub.StateArgIdx);

            string propName;

            switch (GetPackedId(op))
            {
                case 0b11_011_0000_0000_001: propName = nameof(CpuThreadState.CtrEl0);    break;
                case 0b11_011_0000_0000_111: propName = nameof(CpuThreadState.DczidEl0);  break;
                case 0b11_011_0100_0100_000: propName = nameof(CpuThreadState.Fpcr);      break;
                case 0b11_011_0100_0100_001: propName = nameof(CpuThreadState.Fpsr);      break;
                case 0b11_011_1101_0000_010: propName = nameof(CpuThreadState.TpidrEl0);  break;
                case 0b11_011_1101_0000_011: propName = nameof(CpuThreadState.Tpidr);     break;
                case 0b11_011_1110_0000_000: propName = nameof(CpuThreadState.CntfrqEl0); break;
                case 0b11_011_1110_0000_001: propName = nameof(CpuThreadState.CntpctEl0); break;

                default: throw new NotImplementedException($"Unknown MRS at {op.Position:x16}");
            }

            context.EmitCallPropGet(typeof(CpuThreadState), propName);

            PropertyInfo propInfo = typeof(CpuThreadState).GetProperty(propName);

            if (propInfo.PropertyType != typeof(long) &&
                propInfo.PropertyType != typeof(ulong))
            {
                context.Emit(OpCodes.Conv_U8);
            }

            context.EmitStintzr(op.Rt);
        }

        public static void Msr(ILEmitterCtx context)
        {
            OpCodeSystem64 op = (OpCodeSystem64)context.CurrOp;

            context.EmitLdarg(TranslatedSub.StateArgIdx);
            context.EmitLdintzr(op.Rt);

            string propName;

            switch (GetPackedId(op))
            {
                case 0b11_011_0100_0100_000: propName = nameof(CpuThreadState.Fpcr);     break;
                case 0b11_011_0100_0100_001: propName = nameof(CpuThreadState.Fpsr);     break;
                case 0b11_011_1101_0000_010: propName = nameof(CpuThreadState.TpidrEl0); break;

                default: throw new NotImplementedException($"Unknown MSR at {op.Position:x16}");
            }

            PropertyInfo propInfo = typeof(CpuThreadState).GetProperty(propName);

            if (propInfo.PropertyType != typeof(long) &&
                propInfo.PropertyType != typeof(ulong))
            {
                context.Emit(OpCodes.Conv_U4);
            }

            context.EmitCallPropSet(typeof(CpuThreadState), propName);
        }

        public static void Nop(ILEmitterCtx context)
        {
            // Do nothing.
        }

        public static void Sys(ILEmitterCtx context)
        {
            // This instruction is used to do some operations on the CPU like cache invalidation,
            // address translation and the like.
            // We treat it as no-op here since we don't have any cache being emulated anyway.
            OpCodeSystem64 op = (OpCodeSystem64)context.CurrOp;

            switch (GetPackedId(op))
            {
                case 0b11_011_0111_0100_001:
                {
                    // DC ZVA
                    for (int offs = 0; offs < (4 << CpuThreadState.DczSizeLog2); offs += 8)
                    {
                        context.EmitLdintzr(op.Rt);
                        context.EmitLdc_I(offs);

                        context.Emit(OpCodes.Add);

                        context.EmitLdc_I8(0);

                        InstEmitMemoryHelper.EmitWriteCall(context, 3);
                    }

                    break;
                }

                // No-op
                case 0b11_011_0111_1110_001: //DC CIVAC
                    break;
            }
        }

        private static int GetPackedId(OpCodeSystem64 op)
        {
            int id;

            id  = op.Op2 << 0;
            id |= op.CRm << 3;
            id |= op.CRn << 7;
            id |= op.Op1 << 11;
            id |= op.Op0 << 14;

            return id;
        }
    }
}
