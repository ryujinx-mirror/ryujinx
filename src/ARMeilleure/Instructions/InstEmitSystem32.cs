using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System;
using System.Reflection;
using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit32
    {
        public static void Mcr(ArmEmitterContext context)
        {
            OpCode32System op = (OpCode32System)context.CurrOp;

            if (op.Coproc != 15 || op.Opc1 != 0)
            {
                InstEmit.Und(context);

                return;
            }

            switch (op.CRn)
            {
                case 13: // Process and Thread Info.
                    if (op.CRm != 0)
                    {
                        throw new NotImplementedException($"Unknown MRC CRm 0x{op.CRm:X} at 0x{op.Address:X} (0x{op.RawOpCode:X}).");
                    }

                    switch (op.Opc2)
                    {
                        case 2:
                            EmitSetTpidrEl0(context);
                            return;

                        default:
                            throw new NotImplementedException($"Unknown MRC Opc2 0x{op.Opc2:X} at 0x{op.Address:X} (0x{op.RawOpCode:X}).");
                    }

                case 7:
                    switch (op.CRm) // Cache and Memory barrier.
                    {
                        case 10:
                            switch (op.Opc2)
                            {
                                case 5: // Data Memory Barrier Register.
                                    return; // No-op.

                                default:
                                    throw new NotImplementedException($"Unknown MRC Opc2 0x{op.Opc2:X16} at 0x{op.Address:X16} (0x{op.RawOpCode:X}).");
                            }

                        default:
                            throw new NotImplementedException($"Unknown MRC CRm 0x{op.CRm:X16} at 0x{op.Address:X16} (0x{op.RawOpCode:X}).");
                    }

                default:
                    throw new NotImplementedException($"Unknown MRC 0x{op.RawOpCode:X8} at 0x{op.Address:X16}.");
            }
        }

        public static void Mrc(ArmEmitterContext context)
        {
            OpCode32System op = (OpCode32System)context.CurrOp;

            if (op.Coproc != 15 || op.Opc1 != 0)
            {
                InstEmit.Und(context);

                return;
            }

            Operand result;

            switch (op.CRn)
            {
                case 13: // Process and Thread Info.
                    if (op.CRm != 0)
                    {
                        throw new NotImplementedException($"Unknown MRC CRm 0x{op.CRm:X} at 0x{op.Address:X} (0x{op.RawOpCode:X}).");
                    }

                    result = op.Opc2 switch
                    {
                        2 => EmitGetTpidrEl0(context),
                        3 => EmitGetTpidrroEl0(context),
                        _ => throw new NotImplementedException(
                            $"Unknown MRC Opc2 0x{op.Opc2:X} at 0x{op.Address:X} (0x{op.RawOpCode:X})."),
                    };

                    break;

                default:
                    throw new NotImplementedException($"Unknown MRC 0x{op.RawOpCode:X} at 0x{op.Address:X}.");
            }

            if (op.Rt == RegisterAlias.Aarch32Pc)
            {
                // Special behavior: copy NZCV flags into APSR.
                EmitSetNzcv(context, result);

                return;
            }
            else
            {
                SetIntA32(context, op.Rt, result);
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
            MethodInfo info = op.CRm switch
            {
                // Timer.
                14 => opc switch
                {
                    0 => typeof(NativeInterface).GetMethod(nameof(NativeInterface.GetCntpctEl0)),
                    _ => throw new NotImplementedException($"Unknown MRRC Opc1 0x{opc:X} at 0x{op.Address:X} (0x{op.RawOpCode:X})."),
                },
                _ => throw new NotImplementedException($"Unknown MRRC 0x{op.RawOpCode:X} at 0x{op.Address:X}."),
            };
            Operand result = context.Call(info);

            SetIntA32(context, op.Rt, context.ConvertI64ToI32(result));
            SetIntA32(context, op.CRn, context.ConvertI64ToI32(context.ShiftRightUI(result, Const(32))));
        }

        public static void Mrs(ArmEmitterContext context)
        {
            OpCode32Mrs op = (OpCode32Mrs)context.CurrOp;

            if (op.R)
            {
                throw new NotImplementedException("SPSR");
            }
            else
            {
                Operand spsr = context.ShiftLeft(GetFlag(PState.VFlag), Const((int)PState.VFlag));
                spsr = context.BitwiseOr(spsr, context.ShiftLeft(GetFlag(PState.CFlag), Const((int)PState.CFlag)));
                spsr = context.BitwiseOr(spsr, context.ShiftLeft(GetFlag(PState.ZFlag), Const((int)PState.ZFlag)));
                spsr = context.BitwiseOr(spsr, context.ShiftLeft(GetFlag(PState.NFlag), Const((int)PState.NFlag)));
                spsr = context.BitwiseOr(spsr, context.ShiftLeft(GetFlag(PState.QFlag), Const((int)PState.QFlag)));

                // TODO: Remaining flags.

                SetIntA32(context, op.Rd, spsr);
            }
        }

        public static void Msr(ArmEmitterContext context)
        {
            OpCode32MsrReg op = (OpCode32MsrReg)context.CurrOp;

            if (op.R)
            {
                throw new NotImplementedException("SPSR");
            }
            else
            {
                if ((op.Mask & 8) != 0)
                {
                    Operand value = GetIntA32(context, op.Rn);

                    EmitSetNzcv(context, value);

                    Operand q = context.BitwiseAnd(context.ShiftRightUI(value, Const((int)PState.QFlag)), Const(1));

                    SetFlag(context, PState.QFlag, q);
                }

                if ((op.Mask & 4) != 0)
                {
                    throw new NotImplementedException("APSR_g");
                }

                if ((op.Mask & 2) != 0)
                {
                    throw new NotImplementedException("CPSR_x");
                }

                if ((op.Mask & 1) != 0)
                {
                    throw new NotImplementedException("CPSR_c");
                }
            }
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
                    EmitGetFpscr(context);
                    return;
                case 0b0101: // MVFR2
                    throw new NotImplementedException("MVFR2");
                case 0b0110: // MVFR1
                    throw new NotImplementedException("MVFR1");
                case 0b0111: // MVFR0
                    throw new NotImplementedException("MVFR0");
                case 0b1000: // FPEXC
                    throw new NotImplementedException("Supervisor Only");
                default:
                    throw new NotImplementedException($"Unknown VMRS 0x{op.RawOpCode:X} at 0x{op.Address:X}.");
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
                    EmitSetFpscr(context);
                    return;
                case 0b0101: // MVFR2
                    throw new NotImplementedException("MVFR2");
                case 0b0110: // MVFR1
                    throw new NotImplementedException("MVFR1");
                case 0b0111: // MVFR0
                    throw new NotImplementedException("MVFR0");
                case 0b1000: // FPEXC
                    throw new NotImplementedException("Supervisor Only");
                default:
                    throw new NotImplementedException($"Unknown VMSR 0x{op.RawOpCode:X} at 0x{op.Address:X}.");
            }
        }

        private static void EmitSetNzcv(ArmEmitterContext context, Operand t)
        {
            Operand v = context.BitwiseAnd(context.ShiftRightUI(t, Const((int)PState.VFlag)), Const(1));
            Operand c = context.BitwiseAnd(context.ShiftRightUI(t, Const((int)PState.CFlag)), Const(1));
            Operand z = context.BitwiseAnd(context.ShiftRightUI(t, Const((int)PState.ZFlag)), Const(1));
            Operand n = context.BitwiseAnd(context.ShiftRightUI(t, Const((int)PState.NFlag)), Const(1));

            SetFlag(context, PState.VFlag, v);
            SetFlag(context, PState.CFlag, c);
            SetFlag(context, PState.ZFlag, z);
            SetFlag(context, PState.NFlag, n);
        }

        private static void EmitGetFpscr(ArmEmitterContext context)
        {
            OpCode32SimdSpecial op = (OpCode32SimdSpecial)context.CurrOp;

            Operand fpscr = Const(0);

            for (int flag = 0; flag < RegisterConsts.FpFlagsCount; flag++)
            {
                if (FPSCR.Mask.HasFlag((FPSCR)(1u << flag)))
                {
                    fpscr = context.BitwiseOr(fpscr, context.ShiftLeft(GetFpFlag((FPState)flag), Const(flag)));
                }
            }

            SetIntA32(context, op.Rt, fpscr);
        }

        private static void EmitSetFpscr(ArmEmitterContext context)
        {
            OpCode32SimdSpecial op = (OpCode32SimdSpecial)context.CurrOp;

            Operand fpscr = GetIntA32(context, op.Rt);

            for (int flag = 0; flag < RegisterConsts.FpFlagsCount; flag++)
            {
                if (FPSCR.Mask.HasFlag((FPSCR)(1u << flag)))
                {
                    SetFpFlag(context, (FPState)flag, context.BitwiseAnd(context.ShiftRightUI(fpscr, Const(flag)), Const(1)));
                }
            }

            context.UpdateArmFpMode();
        }

        private static Operand EmitGetTpidrEl0(ArmEmitterContext context)
        {
            OpCode32System op = (OpCode32System)context.CurrOp;

            Operand nativeContext = context.LoadArgument(OperandType.I64, 0);

            return context.Load(OperandType.I64, context.Add(nativeContext, Const((ulong)NativeContext.GetTpidrEl0Offset())));
        }

        private static Operand EmitGetTpidrroEl0(ArmEmitterContext context)
        {
            OpCode32System op = (OpCode32System)context.CurrOp;

            Operand nativeContext = context.LoadArgument(OperandType.I64, 0);

            return context.Load(OperandType.I64, context.Add(nativeContext, Const((ulong)NativeContext.GetTpidrroEl0Offset())));
        }

        private static void EmitSetTpidrEl0(ArmEmitterContext context)
        {
            OpCode32System op = (OpCode32System)context.CurrOp;

            Operand value = GetIntA32(context, op.Rt);

            Operand nativeContext = context.LoadArgument(OperandType.I64, 0);

            context.Store(context.Add(nativeContext, Const((ulong)NativeContext.GetTpidrEl0Offset())), context.ZeroExtend32(OperandType.I64, value));
        }
    }
}
