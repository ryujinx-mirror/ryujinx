using ARMeilleure.Common;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.State;
using System.Collections.Generic;

using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Translation
{
    static partial class Ssa
    {
        private class DefMap
        {
            private Dictionary<Register, Operand> _map;

            private BitMap _phiMasks;

            public DefMap()
            {
                _map = new Dictionary<Register, Operand>();

                _phiMasks = new BitMap(RegisterConsts.TotalCount);
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
                return _phiMasks.Set(GetIdFromRegister(reg));
            }

            public bool HasPhi(Register reg)
            {
                return _phiMasks.IsSet(GetIdFromRegister(reg));
            }
        }

        public static void Construct(ControlFlowGraph cfg)
        {
            DefMap[] globalDefs = new DefMap[cfg.Blocks.Count];

            for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
            {
                globalDefs[block.Index] = new DefMap();
            }

            Queue<BasicBlock> dfPhiBlocks = new Queue<BasicBlock>();

            // First pass, get all defs and locals uses.
            for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
            {
                Operand[] localDefs = new Operand[RegisterConsts.TotalCount];

                Node node = block.Operations.First;

                Operand RenameLocal(Operand operand)
                {
                    if (operand != null && operand.Kind == OperandKind.Register)
                    {
                        Operand local = localDefs[GetIdFromRegister(operand.GetRegister())];

                        operand = local ?? operand;
                    }

                    return operand;
                }

                while (node != null)
                {
                    if (node is Operation operation)
                    {
                        for (int index = 0; index < operation.SourcesCount; index++)
                        {
                            operation.SetSource(index, RenameLocal(operation.GetSource(index)));
                        }

                        Operand dest = operation.Destination;

                        if (dest != null && dest.Kind == OperandKind.Register)
                        {
                            Operand local = Local(dest.Type);

                            localDefs[GetIdFromRegister(dest.GetRegister())] = local;

                            operation.Destination = local;
                        }
                    }

                    node = node.ListNext;
                }

                for (int index = 0; index < RegisterConsts.TotalCount; index++)
                {
                    Operand local = localDefs[index];

                    if (local == null)
                    {
                        continue;
                    }

                    Register reg = GetRegisterFromId(index);

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
            for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
            {
                Operand[] localDefs = new Operand[RegisterConsts.TotalCount];

                Node node = block.Operations.First;

                Operand RenameGlobal(Operand operand)
                {
                    if (operand != null && operand.Kind == OperandKind.Register)
                    {
                        int key = GetIdFromRegister(operand.GetRegister());

                        Operand local = localDefs[key];

                        if (local == null)
                        {
                            local = FindDef(globalDefs, block, operand);

                            localDefs[key] = local;
                        }

                        operand = local;
                    }

                    return operand;
                }

                while (node != null)
                {
                    if (node is Operation operation)
                    {
                        for (int index = 0; index < operation.SourcesCount; index++)
                        {
                            operation.SetSource(index, RenameGlobal(operation.GetSource(index)));
                        }
                    }

                    node = node.ListNext;
                }
            }
        }

        private static Operand FindDef(DefMap[] globalDefs, BasicBlock current, Operand operand)
        {
            if (globalDefs[current.Index].HasPhi(operand.GetRegister()))
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

                Register reg = operand.GetRegister();

                if (defMap.TryGetOperand(reg, out Operand lastDef))
                {
                    return lastDef;
                }

                if (defMap.HasPhi(reg))
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

            globalDefs[block.Index].TryAddOperand(operand.GetRegister(), local);

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

        private static int GetIdFromRegister(Register reg)
        {
            if (reg.Type == RegisterType.Integer)
            {
                return reg.Index;
            }
            else if (reg.Type == RegisterType.Vector)
            {
                return RegisterConsts.IntRegsCount + reg.Index;
            }
            else if (reg.Type == RegisterType.Flag)
            {
                return RegisterConsts.IntAndVecRegsCount + reg.Index;
            }
            else /* if (reg.Type == RegisterType.FpFlag) */
            {
                return RegisterConsts.FpFlagsOffset + reg.Index;
            }
        }

        private static Register GetRegisterFromId(int id)
        {
            if (id < RegisterConsts.IntRegsCount)
            {
                return new Register(id, RegisterType.Integer);
            }
            else if (id < RegisterConsts.IntAndVecRegsCount)
            {
                return new Register(id - RegisterConsts.IntRegsCount, RegisterType.Vector);
            }
            else if (id < RegisterConsts.FpFlagsOffset)
            {
                return new Register(id - RegisterConsts.IntAndVecRegsCount, RegisterType.Flag);
            }
            else /* if (id < RegisterConsts.TotalCount) */
            {
                return new Register(id - RegisterConsts.FpFlagsOffset, RegisterType.FpFlag);
            }
        }
    }
}