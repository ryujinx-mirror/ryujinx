using ChocolArm64.Instructions;
using ChocolArm64.Memory;
using ChocolArm64.State;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ChocolArm64.Decoders
{
    static class Decoder
    {
        private delegate object OpActivator(Inst inst, long position, int opCode);

        private static ConcurrentDictionary<Type, OpActivator> _opActivators;

        static Decoder()
        {
            _opActivators = new ConcurrentDictionary<Type, OpActivator>();
        }

        public static Block[] DecodeBasicBlock(MemoryManager memory, ulong address, ExecutionMode mode)
        {
            Block block = new Block(address);

            FillBlock(memory, mode, block, ulong.MaxValue);

            OpCode64 lastOp = block.GetLastOp();

            if (IsBranch(lastOp) && !IsCall(lastOp) && lastOp is IOpCodeBImm op)
            {
                // It's possible that the branch on this block lands on the middle of the block.
                // This is more common on tight loops. In this case, we can improve the codegen
                // a bit by changing the CFG and either making the branch point to the same block
                // (which indicates that the block is a loop that jumps back to the start), and the
                // other possible case is a jump somewhere on the middle of the block, which is
                // also a loop, but in this case we need to split the block in half.
                if ((ulong)op.Imm == address)
                {
                    block.Branch = block;
                }
                else if ((ulong)op.Imm > address &&
                         (ulong)op.Imm < block.EndAddress)
                {
                    Block rightBlock = new Block((ulong)op.Imm);

                    block.Split(rightBlock);

                    return new Block[] { block, rightBlock };
                }
            }

            return new Block[] { block };
        }

        public static Block[] DecodeSubroutine(MemoryManager memory, ulong address, ExecutionMode mode)
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
                    OpCode64 lastOp = currBlock.GetLastOp();

                    bool isCall = IsCall(lastOp);

                    if (lastOp is IOpCodeBImm op && !isCall)
                    {
                        currBlock.Branch = GetBlock((ulong)op.Imm);
                    }

                    if (!IsUnconditionalBranch(lastOp) || isCall)
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

            OpCode64 opCode;

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

        private static bool IsBranch(OpCode64 opCode)
        {
            return opCode is OpCodeBImm64 ||
                   opCode is OpCodeBReg64 || IsAarch32Branch(opCode);
        }

        private static bool IsUnconditionalBranch(OpCode64 opCode)
        {
            return opCode is OpCodeBImmAl64 ||
                   opCode is OpCodeBReg64   || IsAarch32UnconditionalBranch(opCode);
        }

        private static bool IsAarch32UnconditionalBranch(OpCode64 opCode)
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

        private static bool IsAarch32Branch(OpCode64 opCode)
        {
            // Note: On ARM32, most ALU operations can write to R15 (PC),
            // so we must consider such operations as a branch in potential as well.
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
                    if (rt == 14 && opMem.Emitter == InstEmit32.Ldrd)
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

        private static bool IsCall(OpCode64 opCode)
        {
            // TODO (CQ): ARM32 support.
            return opCode.Emitter == InstEmit.Bl ||
                   opCode.Emitter == InstEmit.Blr;
        }

        private static bool IsException(OpCode64 opCode)
        {
            return opCode.Emitter == InstEmit.Brk ||
                   opCode.Emitter == InstEmit.Svc ||
                   opCode.Emitter == InstEmit.Und;
        }

        public static OpCode64 DecodeOpCode(MemoryManager memory, ulong address, ExecutionMode mode)
        {
            int opCode = memory.ReadInt32((long)address);

            Inst inst;

            if (mode == ExecutionMode.Aarch64)
            {
                inst = OpCodeTable.GetInstA64(opCode);
            }
            else
            {
                if (mode == ExecutionMode.Aarch32Arm)
                {
                    inst = OpCodeTable.GetInstA32(opCode);
                }
                else /* if (mode == ExecutionMode.Aarch32Thumb) */
                {
                    inst = OpCodeTable.GetInstT32(opCode);
                }
            }

            OpCode64 decodedOpCode = new OpCode64(Inst.Undefined, (long)address, opCode);

            if (inst.Type != null)
            {
                decodedOpCode = MakeOpCode(inst.Type, inst, (long)address, opCode);
            }

            return decodedOpCode;
        }

        private static OpCode64 MakeOpCode(Type type, Inst inst, long position, int opCode)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            OpActivator createInstance = _opActivators.GetOrAdd(type, CacheOpActivator);

            return (OpCode64)createInstance(inst, position, opCode);
        }

        private static OpActivator CacheOpActivator(Type type)
        {
            Type[] argTypes = new Type[] { typeof(Inst), typeof(long), typeof(int) };

            DynamicMethod mthd = new DynamicMethod($"Make{type.Name}", type, argTypes);

            ILGenerator generator = mthd.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Newobj, type.GetConstructor(argTypes));
            generator.Emit(OpCodes.Ret);

            return (OpActivator)mthd.CreateDelegate(typeof(OpActivator));
        }
    }
}