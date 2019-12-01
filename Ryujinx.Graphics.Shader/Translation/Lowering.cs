using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;
using static Ryujinx.Graphics.Shader.Translation.GlobalMemory;

namespace Ryujinx.Graphics.Shader.Translation
{
    static class Lowering
    {
        public static void RunPass(BasicBlock[] blocks, ShaderConfig config)
        {
            for (int blkIndex = 0; blkIndex < blocks.Length; blkIndex++)
            {
                BasicBlock block = blocks[blkIndex];

                for (LinkedListNode<INode> node = block.Operations.First; node != null; node = node.Next)
                {
                    if (!(node.Value is Operation operation))
                    {
                        continue;
                    }

                    if (UsesGlobalMemory(operation.Inst))
                    {
                        node = LowerGlobal(node, config);
                    }
                }
            }
        }

        private static LinkedListNode<INode> LowerGlobal(LinkedListNode<INode> node, ShaderConfig config)
        {
            Operation operation = (Operation)node.Value;

            Operation storageOp;

            Operand PrependOperation(Instruction inst, params Operand[] sources)
            {
                Operand local = Local();

                node.List.AddBefore(node, new Operation(inst, local, sources));

                return local;
            }

            Operand addrLow  = operation.GetSource(0);
            Operand addrHigh = operation.GetSource(1);

            Operand sbBaseAddrLow = Const(0);
            Operand sbSlot        = Const(0);

            for (int slot = 0; slot < StorageMaxCount; slot++)
            {
                int cbOffset = GetStorageCbOffset(config.Stage, slot);

                Operand baseAddrLow  = Cbuf(0, cbOffset);
                Operand baseAddrHigh = Cbuf(0, cbOffset + 1);
                Operand size         = Cbuf(0, cbOffset + 2);

                Operand offset = PrependOperation(Instruction.Subtract,       addrLow, baseAddrLow);
                Operand borrow = PrependOperation(Instruction.CompareLessU32, addrLow, baseAddrLow);

                Operand inRangeLow = PrependOperation(Instruction.CompareLessU32, offset, size);

                Operand addrHighBorrowed = PrependOperation(Instruction.Add, addrHigh, borrow);

                Operand inRangeHigh = PrependOperation(Instruction.CompareEqual, addrHighBorrowed, baseAddrHigh);

                Operand inRange = PrependOperation(Instruction.BitwiseAnd, inRangeLow, inRangeHigh);

                sbBaseAddrLow = PrependOperation(Instruction.ConditionalSelect, inRange, baseAddrLow, sbBaseAddrLow);
                sbSlot        = PrependOperation(Instruction.ConditionalSelect, inRange, Const(slot), sbSlot);
            }

            Operand alignMask = Const(-config.Capabilities.StorageBufferOffsetAlignment);

            Operand baseAddrTrunc = PrependOperation(Instruction.BitwiseAnd,    sbBaseAddrLow, Const(-64));
            Operand byteOffset    = PrependOperation(Instruction.Subtract,      addrLow, baseAddrTrunc);
            Operand wordOffset    = PrependOperation(Instruction.ShiftRightU32, byteOffset, Const(2));

            Operand[] sources = new Operand[operation.SourcesCount];

            sources[0] = sbSlot;
            sources[1] = wordOffset;

            for (int index = 2; index < operation.SourcesCount; index++)
            {
                sources[index] = operation.GetSource(index);
            }

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
    }
}