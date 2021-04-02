using ARMeilleure.Common;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Translation
{
    static partial class Ssa
    {
        private class DefMap
        {
            private readonly Dictionary<int, Operand> _map;
            private readonly BitMap _phiMasks;

            public DefMap()
            {
                _map = new Dictionary<int, Operand>();
                _phiMasks = new BitMap(RegisterConsts.TotalCount);
            }

            public bool TryAddOperand(int key, Operand operand)
            {
                return _map.TryAdd(key, operand);
            }

            public bool TryGetOperand(int key, out Operand operand)
            {
                return _map.TryGetValue(key, out operand);
            }

            public bool AddPhi(int key)
            {
                return _phiMasks.Set(key);
            }

            public bool HasPhi(int key)
            {
                return _phiMasks.IsSet(key);
            }
        }

        public static void Construct(ControlFlowGraph cfg)
        {
            var globalDefs = new DefMap[cfg.Blocks.Count];
            var localDefs = new Operand[RegisterConsts.TotalCount];

            var dfPhiBlocks = new Queue<BasicBlock>();

            for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
            {
                globalDefs[block.Index] = new DefMap();
            }

            // First pass, get all defs and locals uses.
            for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
            {
                for (Node node = block.Operations.First; node != null; node = node.ListNext)
                {
                    if (node is not Operation operation)
                    {
                        continue;
                    }

                    for (int index = 0; index < operation.SourcesCount; index++)
                    {
                        Operand src = operation.GetSource(index);

                        if (TryGetId(src, out int srcKey))
                        {
                            Operand local = localDefs[srcKey] ?? src;

                            operation.SetSource(index, local);
                        }
                    }

                    Operand dest = operation.Destination;

                    if (TryGetId(dest, out int destKey))
                    {
                        Operand local = Local(dest.Type);

                        localDefs[destKey] = local;

                        operation.Destination = local;
                    }
                }

                for (int key = 0; key < localDefs.Length; key++)
                {
                    Operand local = localDefs[key];

                    if (local is null)
                    {
                        continue;
                    }

                    globalDefs[block.Index].TryAddOperand(key, local);

                    dfPhiBlocks.Enqueue(block);

                    while (dfPhiBlocks.TryDequeue(out BasicBlock dfPhiBlock))
                    {
                        foreach (BasicBlock domFrontier in dfPhiBlock.DominanceFrontiers)
                        {
                            if (globalDefs[domFrontier.Index].AddPhi(key))
                            {
                                dfPhiBlocks.Enqueue(domFrontier);
                            }
                        }
                    }
                }

                Array.Clear(localDefs, 0, localDefs.Length);
            }

            // Second pass, rename variables with definitions on different blocks.
            for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
            {
                for (Node node = block.Operations.First; node != null; node = node.ListNext)
                {
                    if (node is not Operation operation)
                    {
                        continue;
                    }

                    for (int index = 0; index < operation.SourcesCount; index++)
                    {
                        Operand src = operation.GetSource(index);

                        if (TryGetId(src, out int key))
                        {
                            Operand local = localDefs[key];

                            if (local is null)
                            {
                                local = FindDef(globalDefs, block, src);
                                localDefs[key] = local;
                            }

                            operation.SetSource(index, local);
                        }
                    }
                }

                Array.Clear(localDefs, 0, localDefs.Length);
            }
        }

        private static Operand FindDef(DefMap[] globalDefs, BasicBlock current, Operand operand)
        {
            if (globalDefs[current.Index].HasPhi(GetId(operand)))
            {
                return InsertPhi(globalDefs, current, operand);
            }

            if (current != current.ImmediateDominator)
            {
                return FindDefOnPred(globalDefs, current.ImmediateDominator, operand);
            }

            return Undef();
        }

        private static Operand FindDefOnPred(DefMap[] globalDefs, BasicBlock current, Operand operand)
        {
            BasicBlock previous;

            do
            {
                DefMap defMap = globalDefs[current.Index];

                int key = GetId(operand);

                if (defMap.TryGetOperand(key, out Operand lastDef))
                {
                    return lastDef;
                }

                if (defMap.HasPhi(key))
                {
                    return InsertPhi(globalDefs, current, operand);
                }

                previous = current;
                current  = current.ImmediateDominator;
            }
            while (previous != current);

            return Undef();
        }

        private static Operand InsertPhi(DefMap[] globalDefs, BasicBlock block, Operand operand)
        {
            // This block has a Phi that has not been materialized yet, but that
            // would define a new version of the variable we're looking for. We need
            // to materialize the Phi, add all the block/operand pairs into the Phi, and
            // then use the definition from that Phi.
            Operand local = Local(operand.Type);

            PhiNode phi = new PhiNode(local, block.Predecessors.Count);

            AddPhi(block, phi);

            globalDefs[block.Index].TryAddOperand(GetId(operand), local);

            for (int index = 0; index < block.Predecessors.Count; index++)
            {
                BasicBlock predecessor = block.Predecessors[index];

                phi.SetBlock(index, predecessor);
                phi.SetSource(index, FindDefOnPred(globalDefs, predecessor, operand));
            }

            return local;
        }

        private static void AddPhi(BasicBlock block, PhiNode phi)
        {
            Node node = block.Operations.First;

            if (node != null)
            {
                while (node.ListNext is PhiNode)
                {
                    node = node.ListNext;
                }
            }

            if (node is PhiNode)
            {
                block.Operations.AddAfter(node, phi);
            }
            else
            {
                block.Operations.AddFirst(phi);
            }
        }

        private static bool TryGetId(Operand operand, out int result)
        {
            if (operand is { Kind: OperandKind.Register })
            {
                Register reg = operand.GetRegister();

                if (reg.Type == RegisterType.Integer)
                {
                    result = reg.Index;
                }
                else if (reg.Type == RegisterType.Vector)
                {
                    result = RegisterConsts.IntRegsCount + reg.Index;
                }
                else if (reg.Type == RegisterType.Flag)
                {
                    result = RegisterConsts.IntAndVecRegsCount + reg.Index;
                }
                else /* if (reg.Type == RegisterType.FpFlag) */
                {
                    result = RegisterConsts.FpFlagsOffset + reg.Index;
                }

                return true;
            }

            result = -1;

            return false;
        }

        private static int GetId(Operand operand)
        {
            if (!TryGetId(operand, out int key))
            {
                Debug.Fail("OperandKind must be Register.");
            }

            return key;
        }
    }
}