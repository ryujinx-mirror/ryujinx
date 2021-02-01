using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System.Collections.Generic;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Translation
{
    static class Ssa
    {
        private const int GprsAndPredsCount = RegisterConsts.GprsCount + RegisterConsts.PredsCount;

        private class DefMap
        {
            private Dictionary<Register, Operand> _map;

            private long[] _phiMasks;

            public DefMap()
            {
                _map = new Dictionary<Register, Operand>();

                _phiMasks = new long[(RegisterConsts.TotalCount + 63) / 64];
            }

            public bool TryAddOperand(Register reg, Operand operand)
            {
                return _map.TryAdd(reg, operand);
            }

            public bool TryGetOperand(Register reg, out Operand operand)
            {
                return _map.TryGetValue(reg, out operand);
            }

            public bool AddPhi(Register reg)
            {
                int key = GetKeyFromRegister(reg);

                int index = key / 64;
                int bit   = key & 63;

                long mask = 1L << bit;

                if ((_phiMasks[index] & mask) != 0)
                {
                    return false;
                }

                _phiMasks[index] |= mask;

                return true;
            }

            public bool HasPhi(Register reg)
            {
                int key = GetKeyFromRegister(reg);

                int index = key / 64;
                int bit   = key & 63;

                return (_phiMasks[index] & (1L << bit)) != 0;
            }
        }

        private struct Definition
        {
            public BasicBlock Block { get; }
            public Operand    Local { get; }

            public Definition(BasicBlock block, Operand local)
            {
                Block = block;
                Local = local;
            }
        }

        public static void Rename(BasicBlock[] blocks)
        {
            DefMap[] globalDefs = new DefMap[blocks.Length];

            for (int blkIndex = 0; blkIndex < blocks.Length; blkIndex++)
            {
                globalDefs[blkIndex] = new DefMap();
            }

            Queue<BasicBlock> dfPhiBlocks = new Queue<BasicBlock>();

            // First pass, get all defs and locals uses.
            for (int blkIndex = 0; blkIndex < blocks.Length; blkIndex++)
            {
                Operand[] localDefs = new Operand[RegisterConsts.TotalCount];

                Operand RenameLocal(Operand operand)
                {
                    if (operand != null && operand.Type == OperandType.Register)
                    {
                        Operand local = localDefs[GetKeyFromRegister(operand.GetRegister())];

                        operand = local ?? operand;
                    }

                    return operand;
                }

                BasicBlock block = blocks[blkIndex];

                LinkedListNode<INode> node = block.Operations.First;

                while (node != null)
                {
                    if (node.Value is Operation operation)
                    {
                        for (int index = 0; index < operation.SourcesCount; index++)
                        {
                            operation.SetSource(index, RenameLocal(operation.GetSource(index)));
                        }

                        for (int index = 0; index < operation.DestsCount; index++)
                        {
                            Operand dest = operation.GetDest(index);

                            if (dest != null && dest.Type == OperandType.Register)
                            {
                                Operand local = Local();

                                localDefs[GetKeyFromRegister(dest.GetRegister())] = local;

                                operation.SetDest(index, local);
                            }
                        }
                    }

                    node = node.Next;
                }

                for (int index = 0; index < RegisterConsts.TotalCount; index++)
                {
                    Operand local = localDefs[index];

                    if (local == null)
                    {
                        continue;
                    }

                    Register reg = GetRegisterFromKey(index);

                    globalDefs[block.Index].TryAddOperand(reg, local);

                    dfPhiBlocks.Enqueue(block);

                    while (dfPhiBlocks.TryDequeue(out BasicBlock dfPhiBlock))
                    {
                        foreach (BasicBlock domFrontier in dfPhiBlock.DominanceFrontiers)
                        {
                            if (globalDefs[domFrontier.Index].AddPhi(reg))
                            {
                                dfPhiBlocks.Enqueue(domFrontier);
                            }
                        }
                    }
                }
            }

            // Second pass, rename variables with definitions on different blocks.
            for (int blkIndex = 0; blkIndex < blocks.Length; blkIndex++)
            {
                Operand[] localDefs = new Operand[RegisterConsts.TotalCount];

                BasicBlock block = blocks[blkIndex];

                Operand RenameGlobal(Operand operand)
                {
                    if (operand != null && operand.Type == OperandType.Register)
                    {
                        int key = GetKeyFromRegister(operand.GetRegister());

                        Operand local = localDefs[key];

                        if (local != null)
                        {
                            return local;
                        }

                        operand = FindDefinitionForCurr(globalDefs, block, operand.GetRegister());

                        localDefs[key] = operand;
                    }

                    return operand;
                }

                for (LinkedListNode<INode> node = block.Operations.First; node != null; node = node.Next)
                {
                    if (node.Value is Operation operation)
                    {
                        for (int index = 0; index < operation.SourcesCount; index++)
                        {
                            operation.SetSource(index, RenameGlobal(operation.GetSource(index)));
                        }
                    }
                }
            }
        }

        private static Operand FindDefinitionForCurr(DefMap[] globalDefs, BasicBlock current, Register reg)
        {
            if (globalDefs[current.Index].HasPhi(reg))
            {
                return InsertPhi(globalDefs, current, reg);
            }

            if (current != current.ImmediateDominator)
            {
                return FindDefinition(globalDefs, current.ImmediateDominator, reg).Local;
            }

            return Undef();
        }

        private static Definition FindDefinition(DefMap[] globalDefs, BasicBlock current, Register reg)
        {
            foreach (BasicBlock block in SelfAndImmediateDominators(current))
            {
                DefMap defMap = globalDefs[block.Index];

                if (defMap.TryGetOperand(reg, out Operand lastDef))
                {
                    return new Definition(block, lastDef);
                }

                if (defMap.HasPhi(reg))
                {
                    return new Definition(block, InsertPhi(globalDefs, block, reg));
                }
            }

            return new Definition(current, Undef());
        }

        private static IEnumerable<BasicBlock> SelfAndImmediateDominators(BasicBlock block)
        {
            while (block != block.ImmediateDominator)
            {
                yield return block;

                block = block.ImmediateDominator;
            }

            yield return block;
        }

        private static Operand InsertPhi(DefMap[] globalDefs, BasicBlock block, Register reg)
        {
            // This block has a Phi that has not been materialized yet, but that
            // would define a new version of the variable we're looking for. We need
            // to materialize the Phi, add all the block/operand pairs into the Phi, and
            // then use the definition from that Phi.
            Operand local = Local();

            PhiNode phi = new PhiNode(local);

            AddPhi(block, phi);

            globalDefs[block.Index].TryAddOperand(reg, local);

            foreach (BasicBlock predecessor in block.Predecessors)
            {
                Definition def = FindDefinition(globalDefs, predecessor, reg);

                phi.AddSource(def.Block, def.Local);
            }

            return local;
        }

        private static void AddPhi(BasicBlock block, PhiNode phi)
        {
            LinkedListNode<INode> node = block.Operations.First;

            if (node != null)
            {
                while (node.Next?.Value is PhiNode)
                {
                    node = node.Next;
                }
            }

            if (node?.Value is PhiNode)
            {
                block.Operations.AddAfter(node, phi);
            }
            else
            {
                block.Operations.AddFirst(phi);
            }
        }

        private static int GetKeyFromRegister(Register reg)
        {
            if (reg.Type == RegisterType.Gpr)
            {
                return reg.Index;
            }
            else if (reg.Type == RegisterType.Predicate)
            {
                return RegisterConsts.GprsCount + reg.Index;
            }
            else /* if (reg.Type == RegisterType.Flag) */
            {
                return GprsAndPredsCount + reg.Index;
            }
        }

        private static Register GetRegisterFromKey(int key)
        {
            if (key < RegisterConsts.GprsCount)
            {
                return new Register(key, RegisterType.Gpr);
            }
            else if (key < GprsAndPredsCount)
            {
                return new Register(key - RegisterConsts.GprsCount, RegisterType.Predicate);
            }
            else /* if (key < RegisterConsts.TotalCount) */
            {
                return new Register(key - GprsAndPredsCount, RegisterType.Flag);
            }
        }
    }
}