using System.Collections.Generic;

namespace ChocolArm64.Translation
{
    class ALocalAlloc
    {
        private class PathIo
        {
            private Dictionary<AILBlock, long> AllInputs;
            private Dictionary<AILBlock, long> CmnOutputs;

            private long AllOutputs;

            public PathIo()
            {
                AllInputs  = new Dictionary<AILBlock, long>();
                CmnOutputs = new Dictionary<AILBlock, long>();
            }

            public PathIo(AILBlock Root, long Inputs, long Outputs) : this()
            {
                Set(Root, Inputs, Outputs);
            }

            public void Set(AILBlock Root, long Inputs, long Outputs)
            {
                if (!AllInputs.TryAdd(Root, Inputs))
                {
                    AllInputs[Root] |= Inputs;
                }

                if (!CmnOutputs.TryAdd(Root, Outputs))
                {
                    CmnOutputs[Root] &= Outputs;
                }

                AllOutputs |= Outputs;
            }

            public long GetInputs(AILBlock Root)
            {
                if (AllInputs.TryGetValue(Root, out long Inputs))
                {
                    return Inputs | (AllOutputs & ~CmnOutputs[Root]);
                }

                return 0;
            }

            public long GetOutputs()
            {
                return AllOutputs;
            }
        }

        private Dictionary<AILBlock, PathIo> IntPaths;
        private Dictionary<AILBlock, PathIo> VecPaths;

        private struct BlockIo
        {
            public AILBlock Block;
            public AILBlock Entry;

            public long IntInputs;
            public long VecInputs;
            public long IntOutputs;
            public long VecOutputs;
        }

        private const int MaxOptGraphLength = 40;

        public ALocalAlloc(AILBlock[] Graph, AILBlock Root)
        {
            IntPaths = new Dictionary<AILBlock, PathIo>();
            VecPaths = new Dictionary<AILBlock, PathIo>();

            if (Graph.Length > 1 &&
                Graph.Length < MaxOptGraphLength)
            {
                InitializeOptimal(Graph, Root);
            }
            else
            {
                InitializeFast(Graph);
            }
        }

        private void InitializeOptimal(AILBlock[] Graph, AILBlock Root)
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
            HashSet<BlockIo> Visited = new HashSet<BlockIo>();

            Queue<BlockIo> Unvisited = new Queue<BlockIo>();

            void Enqueue(BlockIo Block)
            {
                if (!Visited.Contains(Block))
                {
                    Unvisited.Enqueue(Block);

                    Visited.Add(Block);
                }
            }

            Enqueue(new BlockIo()
            {
                Block = Root,
                Entry = Root
            });

            while (Unvisited.Count > 0)
            {
                BlockIo Current = Unvisited.Dequeue();

                Current.IntInputs  |= Current.Block.IntInputs & ~Current.IntOutputs;
                Current.VecInputs  |= Current.Block.VecInputs & ~Current.VecOutputs;
                Current.IntOutputs |= Current.Block.IntOutputs;
                Current.VecOutputs |= Current.Block.VecOutputs;

                //Check if this is a exit block
                //(a block that returns or calls another sub).
                if ((Current.Block.Next   == null &&
                     Current.Block.Branch == null) || Current.Block.HasStateStore)
                {
                    if (!IntPaths.TryGetValue(Current.Block, out PathIo IntPath))
                    {
                        IntPaths.Add(Current.Block, IntPath = new PathIo());
                    }

                    if (!VecPaths.TryGetValue(Current.Block, out PathIo VecPath))
                    {
                        VecPaths.Add(Current.Block, VecPath = new PathIo());
                    }

                    IntPath.Set(Current.Entry, Current.IntInputs, Current.IntOutputs);
                    VecPath.Set(Current.Entry, Current.VecInputs, Current.VecOutputs);
                }

                void EnqueueFromCurrent(AILBlock Block, bool RetTarget)
                {
                    BlockIo BlkIO = new BlockIo() { Block = Block };

                    if (RetTarget)
                    {
                        BlkIO.Entry = Block;
                    }
                    else
                    {
                        BlkIO.Entry      = Current.Entry;
                        BlkIO.IntInputs  = Current.IntInputs;
                        BlkIO.VecInputs  = Current.VecInputs;
                        BlkIO.IntOutputs = Current.IntOutputs;
                        BlkIO.VecOutputs = Current.VecOutputs;
                    }

                    Enqueue(BlkIO);
                }

                if (Current.Block.Next != null)
                {
                    EnqueueFromCurrent(Current.Block.Next, Current.Block.HasStateStore);
                }

                if (Current.Block.Branch != null)
                {
                    EnqueueFromCurrent(Current.Block.Branch, false);
                }
            }
        }

        private void InitializeFast(AILBlock[] Graph)
        {
            //This is WAY faster than InitializeOptimal, but results in
            //uneeded loads and stores, so the resulting code will be slower.
            long IntInputs = 0, IntOutputs = 0;
            long VecInputs = 0, VecOutputs = 0;

            foreach (AILBlock Block in Graph)
            {
                IntInputs  |= Block.IntInputs;
                IntOutputs |= Block.IntOutputs;
                VecInputs  |= Block.VecInputs;
                VecOutputs |= Block.VecOutputs;
            }

            //It's possible that not all code paths writes to those output registers,
            //in those cases if we attempt to write an output registers that was
            //not written, we will be just writing zero and messing up the old register value.
            //So we just need to ensure that all outputs are loaded.
            if (Graph.Length > 1)
            {
                IntInputs |= IntOutputs;
                VecInputs |= VecOutputs;
            }

            foreach (AILBlock Block in Graph)
            {
                IntPaths.Add(Block, new PathIo(Block, IntInputs, IntOutputs));
                VecPaths.Add(Block, new PathIo(Block, VecInputs, VecOutputs));
            }
        }

        public long GetIntInputs(AILBlock Root) => GetInputsImpl(Root, IntPaths.Values);
        public long GetVecInputs(AILBlock Root) => GetInputsImpl(Root, VecPaths.Values);

        private long GetInputsImpl(AILBlock Root, IEnumerable<PathIo> Values)
        {
            long Inputs = 0;

            foreach (PathIo Path in Values)
            {
                Inputs |= Path.GetInputs(Root);
            }

            return Inputs;
        }

        public long GetIntOutputs(AILBlock Block) => IntPaths[Block].GetOutputs();
        public long GetVecOutputs(AILBlock Block) => VecPaths[Block].GetOutputs();
    }
}