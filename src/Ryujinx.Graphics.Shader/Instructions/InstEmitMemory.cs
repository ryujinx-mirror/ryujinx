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
            if (context.TranslatorContext.Definitions.Stage != ShaderStage.Compute)
            {
                context.TranslatorContext.GpuAccessor.Log($"Atoms instruction is not valid on \"{context.TranslatorContext.Definitions.Stage}\" stage.");
                return;
            }

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
                _ => AtomSize.U32,
            };

            Operand id = Const(context.ResourceManager.SharedMemoryId);
            Operand res = EmitAtomicOp(context, StorageKind.SharedMemory, op.AtomOp, size, id, offset, value);

            context.Copy(GetDest(op.Dest), res);
        }

        public static void Ldc(EmitterContext context)
        {
            InstLdc op = context.GetOp<InstLdc>();

            if (op.LsSize > LsSize2.B64)
            {
                context.TranslatorContext.GpuAccessor.Log($"Invalid LDC size: {op.LsSize}.");
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
                Register dest = new(op.Dest + index, RegisterType.Gpr);

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

            EmitLoad(context, StorageKind.LocalMemory, op.LsSize, GetSrcReg(context, op.SrcA), op.Dest, Imm24ToSInt(op.Imm24));
        }

        public static void Lds(EmitterContext context)
        {
            if (context.TranslatorContext.Definitions.Stage != ShaderStage.Compute)
            {
                context.TranslatorContext.GpuAccessor.Log($"Lds instruction is not valid on \"{context.TranslatorContext.Definitions.Stage}\" stage.");
                return;
            }

            InstLds op = context.GetOp<InstLds>();

            EmitLoad(context, StorageKind.SharedMemory, op.LsSize, GetSrcReg(context, op.SrcA), op.Dest, Imm24ToSInt(op.Imm24));
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

            EmitStore(context, StorageKind.LocalMemory, op.LsSize, GetSrcReg(context, op.SrcA), op.Dest, Imm24ToSInt(op.Imm24));
        }

        public static void Sts(EmitterContext context)
        {
            if (context.TranslatorContext.Definitions.Stage != ShaderStage.Compute)
            {
                context.TranslatorContext.GpuAccessor.Log($"Sts instruction is not valid on \"{context.TranslatorContext.Definitions.Stage}\" stage.");
                return;
            }

            InstSts op = context.GetOp<InstSts>();

            EmitStore(context, StorageKind.SharedMemory, op.LsSize, GetSrcReg(context, op.SrcA), op.Dest, Imm24ToSInt(op.Imm24));
        }

        private static Operand EmitLoadConstant(EmitterContext context, Operand slot, Operand offset)
        {
            Operand vecIndex = context.ShiftRightU32(offset, Const(2));
            Operand elemIndex = context.BitwiseAnd(offset, Const(3));

            if (slot.Type == OperandType.Constant)
            {
                int binding = context.ResourceManager.GetConstantBufferBinding(slot.Value);
                return context.Load(StorageKind.ConstantBuffer, binding, Const(0), vecIndex, elemIndex);
            }
            else
            {
                Operand value = Const(0);

                uint cbUseMask = context.TranslatorContext.GpuAccessor.QueryConstantBufferUse();

                while (cbUseMask != 0)
                {
                    int cbIndex = BitOperations.TrailingZeroCount(cbUseMask);
                    int binding = context.ResourceManager.GetConstantBufferBinding(cbIndex);

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
            Operand e0,
            Operand e1,
            Operand value)
        {
            Operand res = Const(0);

            switch (op)
            {
                case AtomOp.Add:
                    if (type == AtomSize.S32 || type == AtomSize.U32)
                    {
                        res = context.AtomicAdd(storageKind, e0, e1, value);
                    }
                    else
                    {
                        context.TranslatorContext.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                case AtomOp.Min:
                    if (type == AtomSize.S32)
                    {
                        res = context.AtomicMinS32(storageKind, e0, e1, value);
                    }
                    else if (type == AtomSize.U32)
                    {
                        res = context.AtomicMinU32(storageKind, e0, e1, value);
                    }
                    else
                    {
                        context.TranslatorContext.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                case AtomOp.Max:
                    if (type == AtomSize.S32)
                    {
                        res = context.AtomicMaxS32(storageKind, e0, e1, value);
                    }
                    else if (type == AtomSize.U32)
                    {
                        res = context.AtomicMaxU32(storageKind, e0, e1, value);
                    }
                    else
                    {
                        context.TranslatorContext.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                case AtomOp.And:
                    if (type == AtomSize.S32 || type == AtomSize.U32)
                    {
                        res = context.AtomicAnd(storageKind, e0, e1, value);
                    }
                    else
                    {
                        context.TranslatorContext.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                case AtomOp.Or:
                    if (type == AtomSize.S32 || type == AtomSize.U32)
                    {
                        res = context.AtomicOr(storageKind, e0, e1, value);
                    }
                    else
                    {
                        context.TranslatorContext.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                case AtomOp.Xor:
                    if (type == AtomSize.S32 || type == AtomSize.U32)
                    {
                        res = context.AtomicXor(storageKind, e0, e1, value);
                    }
                    else
                    {
                        context.TranslatorContext.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                case AtomOp.Exch:
                    if (type == AtomSize.S32 || type == AtomSize.U32)
                    {
                        res = context.AtomicSwap(storageKind, e0, e1, value);
                    }
                    else
                    {
                        context.TranslatorContext.GpuAccessor.Log($"Invalid reduction type: {type}.");
                    }
                    break;
                default:
                    context.TranslatorContext.GpuAccessor.Log($"Invalid atomic operation: {op}.");
                    break;
            }

            return res;
        }

        private static void EmitLoad(
            EmitterContext context,
            StorageKind storageKind,
            LsSize2 size,
            Operand srcA,
            int rd,
            int offset)
        {
            if (size > LsSize2.B128)
            {
                context.TranslatorContext.GpuAccessor.Log($"Invalid load size: {size}.");
                return;
            }

            int id = storageKind == StorageKind.LocalMemory
                ? context.ResourceManager.LocalMemoryId
                : context.ResourceManager.SharedMemoryId;
            bool isSmallInt = size < LsSize2.B32;

            int count = size switch
            {
                LsSize2.B64 => 2,
                LsSize2.B128 => 4,
                _ => 1,
            };

            Operand baseOffset = context.Copy(srcA);

            for (int index = 0; index < count; index++)
            {
                Register dest = new(rd + index, RegisterType.Gpr);

                if (dest.IsRZ)
                {
                    break;
                }

                Operand byteOffset = context.IAdd(baseOffset, Const(offset + index * 4));
                Operand wordOffset = context.ShiftRightU32(byteOffset, Const(2)); // Word offset = byte offset / 4 (one word = 4 bytes).
                Operand bitOffset = GetBitOffset(context, byteOffset);
                Operand value = context.Load(storageKind, id, wordOffset);

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
            int count = GetVectorCount(size);
            StorageKind storageKind = GetStorageKind(size);

            (_, Operand addrHigh) = Get40BitsAddress(context, new Register(ra, RegisterType.Gpr), extended, offset);

            Operand srcA = context.Copy(new Operand(new Register(ra, RegisterType.Gpr)));

            for (int index = 0; index < count; index++)
            {
                Register dest = new(rd + index, RegisterType.Gpr);

                if (dest.IsRZ)
                {
                    break;
                }

                Operand value = context.Load(storageKind, context.IAdd(srcA, Const(offset + index * 4)), addrHigh);

                context.Copy(Register(dest), value);
            }
        }

        private static void EmitStore(
            EmitterContext context,
            StorageKind storageKind,
            LsSize2 size,
            Operand srcA,
            int rd,
            int offset)
        {
            if (size > LsSize2.B128)
            {
                context.TranslatorContext.GpuAccessor.Log($"Invalid store size: {size}.");
                return;
            }

            int id = storageKind == StorageKind.LocalMemory
                ? context.ResourceManager.LocalMemoryId
                : context.ResourceManager.SharedMemoryId;
            bool isSmallInt = size < LsSize2.B32;

            int count = size switch
            {
                LsSize2.B64 => 2,
                LsSize2.B128 => 4,
                _ => 1,
            };

            Operand baseOffset = context.Copy(srcA);

            for (int index = 0; index < count; index++)
            {
                bool isRz = rd + index >= RegisterConsts.RegisterZeroIndex;

                Operand value = Register(isRz ? rd : rd + index, RegisterType.Gpr);
                Operand byteOffset = context.IAdd(baseOffset, Const(offset + index * 4));
                Operand wordOffset = context.ShiftRightU32(byteOffset, Const(2));
                Operand bitOffset = GetBitOffset(context, byteOffset);

                if (isSmallInt && storageKind == StorageKind.LocalMemory)
                {
                    Operand word = context.Load(storageKind, id, wordOffset);

                    value = InsertSmallInt(context, (LsSize)size, bitOffset, word, value);
                }

                if (storageKind == StorageKind.LocalMemory)
                {
                    context.Store(storageKind, id, wordOffset, value);
                }
                else if (storageKind == StorageKind.SharedMemory)
                {
                    switch (size)
                    {
                        case LsSize2.U8:
                        case LsSize2.S8:
                            context.Store(StorageKind.SharedMemory8, id, byteOffset, value);
                            break;
                        case LsSize2.U16:
                        case LsSize2.S16:
                            context.Store(StorageKind.SharedMemory16, id, byteOffset, value);
                            break;
                        default:
                            context.Store(storageKind, id, wordOffset, value);
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
                context.TranslatorContext.GpuAccessor.Log($"Invalid store size: {size}.");
                return;
            }

            int count = GetVectorCount((LsSize)size);
            StorageKind storageKind = GetStorageKind((LsSize)size);

            (_, Operand addrHigh) = Get40BitsAddress(context, new Register(ra, RegisterType.Gpr), extended, offset);

            Operand srcA = context.Copy(new Operand(new Register(ra, RegisterType.Gpr)));

            for (int index = 0; index < count; index++)
            {
                bool isRz = rd + index >= RegisterConsts.RegisterZeroIndex;

                Operand value = Register(isRz ? rd : rd + index, RegisterType.Gpr);

                Operand addrLowOffset = context.IAdd(srcA, Const(offset + index * 4));

                context.Store(storageKind, addrLowOffset, addrHigh, value);
            }
        }

        private static StorageKind GetStorageKind(LsSize size)
        {
            return size switch
            {
                LsSize.U8 => StorageKind.GlobalMemoryU8,
                LsSize.S8 => StorageKind.GlobalMemoryS8,
                LsSize.U16 => StorageKind.GlobalMemoryU16,
                LsSize.S16 => StorageKind.GlobalMemoryS16,
                _ => StorageKind.GlobalMemory,
            };
        }

        private static int GetVectorCount(LsSize size)
        {
            return size switch
            {
                LsSize.B64 => 2,
                LsSize.B128 or LsSize.UB128 => 4,
                _ => 1,
            };
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
                case LsSize.U8:
                    value = ZeroExtendTo32(context, value, 8);
                    break;
                case LsSize.U16:
                    value = ZeroExtendTo32(context, value, 16);
                    break;
                case LsSize.S8:
                    value = SignExtendTo32(context, value, 8);
                    break;
                case LsSize.S16:
                    value = SignExtendTo32(context, value, 16);
                    break;
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
