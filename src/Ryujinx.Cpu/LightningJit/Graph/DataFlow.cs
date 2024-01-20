namespace Ryujinx.Cpu.LightningJit.Graph
{
    static class DataFlow
    {
        public static (RegisterMask[], RegisterMask[]) GetGlobalUses(IBlockList blocks)
        {
            // Compute local register inputs and outputs used inside blocks.
            RegisterMask[] localInputs = new RegisterMask[blocks.Count];
            RegisterMask[] localOutputs = new RegisterMask[blocks.Count];

            for (int index = 0; index < blocks.Count; index++)
            {
                IBlock block = blocks[index];

                RegisterUse use = block.ComputeUseMasks();

                localInputs[block.Index] = use.Read;
                localOutputs[block.Index] = use.Write;
            }

            // Compute global register inputs and outputs used across blocks.
            RegisterMask[] globalInputs = new RegisterMask[blocks.Count];
            RegisterMask[] globalOutputs = new RegisterMask[blocks.Count];

            bool modified;

            // Compute register outputs.
            do
            {
                modified = false;

                for (int index = 0; index < blocks.Count; index++)
                {
                    IBlock block = blocks[index];

                    int firstPIndex = GetFirstPredecessorIndex(block);
                    if (firstPIndex >= 0)
                    {
                        IBlock predecessor = block.GetPredecessor(firstPIndex);

                        RegisterMask outputs = globalOutputs[predecessor.Index];

                        for (int pIndex = firstPIndex + 1; pIndex < block.PredecessorsCount; pIndex++)
                        {
                            predecessor = block.GetPredecessor(pIndex);

                            if (predecessor.EndsWithContextStore())
                            {
                                // If a block ended with a context store, then we don't need to care
                                // about any of it's outputs, as they have already been saved to the context.
                                // Common outputs must be reset as doing a context stores indicates we will
                                // do a function call and wipe all register values.

                                continue;
                            }

                            outputs |= globalOutputs[predecessor.Index];
                        }

                        outputs |= localOutputs[block.Index];
                        modified |= Exchange(globalOutputs, block.Index, globalOutputs[block.Index] | outputs);
                    }
                    else
                    {
                        modified |= Exchange(globalOutputs, block.Index, localOutputs[block.Index]);
                    }
                }
            }
            while (modified);

            // Compute register inputs.
            do
            {
                modified = false;

                for (int index = blocks.Count - 1; index >= 0; index--)
                {
                    IBlock block = blocks[index];

                    RegisterMask cmnOutputs = RegisterMask.Zero;
                    RegisterMask allOutputs = RegisterMask.Zero;

                    int firstPIndex = GetFirstPredecessorIndex(block);
                    if (firstPIndex == 0)
                    {
                        IBlock predecessor = block.GetPredecessor(0);

                        // Assumes that block index 0 is the entry block.
                        cmnOutputs = block.Index != 0 ? globalOutputs[predecessor.Index] : RegisterMask.Zero;
                        allOutputs = globalOutputs[predecessor.Index];

                        for (int pIndex = 1; pIndex < block.PredecessorsCount; pIndex++)
                        {
                            predecessor = block.GetPredecessor(pIndex);

                            if (!predecessor.EndsWithContextStore())
                            {
                                RegisterMask outputs = globalOutputs[predecessor.Index];

                                cmnOutputs &= outputs;
                                allOutputs |= outputs;
                            }
                            else
                            {
                                cmnOutputs = RegisterMask.Zero;
                            }
                        }
                    }
                    else if (firstPIndex > 0)
                    {
                        IBlock predecessor = block.GetPredecessor(firstPIndex);

                        allOutputs = globalOutputs[predecessor.Index];

                        for (int pIndex = firstPIndex + 1; pIndex < block.PredecessorsCount; pIndex++)
                        {
                            predecessor = block.GetPredecessor(pIndex);

                            if (!predecessor.EndsWithContextStore())
                            {
                                allOutputs |= globalOutputs[predecessor.Index];
                            }
                        }
                    }

                    RegisterMask inputs = localInputs[block.Index];

                    // If this block will load from context at the end,
                    // we don't need to care about what comes next.
                    if (!block.EndsWithContextLoad())
                    {
                        for (int sIndex = 0; sIndex < block.SuccessorsCount; sIndex++)
                        {
                            inputs |= globalInputs[block.GetSuccessor(sIndex).Index] & ~localOutputs[block.Index];
                        }
                    }

                    inputs |= allOutputs & ~localOutputs[block.Index];
                    inputs &= ~cmnOutputs;

                    modified |= Exchange(globalInputs, block.Index, globalInputs[block.Index] | inputs);
                }
            }
            while (modified);

            return (globalInputs, globalOutputs);
        }

        private static bool Exchange(RegisterMask[] masks, int blkIndex, RegisterMask value)
        {
            ref RegisterMask curValue = ref masks[blkIndex];
            bool changed = curValue != value;
            curValue = value;

            return changed;
        }

        private static int GetFirstPredecessorIndex(IBlock block)
        {
            for (int pIndex = 0; pIndex < block.PredecessorsCount; pIndex++)
            {
                if (!block.GetPredecessor(pIndex).EndsWithContextStore())
                {
                    return pIndex;
                }
            }

            return -1;
        }
    }
}
