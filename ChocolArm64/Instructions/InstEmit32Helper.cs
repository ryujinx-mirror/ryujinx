using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System;

namespace ChocolArm64.Instructions
{
    static class InstEmit32Helper
    {
        public static bool IsThumb(OpCode64 op)
        {
            return op is OpCodeT16;
        }

        public static void EmitLoadFromRegister(ILEmitterCtx context, int register)
        {
            if (register == RegisterAlias.Aarch32Pc)
            {
                OpCode32 op = (OpCode32)context.CurrOp;

                context.EmitLdc_I4((int)op.GetPc());
            }
            else
            {
                context.EmitLdint(InstEmit32Helper.GetRegisterAlias(context.Mode, register));
            }
        }

        public static int GetRegisterAlias(Aarch32Mode mode, int register)
        {
            //Only registers >= 8 are banked, with registers in the range [8, 12] being
            //banked for the FIQ mode, and registers 13 and 14 being banked for all modes.
            if ((uint)register < 8)
            {
                return register;
            }

            return GetBankedRegisterAlias(mode, register);
        }

        public static int GetBankedRegisterAlias(Aarch32Mode mode, int register)
        {
            switch (register)
            {
                case 8: return mode == Aarch32Mode.Fiq
                    ? RegisterAlias.R8Fiq
                    : RegisterAlias.R8Usr;

                case 9: return mode == Aarch32Mode.Fiq
                    ? RegisterAlias.R9Fiq
                    : RegisterAlias.R9Usr;

                case 10: return mode == Aarch32Mode.Fiq
                    ? RegisterAlias.R10Fiq
                    : RegisterAlias.R10Usr;

                case 11: return mode == Aarch32Mode.Fiq
                    ? RegisterAlias.R11Fiq
                    : RegisterAlias.R11Usr;

                case 12: return mode == Aarch32Mode.Fiq
                    ? RegisterAlias.R12Fiq
                    : RegisterAlias.R12Usr;

                case 13:
                    switch (mode)
                    {
                        case Aarch32Mode.User:
                        case Aarch32Mode.System:      return RegisterAlias.SpUsr;
                        case Aarch32Mode.Fiq:         return RegisterAlias.SpFiq;
                        case Aarch32Mode.Irq:         return RegisterAlias.SpIrq;
                        case Aarch32Mode.Supervisor:  return RegisterAlias.SpSvc;
                        case Aarch32Mode.Abort:       return RegisterAlias.SpAbt;
                        case Aarch32Mode.Hypervisor:  return RegisterAlias.SpHyp;
                        case Aarch32Mode.Undefined:   return RegisterAlias.SpUnd;

                        default: throw new ArgumentException(nameof(mode));
                    }

                case 14:
                    switch (mode)
                    {
                        case Aarch32Mode.User:
                        case Aarch32Mode.Hypervisor:
                        case Aarch32Mode.System:      return RegisterAlias.LrUsr;
                        case Aarch32Mode.Fiq:         return RegisterAlias.LrFiq;
                        case Aarch32Mode.Irq:         return RegisterAlias.LrIrq;
                        case Aarch32Mode.Supervisor:  return RegisterAlias.LrSvc;
                        case Aarch32Mode.Abort:       return RegisterAlias.LrAbt;
                        case Aarch32Mode.Undefined:   return RegisterAlias.LrUnd;

                        default: throw new ArgumentException(nameof(mode));
                    }

                default: throw new ArgumentOutOfRangeException(nameof(register));
            }
        }
    }
}
