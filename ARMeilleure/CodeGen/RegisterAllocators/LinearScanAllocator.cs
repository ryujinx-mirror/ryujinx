using ARMeilleure.Common;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace ARMeilleure.CodeGen.RegisterAllocators
{
    // Based on:
    // "Linear Scan Register Allocation for the Java(tm) HotSpot Client Compiler".
    // http://www.christianwimmer.at/Publications/Wimmer04a/Wimmer04a.pdf
    class LinearScanAllocator : IRegisterAllocator
    {
        private const int InstructionGap     = 2;
        private const int InstructionGapMask = InstructionGap - 1;

        private const int RegistersCount = 16;

        private HashSet<int> _blockEdges;

        private LiveRange[] _blockRanges;

        private BitMap[] _blockLiveIn;

        private List<LiveInterval> _intervals;

        private LiveInterval[] _parentIntervals;

        private List<(IntrusiveList<Node>, Node)> _operationNodes;

        private int _operationsCount;

        private class AllocationContext : IDisposable
        {
            public RegisterMasks Masks { get; }

            public StackAllocator StackAlloc { get; }

            public BitMap Active   { get; }
            public BitMap Inactive { get; }

            public int IntUsedRegisters { get; set; }
            public int VecUsedRegisters { get; set; }

            public AllocationContext(StackAllocator stackAlloc, RegisterMasks masks, int intervalsCount)
            {
                StackAlloc = stackAlloc;
                Masks      = masks;

                Active   = BitMapPool.Allocate(intervalsCount);
                Inactive = BitMapPool.Allocate(intervalsCount);
            }

            public void MoveActiveToInactive(int bit)
            {
                Move(Active, Inactive, bit);
            }

            public void MoveInactiveToActive(int bit)
            {
                Move(Inactive, Active, bit);
            }

            private static void Move(BitMap source, BitMap dest, int bit)
            {
                source.Clear(bit);

                dest.Set(bit);
            }

            public void Dispose()
            {
                BitMapPool.Release();
            }
        }

        public AllocationResult RunPass(
            ControlFlowGraph cfg,
            StackAllocator stackAlloc,
            RegisterMasks regMasks)
        {
            NumberLocals(cfg);

            AllocationContext context = new AllocationContext(stackAlloc, regMasks, _intervals.Count);

            BuildIntervals(cfg, context);

            for (int index = 0; index < _intervals.Count; index++)
            {
                LiveInterval current = _intervals[index];

                if (current.IsEmpty)
                {
                    continue;
                }

                if (current.IsFixed)
                {
                    context.Active.Set(index);

                    if (current.Register.Type == RegisterType.Integer)
                    {
                        context.IntUsedRegisters |= 1 << current.Register.Index;
                    }
                    else /* if (interval.Register.Type == RegisterType.Vector) */
                    {
                        context.VecUsedRegisters |= 1 << current.Register.Index;
                    }

                    continue;
                }

                AllocateInterval(context, current, index);
            }

            for (int index = RegistersCount * 2; index < _intervals.Count; index++)
            {
                if (!_intervals[index].IsSpilled)
                {
                    ReplaceLocalWithRegister(_intervals[index]);
                }
            }

            InsertSplitCopies();
            InsertSplitCopiesAtEdges(cfg);

            AllocationResult result = new AllocationResult(
                context.IntUsedRegisters,
                context.VecUsedRegisters,
                context.StackAlloc.TotalSize);

            context.Dispose();

            return result;
        }

        private void AllocateInterval(AllocationContext context, LiveInterval current, int cIndex)
        {
            // Check active intervals that already ended.
            foreach (int iIndex in context.Active)
            {
                LiveInterval interval = _intervals[iIndex];

                if (interval.GetEnd() < current.GetStart())
                {
                    context.Active.Clear(iIndex);
                }
                else if (!interval.Overlaps(current.GetStart()))
                {
                    context.MoveActiveToInactive(iIndex);
                }
            }

            // Check inactive intervals that already ended or were reactivated.
            foreach (int iIndex in context.Inactive)
            {
                LiveInterval interval = _intervals[iIndex];

                if (interval.GetEnd() < current.GetStart())
                {
                    context.Inactive.Clear(iIndex);
                }
                else if (interval.Overlaps(current.GetStart()))
                {
                    context.MoveInactiveToActive(iIndex);
                }
            }

            if (!TryAllocateRegWithoutSpill(context, current, cIndex))
            {
                AllocateRegWithSpill(context, current, cIndex);
            }
        }

        private bool TryAllocateRegWithoutSpill(AllocationContext context, LiveInterval current, int cIndex)
        {
            RegisterType regType = current.Local.Type.ToRegisterType();

            int availableRegisters = context.Masks.GetAvailableRegisters(regType);

            int[] freePositions = new int[RegistersCount];

            for (int index = 0; index < RegistersCount; index++)
            {
                if ((availableRegisters & (1 << index)) != 0)
                {
                    freePositions[index] = int.MaxValue;
                }
            }

            foreach (int iIndex in context.Active)
            {
                LiveInterval interval = _intervals[iIndex];

                if (interval.Register.Type == regType)
                {
                    freePositions[interval.Register.Index] = 0;
                }
            }

            foreach (int iIndex in context.Inactive)
            {
                LiveInterval interval = _intervals[iIndex];

                if (interval.Register.Type == regType)
                {
                    int overlapPosition = interval.GetOverlapPosition(current);

                    if (overlapPosition != LiveInterval.NotFound && freePositions[interval.Register.Index] > overlapPosition)
                    {
                        freePositions[interval.Register.Index] = overlapPosition;
                    }
                }
            }

            int selectedReg = GetHighestValueIndex(freePositions);

            int selectedNextUse = freePositions[selectedReg];

            // Intervals starts and ends at odd positions, unless they span an entire
            // block, in this case they will have ranges at a even position.
            // When a interval is loaded from the stack to a register, we can only
            // do the split at a odd position, because otherwise the split interval
            // that is inserted on the list to be processed may clobber a register
            // used by the instruction at the same position as the split.
            // The problem only happens when a interval ends exactly at this instruction,
            // because otherwise they would interfere, and the register wouldn't be selected.
            // When the interval is aligned and the above happens, there's no problem as
            // the instruction that is actually with the last use is the one
            // before that position.
            selectedNextUse &= ~InstructionGapMask;

            if (selectedNextUse <= current.GetStart())
            {
                return false;
            }
            else if (selectedNextUse < current.GetEnd())
            {
                Debug.Assert(selectedNextUse > current.GetStart(), "Trying to split interval at the start.");

                LiveInterval splitChild = current.Split(selectedNextUse);

                if (splitChild.UsesCount != 0)
                {
                    Debug.Assert(splitChild.GetStart() > current.GetStart(), "Split interval has an invalid start position.");

                    InsertInterval(splitChild);
                }
                else
                {
                    Spill(context, splitChild);
                }
            }

            current.Register = new Register(selectedReg, regType);

            if (regType == RegisterType.Integer)
            {
                context.IntUsedRegisters |= 1 << selectedReg;
            }
            else /* if (regType == RegisterType.Vector) */
            {
                context.VecUsedRegisters |= 1 << selectedReg;
            }

            context.Active.Set(cIndex);

            return true;
        }

        private void AllocateRegWithSpill(AllocationContext context, LiveInterval current, int cIndex)
        {
            RegisterType regType = current.Local.Type.ToRegisterType();

            int availableRegisters = context.Masks.GetAvailableRegisters(regType);

            int[] usePositions     = new int[RegistersCount];
            int[] blockedPositions = new int[RegistersCount];

            for (int index = 0; index < RegistersCount; index++)
            {
                if ((availableRegisters & (1 << index)) != 0)
                {
                    usePositions[index] = int.MaxValue;

                    blockedPositions[index] = int.MaxValue;
                }
            }

            void SetUsePosition(int index, int position)
            {
                usePositions[index] = Math.Min(usePositions[index], position);
            }

            void SetBlockedPosition(int index, int position)
            {
                blockedPositions[index] = Math.Min(blockedPositions[index], position);

                SetUsePosition(index, position);
            }

            foreach (int iIndex in context.Active)
            {
                LiveInterval interval = _intervals[iIndex];

                if (!interval.IsFixed && interval.Register.Type == regType)
                {
                    int nextUse = interval.NextUseAfter(current.GetStart());

                    if (nextUse != -1)
                    {
                        SetUsePosition(interval.Register.Index, nextUse);
                    }
                }
            }

            foreach (int iIndex in context.Inactive)
            {
                LiveInterval interval = _intervals[iIndex];

                if (!interval.IsFixed && interval.Register.Type == regType && interval.Overlaps(current))
                {
                    int nextUse = interval.NextUseAfter(current.GetStart());

                    if (nextUse != -1)
                    {
                        SetUsePosition(interval.Register.Index, nextUse);
                    }
                }
            }

            foreach (int iIndex in context.Active)
            {
                LiveInterval interval = _intervals[iIndex];

                if (interval.IsFixed && interval.Register.Type == regType)
                {
                    SetBlockedPosition(interval.Register.Index, 0);
                }
            }

            foreach (int iIndex in context.Inactive)
            {
                LiveInterval interval = _intervals[iIndex];

                if (interval.IsFixed && interval.Register.Type == regType)
                {
                    int overlapPosition = interval.GetOverlapPosition(current);

                    if (overlapPosition != LiveInterval.NotFound)
                    {
                        SetBlockedPosition(interval.Register.Index, overlapPosition);
                    }
                }
            }

            int selectedReg = GetHighestValueIndex(usePositions);

            int currentFirstUse = current.FirstUse();

            Debug.Assert(currentFirstUse >= 0, "Current interval has no uses.");

            if (usePositions[selectedReg] < currentFirstUse)
            {
                // All intervals on inactive and active are being used before current,
                // so spill the current interval.
                Debug.Assert(currentFirstUse > current.GetStart(), "Trying to spill a interval currently being used.");

                LiveInterval splitChild = current.Split(currentFirstUse);

                Debug.Assert(splitChild.GetStart() > current.GetStart(), "Split interval has an invalid start position.");

                InsertInterval(splitChild);

                Spill(context, current);
            }
            else if (blockedPositions[selectedReg] > current.GetEnd())
            {
                // Spill made the register available for the entire current lifetime,
                // so we only need to split the intervals using the selected register.
                current.Register = new Register(selectedReg, regType);

                SplitAndSpillOverlappingIntervals(context, current);

                context.Active.Set(cIndex);
            }
            else
            {
                // There are conflicts even after spill due to the use of fixed registers
                // that can't be spilled, so we need to also split current at the point of
                // the first fixed register use.
                current.Register = new Register(selectedReg, regType);

                int splitPosition = blockedPositions[selectedReg] & ~InstructionGapMask;

                Debug.Assert(splitPosition > current.GetStart(), "Trying to split a interval at a invalid position.");

                LiveInterval splitChild = current.Split(splitPosition);

                if (splitChild.UsesCount != 0)
                {
                    Debug.Assert(splitChild.GetStart() > current.GetStart(), "Split interval has an invalid start position.");

                    InsertInterval(splitChild);
                }
                else
                {
                    Spill(context, splitChild);
                }

                SplitAndSpillOverlappingIntervals(context, current);

                context.Active.Set(cIndex);
            }
        }

        private static int GetHighestValueIndex(int[] array)
        {
            int higuest = array[0];

            if (higuest == int.MaxValue)
            {
                return 0;
            }

            int selected = 0;

            for (int index = 1; index < array.Length; index++)
            {
                int current = array[index];

                if (higuest < current)
                {
                    higuest  = current;
                    selected = index;

                    if (current == int.MaxValue)
                    {
                        break;
                    }
                }
            }

            return selected;
        }

        private void SplitAndSpillOverlappingIntervals(AllocationContext context, LiveInterval current)
        {
            foreach (int iIndex in context.Active)
            {
                LiveInterval interval = _intervals[iIndex];

                if (!interval.IsFixed && interval.Register == current.Register)
                {
                    SplitAndSpillOverlappingInterval(context, current, interval);

                    context.Active.Clear(iIndex);
                }
            }

            foreach (int iIndex in context.Inactive)
            {
                LiveInterval interval = _intervals[iIndex];

                if (!interval.IsFixed && interval.Register == current.Register && interval.Overlaps(current))
                {
                    SplitAndSpillOverlappingInterval(context, current, interval);

                    context.Inactive.Clear(iIndex);
                }
            }
        }

        private void SplitAndSpillOverlappingInterval(
            AllocationContext context,
            LiveInterval      current,
            LiveInterval      interval)
        {
            // If there's a next use after the start of the current interval,
            // we need to split the spilled interval twice, and re-insert it
            // on the "pending" list to ensure that it will get a new register
            // on that use position.
            int nextUse = interval.NextUseAfter(current.GetStart());

            LiveInterval splitChild;

            if (interval.GetStart() < current.GetStart())
            {
                splitChild = interval.Split(current.GetStart());
            }
            else
            {
                splitChild = interval;
            }

            if (nextUse != -1)
            {
                Debug.Assert(nextUse > current.GetStart(), "Trying to spill a interval currently being used.");

                if (nextUse > splitChild.GetStart())
                {
                    LiveInterval right = splitChild.Split(nextUse);

                    Spill(context, splitChild);

                    splitChild = right;
                }

                InsertInterval(splitChild);
            }
            else
            {
                Spill(context, splitChild);
            }
        }

        private void InsertInterval(LiveInterval interval)
        {
            Debug.Assert(interval.UsesCount != 0, "Trying to insert a interval without uses.");
            Debug.Assert(!interval.IsEmpty,       "Trying to insert a empty interval.");
            Debug.Assert(!interval.IsSpilled,     "Trying to insert a spilled interval.");

            int startIndex = RegistersCount * 2;

            int insertIndex = _intervals.BinarySearch(startIndex, _intervals.Count - startIndex, interval, null);

            if (insertIndex < 0)
            {
                insertIndex = ~insertIndex;
            }

            _intervals.Insert(insertIndex, interval);
        }

        private void Spill(AllocationContext context, LiveInterval interval)
        {
            Debug.Assert(!interval.IsFixed,       "Trying to spill a fixed interval.");
            Debug.Assert(interval.UsesCount == 0, "Trying to spill a interval with uses.");

            // We first check if any of the siblings were spilled, if so we can reuse
            // the stack offset. Otherwise, we allocate a new space on the stack.
            // This prevents stack-to-stack copies being necessary for a split interval.
            if (!interval.TrySpillWithSiblingOffset())
            {
                interval.Spill(context.StackAlloc.Allocate(interval.Local.Type));
            }
        }

        private void InsertSplitCopies()
        {
            Dictionary<int, CopyResolver> copyResolvers = new Dictionary<int, CopyResolver>();

            CopyResolver GetCopyResolver(int position)
            {
                CopyResolver copyResolver = new CopyResolver();

                if (copyResolvers.TryAdd(position, copyResolver))
                {
                    return copyResolver;
                }

                return copyResolvers[position];
            }

            foreach (LiveInterval interval in _intervals.Where(x => x.IsSplit))
            {
                LiveInterval previous = interval;

                foreach (LiveInterval splitChild in interval.SplitChilds())
                {
                    int splitPosition = splitChild.GetStart();

                    if (!_blockEdges.Contains(splitPosition) && previous.GetEnd() == splitPosition)
                    {
                        GetCopyResolver(splitPosition).AddSplit(previous, splitChild);
                    }

                    previous = splitChild;
                }
            }

            foreach (KeyValuePair<int, CopyResolver> kv in copyResolvers)
            {
                CopyResolver copyResolver = kv.Value;

                if (!copyResolver.HasCopy)
                {
                    continue;
                }

                int splitPosition = kv.Key;

                (IntrusiveList<Node> nodes, Node node) = GetOperationNode(splitPosition);

                Operation[] sequence = copyResolver.Sequence();

                nodes.AddBefore(node, sequence[0]);

                node = sequence[0];

                for (int index = 1; index < sequence.Length; index++)
                {
                    nodes.AddAfter(node, sequence[index]);

                    node = sequence[index];
                }
            }
        }

        private void InsertSplitCopiesAtEdges(ControlFlowGraph cfg)
        {
            int blocksCount = cfg.Blocks.Count;

            bool IsSplitEdgeBlock(BasicBlock block)
            {
                return block.Index >= blocksCount;
            }

            for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
            {
                if (IsSplitEdgeBlock(block))
                {
                    continue;
                }

                bool hasSingleOrNoSuccessor = block.SuccessorCount <= 1;

                for (int i = 0; i < block.SuccessorCount; i++)
                {
                    BasicBlock successor = block.GetSuccessor(i);

                    int succIndex = successor.Index;

                    // If the current node is a split node, then the actual successor node
                    // (the successor before the split) should be right after it.
                    if (IsSplitEdgeBlock(successor))
                    {
                        succIndex = successor.GetSuccessor(0).Index;
                    }

                    CopyResolver copyResolver = new CopyResolver();

                    foreach (int iIndex in _blockLiveIn[succIndex])
                    {
                        LiveInterval interval = _parentIntervals[iIndex];

                        if (!interval.IsSplit)
                        {
                            continue;
                        }

                        int lEnd   = _blockRanges[block.Index].End - 1;
                        int rStart = _blockRanges[succIndex].Start;

                        LiveInterval left  = interval.GetSplitChild(lEnd);
                        LiveInterval right = interval.GetSplitChild(rStart);

                        if (left != null && right != null && left != right)
                        {
                            copyResolver.AddSplit(left, right);
                        }
                    }

                    if (!copyResolver.HasCopy)
                    {
                        continue;
                    }

                    Operation[] sequence = copyResolver.Sequence();

                    if (hasSingleOrNoSuccessor)
                    {
                        foreach (Operation operation in sequence)
                        {
                            block.Append(operation);
                        }
                    }
                    else if (successor.Predecessors.Count == 1)
                    {
                        successor.Operations.AddFirst(sequence[0]);

                        Node prependNode = sequence[0];

                        for (int index = 1; index < sequence.Length; index++)
                        {
                            Operation operation = sequence[index];

                            successor.Operations.AddAfter(prependNode, operation);

                            prependNode = operation;
                        }
                    }
                    else
                    {
                        // Split the critical edge.
                        BasicBlock splitBlock = cfg.SplitEdge(block, successor);

                        foreach (Operation operation in sequence)
                        {
                            splitBlock.Append(operation);
                        }
                    }
                }
            }
        }

        private void ReplaceLocalWithRegister(LiveInterval current)
        {
            Operand register = GetRegister(current);

            IList<int> usePositions = current.UsePositions();
            for (int i = usePositions.Count - 1; i >= 0; i--)
            {
                int usePosition = -usePositions[i];
                (_, Node operation) = GetOperationNode(usePosition);

                for (int index = 0; index < operation.SourcesCount; index++)
                {
                    Operand source = operation.GetSource(index);

                    if (source == current.Local)
                    {
                        operation.SetSource(index, register);
                    }
                    else if (source.Kind == OperandKind.Memory)
                    {
                        MemoryOperand memOp = (MemoryOperand)source;

                        if (memOp.BaseAddress == current.Local)
                        {
                            memOp.BaseAddress = register;
                        }

                        if (memOp.Index == current.Local)
                        {
                            memOp.Index = register;
                        }
                    }
                }

                for (int index = 0; index < operation.DestinationsCount; index++)
                {
                    Operand dest = operation.GetDestination(index);

                    if (dest == current.Local)
                    {
                        operation.SetDestination(index, register);
                    }
                }
            }
        }

        private static Operand GetRegister(LiveInterval interval)
        {
            Debug.Assert(!interval.IsSpilled, "Spilled intervals are not allowed.");

            return OperandHelper.Register(
                interval.Register.Index,
                interval.Register.Type,
                interval.Local.Type);
        }

        private (IntrusiveList<Node>, Node) GetOperationNode(int position)
        {
            return _operationNodes[position / InstructionGap];
        }

        private void NumberLocals(ControlFlowGraph cfg)
        {
            _operationNodes = new List<(IntrusiveList<Node>, Node)>();

            _intervals = new List<LiveInterval>();

            for (int index = 0; index < RegistersCount; index++)
            {
                _intervals.Add(new LiveInterval(new Register(index, RegisterType.Integer)));
                _intervals.Add(new LiveInterval(new Register(index, RegisterType.Vector)));
            }

            HashSet<Operand> visited = new HashSet<Operand>();

            _operationsCount = 0;

            for (int index = cfg.PostOrderBlocks.Length - 1; index >= 0; index--)
            {
                BasicBlock block = cfg.PostOrderBlocks[index];

                for (Node node = block.Operations.First; node != null; node = node.ListNext)
                {
                    _operationNodes.Add((block.Operations, node));

                    for (int i = 0; i < node.DestinationsCount; i++)
                    {
                        Operand dest = node.GetDestination(i);
                        if (dest.Kind == OperandKind.LocalVariable && visited.Add(dest))
                        {
                            dest.NumberLocal(_intervals.Count);

                            _intervals.Add(new LiveInterval(dest));
                        }
                    }
                }

                _operationsCount += block.Operations.Count * InstructionGap;

                if (block.Operations.Count == 0)
                {
                    // Pretend we have a dummy instruction on the empty block.
                    _operationNodes.Add((null, null));

                    _operationsCount += InstructionGap;
                }
            }

            _parentIntervals = _intervals.ToArray();
        }

        private void BuildIntervals(ControlFlowGraph cfg, AllocationContext context)
        {
            _blockRanges = new LiveRange[cfg.Blocks.Count];

            int mapSize = _intervals.Count;

            BitMap[] blkLiveGen  = new BitMap[cfg.Blocks.Count];
            BitMap[] blkLiveKill = new BitMap[cfg.Blocks.Count];

            // Compute local live sets.
            for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
            {
                BitMap liveGen  = BitMapPool.Allocate(mapSize);
                BitMap liveKill = BitMapPool.Allocate(mapSize);

                for (Node node = block.Operations.First; node != null; node = node.ListNext)
                {
                    Sources(node, (source) =>
                    {
                        int id = GetOperandId(source);

                        if (!liveKill.IsSet(id))
                        {
                            liveGen.Set(id);
                        }
                    });

                    for (int i = 0; i < node.DestinationsCount; i++)
                    {
                        Operand dest = node.GetDestination(i);
                        liveKill.Set(GetOperandId(dest));
                    }
                }

                blkLiveGen [block.Index] = liveGen;
                blkLiveKill[block.Index] = liveKill;
            }

            // Compute global live sets.
            BitMap[] blkLiveIn  = new BitMap[cfg.Blocks.Count];
            BitMap[] blkLiveOut = new BitMap[cfg.Blocks.Count];

            for (int index = 0; index < cfg.Blocks.Count; index++)
            {
                blkLiveIn [index] = BitMapPool.Allocate(mapSize);
                blkLiveOut[index] = BitMapPool.Allocate(mapSize);
            }

            bool modified;

            do
            {
                modified = false;

                for (int index = 0; index < cfg.PostOrderBlocks.Length; index++)
                {
                    BasicBlock block = cfg.PostOrderBlocks[index];

                    BitMap liveOut = blkLiveOut[block.Index];

                    for (int i = 0; i < block.SuccessorCount; i++)
                    {
                        BasicBlock succ = block.GetSuccessor(i);

                        modified |= liveOut.Set(blkLiveIn[succ.Index]);
                    }

                    BitMap liveIn = blkLiveIn[block.Index];

                    liveIn.Set  (liveOut);
                    liveIn.Clear(blkLiveKill[block.Index]);
                    liveIn.Set  (blkLiveGen [block.Index]);
                }
            }
            while (modified);

            _blockLiveIn = blkLiveIn;

            _blockEdges = new HashSet<int>();

            // Compute lifetime intervals.
            int operationPos = _operationsCount;

            for (int index = 0; index < cfg.PostOrderBlocks.Length; index++)
            {
                BasicBlock block = cfg.PostOrderBlocks[index];

                // We handle empty blocks by pretending they have a dummy instruction,
                // because otherwise the block would have the same start and end position,
                // and this is not valid.
                int instCount = Math.Max(block.Operations.Count, 1);

                int blockStart = operationPos - instCount * InstructionGap;
                int blockEnd   = operationPos;

                _blockRanges[block.Index] = new LiveRange(blockStart, blockEnd);

                _blockEdges.Add(blockStart);

                BitMap liveOut = blkLiveOut[block.Index];

                foreach (int id in liveOut)
                {
                    _intervals[id].AddRange(blockStart, blockEnd);
                }

                if (block.Operations.Count == 0)
                {
                    operationPos -= InstructionGap;

                    continue;
                }

                foreach (Node node in BottomOperations(block))
                {
                    operationPos -= InstructionGap;

                    for (int i = 0; i < node.DestinationsCount; i++)
                    {
                        Operand dest = node.GetDestination(i);
                        LiveInterval interval = _intervals[GetOperandId(dest)];

                        interval.SetStart(operationPos + 1);
                        interval.AddUsePosition(operationPos + 1);
                    }

                    Sources(node, (source) =>
                    {
                        LiveInterval interval = _intervals[GetOperandId(source)];

                        interval.AddRange(blockStart, operationPos + 1);
                        interval.AddUsePosition(operationPos);
                    });

                    if (node is Operation operation && operation.Instruction == Instruction.Call)
                    {
                        AddIntervalCallerSavedReg(context.Masks.IntCallerSavedRegisters, operationPos, RegisterType.Integer);
                        AddIntervalCallerSavedReg(context.Masks.VecCallerSavedRegisters, operationPos, RegisterType.Vector);
                    }
                }
            }
        }

        private void AddIntervalCallerSavedReg(int mask, int operationPos, RegisterType regType)
        {
            while (mask != 0)
            {
                int regIndex = BitOperations.TrailingZeroCount(mask);

                Register callerSavedReg = new Register(regIndex, regType);

                LiveInterval interval = _intervals[GetRegisterId(callerSavedReg)];

                interval.AddRange(operationPos + 1, operationPos + InstructionGap);

                mask &= ~(1 << regIndex);
            }
        }

        private static int GetOperandId(Operand operand)
        {
            if (operand.Kind == OperandKind.LocalVariable)
            {
                return operand.AsInt32();
            }
            else if (operand.Kind == OperandKind.Register)
            {
                return GetRegisterId(operand.GetRegister());
            }
            else
            {
                throw new ArgumentException($"Invalid operand kind \"{operand.Kind}\".");
            }
        }

        private static int GetRegisterId(Register register)
        {
            return (register.Index << 1) | (register.Type == RegisterType.Vector ? 1 : 0);
        }

        private static IEnumerable<Node> BottomOperations(BasicBlock block)
        {
            Node node = block.Operations.Last;

            while (node != null && !(node is PhiNode))
            {
                yield return node;

                node = node.ListPrevious;
            }
        }

        private static void Sources(Node node, Action<Operand> action)
        {
            for (int index = 0; index < node.SourcesCount; index++)
            {
                Operand source = node.GetSource(index);

                if (IsLocalOrRegister(source.Kind))
                {
                    action(source);
                }
                else if (source.Kind == OperandKind.Memory)
                {
                    MemoryOperand memOp = (MemoryOperand)source;

                    if (memOp.BaseAddress != null)
                    {
                        action(memOp.BaseAddress);
                    }

                    if (memOp.Index != null)
                    {
                        action(memOp.Index);
                    }
                }
            }
        }

        private static bool IsLocalOrRegister(OperandKind kind)
        {
            return kind == OperandKind.LocalVariable ||
                   kind == OperandKind.Register;
        }
    }
}