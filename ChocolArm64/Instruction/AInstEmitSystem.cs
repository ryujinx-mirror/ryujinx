using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Hint(AILEmitterCtx Context)
        {
            //Execute as no-op.
        }

        public static void Mrs(AILEmitterCtx Context)
        {
            AOpCodeSystem Op = (AOpCodeSystem)Context.CurrOp;

            Context.EmitLdarg(ATranslatedSub.StateArgIdx);

            string PropName;

            switch (GetPackedId(Op))
            {
                case 0b11_011_0000_0000_001: PropName = nameof(AThreadState.CtrEl0);    break;
                case 0b11_011_0000_0000_111: PropName = nameof(AThreadState.DczidEl0);  break;
                case 0b11_011_0100_0100_000: PropName = nameof(AThreadState.Fpcr);      break;
                case 0b11_011_0100_0100_001: PropName = nameof(AThreadState.Fpsr);      break;
                case 0b11_011_1101_0000_010: PropName = nameof(AThreadState.TpidrEl0);  break;
                case 0b11_011_1101_0000_011: PropName = nameof(AThreadState.Tpidr);     break;
                case 0b11_011_1110_0000_000: PropName = nameof(AThreadState.CntfrqEl0); break;
                case 0b11_011_1110_0000_001: PropName = nameof(AThreadState.CntpctEl0); break;

                default: throw new NotImplementedException($"Unknown MRS at {Op.Position:x16}");
            }

            Context.EmitCallPropGet(typeof(AThreadState), PropName);

            PropertyInfo PropInfo = typeof(AThreadState).GetProperty(PropName);

            if (PropInfo.PropertyType != typeof(long) &&
                PropInfo.PropertyType != typeof(ulong))
            {
                Context.Emit(OpCodes.Conv_U8);
            }

            Context.EmitStintzr(Op.Rt);
        }

        public static void Msr(AILEmitterCtx Context)
        {
            AOpCodeSystem Op = (AOpCodeSystem)Context.CurrOp;

            Context.EmitLdarg(ATranslatedSub.StateArgIdx);
            Context.EmitLdintzr(Op.Rt);

            string PropName;

            switch (GetPackedId(Op))
            {
                case 0b11_011_0100_0100_000: PropName = nameof(AThreadState.Fpcr);     break;
                case 0b11_011_0100_0100_001: PropName = nameof(AThreadState.Fpsr);     break;
                case 0b11_011_1101_0000_010: PropName = nameof(AThreadState.TpidrEl0); break;

                default: throw new NotImplementedException($"Unknown MSR at {Op.Position:x16}");
            }

            PropertyInfo PropInfo = typeof(AThreadState).GetProperty(PropName);

            if (PropInfo.PropertyType != typeof(long) &&
                PropInfo.PropertyType != typeof(ulong))
            {
                Context.Emit(OpCodes.Conv_U4);
            }

            Context.EmitCallPropSet(typeof(AThreadState), PropName);
        }

        public static void Nop(AILEmitterCtx Context)
        {
            //Do nothing.
        }

        public static void Sys(AILEmitterCtx Context)
        {
            //This instruction is used to do some operations on the CPU like cache invalidation,
            //address translation and the like.
            //We treat it as no-op here since we don't have any cache being emulated anyway.
            AOpCodeSystem Op = (AOpCodeSystem)Context.CurrOp;

            switch (GetPackedId(Op))
            {
                case 0b11_011_0111_0100_001:
                {
                    //DC ZVA
                    for (int Offs = 0; Offs < (4 << AThreadState.DczSizeLog2); Offs += 8)
                    {
                        Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
                        Context.EmitLdintzr(Op.Rt);
                        Context.EmitLdc_I(Offs);

                        Context.Emit(OpCodes.Add);

                        Context.EmitLdc_I8(0);

                        AInstEmitMemoryHelper.EmitWriteCall(Context, 3);
                    }

                    break;
                }

                //No-op
                case 0b11_011_0111_1110_001: //DC CIVAC
                    break;
            }
        }

        private static int GetPackedId(AOpCodeSystem Op)
        {
            int Id;

            Id  = Op.Op2 << 0;
            Id |= Op.CRm << 3;
            Id |= Op.CRn << 7;
            Id |= Op.Op1 << 11;
            Id |= Op.Op0 << 14;

            return Id;
        }
    }
}