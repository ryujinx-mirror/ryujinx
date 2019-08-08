using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
        private const int DczSizeLog2 = 4;

        public static void Hint(ArmEmitterContext context)
        {
            // Execute as no-op.
        }

        public static void Isb(ArmEmitterContext context)
        {
            // Execute as no-op.
        }

        public static void Mrs(ArmEmitterContext context)
        {
            OpCodeSystem op = (OpCodeSystem)context.CurrOp;

            Delegate dlg;

            switch (GetPackedId(op))
            {
                case 0b11_011_0000_0000_001: dlg = new _U64(NativeInterface.GetCtrEl0);    break;
                case 0b11_011_0000_0000_111: dlg = new _U64(NativeInterface.GetDczidEl0);  break;
                case 0b11_011_0100_0100_000: dlg = new _U64(NativeInterface.GetFpcr);      break;
                case 0b11_011_0100_0100_001: dlg = new _U64(NativeInterface.GetFpsr);      break;
                case 0b11_011_1101_0000_010: dlg = new _U64(NativeInterface.GetTpidrEl0);  break;
                case 0b11_011_1101_0000_011: dlg = new _U64(NativeInterface.GetTpidr);     break;
                case 0b11_011_1110_0000_000: dlg = new _U64(NativeInterface.GetCntfrqEl0); break;
                case 0b11_011_1110_0000_001: dlg = new _U64(NativeInterface.GetCntpctEl0); break;

                default: throw new NotImplementedException($"Unknown MRS 0x{op.RawOpCode:X8} at 0x{op.Address:X16}.");
            }

            SetIntOrZR(context, op.Rt, context.Call(dlg));
        }

        public static void Msr(ArmEmitterContext context)
        {
            OpCodeSystem op = (OpCodeSystem)context.CurrOp;

            Delegate dlg;

            switch (GetPackedId(op))
            {
                case 0b11_011_0100_0100_000: dlg = new _Void_U64(NativeInterface.SetFpcr);     break;
                case 0b11_011_0100_0100_001: dlg = new _Void_U64(NativeInterface.SetFpsr);     break;
                case 0b11_011_1101_0000_010: dlg = new _Void_U64(NativeInterface.SetTpidrEl0); break;

                default: throw new NotImplementedException($"Unknown MSR 0x{op.RawOpCode:X8} at 0x{op.Address:X16}.");
            }

            context.Call(dlg, GetIntOrZR(context, op.Rt));
        }

        public static void Nop(ArmEmitterContext context)
        {
            // Do nothing.
        }

        public static void Sys(ArmEmitterContext context)
        {
            // This instruction is used to do some operations on the CPU like cache invalidation,
            // address translation and the like.
            // We treat it as no-op here since we don't have any cache being emulated anyway.
            OpCodeSystem op = (OpCodeSystem)context.CurrOp;

            switch (GetPackedId(op))
            {
                case 0b11_011_0111_0100_001:
                {
                    // DC ZVA
                    Operand t = GetIntOrZR(context, op.Rt);

                    for (long offset = 0; offset < (4 << DczSizeLog2); offset += 8)
                    {
                        Operand address = context.Add(t, Const(offset));

                        context.Call(new _Void_U64_U64(NativeInterface.WriteUInt64), address, Const(0L));
                    }

                    break;
                }

                // No-op
                case 0b11_011_0111_1110_001: //DC CIVAC
                    break;
            }
        }

        private static int GetPackedId(OpCodeSystem op)
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
