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

                context.FlagAttributeRead(src.Value);

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

        public static void Atom(EmitterContext context)
        {
            OpCodeAtom op = (OpCodeAtom)context.CurrOp;

            ReductionType type = (ReductionType)op.RawOpCode.Extract(49, 2);

            int sOffset = (op.RawOpCode.Extract(28, 20) << 12) >> 12;

            (Operand addrLow, Operand addrHigh) = Get40BitsAddress(context, op.Ra, op.Extended, sOffset);

            Operand value = GetSrcB(context);

            Operand res = EmitAtomicOp(
                context,
                Instruction.MrGlobal,
                op.AtomicOp,
                type,
                addrLow,
                addrHigh,
                value);

            context.Copy(GetDest(context), res);
        }

        public static void Atoms(EmitterContext context)
        {
            OpCodeAtom op = (OpCodeAtom)context.CurrOp;

            ReductionType type = op.RawOpCode.Extract(28, 2) switch
            {
                0 => ReductionType.U32,
                1 => ReductionType.S32,
                2 => ReductionType.U64,
                _ => ReductionType.S64
            };

            Operand offset = context.ShiftRightU32(GetSrcA(context), Const(2));

            int sOffset = (op.RawOpCode.Extract(30, 22) << 10) >> 10;

            offset = context.IAdd(offset, Const(sOffset));

            Operand value = GetSrcB(context);

            Operand res = EmitAtomicOp(
                context,
                Instruction.MrShared,
                op.AtomicOp,
                type,
                offset,
                Const(0),
                value);

            context.Copy(GetDest(context), res);
        }

        public static void Bar(EmitterContext context)
        {
            OpCodeBarrier op = (OpCodeBarrier)context.CurrOp;

            // TODO: Support other modes.
            if (op.Mode == BarrierMode.Sync)
            {
                context.Barrier();
            }
            else
            {
                context.Config.GpuAccessor.Log($"Invalid barrier mode: {op.Mode}.");
            }
        }

        public static void Ipa(EmitterContext context)
        {
            OpCodeIpa op = (OpCodeIpa)context.CurrOp;

            context.FlagAttributeRead(op.AttributeOffset);

            Operand res = Attribute(op.AttributeOffset);

            if (op.AttributeOffset >= AttributeConsts.UserAttributeBase &&
                op.AttributeOffset <  AttributeConsts.UserAttributeEnd)
            {
                int index = (op.AttributeOffset - AttributeConsts.UserAttributeBase) >> 4;

                if (context.Config.ImapTypes[index].GetFirstUsedType() == PixelImap.Perspective)
                {
                    res = context.FPMultiply(res, Attribute(AttributeConsts.PositionW));
                }
            }

            if (op.Mode == InterpolationMode.Default)
            {
                Operand srcB = GetSrcB(context);

                res = context.FPMultiply(res, srcB);
            }

            res = context.FPSaturate(res, op.Saturate);

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
                context.Config.GpuAccessor.Log($"Invalid LDC size: {op.Size}.");
            }

            bool isSmallInt = op.Size < IntegerSize.B32;

            int count = op.Size == IntegerSize.B64 ? 2 : 1;

            Operand slot = Const(op.Slot);
            Operand srcA = GetSrcA(context);

            if (op.IndexMode == CbIndexMode.Is ||
                op.IndexMode == CbIndexMode.Isl)
            {
                slot = context.IAdd(slot, context.BitfieldExtractU32(srcA, Const(16), Const(16)));
                srcA = context.BitwiseAnd(srcA, Const(0xffff));
            }

            Operand addr = context.IAdd(srcA, Const(op.Offset));

            Operand wordOffset = context.ShiftRightU32(addr, Const(2));

            Operand bitOffset = GetBitOffset(context, addr);

            for (int index = 0; index < count; index++)
            {
                Register rd = new Register(op.Rd.Index + index, RegisterType.Gpr);

                if (rd.IsRZ)
                {
                    break;
                }

                Operand offset = context.IAdd(wordOffset, Const(index));

                Operand value = context.LoadConstant(slot, offset);

                if (isSmallInt)
                {
                    value = ExtractSmallInt(context, op.Size, bitOffset, value);
                }

                context.Copy(Register(rd), value);
            }
        }

        public static void Ldg(EmitterContext context)
        {
            EmitLoadGlobal(context);
        }

        public static void Lds(EmitterContext context)
        {
            EmitLoad(context, MemoryRegion.Shared);
        }

        public static void Membar(EmitterContext context)
        {
            OpCodeMemoryBarrier op = (OpCodeMemoryBarrier)context.CurrOp;

            if (op.Level == BarrierLevel.Cta)
            {
                context.GroupMemoryBarrier();
            }
            else
            {
                context.MemoryBarrier();
            }
        }

        public static void Out(EmitterContext context)
        {
            OpCode op = context.CurrOp;

            bool emit = op.RawOpCode.Extract(39);
            bool cut  = op.RawOpCode.Extract(40);

            if (!(emit || cut))
            {
                context.Config.GpuAccessor.Log("Invalid OUT encoding.");
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

            (Operand addrLow, Operand addrHigh) = Get40BitsAddress(context, op.Ra, op.Extended, op.Offset);

            EmitAtomicOp(
                context,
                Instruction.MrGlobal,
                op.AtomicOp,
                op.Type,
                addrLow,
                addrHigh,
                GetDest(context));
        }

        public static void St(EmitterContext context)
        {
            EmitStore(context, MemoryRegion.Local);
        }

        public static void Stg(EmitterContext context)
        {
            EmitStoreGlobal(context);
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
            Operand        addrLow,
            Operand        addrHigh,
            Operand        value)
        {
            Operand res = Const(0);

            switch (op)
            {
                case AtomicOp.Add:
                    if (type == ReductionType.S32 || type == ReductionType.U32)
                    {
                        res = context.AtomicAdd(mr, addrLow, addrHigh, value);
                    }
                    else
                    {
                        context.Config.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                case AtomicOp.BitwiseAnd:
                    if (type == ReductionType.S32 || type == ReductionType.U32)
                    {
                        res = context.AtomicAnd(mr, addrLow, addrHigh, value);
                    }
                    else
                    {
                        context.Config.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                case AtomicOp.BitwiseExclusiveOr:
                    if (type == ReductionType.S32 || type == ReductionType.U32)
                    {
                        res = context.AtomicXor(mr, addrLow, addrHigh, value);
                    }
                    else
                    {
                        context.Config.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                case AtomicOp.BitwiseOr:
                    if (type == ReductionType.S32 || type == ReductionType.U32)
                    {
                        res = context.AtomicOr(mr, addrLow, addrHigh, value);
                    }
                    else
                    {
                        context.Config.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                case AtomicOp.Maximum:
                    if (type == ReductionType.S32)
                    {
                        res = context.AtomicMaxS32(mr, addrLow, addrHigh, value);
                    }
                    else if (type == ReductionType.U32)
                    {
                        res = context.AtomicMaxU32(mr, addrLow, addrHigh, value);
                    }
                    else
                    {
                        context.Config.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                case AtomicOp.Minimum:
                    if (type == ReductionType.S32)
                    {
                        res = context.AtomicMinS32(mr, addrLow, addrHigh, value);
                    }
                    else if (type == ReductionType.U32)
                    {
                        res = context.AtomicMinU32(mr, addrLow, addrHigh, value);
                    }
                    else
                    {
                        context.Config.GpuAccessor.Log($"Invalid reduction type: {type}.");
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
                context.Config.GpuAccessor.Log($"Invalid load size: {op.Size}.");
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

        private static void EmitLoadGlobal(EmitterContext context)
        {
            OpCodeMemory op = (OpCodeMemory)context.CurrOp;

            bool isSmallInt = op.Size < IntegerSize.B32;

            int count = GetVectorCount(op.Size);

            (Operand addrLow, Operand addrHigh) = Get40BitsAddress(context, op.Ra, op.Extended, op.Offset);

            Operand bitOffset = GetBitOffset(context, addrLow);

            for (int index = 0; index < count; index++)
            {
                Register rd = new Register(op.Rd.Index + index, RegisterType.Gpr);

                if (rd.IsRZ)
                {
                    break;
                }

                Operand value = context.LoadGlobal(context.IAdd(addrLow, Const(index * 4)), addrHigh);

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
                context.Config.GpuAccessor.Log($"Invalid store size: {op.Size}.");
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
                bool isRz = op.Rd.IsRZ;

                Register rd = new Register(isRz ? op.Rd.Index : op.Rd.Index + index, RegisterType.Gpr);

                Operand value = Register(rd);

                Operand offset = context.IAdd(wordOffset, Const(index));

                if (isSmallInt)
                {
                    Operand word = null;

                    switch (region)
                    {
                        case MemoryRegion.Local:  word = context.LoadLocal (offset); break;
                        case MemoryRegion.Shared: word = context.LoadShared(offset); break;
                    }

                    value = InsertSmallInt(context, op.Size, bitOffset, word, value);
                }

                switch (region)
                {
                    case MemoryRegion.Local:  context.StoreLocal (offset, value); break;
                    case MemoryRegion.Shared: context.StoreShared(offset, value); break;
                }
            }
        }

        private static void EmitStoreGlobal(EmitterContext context)
        {
            OpCodeMemory op = (OpCodeMemory)context.CurrOp;

            bool isSmallInt = op.Size < IntegerSize.B32;

            int count = GetVectorCount(op.Size);

            (Operand addrLow, Operand addrHigh) = Get40BitsAddress(context, op.Ra, op.Extended, op.Offset);

            Operand bitOffset = GetBitOffset(context, addrLow);

            for (int index = 0; index < count; index++)
            {
                bool isRz = op.Rd.IsRZ;

                Register rd = new Register(isRz ? op.Rd.Index : op.Rd.Index + index, RegisterType.Gpr);

                Operand value = Register(rd);

                if (isSmallInt)
                {
                    Operand word = context.LoadGlobal(addrLow, addrHigh);

                    value = InsertSmallInt(context, op.Size, bitOffset, word, value);
                }

                context.StoreGlobal(context.IAdd(addrLow, Const(index * 4)), addrHigh, value);
            }
        }

        private static int GetVectorCount(IntegerSize size)
        {
            switch (size)
            {
                case IntegerSize.B64:
                    return 2;
                case IntegerSize.B128:
                case IntegerSize.UB128:
                    return 4;
            }

            return 1;
        }

        private static (Operand, Operand) Get40BitsAddress(
            EmitterContext context,
            Register ra,
            bool extended,
            int offset)
        {
            Operand addrLow = GetSrcA(context);
            Operand addrHigh;

            if (extended && !ra.IsRZ)
            {
                addrHigh = Register(ra.Index + 1, RegisterType.Gpr);
            }
            else
            {
                addrHigh = Const(0);
            }

            Operand offs = Const(offset);

            addrLow = context.IAdd(addrLow, offs);

            if (extended)
            {
                Operand carry = context.ICompareLessUnsigned(addrLow, offs);

                addrHigh = context.IAdd(addrHigh, context.ConditionalSelect(carry, Const(1), Const(0)));
            }

            return (addrLow, addrHigh);
        }

        private static Operand GetBitOffset(EmitterContext context, Operand baseOffset)
        {
            // Note: bit offset = (baseOffset & 0b11) * 8.
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