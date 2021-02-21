using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

using static ARMeilleure.IntermediateRepresentation.OperandHelper;
using static ARMeilleure.IntermediateRepresentation.OperationHelper;

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

        private class LocalInfo
        {
            public int Uses     { get; set; }
            public int UseCount { get; set; }

            public bool PreAllocated { get; set; }
            public int  Register     { get; set; }
            public int  SpillOffset  { get; set; }

            public int Sequence { get; set; }

            public Operand Temp { get; set; }

            public OperandType Type { get; }

            private int _first;
            private int _last;

            public bool IsBlockLocal => _first == _last;

            public LocalInfo(OperandType type, int uses)
            {
                Uses = uses;
                Type = type;

                _first = -1;
                _last  = -1;
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

        public AllocationResult RunPass(
            ControlFlowGraph cfg,
            StackAllocator stackAlloc,
            RegisterMasks regMasks)
        {
            int intUsedRegisters = 0;
            int vecUsedRegisters = 0;

            int intFreeRegisters = regMasks.IntAvailableRegisters;
            int vecFreeRegisters = regMasks.VecAvailableRegisters;

            BlockInfo[] blockInfo = new BlockInfo[cfg.Blocks.Count];

            List<LocalInfo> locInfo = new List<LocalInfo>();

            for (int index = cfg.PostOrderBlocks.Length - 1; index >= 0; index--)
            {
                BasicBlock block = cfg.PostOrderBlocks[index];

                int intFixedRegisters = 0;
                int vecFixedRegisters = 0;

                bool hasCall = false;

                for (Node node = block.Operations.First; node != null; node = node.ListNext)
                {
                    if (node is Operation operation && operation.Instruction == Instruction.Call)
                    {
                        hasCall = true;
                    }

                    for (int srcIndex = 0; srcIndex < node.SourcesCount; srcIndex++)
                    {
                        Operand source = node.GetSource(srcIndex);

                        if (source.Kind == OperandKind.LocalVariable)
                        {
                            locInfo[source.AsInt32() - 1].SetBlockIndex(block.Index);
                        }
                        else if (source.Kind == OperandKind.Memory)
                        {
                            MemoryOperand memOp = (MemoryOperand)source;

                            if (memOp.BaseAddress != null)
                            {
                                locInfo[memOp.BaseAddress.AsInt32() - 1].SetBlockIndex(block.Index);
                            }

                            if (memOp.Index != null)
                            {
                                locInfo[memOp.Index.AsInt32() - 1].SetBlockIndex(block.Index);
                            }
                        }
                    }

                    for (int dstIndex = 0; dstIndex < node.DestinationsCount; dstIndex++)
                    {
                        Operand dest = node.GetDestination(dstIndex);

                        if (dest.Kind == OperandKind.LocalVariable)
                        {
                            LocalInfo info;

                            if (dest.Value != 0)
                            {
                                info = locInfo[dest.AsInt32() - 1];
                            }
                            else
                            {
                                dest.NumberLocal(locInfo.Count + 1);

                                info = new LocalInfo(dest.Type, UsesCount(dest));

                                locInfo.Add(info);
                            }

                            info.SetBlockIndex(block.Index);
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

                for (Node node = block.Operations.First; node != null; node = node.ListNext)
                {
                    int intLocalUse = 0;
                    int vecLocalUse = 0;

                    void AllocateRegister(Operand source, MemoryOperand memOp, int srcIndex)
                    {
                        LocalInfo info = locInfo[source.AsInt32() - 1];

                        info.UseCount++;

                        Debug.Assert(info.UseCount <= info.Uses);

                        if (info.Register != -1)
                        {
                            Operand reg = Register(info.Register, source.Type.ToRegisterType(), source.Type);

                            if (memOp != null)
                            {
                                if (srcIndex == 0)
                                {
                                    memOp.BaseAddress = reg;
                                }
                                else /* if (srcIndex == 1) */
                                {
                                    memOp.Index = reg;
                                }
                            }
                            else
                            {
                                node.SetSource(srcIndex, reg);
                            }

                            if (info.UseCount == info.Uses && !info.PreAllocated)
                            {
                                if (source.Type.IsInteger())
                                {
                                    intLocalFreeRegisters |= 1 << info.Register;
                                }
                                else
                                {
                                    vecLocalFreeRegisters |= 1 << info.Register;
                                }
                            }
                        }
                        else if (node is Operation operation && operation.Instruction == Instruction.Copy)
                        {
                            Operation fillOp = Operation(Instruction.Fill, node.Destination, Const(info.SpillOffset));

                            block.Operations.AddBefore(node, fillOp);
                            block.Operations.Remove(node);

                            node = fillOp;
                        }
                        else
                        {
                            Operand temp = info.Temp;

                            if (temp == null || info.Sequence != sequence)
                            {
                                temp = source.Type.IsInteger()
                                    ? GetSpillTemp(source, intSpillTempRegisters, ref intLocalUse)
                                    : GetSpillTemp(source, vecSpillTempRegisters, ref vecLocalUse);

                                info.Sequence = sequence;
                                info.Temp = temp;
                            }

                            if (memOp != null)
                            {
                                if (srcIndex == 0)
                                {
                                    memOp.BaseAddress = temp;
                                }
                                else /* if (srcIndex == 1) */
                                {
                                    memOp.Index = temp;
                                }
                            }
                            else
                            {
                                node.SetSource(srcIndex, temp);
                            }

                            Operation fillOp = Operation(Instruction.Fill, temp, Const(info.SpillOffset));

                            block.Operations.AddBefore(node, fillOp);
                        }
                    }

                    for (int srcIndex = 0; srcIndex < node.SourcesCount; srcIndex++)
                    {
                        Operand source = node.GetSource(srcIndex);

                        if (source.Kind == OperandKind.LocalVariable)
                        {
                            AllocateRegister(source, null, srcIndex);
                        }
                        else if (source.Kind == OperandKind.Memory)
                        {
                            MemoryOperand memOp = (MemoryOperand)source;

                            if (memOp.BaseAddress != null)
                            {
                                AllocateRegister(memOp.BaseAddress, memOp, 0);
                            }

                            if (memOp.Index != null)
                            {
                                AllocateRegister(memOp.Index, memOp, 1);
                            }
                        }
                    }

                    int intLocalAsg = 0;
                    int vecLocalAsg = 0;

                    for (int dstIndex = 0; dstIndex < node.DestinationsCount; dstIndex++)
                    {
                        Operand dest = node.GetDestination(dstIndex);

                        if (dest.Kind != OperandKind.LocalVariable)
                        {
                            continue;
                        }

                        LocalInfo info = locInfo[dest.AsInt32() - 1];

                        if (info.UseCount == 0 && !info.PreAllocated)
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

                        info.UseCount++;

                        Debug.Assert(info.UseCount <= info.Uses);

                        if (info.Register != -1)
                        {
                            node.SetDestination(dstIndex, Register(info.Register, dest.Type.ToRegisterType(), dest.Type));
                        }
                        else
                        {
                            Operand temp = info.Temp;

                            if (temp == null || info.Sequence != sequence)
                            {
                                temp = dest.Type.IsInteger()
                                    ? GetSpillTemp(dest, intSpillTempRegisters, ref intLocalAsg)
                                    : GetSpillTemp(dest, vecSpillTempRegisters, ref vecLocalAsg);

                                info.Sequence = sequence;
                                info.Temp     = temp;
                            }

                            node.SetDestination(dstIndex, temp);

                            Operation spillOp = Operation(Instruction.Spill, null, Const(info.SpillOffset), temp);

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
            return local.Assignments.Count + local.Uses.Count;
        }
    }
}