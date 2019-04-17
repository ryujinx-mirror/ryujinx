using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using System;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    static class StructuredProgram
    {
        public static StructuredProgramInfo MakeStructuredProgram(BasicBlock[] blocks)
        {
            PhiFunctions.Remove(blocks);

            StructuredProgramContext context = new StructuredProgramContext(blocks.Length);

            for (int blkIndex = 0; blkIndex < blocks.Length; blkIndex++)
            {
                BasicBlock block = blocks[blkIndex];

                context.EnterBlock(block);

                foreach (INode node in block.Operations)
                {
                    Operation operation = (Operation)node;

                    if (IsBranchInst(operation.Inst))
                    {
                        context.LeaveBlock(block, operation);
                    }
                    else
                    {
                        AddOperation(context, operation);
                    }
                }
            }

            GotoElimination.Eliminate(context.GetGotos());

            AstOptimizer.Optimize(context.Info);

            return context.Info;
        }

        private static void AddOperation(StructuredProgramContext context, Operation operation)
        {
            Instruction inst = operation.Inst;

            IAstNode[] sources = new IAstNode[operation.SourcesCount];

            for (int index = 0; index < sources.Length; index++)
            {
                sources[index] = context.GetOperandUse(operation.GetSource(index));
            }

            if (operation.Dest != null)
            {
                AstOperand dest = context.GetOperandDef(operation.Dest);

                if (inst == Instruction.LoadConstant)
                {
                    Operand ldcSource = operation.GetSource(0);

                    if (ldcSource.Type != OperandType.Constant)
                    {
                        throw new InvalidOperationException("Found LDC with non-constant constant buffer slot.");
                    }

                    context.Info.CBuffers.Add(ldcSource.Value);
                }

                AstAssignment assignment;

                //If all the sources are bool, it's better to use short-circuiting
                //logical operations, rather than forcing a cast to int and doing
                //a bitwise operation with the value, as it is likely to be used as
                //a bool in the end.
                if (IsBitwiseInst(inst) && AreAllSourceTypesEqual(sources, VariableType.Bool))
                {
                    inst = GetLogicalFromBitwiseInst(inst);
                }

                bool isCondSel = inst == Instruction.ConditionalSelect;
                bool isCopy    = inst == Instruction.Copy;

                if (isCondSel || isCopy)
                {
                    VariableType type = GetVarTypeFromUses(operation.Dest);

                    if (isCondSel && type == VariableType.F32)
                    {
                        inst |= Instruction.FP;
                    }

                    dest.VarType = type;
                }
                else
                {
                    dest.VarType = InstructionInfo.GetDestVarType(inst);
                }

                int componentMask = 1 << operation.ComponentIndex;

                IAstNode source;

                if (operation is TextureOperation texOp)
                {
                    AstTextureOperation astTexOp = new AstTextureOperation(
                        inst,
                        texOp.Type,
                        texOp.Flags,
                        texOp.Handle,
                        componentMask,
                        sources);

                    context.Info.Samplers.Add(astTexOp);

                    source = astTexOp;
                }
                else if (!isCopy)
                {
                    source = new AstOperation(inst, componentMask, sources);
                }
                else
                {
                    source = sources[0];
                }

                assignment = new AstAssignment(dest, source);

                context.AddNode(assignment);
            }
            else
            {
                context.AddNode(new AstOperation(inst, sources));
            }
        }

        private static VariableType GetVarTypeFromUses(Operand dest)
        {
            HashSet<Operand> visited = new HashSet<Operand>();

            Queue<Operand> pending = new Queue<Operand>();

            bool Enqueue(Operand operand)
            {
                if (visited.Add(operand))
                {
                    pending.Enqueue(operand);

                    return true;
                }

                return false;
            }

            Enqueue(dest);

            while (pending.TryDequeue(out Operand operand))
            {
                foreach (INode useNode in operand.UseOps)
                {
                    if (!(useNode is Operation operation))
                    {
                        continue;
                    }

                    if (operation.Inst == Instruction.Copy)
                    {
                        if (operation.Dest.Type == OperandType.LocalVariable)
                        {
                            if (Enqueue(operation.Dest))
                            {
                                break;
                            }
                        }
                        else
                        {
                            return OperandInfo.GetVarType(operation.Dest.Type);
                        }
                    }
                    else
                    {
                        for (int index = 0; index < operation.SourcesCount; index++)
                        {
                            if (operation.GetSource(index) == operand)
                            {
                                return InstructionInfo.GetSrcVarType(operation.Inst, index);
                            }
                        }
                    }
                }
            }

            return VariableType.S32;
        }

        private static bool AreAllSourceTypesEqual(IAstNode[] sources, VariableType type)
        {
            foreach (IAstNode node in sources)
            {
                if (!(node is AstOperand operand))
                {
                    return false;
                }

                if (operand.VarType != type)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsBranchInst(Instruction inst)
        {
            switch (inst)
            {
                case Instruction.Branch:
                case Instruction.BranchIfFalse:
                case Instruction.BranchIfTrue:
                    return true;
            }

            return false;
        }

        private static bool IsBitwiseInst(Instruction inst)
        {
            switch (inst)
            {
                case Instruction.BitwiseAnd:
                case Instruction.BitwiseExclusiveOr:
                case Instruction.BitwiseNot:
                case Instruction.BitwiseOr:
                    return true;
            }

            return false;
        }

        private static Instruction GetLogicalFromBitwiseInst(Instruction inst)
        {
            switch (inst)
            {
                case Instruction.BitwiseAnd:         return Instruction.LogicalAnd;
                case Instruction.BitwiseExclusiveOr: return Instruction.LogicalExclusiveOr;
                case Instruction.BitwiseNot:         return Instruction.LogicalNot;
                case Instruction.BitwiseOr:          return Instruction.LogicalOr;
            }

            throw new ArgumentException($"Unexpected instruction \"{inst}\".");
        }
    }
}