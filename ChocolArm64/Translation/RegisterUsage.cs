using System;
using System.Collections.Generic;

namespace ChocolArm64.Translation
{
    class RegisterUsage
    {
        public const long CallerSavedIntRegistersMask = 0x7fL  << 9;
        public const long PStateNzcvFlagsMask         = 0xfL   << 60;

        public const long CallerSavedVecRegistersMask = 0xffffL << 16;

        private class PathIo
        {
            private Dictionary<ILBlock, long> _allInputs;
            private Dictionary<ILBlock, long> _cmnOutputs;

            private long _allOutputs;

            public PathIo()
            {
                _allInputs  = new Dictionary<ILBlock, long>();
                _cmnOutputs = new Dictionary<ILBlock, long>();
            }

            public void Set(ILBlock entry, long inputs, long outputs)
            {
                if (!_allInputs.TryAdd(entry, inputs))
                {
                    _allInputs[entry] |= inputs;
                }

                if (!_cmnOutputs.TryAdd(entry, outputs))
                {
                    _cmnOutputs[entry] &= outputs;
                }

                _allOutputs |= outputs;
            }

            public long GetInputs(ILBlock entry)
            {
                if (_allInputs.TryGetValue(entry, out long inputs))
                {
                    //We also need to read the registers that may not be written
                    //by all paths that can reach a exit point, to ensure that
                    //the local variable will not remain uninitialized depending
                    //on the flow path taken.
                    return inputs | (_allOutputs & ~_cmnOutputs[entry]);
                }

                return 0;
            }

            public long GetOutputs()
            {
                return _allOutputs;
            }
        }

        private Dictionary<ILBlock, PathIo> _intPaths;
        private Dictionary<ILBlock, PathIo> _vecPaths;

        private struct BlockIo : IEquatable<BlockIo>
        {
            public ILBlock Block { get; }
            public ILBlock Entry { get; }

            public long IntInputs  { get; set; }
            public long VecInputs  { get; set; }
            public long IntOutputs { get; set; }
            public long VecOutputs { get; set; }

            public BlockIo(ILBlock block, ILBlock entry)
            {
                Block = block;
                Entry = entry;

                IntInputs = IntOutputs = 0;
                VecInputs = VecOutputs = 0;
            }

            public BlockIo(
                ILBlock block,
                ILBlock entry,
                long    intInputs,
                long    vecInputs,
                long    intOutputs,
                long    vecOutputs) : this(block, entry)
            {
                IntInputs  = intInputs;
                VecInputs  = vecInputs;
                IntOutputs = intOutputs;
                VecOutputs = vecOutputs;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is BlockIo other))
                {
                    return false;
                }

                return Equals(other);
            }

            public bool Equals(BlockIo other)
            {
                return other.Block      == Block      &&
                       other.Entry      == Entry      &&
                       other.IntInputs  == IntInputs  &&
                       other.VecInputs  == VecInputs  &&
                       other.IntOutputs == IntOutputs &&
                       other.VecOutputs == VecOutputs;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Block, Entry, IntInputs, VecInputs, IntOutputs, VecOutputs);
            }

            public static bool operator ==(BlockIo lhs, BlockIo rhs)
            {
                return lhs.Equals(rhs);
            }

            public static bool operator !=(BlockIo lhs, BlockIo rhs)
            {
                return !(lhs == rhs);
            }
        }

        public RegisterUsage()
        {
            _intPaths = new Dictionary<ILBlock, PathIo>();
            _vecPaths = new Dictionary<ILBlock, PathIo>();
        }

        public void BuildUses(ILBlock entry)
        {
            //This will go through all possible paths on the graph,
            //and store all inputs/outputs for each block. A register
            //that was previously written to already is not considered an input.
            //When a block can be reached by more than one path, then the
            //output from all paths needs to be set for this block, and
            //only outputs present in all of the parent blocks can be considered
            //when doing input elimination. Each block chain has a entry, that's where
            //the code starts executing. They are present on the subroutine start point,
            //and on call return points too (address written to X30 by BL).
            HashSet<BlockIo> visited = new HashSet<BlockIo>();

            Queue<BlockIo> unvisited = new Queue<BlockIo>();

            void Enqueue(BlockIo block)
            {
                if (visited.Add(block))
                {
                    unvisited.Enqueue(block);
                }
            }

            Enqueue(new BlockIo(entry, entry));

            while (unvisited.Count > 0)
            {
                BlockIo current = unvisited.Dequeue();

                current.IntInputs  |= current.Block.IntInputs & ~current.IntOutputs;
                current.VecInputs  |= current.Block.VecInputs & ~current.VecOutputs;
                current.IntOutputs |= current.Block.IntOutputs;
                current.VecOutputs |= current.Block.VecOutputs;

                //Check if this is a exit block
                //(a block that returns or calls another sub).
                if ((current.Block.Next   == null &&
                     current.Block.Branch == null) || current.Block.HasStateStore)
                {
                    if (!_intPaths.TryGetValue(current.Block, out PathIo intPath))
                    {
                        _intPaths.Add(current.Block, intPath = new PathIo());
                    }

                    if (!_vecPaths.TryGetValue(current.Block, out PathIo vecPath))
                    {
                        _vecPaths.Add(current.Block, vecPath = new PathIo());
                    }

                    intPath.Set(current.Entry, current.IntInputs, current.IntOutputs);
                    vecPath.Set(current.Entry, current.VecInputs, current.VecOutputs);
                }

                void EnqueueFromCurrent(ILBlock block, bool retTarget)
                {
                    BlockIo blockIo;

                    if (retTarget)
                    {
                        blockIo = new BlockIo(block, block);
                    }
                    else
                    {
                        blockIo = new BlockIo(
                            block,
                            current.Entry,
                            current.IntInputs,
                            current.VecInputs,
                            current.IntOutputs,
                            current.VecOutputs);
                    }

                    Enqueue(blockIo);
                }

                if (current.Block.Next != null)
                {
                    EnqueueFromCurrent(current.Block.Next, current.Block.HasStateStore);
                }

                if (current.Block.Branch != null)
                {
                    EnqueueFromCurrent(current.Block.Branch, false);
                }
            }
        }

        public long GetIntInputs(ILBlock entry) => GetInputsImpl(entry, _intPaths.Values);
        public long GetVecInputs(ILBlock entry) => GetInputsImpl(entry, _vecPaths.Values);

        private long GetInputsImpl(ILBlock entry, IEnumerable<PathIo> values)
        {
            long inputs = 0;

            foreach (PathIo path in values)
            {
                inputs |= path.GetInputs(entry);
            }

            return inputs;
        }

        public long GetIntNotInputs(ILBlock entry) => GetNotInputsImpl(entry, _intPaths.Values);
        public long GetVecNotInputs(ILBlock entry) => GetNotInputsImpl(entry, _vecPaths.Values);

        private long GetNotInputsImpl(ILBlock entry, IEnumerable<PathIo> values)
        {
            //Returns a mask with registers that are written to
            //before being read. Only those registers that are
            //written in all paths, and is not read before being
            //written to on those paths, should be set on the mask.
            long mask = -1L;

            foreach (PathIo path in values)
            {
                mask &= path.GetOutputs() & ~path.GetInputs(entry);
            }

            return mask;
        }

        public long GetIntOutputs(ILBlock block) => _intPaths[block].GetOutputs();
        public long GetVecOutputs(ILBlock block) => _vecPaths[block].GetOutputs();

        public static long ClearCallerSavedIntRegs(long mask, bool isAarch64)
        {
            //TODO: ARM32 support.
            if (isAarch64)
            {
                mask &= ~(CallerSavedIntRegistersMask | PStateNzcvFlagsMask);
            }

            return mask;
        }

        public static long ClearCallerSavedVecRegs(long mask, bool isAarch64)
        {
            //TODO: ARM32 support.
            if (isAarch64)
            {
                mask &= ~CallerSavedVecRegistersMask;
            }

            return mask;
        }
    }
}