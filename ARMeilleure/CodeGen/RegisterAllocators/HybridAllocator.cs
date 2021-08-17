using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;
using System.Diagnostics;
using System.Numerics;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;
using static ARMeilleure.IntermediateRepresentation.Operation.Factory;

namespace ARMeilleure.CodeGen.RegisterAllocators
{
    class HybridAllocator : IRegisterAllocator
    {
        private const int RegistersCount = 16;
        private const int MaxIROperands  = 4;

        private struct BlockInfo
        {
            public bool HasCall { get; }

            public int IntFixedRegisters { get; }
            public int VecFixedRegisters { get; }

            public BlockInfo(bool hasCall, int intFixedRegisters, int vecFixedRegisters)
            {
                HasCall           = hasCall;
                IntFixedRegisters = intFixedRegisters;
                VecFixedRegisters = vecFixedRegisters;
            }
        }

        private struct LocalInfo
        {
            public int Uses { get; set; }
            public int UsesAllocated { get; set; }
            public int Register { get; set; }
            public int SpillOffset { get; set; }
            public int Sequence { get; set; }
            public Operand Temp { get; set; }
            public OperandType Type { get; }

            private int _first;
            private int _last;

            public bool IsBlockLocal => _first == _last;

            public LocalInfo(OperandType type, int uses, int blkIndex)
            {
                Uses = uses;
                Type = type;

                UsesAllocated = 0;
                Register = 0;
                SpillOffset = 0;
                Sequence = 0;
                Temp = default;

                _first = -1;
                _last  = -1;

                SetBlockIndex(blkIndex);
            }

            public void SetBlockIndex(int blkIndex)
            {
                if (_first == -1 || blkIndex < _first)
                {
                    _first = blkIndex;
                }

                if (_last == -1 || blkIndex > _last)
                {
                    _last = blkIndex;
                }
            }
        }

        public AllocationResult RunPass(ControlFlowGraph cfg, StackAllocator stackAlloc, RegisterMasks regMasks)
        {
            int intUsedRegisters = 0;
            int vecUsedRegisters = 0;

            int intFreeRegisters = regMasks.IntAvailableRegisters;
            int vecFreeRegisters = regMasks.VecAvailableRegisters;

            var blockInfo = new BlockInfo[cfg.Blocks.Count];
            var localInfo = new LocalInfo[cfg.Blocks.Count * 3];
            int localInfoCount = 0;

            // The "visited" state is stored in the MSB of the local's value.
            const ulong VisitedMask = 1ul << 63;

            bool IsVisited(Operand local)
            {
                return (local.GetValueUnsafe() & VisitedMask) != 0;
            }

            void SetVisited(Operand local)
            {
                local.GetValueUnsafe() |= VisitedMask | (uint)++localInfoCount;
            }

            ref LocalInfo GetLocalInfo(Operand local)
            {
                Debug.Assert(local.Kind == OperandKind.LocalVariable);

                if (!IsVisited(local))
                {
                    throw new InvalidOperationException("Local was not visisted yet. Used before defined?");
                }

                return ref localInfo[(uint)local.GetValueUnsafe() - 1];
            }

            for (int index = cfg.PostOrderBlocks.Length - 1; index >= 0; index--)
            {
                BasicBlock block = cfg.PostOrderBlocks[index];

                int intFixedRegisters = 0;
                int vecFixedRegisters = 0;

                bool hasCall = false;

                for (Operation node = block.Operations.First; node != default; node = node.ListNext)
                {
                    if (node.Instruction == Instruction.Call)
                    {
                        hasCall = true;
                    }

                    for (int i = 0; i < node.SourcesCount; i++)
                    {
                        Operand source = node.GetSource(i);

                        if (source.Kind == OperandKind.LocalVariable)
                        {
                            GetLocalInfo(source).SetBlockIndex(block.Index);
                        }
                        else if (source.Kind == OperandKind.Memory)
                        {
                            MemoryOperand memOp = source.GetMemory();

                            if (memOp.BaseAddress != default)
                            {
                                GetLocalInfo(memOp.BaseAddress).SetBlockIndex(block.Index);
                            }

                            if (memOp.Index != default)
                            {
                                GetLocalInfo(memOp.Index).SetBlockIndex(block.Index);
                            }
                        }
                    }

                    for (int i = 0; i < node.DestinationsCount; i++)
                    {
                        Operand dest = node.GetDestination(i);

                        if (dest.Kind == OperandKind.LocalVariable)
                        {
                            if (IsVisited(dest))
                            {
                                GetLocalInfo(dest).SetBlockIndex(block.Index);
                            }
                            else
                            {
                                SetVisited(dest);

                                if (localInfoCount > localInfo.Length)
                                {
                                    Array.Resize(ref localInfo, localInfoCount * 2);
                                }

                                GetLocalInfo(dest) = new LocalInfo(dest.Type, UsesCount(dest), block.Index);
                            }
                        }
                        else if (dest.Kind == OperandKind.Register)
                        {
                            if (dest.Type.IsInteger())
                            {
                                intFixedRegisters |= 1 << dest.GetRegister().Index;
                            }
                            else
                            {
                                vecFixedRegisters |= 1 << dest.GetRegister().Index;
                            }
                        }
                    }
                }

                blockInfo[block.Index] = new BlockInfo(hasCall, intFixedRegisters, vecFixedRegisters);
            }

            int sequence = 0;

            for (int index = cfg.PostOrderBlocks.Length - 1; index >= 0; index--)
            {
                BasicBlock block = cfg.PostOrderBlocks[index];

                BlockInfo blkInfo = blockInfo[block.Index];

                int intLocalFreeRegisters = intFreeRegisters & ~blkInfo.IntFixedRegisters;
                int vecLocalFreeRegisters = vecFreeRegisters & ~blkInfo.VecFixedRegisters;

                int intCallerSavedRegisters = blkInfo.HasCall ? regMasks.IntCallerSavedRegisters : 0;
                int vecCallerSavedRegisters = blkInfo.HasCall ? regMasks.VecCallerSavedRegisters : 0;

                int intSpillTempRegisters = SelectSpillTemps(
                    intCallerSavedRegisters & ~blkInfo.IntFixedRegisters,
                    intLocalFreeRegisters);
                int vecSpillTempRegisters = SelectSpillTemps(
                    vecCallerSavedRegisters & ~blkInfo.VecFixedRegisters,
                    vecLocalFreeRegisters);

                intLocalFreeRegisters &= ~(intSpillTempRegisters | intCallerSavedRegisters);
                vecLocalFreeRegisters &= ~(vecSpillTempRegisters | vecCallerSavedRegisters);

                for (Operation node = block.Operations.First; node != default; node = node.ListNext)
                {
                    int intLocalUse = 0;
                    int vecLocalUse = 0;

                    Operand AllocateRegister(Operand local)
                    {
                        ref LocalInfo info = ref GetLocalInfo(local);

                        info.UsesAllocated++;

                        Debug.Assert(info.UsesAllocated <= info.Uses);

                        if (info.Register != -1)
                        {
                            Operand reg = Register(info.Register, local.Type.ToRegisterType(), local.Type);

                            if (info.UsesAllocated == info.Uses)
                            {
                                if (local.Type.IsInteger())
                                {
                                    intLocalFreeRegisters |= 1 << info.Register;
                                }
                                else
                                {
                                    vecLocalFreeRegisters |= 1 << info.Register;
                                }
                            }

                            return reg;
                        }
                        else
                        {
                            Operand temp = info.Temp;

                            if (temp == default || info.Sequence != sequence)
                            {
                                temp = local.Type.IsInteger()
                                    ? GetSpillTemp(local, intSpillTempRegisters, ref intLocalUse)
                                    : GetSpillTemp(local, vecSpillTempRegisters, ref vecLocalUse);

                                info.Sequence = sequence;
                                info.Temp = temp;
                            }

                            Operation fillOp = Operation(Instruction.Fill, temp, Const(info.SpillOffset));

                            block.Operations.AddBefore(node, fillOp);

                            return temp;
                        }
                    }

                    bool folded = false;

                    // If operation is a copy of a local and that local is living on the stack, we turn the copy into
                    // a fill, instead of inserting a fill before it.
                    if (node.Instruction == Instruction.Copy)
                    {
                        Operand source = node.GetSource(0);

                        if (source.Kind == OperandKind.LocalVariable)
                        {
                            ref LocalInfo info = ref GetLocalInfo(source);

                            if (info.Register == -1)
                            {
                                Operation fillOp = Operation(Instruction.Fill, node.Destination, Const(info.SpillOffset));

                                block.Operations.AddBefore(node, fillOp);
                                block.Operations.Remove(node);

                                node = fillOp;

                                folded = true;
                            }
                        }
                    }

                    if (!folded)
                    {
                        for (int i = 0; i < node.SourcesCount; i++)
                        {
                            Operand source = node.GetSource(i);

                            if (source.Kind == OperandKind.LocalVariable)
                            {
                                node.SetSource(i, AllocateRegister(source));
                            }
                            else if (source.Kind == OperandKind.Memory)
                            {
                                MemoryOperand memOp = source.GetMemory();

                                if (memOp.BaseAddress != default)
                                {
                                    memOp.BaseAddress = AllocateRegister(memOp.BaseAddress);
                                }

                                if (memOp.Index != default)
                                {
                                    memOp.Index = AllocateRegister(memOp.Index);
                                }
                            }
                        }
                    }

                    int intLocalAsg = 0;
                    int vecLocalAsg = 0;

                    for (int i = 0; i < node.DestinationsCount; i++)
                    {
                        Operand dest = node.GetDestination(i);

                        if (dest.Kind != OperandKind.LocalVariable)
                        {
                            continue;
                        }

                        ref LocalInfo info = ref GetLocalInfo(dest);

                        if (info.UsesAllocated == 0)
                        {
                            int mask = dest.Type.IsInteger()
                                ? intLocalFreeRegisters
                                : vecLocalFreeRegisters;

                            if (info.IsBlockLocal && mask != 0)
                            {
                                int selectedReg = BitOperations.TrailingZeroCount(mask);

                                info.Register = selectedReg;

                                if (dest.Type.IsInteger())
                                {
                                    intLocalFreeRegisters &= ~(1 << selectedReg);
                                    intUsedRegisters      |=   1 << selectedReg;
                                }
                                else
                                {
                                    vecLocalFreeRegisters &= ~(1 << selectedReg);
                                    vecUsedRegisters      |=   1 << selectedReg;
                                }
                            }
                            else
                            {
                                info.Register    = -1;
                                info.SpillOffset = stackAlloc.Allocate(dest.Type.GetSizeInBytes());
                            }
                        }

                        info.UsesAllocated++;

                        Debug.Assert(info.UsesAllocated <= info.Uses);

                        if (info.Register != -1)
                        {
                            node.SetDestination(i, Register(info.Register, dest.Type.ToRegisterType(), dest.Type));
                        }
                        else
                        {
                            Operand temp = info.Temp;

                            if (temp == default || info.Sequence != sequence)
                            {
                                temp = dest.Type.IsInteger()
                                    ? GetSpillTemp(dest, intSpillTempRegisters, ref intLocalAsg)
                                    : GetSpillTemp(dest, vecSpillTempRegisters, ref vecLocalAsg);

                                info.Sequence = sequence;
                                info.Temp     = temp;
                            }

                            node.SetDestination(i, temp);

                            Operation spillOp = Operation(Instruction.Spill, default, Const(info.SpillOffset), temp);

                            block.Operations.AddAfter(node, spillOp);

                            node = spillOp;
                        }
                    }

                    sequence++;

                    intUsedRegisters |= intLocalAsg | intLocalUse;
                    vecUsedRegisters |= vecLocalAsg | vecLocalUse;
                }
            }

            return new AllocationResult(intUsedRegisters, vecUsedRegisters, stackAlloc.TotalSize);
        }

        private static int SelectSpillTemps(int mask0, int mask1)
        {
            int selection = 0;
            int count     = 0;

            while (count < MaxIROperands && mask0 != 0)
            {
                int mask = mask0 & -mask0;

                selection |= mask;

                mask0 &= ~mask;

                count++;
            }

            while (count < MaxIROperands && mask1 != 0)
            {
                int mask = mask1 & -mask1;

                selection |= mask;

                mask1 &= ~mask;

                count++;
            }

            Debug.Assert(count == MaxIROperands, "No enough registers for spill temps.");

            return selection;
        }

        private static Operand GetSpillTemp(Operand local, int freeMask, ref int useMask)
        {
            int selectedReg = BitOperations.TrailingZeroCount(freeMask & ~useMask);

            useMask |= 1 << selectedReg;

            return Register(selectedReg, local.Type.ToRegisterType(), local.Type);
        }

        private static int UsesCount(Operand local)
        {
            return local.AssignmentsCount + local.UsesCount;
        }
    }
}