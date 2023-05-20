using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;
using System.Numerics;

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

        public static void Atom(EmitterContext context)
        {
            InstAtom op = context.GetOp<InstAtom>();

            int sOffset = (op.Imm20 << 12) >> 12;

            (Operand addrLow, Operand addrHigh) = Get40BitsAddress(context, new Register(op.SrcA, RegisterType.Gpr), op.E, sOffset);

            Operand value = GetSrcReg(context, op.SrcB);

            Operand res = EmitAtomicOp(context, StorageKind.GlobalMemory, op.Op, op.Size, addrLow, addrHigh, value);

            context.Copy(GetDest(op.Dest), res);
        }

        public static void Atoms(EmitterContext context)
        {
            InstAtoms op = context.GetOp<InstAtoms>();

            Operand offset = context.ShiftRightU32(GetSrcReg(context, op.SrcA), Const(2));

            int sOffset = (op.Imm22 << 10) >> 10;

            offset = context.IAdd(offset, Const(sOffset));

            Operand value = GetSrcReg(context, op.SrcB);

            AtomSize size = op.AtomsSize switch
            {
                AtomsSize.S32 => AtomSize.S32,
                AtomsSize.U64 => AtomSize.U64,
                AtomsSize.S64 => AtomSize.S64,
                _ => AtomSize.U32
            };

            Operand res = EmitAtomicOp(context, StorageKind.SharedMemory, op.AtomOp, size, offset, Const(0), value);

            context.Copy(GetDest(op.Dest), res);
        }

        public static void Ldc(EmitterContext context)
        {
            InstLdc op = context.GetOp<InstLdc>();

            if (op.LsSize > LsSize2.B64)
            {
                context.Config.GpuAccessor.Log($"Invalid LDC size: {op.LsSize}.");
                return;
            }

            bool isSmallInt = op.LsSize < LsSize2.B32;

            int count = op.LsSize == LsSize2.B64 ? 2 : 1;

            Operand slot = Const(op.CbufSlot);
            Operand srcA = GetSrcReg(context, op.SrcA);

            if (op.AddressMode == AddressMode.Is || op.AddressMode == AddressMode.Isl)
            {
                slot = context.IAdd(slot, context.BitfieldExtractU32(srcA, Const(16), Const(16)));
                srcA = context.BitwiseAnd(srcA, Const(0xffff));
            }

            Operand addr = context.IAdd(srcA, Const(Imm16ToSInt(op.CbufOffset)));
            Operand wordOffset = context.ShiftRightU32(addr, Const(2));

            for (int index = 0; index < count; index++)
            {
                Register dest = new Register(op.Dest + index, RegisterType.Gpr);

                if (dest.IsRZ)
                {
                    break;
                }

                Operand offset = context.IAdd(wordOffset, Const(index));
                Operand value = EmitLoadConstant(context, slot, offset);

                if (isSmallInt)
                {
                    value = ExtractSmallInt(context, (LsSize)op.LsSize, GetBitOffset(context, addr), value);
                }

                context.Copy(Register(dest), value);
            }
        }

        public static void Ldg(EmitterContext context)
        {
            InstLdg op = context.GetOp<InstLdg>();

            EmitLdg(context, op.LsSize, op.SrcA, op.Dest, Imm24ToSInt(op.Imm24), op.E);
        }

        public static void Ldl(EmitterContext context)
        {
            InstLdl op = context.GetOp<InstLdl>();

            EmitLoad(context, MemoryRegion.Local, op.LsSize, GetSrcReg(context, op.SrcA), op.Dest, Imm24ToSInt(op.Imm24));
        }

        public static void Lds(EmitterContext context)
        {
            InstLds op = context.GetOp<InstLds>();

            EmitLoad(context, MemoryRegion.Shared, op.LsSize, GetSrcReg(context, op.SrcA), op.Dest, Imm24ToSInt(op.Imm24));
        }

        public static void Red(EmitterContext context)
        {
            InstRed op = context.GetOp<InstRed>();

            (Operand addrLow, Operand addrHigh) = Get40BitsAddress(context, new Register(op.SrcA, RegisterType.Gpr), op.E, op.Imm20);

            EmitAtomicOp(context, StorageKind.GlobalMemory, (AtomOp)op.RedOp, op.RedSize, addrLow, addrHigh, GetDest(op.SrcB));
        }

        public static void Stg(EmitterContext context)
        {
            InstStg op = context.GetOp<InstStg>();

            EmitStg(context, op.LsSize, op.SrcA, op.Dest, Imm24ToSInt(op.Imm24), op.E);
        }

        public static void Stl(EmitterContext context)
        {
            InstStl op = context.GetOp<InstStl>();

            EmitStore(context, MemoryRegion.Local, op.LsSize, GetSrcReg(context, op.SrcA), op.Dest, Imm24ToSInt(op.Imm24));
        }

        public static void Sts(EmitterContext context)
        {
            InstSts op = context.GetOp<InstSts>();

            EmitStore(context, MemoryRegion.Shared, op.LsSize, GetSrcReg(context, op.SrcA), op.Dest, Imm24ToSInt(op.Imm24));
        }

        private static Operand EmitLoadConstant(EmitterContext context, Operand slot, Operand offset)
        {
            Operand vecIndex = context.ShiftRightU32(offset, Const(2));
            Operand elemIndex = context.BitwiseAnd(offset, Const(3));

            if (slot.Type == OperandType.Constant)
            {
                int binding = context.Config.ResourceManager.GetConstantBufferBinding(slot.Value);
                return context.Load(StorageKind.ConstantBuffer, binding, Const(0), vecIndex, elemIndex);
            }
            else
            {
                Operand value = Const(0);

                uint cbUseMask = context.Config.GpuAccessor.QueryConstantBufferUse();

                while (cbUseMask != 0)
                {
                    int cbIndex = BitOperations.TrailingZeroCount(cbUseMask);
                    int binding = context.Config.ResourceManager.GetConstantBufferBinding(cbIndex);

                    Operand isCurrent = context.ICompareEqual(slot, Const(cbIndex));
                    Operand currentValue = context.Load(StorageKind.ConstantBuffer, binding, Const(0), vecIndex, elemIndex);

                    value = context.ConditionalSelect(isCurrent, currentValue, value);

                    cbUseMask &= ~(1u << cbIndex);
                }

                return value;
            }
        }

        private static Operand EmitAtomicOp(
            EmitterContext context,
            StorageKind storageKind,
            AtomOp op,
            AtomSize type,
            Operand addrLow,
            Operand addrHigh,
            Operand value)
        {
            Operand res = Const(0);

            switch (op)
            {
                case AtomOp.Add:
                    if (type == AtomSize.S32 || type == AtomSize.U32)
                    {
                        res = context.AtomicAdd(storageKind, addrLow, addrHigh, value);
                    }
                    else
                    {
                        context.Config.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                case AtomOp.And:
                    if (type == AtomSize.S32 || type == AtomSize.U32)
                    {
                        res = context.AtomicAnd(storageKind, addrLow, addrHigh, value);
                    }
                    else
                    {
                        context.Config.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                case AtomOp.Xor:
                    if (type == AtomSize.S32 || type == AtomSize.U32)
                    {
                        res = context.AtomicXor(storageKind, addrLow, addrHigh, value);
                    }
                    else
                    {
                        context.Config.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                case AtomOp.Or:
                    if (type == AtomSize.S32 || type == AtomSize.U32)
                    {
                        res = context.AtomicOr(storageKind, addrLow, addrHigh, value);
                    }
                    else
                    {
                        context.Config.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                case AtomOp.Max:
                    if (type == AtomSize.S32)
                    {
                        res = context.AtomicMaxS32(storageKind, addrLow, addrHigh, value);
                    }
                    else if (type == AtomSize.U32)
                    {
                        res = context.AtomicMaxU32(storageKind, addrLow, addrHigh, value);
                    }
                    else
                    {
                        context.Config.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                case AtomOp.Min:
                    if (type == AtomSize.S32)
                    {
                        res = context.AtomicMinS32(storageKind, addrLow, addrHigh, value);
                    }
                    else if (type == AtomSize.U32)
                    {
                        res = context.AtomicMinU32(storageKind, addrLow, addrHigh, value);
                    }
                    else
                    {
                        context.Config.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
            }

            return res;
        }

        private static void EmitLoad(
            EmitterContext context,
            MemoryRegion region,
            LsSize2 size,
            Operand srcA,
            int rd,
            int offset)
        {
            if (size > LsSize2.B128)
            {
                context.Config.GpuAccessor.Log($"Invalid load size: {size}.");
                return;
            }

            bool isSmallInt = size < LsSize2.B32;

            int count = 1;

            switch (size)
            {
                case LsSize2.B64: count = 2; break;
                case LsSize2.B128: count = 4; break;
            }

            Operand baseOffset = context.IAdd(srcA, Const(offset));
            Operand wordOffset = context.ShiftRightU32(baseOffset, Const(2)); // Word offset = byte offset / 4 (one word = 4 bytes).
            Operand bitOffset = GetBitOffset(context, baseOffset);

            for (int index = 0; index < count; index++)
            {
                Register dest = new Register(rd + index, RegisterType.Gpr);

                if (dest.IsRZ)
                {
                    break;
                }

                Operand elemOffset = context.IAdd(wordOffset, Const(index));
                Operand value = null;

                switch (region)
                {
                    case MemoryRegion.Local: value = context.LoadLocal(elemOffset); break;
                    case MemoryRegion.Shared: value = context.LoadShared(elemOffset); break;
                }

                if (isSmallInt)
                {
                    value = ExtractSmallInt(context, (LsSize)size, bitOffset, value);
                }

                context.Copy(Register(dest), value);
            }
        }

        private static void EmitLdg(
            EmitterContext context,
            LsSize size,
            int ra,
            int rd,
            int offset,
            bool extended)
        {
            bool isSmallInt = size < LsSize.B32;

            int count = GetVectorCount(size);

            (Operand addrLow, Operand addrHigh) = Get40BitsAddress(context, new Register(ra, RegisterType.Gpr), extended, offset);

            Operand bitOffset = GetBitOffset(context, addrLow);

            for (int index = 0; index < count; index++)
            {
                Register dest = new Register(rd + index, RegisterType.Gpr);

                if (dest.IsRZ)
                {
                    break;
                }

                Operand value = context.LoadGlobal(context.IAdd(addrLow, Const(index * 4)), addrHigh);

                if (isSmallInt)
                {
                    value = ExtractSmallInt(context, size, bitOffset, value);
                }

                context.Copy(Register(dest), value);
            }
        }

        private static void EmitStore(
            EmitterContext context,
            MemoryRegion region,
            LsSize2 size,
            Operand srcA,
            int rd,
            int offset)
        {
            if (size > LsSize2.B128)
            {
                context.Config.GpuAccessor.Log($"Invalid store size: {size}.");
                return;
            }

            bool isSmallInt = size < LsSize2.B32;

            int count = 1;

            switch (size)
            {
                case LsSize2.B64: count = 2; break;
                case LsSize2.B128: count = 4; break;
            }

            Operand baseOffset = context.IAdd(srcA, Const(offset));
            Operand wordOffset = context.ShiftRightU32(baseOffset, Const(2));
            Operand bitOffset = GetBitOffset(context, baseOffset);

            for (int index = 0; index < count; index++)
            {
                bool isRz = rd + index >= RegisterConsts.RegisterZeroIndex;

                Operand value = Register(isRz ? rd : rd + index, RegisterType.Gpr);
                Operand elemOffset = context.IAdd(wordOffset, Const(index));

                if (isSmallInt && region == MemoryRegion.Local)
                {
                    Operand word = context.LoadLocal(elemOffset);

                    value = InsertSmallInt(context, (LsSize)size, bitOffset, word, value);
                }

                if (region == MemoryRegion.Local)
                {
                    context.StoreLocal(elemOffset, value);
                }
                else if (region == MemoryRegion.Shared)
                {
                    switch (size)
                    {
                        case LsSize2.U8:
                        case LsSize2.S8:
                            context.StoreShared8(baseOffset, value);
                            break;
                        case LsSize2.U16:
                        case LsSize2.S16:
                            context.StoreShared16(baseOffset, value);
                            break;
                        default:
                            context.StoreShared(elemOffset, value);
                            break;
                    }
                }
            }
        }

        private static void EmitStg(
            EmitterContext context,
            LsSize2 size,
            int ra,
            int rd,
            int offset,
            bool extended)
        {
            if (size > LsSize2.B128)
            {
                context.Config.GpuAccessor.Log($"Invalid store size: {size}.");
                return;
            }

            int count = GetVectorCount((LsSize)size);

            (Operand addrLow, Operand addrHigh) = Get40BitsAddress(context, new Register(ra, RegisterType.Gpr), extended, offset);

            Operand bitOffset = GetBitOffset(context, addrLow);

            for (int index = 0; index < count; index++)
            {
                bool isRz = rd + index >= RegisterConsts.RegisterZeroIndex;

                Operand value = Register(isRz ? rd : rd + index, RegisterType.Gpr);

                Operand addrLowOffset = context.IAdd(addrLow, Const(index * 4));

                if (size == LsSize2.U8 || size == LsSize2.S8)
                {
                    context.StoreGlobal8(addrLowOffset, addrHigh, value);
                }
                else if (size == LsSize2.U16 || size == LsSize2.S16)
                {
                    context.StoreGlobal16(addrLowOffset, addrHigh, value);
                }
                else
                {
                    context.StoreGlobal(addrLowOffset, addrHigh, value);
                }
            }
        }

        private static int GetVectorCount(LsSize size)
        {
            switch (size)
            {
                case LsSize.B64:
                    return 2;
                case LsSize.B128:
                case LsSize.UB128:
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
            Operand addrLow = Register(ra);
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
            LsSize size,
            Operand bitOffset,
            Operand value)
        {
            value = context.ShiftRightU32(value, bitOffset);

            switch (size)
            {
                case LsSize.U8: value = ZeroExtendTo32(context, value, 8); break;
                case LsSize.U16: value = ZeroExtendTo32(context, value, 16); break;
                case LsSize.S8: value = SignExtendTo32(context, value, 8); break;
                case LsSize.S16: value = SignExtendTo32(context, value, 16); break;
            }

            return value;
        }

        private static Operand InsertSmallInt(
            EmitterContext context,
            LsSize size,
            Operand bitOffset,
            Operand word,
            Operand value)
        {
            switch (size)
            {
                case LsSize.U8:
                case LsSize.S8:
                    value = context.BitwiseAnd(value, Const(0xff));
                    value = context.BitfieldInsert(word, value, bitOffset, Const(8));
                    break;

                case LsSize.U16:
                case LsSize.S16:
                    value = context.BitwiseAnd(value, Const(0xffff));
                    value = context.BitfieldInsert(word, value, bitOffset, Const(16));
                    break;
            }

            return value;
        }
    }
}