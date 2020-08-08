using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System;
using System.Reflection;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit32
    {
        public static void Mcr(ArmEmitterContext context)
        {
            OpCode32System op = (OpCode32System)context.CurrOp;

            if (op.Coproc != 15)
            {
                InstEmit.Und(context);

                return;
            }

            if (op.Opc1 != 0)
            {
                throw new NotImplementedException($"Unknown MRC Opc1 0x{op.Opc1:X16} at 0x{op.Address:X16}.");
            }

            MethodInfo info;

            switch (op.CRn)
            {
                case 13: // Process and Thread Info.
                    if (op.CRm != 0)
                    {
                        throw new NotImplementedException($"Unknown MRC CRm 0x{op.CRm:X16} at 0x{op.Address:X16}.");
                    }

                    switch (op.Opc2)
                    {
                        case 2:
                            info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.SetTpidrEl032)); break;

                        default:
                            throw new NotImplementedException($"Unknown MRC Opc2 0x{op.Opc2:X16} at 0x{op.Address:X16}.");
                    }

                    break;

                case 7:
                    switch (op.CRm) // Cache and Memory barrier.
                    {
                        case 10:
                            switch (op.Opc2)
                            {
                                case 5: // Data Memory Barrier Register.
                                    return; // No-op.

                                default:
                                    throw new NotImplementedException($"Unknown MRC Opc2 0x{op.Opc2:X16} at 0x{op.Address:X16}.");
                            }

                        default:
                            throw new NotImplementedException($"Unknown MRC CRm 0x{op.CRm:X16} at 0x{op.Address:X16}.");
                    }

                default:
                    throw new NotImplementedException($"Unknown MRC 0x{op.RawOpCode:X8} at 0x{op.Address:X16}.");
            }

            context.Call(info, GetIntA32(context, op.Rt));
        }

        public static void Mrc(ArmEmitterContext context)
        {
            OpCode32System op = (OpCode32System)context.CurrOp;

            if (op.Coproc != 15)
            {
                InstEmit.Und(context);

                return;
            }

            if (op.Opc1 != 0)
            {
                throw new NotImplementedException($"Unknown MRC Opc1 0x{op.Opc1:X16} at 0x{op.Address:X16}.");
            }

            MethodInfo info;

            switch (op.CRn)
            {
                case 13: // Process and Thread Info.
                    if (op.CRm != 0)
                    {
                        throw new NotImplementedException($"Unknown MRC CRm 0x{op.CRm:X16} at 0x{op.Address:X16}.");
                    }

                    switch (op.Opc2)
                    {
                        case 2:
                            info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetTpidrEl032)); break;

                        case 3:
                            info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetTpidr32)); break;

                        default:
                            throw new NotImplementedException($"Unknown MRC Opc2 0x{op.Opc2:X16} at 0x{op.Address:X16}.");
                    }

                    break;

                default:
                    throw new NotImplementedException($"Unknown MRC 0x{op.RawOpCode:X8} at 0x{op.Address:X16}.");
            }

            if (op.Rt == RegisterAlias.Aarch32Pc)
            {
                // Special behavior: copy NZCV flags into APSR.
                EmitSetNzcv(context, context.Call(info));

                return;
            }
            else
            {
                SetIntA32(context, op.Rt, context.Call(info));
            }
        }

        public static void Mrrc(ArmEmitterContext context)
        {
            OpCode32System op = (OpCode32System)context.CurrOp;

            if (op.Coproc != 15)
            {
                InstEmit.Und(context);

                return;
            }

            int opc = op.MrrcOp;

            MethodInfo info;

            switch (op.CRm)
            {
                case 14: // Timer.
                    switch (opc)
                    {
                        case 0:
                            info = typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetCntpctEl0)); break;

                        default:
                            throw new NotImplementedException($"Unknown MRRC Opc1 0x{opc:X16} at 0x{op.Address:X16}.");
                    }

                    break;

                default:
                    throw new NotImplementedException($"Unknown MRRC 0x{op.RawOpCode:X8} at 0x{op.Address:X16}.");
            }

            Operand result = context.Call(info);

            SetIntA32(context, op.Rt, context.ConvertI64ToI32(result));
            SetIntA32(context, op.CRn, context.ConvertI64ToI32(context.ShiftRightUI(result, Const(32))));
        }

        public static void Nop(ArmEmitterContext context) { }

        public static void Vmrs(ArmEmitterContext context)
        {
            OpCode32SimdSpecial op = (OpCode32SimdSpecial)context.CurrOp;

            if (op.Rt == RegisterAlias.Aarch32Pc && op.Sreg == 0b0001)
            {
                // Special behavior: copy NZCV flags into APSR.
                SetFlag(context, PState.VFlag, GetFpFlag(FPState.VFlag));
                SetFlag(context, PState.CFlag, GetFpFlag(FPState.CFlag));
                SetFlag(context, PState.ZFlag, GetFpFlag(FPState.ZFlag));
                SetFlag(context, PState.NFlag, GetFpFlag(FPState.NFlag));

                return;
            }

            switch (op.Sreg)
            {
                case 0b0000: // FPSID
                    throw new NotImplementedException("Supervisor Only");
                case 0b0001: // FPSCR
                    EmitGetFpscr(context); return;
                case 0b0101: // MVFR2
                    throw new NotImplementedException("MVFR2");
                case 0b0110: // MVFR1
                    throw new NotImplementedException("MVFR1");
                case 0b0111: // MVFR0
                    throw new NotImplementedException("MVFR0");
                case 0b1000: // FPEXC
                    throw new NotImplementedException("Supervisor Only");
                default:
                    throw new NotImplementedException($"Unknown VMRS 0x{op.RawOpCode:X8} at 0x{op.Address:X16}.");
            }
        }

        public static void Vmsr(ArmEmitterContext context)
        {
            OpCode32SimdSpecial op = (OpCode32SimdSpecial)context.CurrOp;

            switch (op.Sreg)
            {
                case 0b0000: // FPSID
                    throw new NotImplementedException("Supervisor Only");
                case 0b0001: // FPSCR
                    EmitSetFpscr(context); return;
                case 0b0101: // MVFR2
                    throw new NotImplementedException("MVFR2");
                case 0b0110: // MVFR1
                    throw new NotImplementedException("MVFR1");
                case 0b0111: // MVFR0
                    throw new NotImplementedException("MVFR0");
                case 0b1000: // FPEXC
                    throw new NotImplementedException("Supervisor Only");
                default:
                    throw new NotImplementedException($"Unknown VMSR 0x{op.RawOpCode:X8} at 0x{op.Address:X16}.");
            }
        }

        private static void EmitSetNzcv(ArmEmitterContext context, Operand t)
        {
            Operand v = context.ShiftRightUI(t, Const((int)PState.VFlag));
            v = context.BitwiseAnd(v, Const(1));

            Operand c = context.ShiftRightUI(t, Const((int)PState.CFlag));
            c = context.BitwiseAnd(c, Const(1));

            Operand z = context.ShiftRightUI(t, Const((int)PState.ZFlag));
            z = context.BitwiseAnd(z, Const(1));

            Operand n = context.ShiftRightUI(t, Const((int)PState.NFlag));
            n = context.BitwiseAnd(n, Const(1));

            SetFlag(context, PState.VFlag, v);
            SetFlag(context, PState.CFlag, c);
            SetFlag(context, PState.ZFlag, z);
            SetFlag(context, PState.NFlag, n);
        }

        private static void EmitGetFpscr(ArmEmitterContext context)
        {
            OpCode32SimdSpecial op = (OpCode32SimdSpecial)context.CurrOp;

            Operand vSh = context.ShiftLeft(GetFpFlag(FPState.VFlag), Const((int)FPState.VFlag));
            Operand cSh = context.ShiftLeft(GetFpFlag(FPState.CFlag), Const((int)FPState.CFlag));
            Operand zSh = context.ShiftLeft(GetFpFlag(FPState.ZFlag), Const((int)FPState.ZFlag));
            Operand nSh = context.ShiftLeft(GetFpFlag(FPState.NFlag), Const((int)FPState.NFlag));

            Operand nzcvSh = context.BitwiseOr(context.BitwiseOr(nSh, zSh), context.BitwiseOr(cSh, vSh));

            Operand fpscr = context.Call(typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetFpscr)));

            SetIntA32(context, op.Rt, context.BitwiseOr(nzcvSh, fpscr));
        }

        private static void EmitSetFpscr(ArmEmitterContext context)
        {
            OpCode32SimdSpecial op = (OpCode32SimdSpecial)context.CurrOp;

            Operand t = GetIntA32(context, op.Rt);

            Operand v = context.ShiftRightUI(t, Const((int)FPState.VFlag));
            v = context.BitwiseAnd(v, Const(1));

            Operand c = context.ShiftRightUI(t, Const((int)FPState.CFlag));
            c = context.BitwiseAnd(c, Const(1));

            Operand z = context.ShiftRightUI(t, Const((int)FPState.ZFlag));
            z = context.BitwiseAnd(z, Const(1));

            Operand n = context.ShiftRightUI(t, Const((int)FPState.NFlag));
            n = context.BitwiseAnd(n, Const(1));

            SetFpFlag(context, FPState.VFlag, v);
            SetFpFlag(context, FPState.CFlag, c);
            SetFpFlag(context, FPState.ZFlag, z);
            SetFpFlag(context, FPState.NFlag, n);

            context.Call(typeof(NativeInterface).GetMethod(nameof(NativeInterface.SetFpscr)), t);
        }
    }
}
