using ARMeilleure.Decoders;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using ARMeilleure.Translation;
using System;
using static ARMeilleure.Instructions.InstEmitHelper;
using static ARMeilleure.Instructions.InstEmitMemoryHelper;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit32
    {
        private const int ByteSizeLog2 = 0;
        private const int HWordSizeLog2 = 1;
        private const int WordSizeLog2 = 2;
        private const int DWordSizeLog2 = 3;

        [Flags]
        enum AccessType
        {
            Store = 0,
            Signed = 1,
            Load = 2,
            Ordered = 4,
            Exclusive = 8,

            LoadZx = Load,
            LoadSx = Load | Signed,
        }

        public static void Ldm(ArmEmitterContext context)
        {
            IOpCode32MemMult op = (IOpCode32MemMult)context.CurrOp;

            Operand n = GetIntA32(context, op.Rn);

            Operand baseAddress = context.Add(n, Const(op.Offset));

            bool writesToPc = (op.RegisterMask & (1 << RegisterAlias.Aarch32Pc)) != 0;

            bool writeBack = op.PostOffset != 0 && (op.Rn != RegisterAlias.Aarch32Pc || !writesToPc);

            if (writeBack)
            {
                SetIntA32(context, op.Rn, context.Add(n, Const(op.PostOffset)));
            }

            int mask = op.RegisterMask;
            int offset = 0;

            for (int register = 0; mask != 0; mask >>= 1, register++)
            {
                if ((mask & 1) != 0)
                {
                    Operand address = context.Add(baseAddress, Const(offset));

                    EmitLoadZx(context, address, register, WordSizeLog2);

                    offset += 4;
                }
            }
        }

        public static void Ldr(ArmEmitterContext context)
        {
            EmitLoadOrStore(context, WordSizeLog2, AccessType.LoadZx);
        }

        public static void Ldrb(ArmEmitterContext context)
        {
            EmitLoadOrStore(context, ByteSizeLog2, AccessType.LoadZx);
        }

        public static void Ldrd(ArmEmitterContext context)
        {
            EmitLoadOrStore(context, DWordSizeLog2, AccessType.LoadZx);
        }

        public static void Ldrh(ArmEmitterContext context)
        {
            EmitLoadOrStore(context, HWordSizeLog2, AccessType.LoadZx);
        }

        public static void Ldrsb(ArmEmitterContext context)
        {
            EmitLoadOrStore(context, ByteSizeLog2, AccessType.LoadSx);
        }

        public static void Ldrsh(ArmEmitterContext context)
        {
            EmitLoadOrStore(context, HWordSizeLog2, AccessType.LoadSx);
        }

        public static void Stm(ArmEmitterContext context)
        {
            IOpCode32MemMult op = (IOpCode32MemMult)context.CurrOp;

            Operand n = context.Copy(GetIntA32(context, op.Rn));

            Operand baseAddress = context.Add(n, Const(op.Offset));

            int mask = op.RegisterMask;
            int offset = 0;

            for (int register = 0; mask != 0; mask >>= 1, register++)
            {
                if ((mask & 1) != 0)
                {
                    Operand address = context.Add(baseAddress, Const(offset));

                    EmitStore(context, address, register, WordSizeLog2);

                    // Note: If Rn is also specified on the register list,
                    // and Rn is the first register on this list, then the
                    // value that is written to memory is the unmodified value,
                    // before the write back. If it is on the list, but it's
                    // not the first one, then the value written to memory
                    // varies between CPUs.
                    if (offset == 0 && op.PostOffset != 0)
                    {
                        // Emit write back after the first write.
                        SetIntA32(context, op.Rn, context.Add(n, Const(op.PostOffset)));
                    }

                    offset += 4;
                }
            }
        }

        public static void Str(ArmEmitterContext context)
        {
            EmitLoadOrStore(context, WordSizeLog2, AccessType.Store);
        }

        public static void Strb(ArmEmitterContext context)
        {
            EmitLoadOrStore(context, ByteSizeLog2, AccessType.Store);
        }

        public static void Strd(ArmEmitterContext context)
        {
            EmitLoadOrStore(context, DWordSizeLog2, AccessType.Store);
        }

        public static void Strh(ArmEmitterContext context)
        {
            EmitLoadOrStore(context, HWordSizeLog2, AccessType.Store);
        }

        private static void EmitLoadOrStore(ArmEmitterContext context, int size, AccessType accType)
        {
            IOpCode32Mem op = (IOpCode32Mem)context.CurrOp;

            Operand n = context.Copy(GetIntA32AlignedPC(context, op.Rn));
            Operand m = GetMemM(context, setCarry: false);

            Operand temp = default;

            if (op.Index || op.WBack)
            {
                temp = op.Add
                    ? context.Add(n, m)
                    : context.Subtract(n, m);
            }

            if (op.WBack)
            {
                SetIntA32(context, op.Rn, temp);
            }

            Operand address;

            if (op.Index)
            {
                address = temp;
            }
            else
            {
                address = n;
            }

            if ((accType & AccessType.Load) != 0)
            {
                void Load(int rt, int offs, int loadSize)
                {
                    Operand addr = context.Add(address, Const(offs));

                    if ((accType & AccessType.Signed) != 0)
                    {
                        EmitLoadSx32(context, addr, rt, loadSize);
                    }
                    else
                    {
                        EmitLoadZx(context, addr, rt, loadSize);
                    }
                }

                if (size == DWordSizeLog2)
                {
                    Operand lblBigEndian = Label();
                    Operand lblEnd = Label();

                    context.BranchIfTrue(lblBigEndian, GetFlag(PState.EFlag));

                    Load(op.Rt, 0, WordSizeLog2);
                    Load(op.Rt2, 4, WordSizeLog2);

                    context.Branch(lblEnd);

                    context.MarkLabel(lblBigEndian);

                    Load(op.Rt2, 0, WordSizeLog2);
                    Load(op.Rt, 4, WordSizeLog2);

                    context.MarkLabel(lblEnd);
                }
                else
                {
                    Load(op.Rt, 0, size);
                }
            }
            else
            {
                void Store(int rt, int offs, int storeSize)
                {
                    Operand addr = context.Add(address, Const(offs));

                    EmitStore(context, addr, rt, storeSize);
                }

                if (size == DWordSizeLog2)
                {
                    Operand lblBigEndian = Label();
                    Operand lblEnd = Label();

                    context.BranchIfTrue(lblBigEndian, GetFlag(PState.EFlag));

                    Store(op.Rt, 0, WordSizeLog2);
                    Store(op.Rt2, 4, WordSizeLog2);

                    context.Branch(lblEnd);

                    context.MarkLabel(lblBigEndian);

                    Store(op.Rt2, 0, WordSizeLog2);
                    Store(op.Rt, 4, WordSizeLog2);

                    context.MarkLabel(lblEnd);
                }
                else
                {
                    Store(op.Rt, 0, size);
                }
            }
        }

        public static void Adr(ArmEmitterContext context)
        {
            IOpCode32Adr op = (IOpCode32Adr)context.CurrOp;
            SetIntA32(context, op.Rd, Const(op.Immediate));
        }
    }
}
