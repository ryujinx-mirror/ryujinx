using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        private enum MemoryRegion
        {
            Global,
            Local,
            Shared
        }

        public static void Ald(EmitterContext context)
        {
            OpCodeAttribute op = (OpCodeAttribute)context.CurrOp;

            Operand primVertex = context.Copy(GetSrcC(context));

            for (int index = 0; index < op.Count; index++)
            {
                Register rd = new Register(op.Rd.Index + index, RegisterType.Gpr);

                if (rd.IsRZ)
                {
                    break;
                }

                Operand src = Attribute(op.AttributeOffset + index * 4);

                context.Copy(Register(rd), context.LoadAttribute(src, primVertex));
            }
        }

        public static void Ast(EmitterContext context)
        {
            OpCodeAttribute op = (OpCodeAttribute)context.CurrOp;

            for (int index = 0; index < op.Count; index++)
            {
                if (op.Rd.Index + index > RegisterConsts.RegisterZeroIndex)
                {
                    break;
                }

                Register rd = new Register(op.Rd.Index + index, RegisterType.Gpr);

                Operand dest = Attribute(op.AttributeOffset + index * 4);

                context.Copy(dest, Register(rd));
            }
        }

        public static void Atoms(EmitterContext context)
        {
            OpCodeAtom op = (OpCodeAtom)context.CurrOp;

            Operand mem = context.ShiftRightU32(GetSrcA(context), Const(2));

            mem = context.IAdd(mem, Const(op.Offset));

            Operand value = GetSrcB(context);

            Operand res = EmitAtomicOp(context, Instruction.MrShared, op.AtomicOp, op.Type, mem, value);

            context.Copy(GetDest(context), res);
        }

        public static void Ipa(EmitterContext context)
        {
            OpCodeIpa op = (OpCodeIpa)context.CurrOp;

            InterpolationQualifier iq = InterpolationQualifier.None;

            switch (op.Mode)
            {
                case InterpolationMode.Pass: iq = InterpolationQualifier.NoPerspective; break;
            }

            Operand srcA = Attribute(op.AttributeOffset, iq);

            Operand srcB = GetSrcB(context);

            Operand res = context.FPSaturate(srcA, op.Saturate);

            context.Copy(GetDest(context), res);
        }

        public static void Isberd(EmitterContext context)
        {
            // This instruction performs a load from ISBE memory,
            // however it seems to be only used to get some vertex
            // input data, so we instead propagate the offset so that
            // it can be used on the attribute load.
            context.Copy(GetDest(context), GetSrcA(context));
        }

        public static void Ld(EmitterContext context)
        {
            EmitLoad(context, MemoryRegion.Local);
        }

        public static void Ldc(EmitterContext context)
        {
            OpCodeLdc op = (OpCodeLdc)context.CurrOp;

            if (op.Size > IntegerSize.B64)
            {
                // TODO: Warning.
            }

            bool isSmallInt = op.Size < IntegerSize.B32;

            int count = op.Size == IntegerSize.B64 ? 2 : 1;

            Operand wordOffset = context.ShiftRightU32(GetSrcA(context), Const(2));

            wordOffset = context.IAdd(wordOffset, Const(op.Offset));

            Operand bitOffset = GetBitOffset(context, GetSrcA(context));

            for (int index = 0; index < count; index++)
            {
                Register rd = new Register(op.Rd.Index + index, RegisterType.Gpr);

                if (rd.IsRZ)
                {
                    break;
                }

                Operand offset = context.IAdd(wordOffset, Const(index));

                Operand value = context.LoadConstant(Const(op.Slot), offset);

                if (isSmallInt)
                {
                    value = ExtractSmallInt(context, op.Size, wordOffset, value);
                }

                context.Copy(Register(rd), value);
            }
        }

        public static void Ldg(EmitterContext context)
        {
            EmitLoad(context, MemoryRegion.Global);
        }

        public static void Lds(EmitterContext context)
        {
            EmitLoad(context, MemoryRegion.Shared);
        }

        public static void Out(EmitterContext context)
        {
            OpCode op = context.CurrOp;

            bool emit = op.RawOpCode.Extract(39);
            bool cut  = op.RawOpCode.Extract(40);

            if (!(emit || cut))
            {
                // TODO: Warning.
            }

            if (emit)
            {
                context.EmitVertex();
            }

            if (cut)
            {
                context.EndPrimitive();
            }
        }

        public static void Red(EmitterContext context)
        {
            OpCodeRed op = (OpCodeRed)context.CurrOp;

            Operand offset = context.IAdd(GetSrcA(context), Const(op.Offset));

            Operand mem = context.ShiftRightU32(offset, Const(2));

            EmitAtomicOp(context, Instruction.MrGlobal, op.AtomicOp, op.Type, mem, GetDest(context));
        }

        public static void St(EmitterContext context)
        {
            EmitStore(context, MemoryRegion.Local);
        }

        public static void Stg(EmitterContext context)
        {
            EmitStore(context, MemoryRegion.Global);
        }

        public static void Sts(EmitterContext context)
        {
            EmitStore(context, MemoryRegion.Shared);
        }

        private static Operand EmitAtomicOp(
            EmitterContext context,
            Instruction    mr,
            AtomicOp       op,
            ReductionType  type,
            Operand        mem,
            Operand        value)
        {
            Operand res = null;

            switch (op)
            {
                case AtomicOp.Add:
                    if (type == ReductionType.S32 || type == ReductionType.U32)
                    {
                        res = context.AtomicAdd(mr, mem, value);
                    }
                    else
                    {
                        // Not supported or invalid.
                    }
                    break;
                case AtomicOp.BitwiseAnd:
                    if (type == ReductionType.S32 || type == ReductionType.U32)
                    {
                        res = context.AtomicAnd(mr, mem, value);
                    }
                    else
                    {
                        // Not supported or invalid.
                    }
                    break;
                case AtomicOp.BitwiseExclusiveOr:
                    if (type == ReductionType.S32 || type == ReductionType.U32)
                    {
                        res = context.AtomicXor(mr, mem, value);
                    }
                    else
                    {
                        // Not supported or invalid.
                    }
                    break;
                case AtomicOp.BitwiseOr:
                    if (type == ReductionType.S32 || type == ReductionType.U32)
                    {
                        res = context.AtomicOr(mr, mem, value);
                    }
                    else
                    {
                        // Not supported or invalid.
                    }
                    break;
                case AtomicOp.Maximum:
                    if (type == ReductionType.S32)
                    {
                        res = context.AtomicMaxS32(mr, mem, value);
                    }
                    else if (type == ReductionType.U32)
                    {
                        res = context.AtomicMaxU32(mr, mem, value);
                    }
                    else
                    {
                        // Not supported or invalid.
                    }
                    break;
                case AtomicOp.Minimum:
                    if (type == ReductionType.S32)
                    {
                        res = context.AtomicMinS32(mr, mem, value);
                    }
                    else if (type == ReductionType.U32)
                    {
                        res = context.AtomicMinU32(mr, mem, value);
                    }
                    else
                    {
                        // Not supported or invalid.
                    }
                    break;
            }

            return res;
        }

        private static void EmitLoad(EmitterContext context, MemoryRegion region)
        {
            OpCodeMemory op = (OpCodeMemory)context.CurrOp;

            if (op.Size > IntegerSize.B128)
            {
                // TODO: Warning.
            }

            bool isSmallInt = op.Size < IntegerSize.B32;

            int count = 1;

            switch (op.Size)
            {
                case IntegerSize.B64:  count = 2; break;
                case IntegerSize.B128: count = 4; break;
            }

            Operand baseOffset = context.IAdd(GetSrcA(context), Const(op.Offset));

            // Word offset = byte offset / 4 (one word = 4 bytes).
            Operand wordOffset = context.ShiftRightU32(baseOffset, Const(2));

            Operand bitOffset = GetBitOffset(context, baseOffset);

            for (int index = 0; index < count; index++)
            {
                Register rd = new Register(op.Rd.Index + index, RegisterType.Gpr);

                if (rd.IsRZ)
                {
                    break;
                }

                Operand offset = context.IAdd(wordOffset, Const(index));

                Operand value = null;

                switch (region)
                {
                    case MemoryRegion.Global: value = context.LoadGlobal(offset); break;
                    case MemoryRegion.Local:  value = context.LoadLocal (offset); break;
                    case MemoryRegion.Shared: value = context.LoadShared(offset); break;
                }

                if (isSmallInt)
                {
                    value = ExtractSmallInt(context, op.Size, bitOffset, value);
                }

                context.Copy(Register(rd), value);
            }
        }

        private static void EmitStore(EmitterContext context, MemoryRegion region)
        {
            OpCodeMemory op = (OpCodeMemory)context.CurrOp;

            if (op.Size > IntegerSize.B128)
            {
                // TODO: Warning.
            }

            bool isSmallInt = op.Size < IntegerSize.B32;

            int count = 1;

            switch (op.Size)
            {
                case IntegerSize.B64:  count = 2; break;
                case IntegerSize.B128: count = 4; break;
            }

            Operand baseOffset = context.IAdd(GetSrcA(context), Const(op.Offset));

            Operand wordOffset = context.ShiftRightU32(baseOffset, Const(2));

            Operand bitOffset = GetBitOffset(context, baseOffset);

            for (int index = 0; index < count; index++)
            {
                Register rd = new Register(op.Rd.Index + index, RegisterType.Gpr);

                Operand value = Register(rd);

                Operand offset = context.IAdd(wordOffset, Const(index));

                if (isSmallInt)
                {
                    Operand word = null;

                    switch (region)
                    {
                        case MemoryRegion.Global: word = context.LoadGlobal(offset); break;
                        case MemoryRegion.Local:  word = context.LoadLocal (offset); break;
                        case MemoryRegion.Shared: word = context.LoadShared(offset); break;
                    }

                    value = InsertSmallInt(context, op.Size, bitOffset, word, value);
                }

                switch (region)
                {
                    case MemoryRegion.Global: context.StoreGlobal(offset, value); break;
                    case MemoryRegion.Local:  context.StoreLocal (offset, value); break;
                    case MemoryRegion.Shared: context.StoreShared(offset, value); break;
                }

                if (rd.IsRZ)
                {
                    break;
                }
            }
        }

        private static Operand GetBitOffset(EmitterContext context, Operand baseOffset)
        {
            // Note: byte offset = (baseOffset & 0b11) * 8.
            // Addresses should be always aligned to the integer type,
            // so we don't need to take unaligned addresses into account.
            return context.ShiftLeft(context.BitwiseAnd(baseOffset, Const(3)), Const(3));
        }

        private static Operand ExtractSmallInt(
            EmitterContext context,
            IntegerSize    size,
            Operand        bitOffset,
            Operand        value)
        {
            value = context.ShiftRightU32(value, bitOffset);

            switch (size)
            {
                case IntegerSize.U8:  value = ZeroExtendTo32(context, value, 8);  break;
                case IntegerSize.U16: value = ZeroExtendTo32(context, value, 16); break;
                case IntegerSize.S8:  value = SignExtendTo32(context, value, 8);  break;
                case IntegerSize.S16: value = SignExtendTo32(context, value, 16); break;
            }

            return value;
        }

        private static Operand InsertSmallInt(
            EmitterContext context,
            IntegerSize    size,
            Operand        bitOffset,
            Operand        word,
            Operand        value)
        {
            switch (size)
            {
                case IntegerSize.U8:
                case IntegerSize.S8:
                    value = context.BitwiseAnd(value, Const(0xff));
                    value = context.BitfieldInsert(word, value, bitOffset, Const(8));
                    break;

                case IntegerSize.U16:
                case IntegerSize.S16:
                    value = context.BitwiseAnd(value, Const(0xffff));
                    value = context.BitfieldInsert(word, value, bitOffset, Const(16));
                    break;
            }

            return value;
        }
    }
}