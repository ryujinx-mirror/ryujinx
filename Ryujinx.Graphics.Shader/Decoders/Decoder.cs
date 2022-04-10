using Ryujinx.Graphics.Shader.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Decoders
{
    static class Decoder
    {
        public static DecodedProgram Decode(ShaderConfig config, ulong startAddress)
        {
            Queue<DecodedFunction> functionsQueue = new Queue<DecodedFunction>();
            Dictionary<ulong, DecodedFunction> functionsVisited = new Dictionary<ulong, DecodedFunction>();

            DecodedFunction EnqueueFunction(ulong address)
            {
                if (!functionsVisited.TryGetValue(address, out DecodedFunction function))
                {
                    functionsVisited.Add(address, function = new DecodedFunction(address));
                    functionsQueue.Enqueue(function);
                }

                return function;
            }

            DecodedFunction mainFunction = EnqueueFunction(0);

            while (functionsQueue.TryDequeue(out DecodedFunction currentFunction))
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

                GetBlock(currentFunction.Address);

                bool hasNewTarget;

                do
                {
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

                        FillBlock(config, currBlock, limitAddress, startAddress);

                        if (currBlock.OpCodes.Count != 0)
                        {
                            // We should have blocks for all possible branch targets,
                            // including those from PBK/PCNT/SSY instructions.
                            foreach (PushOpInfo pushOp in currBlock.PushOpCodes)
                            {
                                GetBlock(pushOp.Op.GetAbsoluteAddress());
                            }

                            // Set child blocks. "Branch" is the block the branch instruction
                            // points to (when taken), "Next" is the block at the next address,
                            // executed when the branch is not taken. For Unconditional Branches
                            // or end of program, Next is null.
                            InstOp lastOp = currBlock.GetLastOp();

                            if (lastOp.Name == InstName.Cal)
                            {
                                EnqueueFunction(lastOp.GetAbsoluteAddress()).AddCaller(currentFunction);
                            }
                            else if (lastOp.Name == InstName.Bra)
                            {
                                Block succBlock = GetBlock(lastOp.GetAbsoluteAddress());
                                currBlock.Successors.Add(succBlock);
                                succBlock.Predecessors.Add(currBlock);
                            }

                            if (!IsUnconditionalBranch(ref lastOp))
                            {
                                Block succBlock = GetBlock(currBlock.EndAddress);
                                currBlock.Successors.Insert(0, succBlock);
                                succBlock.Predecessors.Add(currBlock);
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

                    // Propagate SSY/PBK addresses into their uses (SYNC/BRK).
                    foreach (Block block in blocks.Where(x => x.PushOpCodes.Count != 0))
                    {
                        for (int pushOpIndex = 0; pushOpIndex < block.PushOpCodes.Count; pushOpIndex++)
                        {
                            PropagatePushOp(visited, block, pushOpIndex);
                        }
                    }

                    // Try to find targets for BRX (indirect branch) instructions.
                    hasNewTarget = FindBrxTargets(config, blocks, GetBlock);

                    // If we discovered new branch targets from the BRX instruction,
                    // we need another round of decoding to decode the new blocks.
                    // Additionally, we may have more SSY/PBK targets to propagate,
                    // and new BRX instructions.
                }
                while (hasNewTarget);

                currentFunction.SetBlocks(blocks.ToArray());
            }

            return new DecodedProgram(mainFunction, functionsVisited);
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

        private static void FillBlock(ShaderConfig config, Block block, ulong limitAddress, ulong startAddress)
        {
            IGpuAccessor gpuAccessor = config.GpuAccessor;

            ulong address = block.Address;
            int bufferOffset = 0;
            ReadOnlySpan<ulong> buffer = ReadOnlySpan<ulong>.Empty;

            InstOp op = default;

            do
            {
                if (address + 7 >= limitAddress)
                {
                    break;
                }

                // Ignore scheduling instructions, which are written every 32 bytes.
                if ((address & 0x1f) == 0)
                {
                    address += 8;
                    bufferOffset++;
                    continue;
                }

                if (bufferOffset >= buffer.Length)
                {
                    buffer = gpuAccessor.GetCode(startAddress + address, 8);
                    bufferOffset = 0;
                }

                ulong opCode = buffer[bufferOffset++];

                op = InstTable.GetOp(address, opCode);

                if (op.Props.HasFlag(InstProps.TexB))
                {
                    config.SetUsedFeature(FeatureFlags.Bindless);
                }

                if (op.Name == InstName.Ald || op.Name == InstName.Ast || op.Name == InstName.Ipa)
                {
                    SetUserAttributeUses(config, op.Name, opCode);
                }
                else if (op.Name == InstName.Pbk || op.Name == InstName.Pcnt || op.Name == InstName.Ssy)
                {
                    block.AddPushOp(op);
                }

                block.OpCodes.Add(op);

                address += 8;
            }
            while (!op.Props.HasFlag(InstProps.Bra));

            block.EndAddress = address;
        }

        private static void SetUserAttributeUses(ShaderConfig config, InstName name, ulong opCode)
        {
            int offset;
            int count = 1;
            bool isStore = false;
            bool indexed = false;
            bool perPatch = false;

            if (name == InstName.Ast)
            {
                InstAst opAst = new InstAst(opCode);
                count = (int)opAst.AlSize + 1;
                offset = opAst.Imm11;
                indexed = opAst.Phys;
                perPatch = opAst.P;
                isStore = true;
            }
            else if (name == InstName.Ald)
            {
                InstAld opAld = new InstAld(opCode);
                count = (int)opAld.AlSize + 1;
                offset = opAld.Imm11;
                indexed = opAld.Phys;
                perPatch = opAld.P;
                isStore = opAld.O;
            }
            else /* if (name == InstName.Ipa) */
            {
                InstIpa opIpa = new InstIpa(opCode);
                offset = opIpa.Imm10;
                indexed = opIpa.Idx;
            }

            if (indexed)
            {
                if (isStore)
                {
                    config.SetAllOutputUserAttributes();
                }
                else
                {
                    config.SetAllInputUserAttributes();
                }
            }
            else
            {
                for (int elemIndex = 0; elemIndex < count; elemIndex++)
                {
                    int attr = offset + elemIndex * 4;
                    if (attr >= AttributeConsts.UserAttributeBase && attr < AttributeConsts.UserAttributeEnd)
                    {
                        int userAttr = attr - AttributeConsts.UserAttributeBase;
                        int index = userAttr / 16;

                        if (isStore)
                        {
                            config.SetOutputUserAttribute(index, perPatch);
                        }
                        else
                        {
                            config.SetInputUserAttribute(index, (userAttr >> 2) & 3, perPatch);
                        }
                    }

                    if (!isStore &&
                        ((attr >= AttributeConsts.FrontColorDiffuseR && attr < AttributeConsts.ClipDistance0) ||
                        (attr >= AttributeConsts.TexCoordBase && attr < AttributeConsts.TexCoordEnd)))
                    {
                        config.SetUsedFeature(FeatureFlags.FixedFuncAttr);
                    }
                }
            }
        }

        public static bool IsUnconditionalBranch(ref InstOp op)
        {
            return IsUnconditional(ref op) && op.Props.HasFlag(InstProps.Bra);
        }

        private static bool IsUnconditional(ref InstOp op)
        {
            InstConditional condOp = new InstConditional(op.RawOpCode);

            if (op.Name == InstName.Exit && condOp.Ccc != Ccc.T)
            {
                return false;
            }

            return condOp.Pred == RegisterConsts.PredicateTrueIndex && !condOp.PredInv;
        }

        private static bool FindBrxTargets(ShaderConfig config, IEnumerable<Block> blocks, Func<ulong, Block> getBlock)
        {
            bool hasNewTarget = false;

            foreach (Block block in blocks)
            {
                InstOp lastOp = block.GetLastOp();
                bool hasNext = block.HasNext();

                if (lastOp.Name == InstName.Brx && block.Successors.Count == (hasNext ? 1 : 0))
                {
                    InstBrx opBrx = new InstBrx(lastOp.RawOpCode);
                    ulong baseOffset = lastOp.GetAbsoluteAddress();

                    // An indirect branch could go anywhere,
                    // try to get the possible target offsets from the constant buffer.
                    (int cbBaseOffset, int cbOffsetsCount) = FindBrxTargetRange(block, opBrx.SrcA);

                    if (cbOffsetsCount != 0)
                    {
                        hasNewTarget = true;
                    }

                    for (int i = 0; i < cbOffsetsCount; i++)
                    {
                        uint targetOffset = config.ConstantBuffer1Read(cbBaseOffset + i * 4);
                        Block target = getBlock(baseOffset + targetOffset);
                        target.Predecessors.Add(block);
                        block.Successors.Add(target);
                    }
                }
            }

            return hasNewTarget;
        }

        private static (int, int) FindBrxTargetRange(Block block, int brxReg)
        {
            // Try to match the following pattern:
            //
            // IMNMX.U32 Rx, Rx, UpperBound, PT
            // SHL Rx, Rx, 0x2
            // LDC Rx, c[0x1][Rx+BaseOffset]
            //
            // Here, Rx is an arbitrary register, "UpperBound" and "BaseOffset" are constants.
            // The above pattern is assumed to be generated by the compiler before BRX,
            // as the instruction is usually used to implement jump tables for switch statement optimizations.
            // On a successful match, "BaseOffset" is the offset in bytes where the jump offsets are
            // located on the constant buffer, and "UpperBound" is the total number of offsets for the BRX, minus 1.

            HashSet<Block> visited = new HashSet<Block>();

            var ldcLocation = FindFirstRegWrite(visited, new BlockLocation(block, block.OpCodes.Count - 1), brxReg);
            if (ldcLocation.Block == null || ldcLocation.Block.OpCodes[ldcLocation.Index].Name != InstName.Ldc)
            {
                return (0, 0);
            }

            GetOp<InstLdc>(ldcLocation, out var opLdc);

            if (opLdc.CbufSlot != 1 || opLdc.AddressMode != 0)
            {
                return (0, 0);
            }

            var shlLocation = FindFirstRegWrite(visited, ldcLocation, opLdc.SrcA);
            if (shlLocation.Block == null || !shlLocation.IsImmInst(InstName.Shl))
            {
                return (0, 0);
            }

            GetOp<InstShlI>(shlLocation, out var opShl);

            if (opShl.Imm20 != 2)
            {
                return (0, 0);
            }

            var imnmxLocation = FindFirstRegWrite(visited, shlLocation, opShl.SrcA);
            if (imnmxLocation.Block == null || !imnmxLocation.IsImmInst(InstName.Imnmx))
            {
                return (0, 0);
            }

            GetOp<InstImnmxI>(imnmxLocation, out var opImnmx);

            if (opImnmx.Signed || opImnmx.SrcPred != RegisterConsts.PredicateTrueIndex || opImnmx.SrcPredInv)
            {
                return (0, 0);
            }

            return (opLdc.CbufOffset, opImnmx.Imm20 + 1);
        }

        private static void GetOp<T>(BlockLocation location, out T op) where T : unmanaged
        {
            ulong rawOp = location.Block.OpCodes[location.Index].RawOpCode;
            op = Unsafe.As<ulong, T>(ref rawOp);
        }

        private struct BlockLocation
        {
            public Block Block { get; }
            public int Index { get; }

            public BlockLocation(Block block, int index)
            {
                Block = block;
                Index = index;
            }

            public bool IsImmInst(InstName name)
            {
                InstOp op = Block.OpCodes[Index];
                return op.Name == name && op.Props.HasFlag(InstProps.Ib);
            }
        }

        private static BlockLocation FindFirstRegWrite(HashSet<Block> visited, BlockLocation location, int regIndex)
        {
            Queue<BlockLocation> toVisit = new Queue<BlockLocation>();
            toVisit.Enqueue(location);
            visited.Add(location.Block);

            while (toVisit.TryDequeue(out var currentLocation))
            {
                Block block = currentLocation.Block;
                for (int i = currentLocation.Index - 1; i >= 0; i--)
                {
                    if (WritesToRegister(block.OpCodes[i], regIndex))
                    {
                        return new BlockLocation(block, i);
                    }
                }

                foreach (Block predecessor in block.Predecessors)
                {
                    if (visited.Add(predecessor))
                    {
                        toVisit.Enqueue(new BlockLocation(predecessor, predecessor.OpCodes.Count));
                    }
                }
            }

            return new BlockLocation(null, 0);
        }

        private static bool WritesToRegister(InstOp op, int regIndex)
        {
            // Predicate instruction only ever writes to predicate, so we shouldn't check those.
            if ((op.Props & (InstProps.Rd | InstProps.Rd2)) == 0)
            {
                return false;
            }

            if (op.Props.HasFlag(InstProps.Rd2) && (byte)(op.RawOpCode >> 28) == regIndex)
            {
                return true;
            }

            return (byte)op.RawOpCode == regIndex;
        }

        private enum MergeType
        {
            Brk,
            Cont,
            Sync
        }

        private struct PathBlockState
        {
            public Block Block { get; }

            private enum RestoreType
            {
                None,
                PopPushOp,
                PushBranchOp
            }

            private RestoreType _restoreType;

            private ulong _restoreValue;
            private MergeType _restoreMergeType;

            public bool ReturningFromVisit => _restoreType != RestoreType.None;

            public PathBlockState(Block block)
            {
                Block             = block;
                _restoreType      = RestoreType.None;
                _restoreValue     = 0;
                _restoreMergeType = default;
            }

            public PathBlockState(int oldStackSize)
            {
                Block             = null;
                _restoreType      = RestoreType.PopPushOp;
                _restoreValue     = (ulong)oldStackSize;
                _restoreMergeType = default;
            }

            public PathBlockState(ulong syncAddress, MergeType mergeType)
            {
                Block             = null;
                _restoreType      = RestoreType.PushBranchOp;
                _restoreValue     = syncAddress;
                _restoreMergeType = mergeType;
            }

            public void RestoreStackState(Stack<(ulong, MergeType)> branchStack)
            {
                if (_restoreType == RestoreType.PushBranchOp)
                {
                    branchStack.Push((_restoreValue, _restoreMergeType));
                }
                else if (_restoreType == RestoreType.PopPushOp)
                {
                    while (branchStack.Count > (uint)_restoreValue)
                    {
                        branchStack.Pop();
                    }
                }
            }
        }

        private static void PropagatePushOp(Dictionary<ulong, Block> blocks, Block currBlock, int pushOpIndex)
        {
            PushOpInfo pushOpInfo = currBlock.PushOpCodes[pushOpIndex];
            InstOp pushOp = pushOpInfo.Op;

            Block target = blocks[pushOp.GetAbsoluteAddress()];

            Stack<PathBlockState> workQueue = new Stack<PathBlockState>();
            HashSet<Block> visited = new HashSet<Block>();
            Stack<(ulong, MergeType)> branchStack = new Stack<(ulong, MergeType)>();

            void Push(PathBlockState pbs)
            {
                // When block is null, this means we are pushing a restore operation.
                // Restore operations are used to undo the work done inside a block
                // when we return from it, for example it pops addresses pushed by
                // SSY/PBK instructions inside the block, and pushes addresses poped
                // by SYNC/BRK.
                // For blocks, if it's already visited, we just ignore to avoid going
                // around in circles and getting stuck here.
                if (pbs.Block == null || !visited.Contains(pbs.Block))
                {
                    workQueue.Push(pbs);
                }
            }

            Push(new PathBlockState(currBlock));

            while (workQueue.TryPop(out PathBlockState pbs))
            {
                if (pbs.ReturningFromVisit)
                {
                    pbs.RestoreStackState(branchStack);

                    continue;
                }

                Block current = pbs.Block;

                // If the block was already processed, we just ignore it, otherwise
                // we would push the same child blocks of an already processed block,
                // and go around in circles until memory is exhausted.
                if (!visited.Add(current))
                {
                    continue;
                }

                int pushOpsCount = current.PushOpCodes.Count;
                if (pushOpsCount != 0)
                {
                    Push(new PathBlockState(branchStack.Count));

                    for (int index = pushOpIndex; index < pushOpsCount; index++)
                    {
                        InstOp currentPushOp = current.PushOpCodes[index].Op;
                        MergeType pushMergeType = GetMergeTypeFromPush(currentPushOp.Name);
                        branchStack.Push((currentPushOp.GetAbsoluteAddress(), pushMergeType));
                    }
                }

                pushOpIndex = 0;

                bool hasNext = current.HasNext();
                if (hasNext)
                {
                    Push(new PathBlockState(current.Successors[0]));
                }

                InstOp lastOp = current.GetLastOp();
                if (IsPopBranch(lastOp.Name))
                {
                    MergeType popMergeType = GetMergeTypeFromPop(lastOp.Name);

                    bool found = true;
                    ulong targetAddress = 0UL;
                    MergeType mergeType;

                    do
                    {
                        if (branchStack.Count == 0)
                        {
                            found = false;
                            break;
                        }

                        (targetAddress, mergeType) = branchStack.Pop();

                        // Push the target address (this will be used to push the address
                        // back into the PBK/PCNT/SSY stack when we return from that block),
                        Push(new PathBlockState(targetAddress, mergeType));
                    }
                    while (mergeType != popMergeType);

                    // Make sure we found the correct address,
                    // the push and pop instruction types must match, so:
                    // - BRK can only consume addresses pushed by PBK.
                    // - SYNC can only consume addresses pushed by SSY.
                    if (found)
                    {
                        if (branchStack.Count == 0)
                        {
                            // If the entire stack was consumed, then the current pop instruction
                            // just consumed the address from our push instruction.
                            if (current.SyncTargets.TryAdd(pushOp.Address, new SyncTarget(pushOpInfo, current.SyncTargets.Count)))
                            {
                                pushOpInfo.Consumers.Add(current, Local());
                                target.Predecessors.Add(current);
                                current.Successors.Add(target);
                            }
                        }
                        else
                        {
                            // Push the block itself into the work queue for processing.
                            Push(new PathBlockState(blocks[targetAddress]));
                        }
                    }
                }
                else
                {
                    // By adding them in descending order (sorted by address), we process the blocks
                    // in order (of ascending address), since we work with a LIFO.
                    foreach (Block possibleTarget in current.Successors.OrderByDescending(x => x.Address))
                    {
                        if (!hasNext || possibleTarget != current.Successors[0])
                        {
                            Push(new PathBlockState(possibleTarget));
                        }
                    }
                }
            }
        }

        public static bool IsPopBranch(InstName name)
        {
            return name == InstName.Brk || name == InstName.Cont || name == InstName.Sync;
        }

        private static MergeType GetMergeTypeFromPush(InstName name)
        {
            return name switch
            {
                InstName.Pbk => MergeType.Brk,
                InstName.Pcnt => MergeType.Cont,
                _ => MergeType.Sync
            };
        }

        private static MergeType GetMergeTypeFromPop(InstName name)
        {
            return name switch
            {
                InstName.Brk => MergeType.Brk,
                InstName.Cont => MergeType.Cont,
                _ => MergeType.Sync
            };
        }
    }
}