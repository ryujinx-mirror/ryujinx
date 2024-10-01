using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitMemoryHelper;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
        public static void Adr(ArmEmitterContext context)
        {
            OpCodeAdr op = (OpCodeAdr)context.CurrOp;

            SetIntOrZR(context, op.Rd, Const(op.Address + (ulong)op.Immediate));
        }

        public static void Adrp(ArmEmitterContext context)
        {
            OpCodeAdr op = (OpCodeAdr)context.CurrOp;

            ulong address = (op.Address & ~0xfffUL) + ((ulong)op.Immediate << 12);

            SetIntOrZR(context, op.Rd, Const(address));
        }

        public static void Ldr(ArmEmitterContext context) => EmitLdr(context, signed: false);
        public static void Ldrs(ArmEmitterContext context) => EmitLdr(context, signed: true);

        private static void EmitLdr(ArmEmitterContext context, bool signed)
        {
            OpCodeMem op = (OpCodeMem)context.CurrOp;

            Operand address = GetAddress(context);

            if (signed && op.Extend64)
            {
                EmitLoadSx64(context, address, op.Rt, op.Size);
            }
            else if (signed)
            {
                EmitLoadSx32(context, address, op.Rt, op.Size);
            }
            else
            {
                EmitLoadZx(context, address, op.Rt, op.Size);
            }

            EmitWBackIfNeeded(context, address);
        }

        public static void Ldr_Literal(ArmEmitterContext context)
        {
            IOpCodeLit op = (IOpCodeLit)context.CurrOp;

            if (op.Prefetch)
            {
                return;
            }

            if (op.Signed)
            {
                EmitLoadSx64(context, Const(op.Immediate), op.Rt, op.Size);
            }
            else
            {
                EmitLoadZx(context, Const(op.Immediate), op.Rt, op.Size);
            }
        }

        public static void Ldp(ArmEmitterContext context)
        {
            OpCodeMemPair op = (OpCodeMemPair)context.CurrOp;

            void EmitLoad(int rt, Operand ldAddr)
            {
                if (op.Extend64)
                {
                    EmitLoadSx64(context, ldAddr, rt, op.Size);
                }
                else
                {
                    EmitLoadZx(context, ldAddr, rt, op.Size);
                }
            }

            Operand address = GetAddress(context);
            Operand address2 = GetAddress(context, 1L << op.Size);

            EmitLoad(op.Rt, address);
            EmitLoad(op.Rt2, address2);

            EmitWBackIfNeeded(context, address);
        }

        public static void Str(ArmEmitterContext context)
        {
            OpCodeMem op = (OpCodeMem)context.CurrOp;

            Operand address = GetAddress(context);

            EmitStore(context, address, op.Rt, op.Size);

            EmitWBackIfNeeded(context, address);
        }

        public static void Stp(ArmEmitterContext context)
        {
            OpCodeMemPair op = (OpCodeMemPair)context.CurrOp;

            Operand address = GetAddress(context);
            Operand address2 = GetAddress(context, 1L << op.Size);

            EmitStore(context, address, op.Rt, op.Size);
            EmitStore(context, address2, op.Rt2, op.Size);

            EmitWBackIfNeeded(context, address);
        }

        private static Operand GetAddress(ArmEmitterContext context, long addend = 0)
        {
            Operand address = default;

            switch (context.CurrOp)
            {
                case OpCodeMemImm op:
                    {
                        address = context.Copy(GetIntOrSP(context, op.Rn));

                        // Pre-indexing.
                        if (!op.PostIdx)
                        {
                            address = context.Add(address, Const(op.Immediate + addend));
                        }
                        else if (addend != 0)
                        {
                            address = context.Add(address, Const(addend));
                        }

                        break;
                    }

                case OpCodeMemReg op:
                    {
                        Operand n = GetIntOrSP(context, op.Rn);

                        Operand m = GetExtendedM(context, op.Rm, op.IntType);

                        if (op.Shift)
                        {
                            m = context.ShiftLeft(m, Const(op.Size));
                        }

                        address = context.Add(n, m);

                        if (addend != 0)
                        {
                            address = context.Add(address, Const(addend));
                        }

                        break;
                    }
            }

            return address;
        }

        private static void EmitWBackIfNeeded(ArmEmitterContext context, Operand address)
        {
            // Check whenever the current OpCode has post-indexed write back, if so write it.
            if (context.CurrOp is OpCodeMemImm op && op.WBack)
            {
                if (op.PostIdx)
                {
                    address = context.Add(address, Const(op.Immediate));
                }

                SetIntOrSP(context, op.Rn, address);
            }
        }
    }
}
