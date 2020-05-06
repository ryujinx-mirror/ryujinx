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

                    if (source.AsgOp is Operation asgOperation)
                    {
                        int storageIndex = SearchForStorageBase(asgOperation, sbStart, sbEnd);

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
                            storageIndex = SearchForStorageBase(asgOperation, UbeBaseOffset, UbeBaseOffset + UbeDescsSize);

                            if (storageIndex >= 0)
                            {
                                node = ReplaceLdgWithLdc(node, config, storageIndex);
                            }
                        }
                    }
                }
            }
        }

        private static LinkedListNode<INode> ReplaceGlobalWithStorage(LinkedListNode<INode> node, ShaderConfig config, int storageIndex)
        {
            Operation operation = (Operation)node.Value;

            Operand GetStorageOffset()
            {
                Operand addrLow = operation.GetSource(0);

                Operand baseAddrLow = Cbuf(0, GetStorageCbOffset(config.Stage, storageIndex));

                Operand baseAddrTrunc = Local();

                Operand alignMask = Const(-config.GpuAccessor.QueryStorageBufferOffsetAlignment());

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

            sources[0] = Const(storageIndex);
            sources[1] = GetStorageOffset();

            for (int index = 2; index < operation.SourcesCount; index++)
            {
                sources[index] = operation.GetSource(index);
            }

            Operation storageOp;

            if (operation.Inst.IsAtomic())
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
                storageOp = new Operation(Instruction.StoreStorage, null, sources);
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

                Operand alignMask = Const(-config.GpuAccessor.QueryStorageBufferOffsetAlignment());

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

            sources[0] = Const(UbeFirstCbuf + storageIndex);
            sources[1] = GetCbufOffset();

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

        private static int SearchForStorageBase(Operation operation, int sbStart, int sbEnd)
        {
            Queue<Operation> assignments = new Queue<Operation>();

            assignments.Enqueue(operation);

            while (assignments.TryDequeue(out operation))
            {
                for (int index = 0; index < operation.SourcesCount; index++)
                {
                    Operand source = operation.GetSource(index);

                    if (source.Type == OperandType.ConstantBuffer)
                    {
                        int slot   = source.GetCbufSlot();
                        int offset = source.GetCbufOffset();

                        if (slot == 0 && offset >= sbStart && offset < sbEnd)
                        {
                            int storageIndex = (offset - sbStart) / StorageDescSize;

                            return storageIndex;
                        }
                    }

                    if (source.AsgOp is Operation asgOperation)
                    {
                        assignments.Enqueue(asgOperation);
                    }
                }
            }

            return -1;
        }
    }
}