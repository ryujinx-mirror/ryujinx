using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;
using static Ryujinx.Graphics.Shader.Translation.GlobalMemory;

namespace Ryujinx.Graphics.Shader.Translation.Optimizations
{
    static class GlobalToStorage
    {
        public static void RunPass(BasicBlock block, ShaderConfig config, ref int sbUseMask, ref int ubeUseMask)
        {
            int sbStart = GetStorageBaseCbOffset(config.Stage);
            int sbEnd = sbStart + StorageDescsSize;

            int ubeStart = UbeBaseOffset;
            int ubeEnd = UbeBaseOffset + UbeDescsSize;

            for (LinkedListNode<INode> node = block.Operations.First; node != null; node = node.Next)
            {
                for (int index = 0; index < node.Value.SourcesCount; index++)
                {
                    Operand src = node.Value.GetSource(index);

                    int storageIndex = GetStorageIndex(src, sbStart, sbEnd);

                    if (storageIndex >= 0)
                    {
                        sbUseMask |= 1 << storageIndex;
                    }

                    if (config.Stage == ShaderStage.Compute)
                    {
                        int constantIndex = GetStorageIndex(src, ubeStart, ubeEnd);

                        if (constantIndex >= 0)
                        {
                            ubeUseMask |= 1 << constantIndex;
                        }
                    }
                }

                if (!(node.Value is Operation operation))
                {
                    continue;
                }

                if (UsesGlobalMemory(operation.Inst, operation.StorageKind))
                {
                    Operand source = operation.GetSource(0);

                    int storageIndex = SearchForStorageBase(block, source, sbStart, sbEnd);

                    if (storageIndex >= 0)
                    {
                        // Storage buffers are implemented using global memory access.
                        // If we know from where the base address of the access is loaded,
                        // we can guess which storage buffer it is accessing.
                        // We can then replace the global memory access with a storage
                        // buffer access.
                        node = ReplaceGlobalWithStorage(block, node, config, storageIndex);
                    }
                    else if (config.Stage == ShaderStage.Compute && operation.Inst == Instruction.LoadGlobal)
                    {
                        // Here we effectively try to replace a LDG instruction with LDC.
                        // The hardware only supports a limited amount of constant buffers
                        // so NVN "emulates" more constant buffers using global memory access.
                        // Here we try to replace the global access back to a constant buffer
                        // load.
                        storageIndex = SearchForStorageBase(block, source, ubeStart, ubeStart + ubeEnd);

                        if (storageIndex >= 0)
                        {
                            node = ReplaceLdgWithLdc(node, config, storageIndex);
                        }
                    }
                }
            }

            config.SetAccessibleBufferMasks(sbUseMask, ubeUseMask);
        }

        private static LinkedListNode<INode> ReplaceGlobalWithStorage(BasicBlock block, LinkedListNode<INode> node, ShaderConfig config, int storageIndex)
        {
            Operation operation = (Operation)node.Value;

            bool isAtomic = operation.Inst.IsAtomic();
            bool isStg16Or8 = operation.Inst == Instruction.StoreGlobal16 || operation.Inst == Instruction.StoreGlobal8;
            bool isWrite = isAtomic || operation.Inst == Instruction.StoreGlobal || isStg16Or8;

            config.SetUsedStorageBuffer(storageIndex, isWrite);

            Operand[] sources = new Operand[operation.SourcesCount];

            sources[0] = Const(storageIndex);
            sources[1] = GetStorageOffset(block, node, config, storageIndex, operation.GetSource(0), isStg16Or8);

            for (int index = 2; index < operation.SourcesCount; index++)
            {
                sources[index] = operation.GetSource(index);
            }

            Operation storageOp;

            if (isAtomic)
            {
                storageOp = new Operation(operation.Inst, StorageKind.StorageBuffer, operation.Dest, sources);
            }
            else if (operation.Inst == Instruction.LoadGlobal)
            {
                storageOp = new Operation(Instruction.LoadStorage, operation.Dest, sources);
            }
            else
            {
                Instruction storeInst = operation.Inst switch
                {
                    Instruction.StoreGlobal16 => Instruction.StoreStorage16,
                    Instruction.StoreGlobal8 => Instruction.StoreStorage8,
                    _ => Instruction.StoreStorage
                };

                storageOp = new Operation(storeInst, null, sources);
            }

            for (int index = 0; index < operation.SourcesCount; index++)
            {
                operation.SetSource(index, null);
            }

            LinkedListNode<INode> oldNode = node;

            node = node.List.AddBefore(node, storageOp);

            node.List.Remove(oldNode);

            return node;
        }

        private static Operand GetStorageOffset(
            BasicBlock block,
            LinkedListNode<INode> node,
            ShaderConfig config,
            int storageIndex,
            Operand addrLow,
            bool isStg16Or8)
        {
            int baseAddressCbOffset = GetStorageCbOffset(config.Stage, storageIndex);

            bool storageAligned = !(config.GpuAccessor.QueryHasUnalignedStorageBuffer() || config.GpuAccessor.QueryHostStorageBufferOffsetAlignment() > Constants.StorageAlignment);

            (Operand byteOffset, int constantOffset) = storageAligned ?
                GetStorageOffset(block, Utils.FindLastOperation(addrLow, block), baseAddressCbOffset) :
                (null, 0);

            if (byteOffset != null)
            {
                ReplaceAddressAlignment(node.List, addrLow, byteOffset, constantOffset);
            }

            if (byteOffset == null)
            {
                Operand baseAddrLow = Cbuf(0, baseAddressCbOffset);
                Operand baseAddrTrunc = Local();

                Operand alignMask = Const(-config.GpuAccessor.QueryHostStorageBufferOffsetAlignment());

                Operation andOp = new Operation(Instruction.BitwiseAnd, baseAddrTrunc, baseAddrLow, alignMask);

                node.List.AddBefore(node, andOp);

                Operand offset = Local();
                Operation subOp = new Operation(Instruction.Subtract, offset, addrLow, baseAddrTrunc);

                node.List.AddBefore(node, subOp);

                byteOffset = offset;
            }
            else if (constantOffset != 0)
            {
                Operand offset = Local();
                Operation addOp = new Operation(Instruction.Add, offset, byteOffset, Const(constantOffset));

                node.List.AddBefore(node, addOp);

                byteOffset = offset;
            }

            if (isStg16Or8)
            {
                return byteOffset;
            }

            Operand wordOffset = Local();
            Operation shrOp = new Operation(Instruction.ShiftRightU32, wordOffset, byteOffset, Const(2));

            node.List.AddBefore(node, shrOp);

            return wordOffset;
        }

        private static bool IsCb0Offset(Operand operand, int offset)
        {
            return operand.Type == OperandType.ConstantBuffer && operand.GetCbufSlot() == 0 && operand.GetCbufOffset() == offset;
        }

        private static void ReplaceAddressAlignment(LinkedList<INode> list, Operand address, Operand byteOffset, int constantOffset)
        {
            // When we emit 16/8-bit LDG, we add extra code to determine the address alignment.
            // Eliminate the storage buffer base address from this too, leaving only the byte offset.

            foreach (INode useNode in address.UseOps)
            {
                if (useNode is Operation op && op.Inst == Instruction.BitwiseAnd)
                {
                    Operand src1 = op.GetSource(0);
                    Operand src2 = op.GetSource(1);

                    int addressIndex = -1;

                    if (src1 == address && src2.Type == OperandType.Constant && src2.Value == 3)
                    {
                        addressIndex = 0;
                    }
                    else if (src2 == address && src1.Type == OperandType.Constant && src1.Value == 3)
                    {
                        addressIndex = 1;
                    }

                    if (addressIndex != -1)
                    {
                        LinkedListNode<INode> node = list.Find(op);

                        // Add offset calculation before the use. Needs to be on the same block.
                        if (node != null)
                        {
                            Operand offset = Local();
                            Operation addOp = new Operation(Instruction.Add, offset, byteOffset, Const(constantOffset));
                            list.AddBefore(node, addOp);

                            op.SetSource(addressIndex, offset);
                        }
                    }
                }
            }
        }

        private static (Operand, int) GetStorageOffset(BasicBlock block, Operand address, int baseAddressCbOffset)
        {
            if (IsCb0Offset(address, baseAddressCbOffset))
            {
                // Direct offset: zero.
                return (Const(0), 0);
            }

            (address, int constantOffset) = GetStorageConstantOffset(block, address);

            address = Utils.FindLastOperation(address, block);

            if (IsCb0Offset(address, baseAddressCbOffset))
            {
                // Only constant offset
                return (Const(0), constantOffset);
            }

            if (!(address.AsgOp is Operation offsetAdd) || offsetAdd.Inst != Instruction.Add)
            {
                return (null, 0);
            }

            Operand src1 = offsetAdd.GetSource(0);
            Operand src2 = Utils.FindLastOperation(offsetAdd.GetSource(1), block);

            if (IsCb0Offset(src2, baseAddressCbOffset))
            {
                return (src1, constantOffset);
            }
            else if (IsCb0Offset(src1, baseAddressCbOffset))
            {
                return (src2, constantOffset);
            }

            return (null, 0);
        }

        private static (Operand, int) GetStorageConstantOffset(BasicBlock block, Operand address)
        {
            if (!(address.AsgOp is Operation offsetAdd) || offsetAdd.Inst != Instruction.Add)
            {
                return (address, 0);
            }

            Operand src1 = offsetAdd.GetSource(0);
            Operand src2 = offsetAdd.GetSource(1);

            if (src2.Type != OperandType.Constant)
            {
                return (address, 0);
            }

            return (src1, src2.Value);
        }

        private static LinkedListNode<INode> ReplaceLdgWithLdc(LinkedListNode<INode> node, ShaderConfig config, int storageIndex)
        {
            Operation operation = (Operation)node.Value;

            Operand GetCbufOffset()
            {
                Operand addrLow = operation.GetSource(0);

                Operand baseAddrLow = Cbuf(0, UbeBaseOffset + storageIndex * StorageDescSize);

                Operand baseAddrTrunc = Local();

                Operand alignMask = Const(-config.GpuAccessor.QueryHostStorageBufferOffsetAlignment());

                Operation andOp = new Operation(Instruction.BitwiseAnd, baseAddrTrunc, baseAddrLow, alignMask);

                node.List.AddBefore(node, andOp);

                Operand byteOffset = Local();
                Operand wordOffset = Local();

                Operation subOp = new Operation(Instruction.Subtract, byteOffset, addrLow, baseAddrTrunc);
                Operation shrOp = new Operation(Instruction.ShiftRightU32, wordOffset, byteOffset, Const(2));

                node.List.AddBefore(node, subOp);
                node.List.AddBefore(node, shrOp);

                return wordOffset;
            }

            Operand[] sources = new Operand[operation.SourcesCount];

            int cbSlot = UbeFirstCbuf + storageIndex;

            sources[0] = Const(cbSlot);
            sources[1] = GetCbufOffset();

            config.SetUsedConstantBuffer(cbSlot);

            for (int index = 2; index < operation.SourcesCount; index++)
            {
                sources[index] = operation.GetSource(index);
            }

            Operation ldcOp = new Operation(Instruction.LoadConstant, operation.Dest, sources);

            for (int index = 0; index < operation.SourcesCount; index++)
            {
                operation.SetSource(index, null);
            }

            LinkedListNode<INode> oldNode = node;

            node = node.List.AddBefore(node, ldcOp);

            node.List.Remove(oldNode);

            return node;
        }

        private static int SearchForStorageBase(BasicBlock block, Operand globalAddress, int sbStart, int sbEnd)
        {
            globalAddress = Utils.FindLastOperation(globalAddress, block);

            if (globalAddress.Type == OperandType.ConstantBuffer)
            {
                return GetStorageIndex(globalAddress, sbStart, sbEnd);
            }

            Operation operation = globalAddress.AsgOp as Operation;

            if (operation == null || operation.Inst != Instruction.Add)
            {
                return -1;
            }

            Operand src1 = operation.GetSource(0);
            Operand src2 = operation.GetSource(1);

            if ((src1.Type == OperandType.LocalVariable && src2.Type == OperandType.Constant) ||
                (src2.Type == OperandType.LocalVariable && src1.Type == OperandType.Constant))
            {
                if (src1.Type == OperandType.LocalVariable)
                {
                    operation = Utils.FindLastOperation(src1, block).AsgOp as Operation;
                }
                else
                {
                    operation = Utils.FindLastOperation(src2, block).AsgOp as Operation;
                }

                if (operation == null || operation.Inst != Instruction.Add)
                {
                    return -1;
                }
            }

            for (int index = 0; index < operation.SourcesCount; index++)
            {
                Operand source = operation.GetSource(index);

                int storageIndex = GetStorageIndex(source, sbStart, sbEnd);

                if (storageIndex != -1)
                {
                    return storageIndex;
                }
            }

            return -1;
        }

        private static int GetStorageIndex(Operand operand, int sbStart, int sbEnd)
        {
            if (operand.Type == OperandType.ConstantBuffer)
            {
                int slot = operand.GetCbufSlot();
                int offset = operand.GetCbufOffset();

                if (slot == 0 && offset >= sbStart && offset < sbEnd)
                {
                    int storageIndex = (offset - sbStart) / StorageDescSize;

                    return storageIndex;
                }
            }

            return -1;
        }
    }
}