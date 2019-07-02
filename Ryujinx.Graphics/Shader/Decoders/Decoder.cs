using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Shader.Instructions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Decoders
{
    static class Decoder
    {
        private const long HeaderSize = 0x50;

        private delegate object OpActivator(InstEmitter emitter, ulong address, long opCode);

        private static ConcurrentDictionary<Type, OpActivator> _opActivators;

        static Decoder()
        {
            _opActivators = new ConcurrentDictionary<Type, OpActivator>();
        }

        public static Block[] Decode(IGalMemory memory, ulong address)
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

            ulong startAddress = address + HeaderSize;

            GetBlock(startAddress);

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

                FillBlock(memory, currBlock, limitAddress, startAddress);

                if (currBlock.OpCodes.Count != 0)
                {
                    foreach (OpCodeSsy ssyOp in currBlock.SsyOpCodes)
                    {
                        GetBlock(ssyOp.GetAbsoluteAddress());
                    }

                    // Set child blocks. "Branch" is the block the branch instruction
                    // points to (when taken), "Next" is the block at the next address,
                    // executed when the branch is not taken. For Unconditional Branches
                    // or end of program, Next is null.
                    OpCode lastOp = currBlock.GetLastOp();

                    if (lastOp is OpCodeBranch op)
                    {
                        currBlock.Branch = GetBlock(op.GetAbsoluteAddress());
                    }

                    if (!IsUnconditionalBranch(lastOp))
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

            foreach (Block ssyBlock in blocks.Where(x => x.SsyOpCodes.Count != 0))
            {
                for (int ssyIndex = 0; ssyIndex < ssyBlock.SsyOpCodes.Count; ssyIndex++)
                {
                    PropagateSsy(visited, ssyBlock, ssyIndex);
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
            IGalMemory memory,
            Block      block,
            ulong      limitAddress,
            ulong      startAddress)
        {
            ulong address = block.Address;

            do
            {
                if (address >= limitAddress)
                {
                    break;
                }

                // Ignore scheduling instructions, which are written every 32 bytes.
                if (((address - startAddress) & 0x1f) == 0)
                {
                    address += 8;

                    continue;
                }

                uint word0 = (uint)memory.ReadInt32((long)(address + 0));
                uint word1 = (uint)memory.ReadInt32((long)(address + 4));

                ulong opAddress = address;

                address += 8;

                long opCode = word0 | (long)word1 << 32;

                (InstEmitter emitter, Type opCodeType) = OpCodeTable.GetEmitter(opCode);

                if (emitter == null)
                {
                    // TODO: Warning, illegal encoding.
                    continue;
                }

                OpCode op = MakeOpCode(opCodeType, emitter, opAddress, opCode);

                block.OpCodes.Add(op);
            }
            while (!IsBranch(block.GetLastOp()));

            block.EndAddress = address;

            block.UpdateSsyOpCodes();
        }

        private static bool IsUnconditionalBranch(OpCode opCode)
        {
            return IsUnconditional(opCode) && IsBranch(opCode);
        }

        private static bool IsUnconditional(OpCode opCode)
        {
            if (opCode is OpCodeExit op && op.Condition != Condition.Always)
            {
                return false;
            }

            return opCode.Predicate.Index == RegisterConsts.PredicateTrueIndex && !opCode.InvertPredicate;
        }

        private static bool IsBranch(OpCode opCode)
        {
            return (opCode is OpCodeBranch && opCode.Emitter != InstEmit.Ssy) ||
                    opCode is OpCodeSync ||
                    opCode is OpCodeExit;
        }

        private static OpCode MakeOpCode(Type type, InstEmitter emitter, ulong address, long opCode)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            OpActivator createInstance = _opActivators.GetOrAdd(type, CacheOpActivator);

            return (OpCode)createInstance(emitter, address, opCode);
        }

        private static OpActivator CacheOpActivator(Type type)
        {
            Type[] argTypes = new Type[] { typeof(InstEmitter), typeof(ulong), typeof(long) };

            DynamicMethod mthd = new DynamicMethod($"Make{type.Name}", type, argTypes);

            ILGenerator generator = mthd.GetILGenerator();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Newobj, type.GetConstructor(argTypes));
            generator.Emit(OpCodes.Ret);

            return (OpActivator)mthd.CreateDelegate(typeof(OpActivator));
        }

        private struct PathBlockState
        {
            public Block Block { get; }

            private enum RestoreType
            {
                None,
                PopSsy,
                PushSync
            }

            private RestoreType _restoreType;

            private ulong _restoreValue;

            public bool ReturningFromVisit => _restoreType != RestoreType.None;

            public PathBlockState(Block block)
            {
                Block         = block;
                _restoreType  = RestoreType.None;
                _restoreValue = 0;
            }

            public PathBlockState(int oldSsyStackSize)
            {
                Block         = null;
                _restoreType  = RestoreType.PopSsy;
                _restoreValue = (ulong)oldSsyStackSize;
            }

            public PathBlockState(ulong syncAddress)
            {
                Block         = null;
                _restoreType  = RestoreType.PushSync;
                _restoreValue = syncAddress;
            }

            public void RestoreStackState(Stack<ulong> ssyStack)
            {
                if (_restoreType == RestoreType.PushSync)
                {
                    ssyStack.Push(_restoreValue);
                }
                else if (_restoreType == RestoreType.PopSsy)
                {
                    while (ssyStack.Count > (uint)_restoreValue)
                    {
                        ssyStack.Pop();
                    }
                }
            }
        }

        private static void PropagateSsy(Dictionary<ulong, Block> blocks, Block ssyBlock, int ssyIndex)
        {
            OpCodeSsy ssyOp = ssyBlock.SsyOpCodes[ssyIndex];

            Stack<PathBlockState> workQueue = new Stack<PathBlockState>();

            HashSet<Block> visited = new HashSet<Block>();

            Stack<ulong> ssyStack = new Stack<ulong>();

            void Push(PathBlockState pbs)
            {
                if (pbs.Block == null || visited.Add(pbs.Block))
                {
                    workQueue.Push(pbs);
                }
            }

            Push(new PathBlockState(ssyBlock));

            while (workQueue.TryPop(out PathBlockState pbs))
            {
                if (pbs.ReturningFromVisit)
                {
                    pbs.RestoreStackState(ssyStack);

                    continue;
                }

                Block current = pbs.Block;

                int ssyOpCodesCount = current.SsyOpCodes.Count;

                if (ssyOpCodesCount != 0)
                {
                    Push(new PathBlockState(ssyStack.Count));

                    for (int index = ssyIndex; index < ssyOpCodesCount; index++)
                    {
                        ssyStack.Push(current.SsyOpCodes[index].GetAbsoluteAddress());
                    }
                }

                ssyIndex = 0;

                if (current.Next != null)
                {
                    Push(new PathBlockState(current.Next));
                }

                if (current.Branch != null)
                {
                    Push(new PathBlockState(current.Branch));
                }
                else if (current.GetLastOp() is OpCodeSync op)
                {
                    ulong syncAddress = ssyStack.Pop();

                    if (ssyStack.Count == 0)
                    {
                        ssyStack.Push(syncAddress);

                        op.Targets.Add(ssyOp, op.Targets.Count);

                        ssyOp.Syncs.TryAdd(op, Local());
                    }
                    else
                    {
                        Push(new PathBlockState(syncAddress));
                        Push(new PathBlockState(blocks[syncAddress]));
                    }
                }
            }
        }
    }
}