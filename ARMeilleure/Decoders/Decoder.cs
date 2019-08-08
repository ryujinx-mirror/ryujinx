using ARMeilleure.Instructions;
using ARMeilleure.Memory;
using ARMeilleure.State;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ARMeilleure.Decoders
{
    static class Decoder
    {
        private delegate object MakeOp(InstDescriptor inst, ulong address, int opCode);

        private static ConcurrentDictionary<Type, MakeOp> _opActivators;

        static Decoder()
        {
            _opActivators = new ConcurrentDictionary<Type, MakeOp>();
        }

        public static Block[] DecodeBasicBlock(MemoryManager memory, ulong address, ExecutionMode mode)
        {
            Block block = new Block(address);

            FillBlock(memory, mode, block, ulong.MaxValue);

            return new Block[] { block };
        }

        public static Block[] DecodeFunction(MemoryManager memory, ulong address, ExecutionMode mode)
        {
            List<Block> blocks = new List<Block>();

            Queue<Block> workQueue = new Queue<Block>();

            Dictionary<ulong, Block> visited = new Dictionary<ulong, Block>();

            Block GetBlock(ulong blkAddress)
            {
                if (!visited.TryGetValue(blkAddress, out Block block))
                {
                    block = new Block(blkAddress);

                    workQueue.Enqueue(block);

                    visited.Add(blkAddress, block);
                }

                return block;
            }

            GetBlock(address);

            while (workQueue.TryDequeue(out Block currBlock))
            {
                // Check if the current block is inside another block.
                if (BinarySearch(blocks, currBlock.Address, out int nBlkIndex))
                {
                    Block nBlock = blocks[nBlkIndex];

                    if (nBlock.Address == currBlock.Address)
                    {
                        throw new InvalidOperationException("Found duplicate block address on the list.");
                    }

                    nBlock.Split(currBlock);

                    blocks.Insert(nBlkIndex + 1, currBlock);

                    continue;
                }

                // If we have a block after the current one, set the limit address.
                ulong limitAddress = ulong.MaxValue;

                if (nBlkIndex != blocks.Count)
                {
                    Block nBlock = blocks[nBlkIndex];

                    int nextIndex = nBlkIndex + 1;

                    if (nBlock.Address < currBlock.Address && nextIndex < blocks.Count)
                    {
                        limitAddress = blocks[nextIndex].Address;
                    }
                    else if (nBlock.Address > currBlock.Address)
                    {
                        limitAddress = blocks[nBlkIndex].Address;
                    }
                }

                FillBlock(memory, mode, currBlock, limitAddress);

                if (currBlock.OpCodes.Count != 0)
                {
                    // Set child blocks. "Branch" is the block the branch instruction
                    // points to (when taken), "Next" is the block at the next address,
                    // executed when the branch is not taken. For Unconditional Branches
                    // (except BL/BLR that are sub calls) or end of executable, Next is null.
                    OpCode lastOp = currBlock.GetLastOp();

                    bool isCall = IsCall(lastOp);

                    if (lastOp is IOpCodeBImm op && !isCall)
                    {
                        currBlock.Branch = GetBlock((ulong)op.Immediate);
                    }

                    if (!IsUnconditionalBranch(lastOp) /*|| isCall*/)
                    {
                        currBlock.Next = GetBlock(currBlock.EndAddress);
                    }
                }

                // Insert the new block on the list (sorted by address).
                if (blocks.Count != 0)
                {
                    Block nBlock = blocks[nBlkIndex];

                    blocks.Insert(nBlkIndex + (nBlock.Address < currBlock.Address ? 1 : 0), currBlock);
                }
                else
                {
                    blocks.Add(currBlock);
                }
            }

            return blocks.ToArray();
        }

        private static bool BinarySearch(List<Block> blocks, ulong address, out int index)
        {
            index = 0;

            int left  = 0;
            int right = blocks.Count - 1;

            while (left <= right)
            {
                int size = right - left;

                int middle = left + (size >> 1);

                Block block = blocks[middle];

                index = middle;

                if (address >= block.Address && address < block.EndAddress)
                {
                    return true;
                }

                if (address < block.Address)
                {
                    right = middle - 1;
                }
                else
                {
                    left = middle + 1;
                }
            }

            return false;
        }

        private static void FillBlock(
            MemoryManager memory,
            ExecutionMode mode,
            Block         block,
            ulong         limitAddress)
        {
            ulong address = block.Address;

            OpCode opCode;

            do
            {
                if (address >= limitAddress)
                {
                    break;
                }

                opCode = DecodeOpCode(memory, address, mode);

                block.OpCodes.Add(opCode);

                address += (ulong)opCode.OpCodeSizeInBytes;
            }
            while (!(IsBranch(opCode) || IsException(opCode)));

            block.EndAddress = address;
        }

        private static bool IsBranch(OpCode opCode)
        {
            return opCode is OpCodeBImm ||
                   opCode is OpCodeBReg || IsAarch32Branch(opCode);
        }

        private static bool IsUnconditionalBranch(OpCode opCode)
        {
            return opCode is OpCodeBImmAl ||
                   opCode is OpCodeBReg   || IsAarch32UnconditionalBranch(opCode);
        }

        private static bool IsAarch32UnconditionalBranch(OpCode opCode)
        {
            if (!(opCode is OpCode32 op))
            {
                return false;
            }

            // Note: On ARM32, most instructions have conditional execution,
            // so there's no "Always" (unconditional) branch like on ARM64.
            // We need to check if the condition is "Always" instead.
            return IsAarch32Branch(op) && op.Cond >= Condition.Al;
        }

        private static bool IsAarch32Branch(OpCode opCode)
        {
            // Note: On ARM32, most ALU operations can write to R15 (PC),
            // so we must consider such operations as a branch in potential aswell.
            if (opCode is IOpCode32Alu opAlu && opAlu.Rd == RegisterAlias.Aarch32Pc)
            {
                return true;
            }

            // Same thing for memory operations. We have the cases where PC is a target
            // register (Rt == 15 or (mask & (1 << 15)) != 0), and cases where there is
            // a write back to PC (wback == true && Rn == 15), however the later may
            // be "undefined" depending on the CPU, so compilers should not produce that.
            if (opCode is IOpCode32Mem || opCode is IOpCode32MemMult)
            {
                int rt, rn;

                bool wBack, isLoad;

                if (opCode is IOpCode32Mem opMem)
                {
                    rt     = opMem.Rt;
                    rn     = opMem.Rn;
                    wBack  = opMem.WBack;
                    isLoad = opMem.IsLoad;

                    // For the dual load, we also need to take into account the
                    // case were Rt2 == 15 (PC).
                    if (rt == 14 && opMem.Instruction.Name == InstName.Ldrd)
                    {
                        rt = RegisterAlias.Aarch32Pc;
                    }
                }
                else if (opCode is IOpCode32MemMult opMemMult)
                {
                    const int pcMask = 1 << RegisterAlias.Aarch32Pc;

                    rt     = (opMemMult.RegisterMask & pcMask) != 0 ? RegisterAlias.Aarch32Pc : 0;
                    rn     =  opMemMult.Rn;
                    wBack  =  opMemMult.PostOffset != 0;
                    isLoad =  opMemMult.IsLoad;
                }
                else
                {
                    throw new NotImplementedException($"The type \"{opCode.GetType().Name}\" is not implemented on the decoder.");
                }

                if ((rt == RegisterAlias.Aarch32Pc && isLoad) ||
                    (rn == RegisterAlias.Aarch32Pc && wBack))
                {
                    return true;
                }
            }

            // Explicit branch instructions.
            return opCode is IOpCode32BImm ||
                   opCode is IOpCode32BReg;
        }

        private static bool IsCall(OpCode opCode)
        {
            // TODO (CQ): ARM32 support.
            return opCode.Instruction.Name == InstName.Bl ||
                   opCode.Instruction.Name == InstName.Blr;
        }

        private static bool IsException(OpCode opCode)
        {
            return opCode.Instruction.Name == InstName.Brk ||
                   opCode.Instruction.Name == InstName.Svc ||
                   opCode.Instruction.Name == InstName.Und;
        }

        public static OpCode DecodeOpCode(MemoryManager memory, ulong address, ExecutionMode mode)
        {
            int opCode = memory.ReadInt32((long)address);

            InstDescriptor inst;

            Type type;

            if (mode == ExecutionMode.Aarch64)
            {
                (inst, type) = OpCodeTable.GetInstA64(opCode);
            }
            else
            {
                if (mode == ExecutionMode.Aarch32Arm)
                {
                    (inst, type) = OpCodeTable.GetInstA32(opCode);
                }
                else /* if (mode == ExecutionMode.Aarch32Thumb) */
                {
                    (inst, type) = OpCodeTable.GetInstT32(opCode);
                }
            }

            if (type != null)
            {
                return MakeOpCode(inst, type, address, opCode);
            }
            else
            {
                return new OpCode(inst, address, opCode);
            }
        }

        private static OpCode MakeOpCode(InstDescriptor inst, Type type, ulong address, int opCode)
        {
            MakeOp createInstance = _opActivators.GetOrAdd(type, CacheOpActivator);

            return (OpCode)createInstance(inst, address, opCode);
        }

        private static MakeOp CacheOpActivator(Type type)
        {
            Type[] argTypes = new Type[] { typeof(InstDescriptor), typeof(ulong), typeof(int) };

            DynamicMethod mthd = new DynamicMethod($"Make{type.Name}", type, argTypes);

            ILGenerator generator = mthd.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Newobj, type.GetConstructor(argTypes));
            generator.Emit(OpCodes.Ret);

            return (MakeOp)mthd.CreateDelegate(typeof(MakeOp));
        }
    }
}