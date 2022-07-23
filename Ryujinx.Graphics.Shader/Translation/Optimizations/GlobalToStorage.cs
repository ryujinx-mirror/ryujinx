using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;
using static Ryujinx.Graphics.Shader.Translation.GlobalMemory;

namespace Ryujinx.Graphics.Shader.Translation.Optimizations
{
    static class GlobalToStorage
    {
        public static void RunPass(BasicBlock block, ShaderConfig config)
        {
            int sbStart = GetStorageBaseCbOffset(config.Stage);

            int sbEnd = sbStart + StorageDescsSize;

            for (LinkedListNode<INode> node = block.Operations.First; node != null; node = node.Next)
            {
                if (!(node.Value is Operation operation))
                {
                    continue;
                }

                if (UsesGlobalMemory(operation.Inst))
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
                        node = ReplaceGlobalWithStorage(node, config, storageIndex);
                    }
                    else if (config.Stage == ShaderStage.Compute && operation.Inst == Instruction.LoadGlobal)
                    {
                        // Here we effectively try to replace a LDG instruction with LDC.
                        // The hardware only supports a limited amount of constant buffers
                        // so NVN "emulates" more constant buffers using global memory access.
                        // Here we try to replace the global access back to a constant buffer
                        // load.
                        storageIndex = SearchForStorageBase(block, source, UbeBaseOffset, UbeBaseOffset + UbeDescsSize);

                        if (storageIndex >= 0)
                        {
                            node = ReplaceLdgWithLdc(node, config, storageIndex);
                        }
                    }
                }
            }
        }

        private static LinkedListNode<INode> ReplaceGlobalWithStorage(LinkedListNode<INode> node, ShaderConfig config, int storageIndex)
        {
            Operation operation = (Operation)node.Value;

            bool isAtomic = operation.Inst.IsAtomic();
            bool isStg16Or8 = operation.Inst == Instruction.StoreGlobal16 || operation.Inst == Instruction.StoreGlobal8;
            bool isWrite = isAtomic || operation.Inst == Instruction.StoreGlobal || isStg16Or8;

            config.SetUsedStorageBuffer(storageIndex, isWrite);

            Operand GetStorageOffset()
            {
                Operand addrLow = operation.GetSource(0);

                Operand baseAddrLow = Cbuf(0, GetStorageCbOffset(config.Stage, storageIndex));

                Operand baseAddrTrunc = Local();

                Operand alignMask = Const(-config.GpuAccessor.QueryHostStorageBufferOffsetAlignment());

                Operation andOp = new Operation(Instruction.BitwiseAnd, baseAddrTrunc, baseAddrLow, alignMask);

                node.List.AddBefore(node, andOp);

                Operand byteOffset = Local();
                Operation subOp = new Operation(Instruction.Subtract, byteOffset, addrLow, baseAddrTrunc);

                node.List.AddBefore(node, subOp);

                if (isStg16Or8)
                {
                    return byteOffset;
                }

                Operand wordOffset = Local();
                Operation shrOp = new Operation(Instruction.ShiftRightU32, wordOffset, byteOffset, Const(2));

                node.List.AddBefore(node, shrOp);

                return wordOffset;
            }

            Operand[] sources = new Operand[operation.SourcesCount];

            sources[0] = Const(storageIndex);
            sources[1] = GetStorageOffset();

            for (int index = 2; index < operation.SourcesCount; index++)
            {
                sources[index] = operation.GetSource(index);
            }

            Operation storageOp;

            if (isAtomic)
            {
                Instruction inst = (operation.Inst & ~Instruction.MrMask) | Instruction.MrStorage;

                storageOp = new Operation(inst, operation.Dest, sources);
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

                Operation subOp = new Operation(Instruction.Subtract,      byteOffset, addrLow, baseAddrTrunc);
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
                int slot   = operand.GetCbufSlot();
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