using ChocolArm64.IntermediateRepresentation;
using ChocolArm64.State;
using System.Collections.Generic;

namespace ChocolArm64.Translation
{
    class RegisterUsage
    {
        private const long CallerSavedIntRegistersMask = 0x7fL  << 9;
        private const long PStateNzcvFlagsMask         = 0xfL   << 60;

        private const long CallerSavedVecRegistersMask = 0xffffL << 16;

        private RegisterMask[] _inputs;
        private RegisterMask[] _outputs;

        public RegisterUsage(BasicBlock entryBlock, int blocksCount)
        {
            _inputs  = new RegisterMask[blocksCount];
            _outputs = new RegisterMask[blocksCount];

            HashSet<BasicBlock> visited = new HashSet<BasicBlock>();

            Stack<BasicBlock> blockStack = new Stack<BasicBlock>();

            List<BasicBlock> postOrderBlocks = new List<BasicBlock>(blocksCount);

            visited.Add(entryBlock);

            blockStack.Push(entryBlock);

            while (blockStack.TryPop(out BasicBlock block))
            {
                if (block.Next != null && visited.Add(block.Next))
                {
                    blockStack.Push(block);
                    blockStack.Push(block.Next);
                }
                else if (block.Branch != null && visited.Add(block.Branch))
                {
                    blockStack.Push(block);
                    blockStack.Push(block.Branch);
                }
                else
                {
                    postOrderBlocks.Add(block);
                }
            }

            RegisterMask[] cmnOutputMasks = new RegisterMask[blocksCount];

            bool modified;

            bool firstPass = true;

            do
            {
                modified = false;

                for (int blkIndex = postOrderBlocks.Count - 1; blkIndex >= 0; blkIndex--)
                {
                    BasicBlock block = postOrderBlocks[blkIndex];

                    if (block.Predecessors.Count != 0 && !block.HasStateLoad)
                    {
                        BasicBlock predecessor = block.Predecessors[0];

                        RegisterMask cmnOutputs = predecessor.RegOutputs | cmnOutputMasks[predecessor.Index];

                        RegisterMask outputs = _outputs[predecessor.Index];

                        for (int pIndex = 1; pIndex < block.Predecessors.Count; pIndex++)
                        {
                            predecessor = block.Predecessors[pIndex];

                            cmnOutputs &= predecessor.RegOutputs | cmnOutputMasks[predecessor.Index];

                            outputs |= _outputs[predecessor.Index];
                        }

                        _inputs[block.Index] |= outputs & ~cmnOutputs;

                        if (!firstPass)
                        {
                            cmnOutputs &= cmnOutputMasks[block.Index];
                        }

                        if (Exchange(cmnOutputMasks, block.Index, cmnOutputs))
                        {
                            modified = true;
                        }

                        outputs |= block.RegOutputs;

                        if (Exchange(_outputs, block.Index, _outputs[block.Index] | outputs))
                        {
                            modified = true;
                        }
                    }
                    else if (Exchange(_outputs, block.Index, block.RegOutputs))
                    {
                        modified = true;
                    }
                }

                firstPass = false;
            }
            while (modified);

            do
            {
                modified = false;

                for (int blkIndex = 0; blkIndex < postOrderBlocks.Count; blkIndex++)
                {
                    BasicBlock block = postOrderBlocks[blkIndex];

                    RegisterMask inputs = block.RegInputs;

                    if (block.Next != null)
                    {
                        inputs |= _inputs[block.Next.Index];
                    }

                    if (block.Branch != null)
                    {
                        inputs |= _inputs[block.Branch.Index];
                    }

                    inputs &= ~cmnOutputMasks[block.Index];

                    if (Exchange(_inputs, block.Index, _inputs[block.Index] | inputs))
                    {
                        modified = true;
                    }
                }
            }
            while (modified);
        }

        private static bool Exchange(RegisterMask[] masks, int blkIndex, RegisterMask value)
        {
            RegisterMask oldValue = masks[blkIndex];

            masks[blkIndex] = value;

            return oldValue != value;
        }

        public RegisterMask GetInputs(BasicBlock entryBlock) => _inputs[entryBlock.Index];

        public RegisterMask GetOutputs(BasicBlock block) => _outputs[block.Index];

        public static long ClearCallerSavedIntRegs(long mask, ExecutionMode mode)
        {
            // TODO: ARM32 support.
            if (mode == ExecutionMode.Aarch64)
            {
                mask &= ~(CallerSavedIntRegistersMask | PStateNzcvFlagsMask);
            }

            return mask;
        }

        public static long ClearCallerSavedVecRegs(long mask, ExecutionMode mode)
        {
            // TODO: ARM32 support.
            if (mode == ExecutionMode.Aarch64)
            {
                mask &= ~CallerSavedVecRegistersMask;
            }

            return mask;
        }
    }
}