using System.Collections.Generic;

namespace ChocolArm64.Translation
{
    class LocalAlloc
    {
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

            public PathIo(ILBlock root, long inputs, long outputs) : this()
            {
                Set(root, inputs, outputs);
            }

            public void Set(ILBlock root, long inputs, long outputs)
            {
                if (!_allInputs.TryAdd(root, inputs))
                {
                    _allInputs[root] |= inputs;
                }

                if (!_cmnOutputs.TryAdd(root, outputs))
                {
                    _cmnOutputs[root] &= outputs;
                }

                _allOutputs |= outputs;
            }

            public long GetInputs(ILBlock root)
            {
                if (_allInputs.TryGetValue(root, out long inputs))
                {
                    return inputs | (_allOutputs & ~_cmnOutputs[root]);
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

        private struct BlockIo
        {
            public ILBlock Block;
            public ILBlock Entry;

            public long IntInputs;
            public long VecInputs;
            public long IntOutputs;
            public long VecOutputs;
        }

        private const int MaxOptGraphLength = 40;

        public LocalAlloc(ILBlock[] graph, ILBlock root)
        {
            _intPaths = new Dictionary<ILBlock, PathIo>();
            _vecPaths = new Dictionary<ILBlock, PathIo>();

            if (graph.Length > 1 &&
                graph.Length < MaxOptGraphLength)
            {
                InitializeOptimal(graph, root);
            }
            else
            {
                InitializeFast(graph);
            }
        }

        private void InitializeOptimal(ILBlock[] graph, ILBlock root)
        {
            //This will go through all possible paths on the graph,
            //and store all inputs/outputs for each block. A register
            //that was previously written to already is not considered an input.
            //When a block can be reached by more than one path, then the
            //output from all paths needs to be set for this block, and
            //only outputs present in all of the parent blocks can be considered
            //when doing input elimination. Each block chain have a root, that's where
            //the code starts executing. They are present on the subroutine start point,
            //and on call return points too (address written to X30 by BL).
            HashSet<BlockIo> visited = new HashSet<BlockIo>();

            Queue<BlockIo> unvisited = new Queue<BlockIo>();

            void Enqueue(BlockIo block)
            {
                if (!visited.Contains(block))
                {
                    unvisited.Enqueue(block);

                    visited.Add(block);
                }
            }

            Enqueue(new BlockIo()
            {
                Block = root,
                Entry = root
            });

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
                    BlockIo blkIO = new BlockIo() { Block = block };

                    if (retTarget)
                    {
                        blkIO.Entry = block;
                    }
                    else
                    {
                        blkIO.Entry      = current.Entry;
                        blkIO.IntInputs  = current.IntInputs;
                        blkIO.VecInputs  = current.VecInputs;
                        blkIO.IntOutputs = current.IntOutputs;
                        blkIO.VecOutputs = current.VecOutputs;
                    }

                    Enqueue(blkIO);
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

        private void InitializeFast(ILBlock[] graph)
        {
            //This is WAY faster than InitializeOptimal, but results in
            //uneeded loads and stores, so the resulting code will be slower.
            long intInputs = 0, intOutputs = 0;
            long vecInputs = 0, vecOutputs = 0;

            foreach (ILBlock block in graph)
            {
                intInputs  |= block.IntInputs;
                intOutputs |= block.IntOutputs;
                vecInputs  |= block.VecInputs;
                vecOutputs |= block.VecOutputs;
            }

            //It's possible that not all code paths writes to those output registers,
            //in those cases if we attempt to write an output registers that was
            //not written, we will be just writing zero and messing up the old register value.
            //So we just need to ensure that all outputs are loaded.
            if (graph.Length > 1)
            {
                intInputs |= intOutputs;
                vecInputs |= vecOutputs;
            }

            foreach (ILBlock block in graph)
            {
                _intPaths.Add(block, new PathIo(block, intInputs, intOutputs));
                _vecPaths.Add(block, new PathIo(block, vecInputs, vecOutputs));
            }
        }

        public long GetIntInputs(ILBlock root) => GetInputsImpl(root, _intPaths.Values);
        public long GetVecInputs(ILBlock root) => GetInputsImpl(root, _vecPaths.Values);

        private long GetInputsImpl(ILBlock root, IEnumerable<PathIo> values)
        {
            long inputs = 0;

            foreach (PathIo path in values)
            {
                inputs |= path.GetInputs(root);
            }

            return inputs;
        }

        public long GetIntOutputs(ILBlock block) => _intPaths[block].GetOutputs();
        public long GetVecOutputs(ILBlock block) => _vecPaths[block].GetOutputs();
    }
}