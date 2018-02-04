using ChocolArm64.Decoder;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Mrs(AILEmitterCtx Context)
        {
            AOpCodeSystem Op = (AOpCodeSystem)Context.CurrOp;

            Context.EmitLdarg(ATranslatedSub.RegistersArgIdx);

            Context.EmitLdc_I4(Op.Op0);
            Context.EmitLdc_I4(Op.Op1);
            Context.EmitLdc_I4(Op.CRn);
            Context.EmitLdc_I4(Op.CRm);
            Context.EmitLdc_I4(Op.Op2);

            Context.EmitCall(typeof(ARegisters), nameof(ARegisters.GetSystemReg));

            Context.EmitStintzr(Op.Rt);
        }

        public static void Msr(AILEmitterCtx Context)
        {
            AOpCodeSystem Op = (AOpCodeSystem)Context.CurrOp;

            Context.EmitLdarg(ATranslatedSub.RegistersArgIdx);

            Context.EmitLdc_I4(Op.Op0);
            Context.EmitLdc_I4(Op.Op1);
            Context.EmitLdc_I4(Op.CRn);
            Context.EmitLdc_I4(Op.CRm);
            Context.EmitLdc_I4(Op.Op2);
            Context.EmitLdintzr(Op.Rt);

            Context.EmitCall(typeof(ARegisters), nameof(ARegisters.SetSystemReg));
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

            int Id;

            Id  = Op.Op2 << 0;
            Id |= Op.CRm << 3;
            Id |= Op.CRn << 7;
            Id |= Op.Op1 << 11;

            switch (Id)
            {
                case 0b011_0111_0100_001:
                {
                    //DC ZVA
                    for (int Offs = 0; Offs < 64; Offs += 8)
                    {
                        Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
                        Context.EmitLdint(Op.Rt);
                        Context.EmitLdc_I(Offs);

                        Context.Emit(OpCodes.Add);

                        Context.EmitLdc_I8(0);

                        AInstEmitMemoryHelper.EmitWriteCall(Context, 3);
                    }
                    break;
                }
            }
        }
    }
}