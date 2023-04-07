using ARMeilleure.IntermediateRepresentation;
using System;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.CodeGen
{
    static class PreAllocatorCommon
    {
        public static void Propagate(ref Span<Operation> buffer, Operand dest, Operand value)
        {
            ReadOnlySpan<Operation> uses = dest.GetUses(ref buffer);

            foreach (Operation use in uses)
            {
                for (int srcIndex = 0; srcIndex < use.SourcesCount; srcIndex++)
                {
                    Operand useSrc = use.GetSource(srcIndex);

                    if (useSrc == dest)
                    {
                        use.SetSource(srcIndex, value);
                    }
                    else if (useSrc.Kind == OperandKind.Memory)
                    {
                        MemoryOperand memoryOp = useSrc.GetMemory();

                        Operand baseAddr = memoryOp.BaseAddress;
                        Operand index = memoryOp.Index;
                        bool changed = false;

                        if (baseAddr == dest)
                        {
                            baseAddr = value;
                            changed = true;
                        }

                        if (index == dest)
                        {
                            index = value;
                            changed = true;
                        }

                        if (changed)
                        {
                            use.SetSource(srcIndex, MemoryOp(
                                useSrc.Type,
                                baseAddr,
                                index,
                                memoryOp.Scale,
                                memoryOp.Displacement));
                        }
                    }
                }
            }
        }
    }
}
