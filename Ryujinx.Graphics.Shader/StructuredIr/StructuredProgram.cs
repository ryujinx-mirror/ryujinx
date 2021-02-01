using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Ryujinx.Graphics.Shader.StructuredIr
{
    static class StructuredProgram
    {
        public static StructuredProgramInfo MakeStructuredProgram(Function[] functions, ShaderConfig config)
        {
            StructuredProgramContext context = new StructuredProgramContext(config);

            for (int funcIndex = 0; funcIndex < functions.Length; funcIndex++)
            {
                Function function = functions[funcIndex];

                BasicBlock[] blocks = function.Blocks;

                VariableType returnType = function.ReturnsValue ? VariableType.S32 : VariableType.None;

                VariableType[] inArguments  = new VariableType[function.InArgumentsCount];
                VariableType[] outArguments = new VariableType[function.OutArgumentsCount];

                for (int i = 0; i < inArguments.Length; i++)
                {
                    inArguments[i] = VariableType.S32;
                }

                for (int i = 0; i < outArguments.Length; i++)
                {
                    outArguments[i] = VariableType.S32;
                }

                context.EnterFunction(blocks.Length, function.Name, returnType, inArguments, outArguments);

                PhiFunctions.Remove(blocks);

                for (int blkIndex = 0; blkIndex < blocks.Length; blkIndex++)
                {
                    BasicBlock block = blocks[blkIndex];

                    context.EnterBlock(block);

                    for (LinkedListNode<INode> opNode = block.Operations.First; opNode != null; opNode = opNode.Next)
                    {
                        Operation operation = (Operation)opNode.Value;

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

                AstOptimizer.Optimize(context);

                context.LeaveFunction();
            }

            return context.Info;
        }

        private static void AddOperation(StructuredProgramContext context, Operation operation)
        {
            Instruction inst = operation.Inst;

            int sourcesCount = operation.SourcesCount;
            int outDestsCount = operation.DestsCount != 0 ? operation.DestsCount - 1 : 0;

            IAstNode[] sources = new IAstNode[sourcesCount + outDestsCount];

            for (int index = 0; index < operation.SourcesCount; index++)
            {
                sources[index] = context.GetOperandUse(operation.GetSource(index));
            }

            for (int index = 0; index < outDestsCount; index++)
            {
                AstOperand oper = context.GetOperandDef(operation.GetDest(1 + index));

                oper.VarType = InstructionInfo.GetSrcVarType(inst, sourcesCount + index);

                sources[sourcesCount + index] = oper;
            }

            AstTextureOperation GetAstTextureOperation(TextureOperation texOp)
            {
                return new AstTextureOperation(
                    inst,
                    texOp.Type,
                    texOp.Format,
                    texOp.Flags,
                    texOp.CbufSlot,
                    texOp.Handle,
                    4, // TODO: Non-hardcoded array size.
                    texOp.Index,
                    sources);
            }

            if (operation.Dest != null)
            {
                AstOperand dest = context.GetOperandDef(operation.Dest);

                if (inst == Instruction.LoadConstant)
                {
                    Operand slot = operation.GetSource(0);

                    if (slot.Type == OperandType.Constant)
                    {
                        context.Info.CBuffers.Add(slot.Value);
                    }
                    else
                    {
                        // If the value is not constant, then we don't know
                        // how many constant buffers are used, so we assume
                        // all of them are used.
                        int cbCount = 32 - BitOperations.LeadingZeroCount(context.Config.GpuAccessor.QueryConstantBufferUse());

                        for (int index = 0; index < cbCount; index++)
                        {
                            context.Info.CBuffers.Add(index);
                        }

                        context.Info.UsesCbIndexing = true;
                    }
                }
                else if (UsesStorage(inst))
                {
                    AddSBufferUse(context.Info.SBuffers, operation);
                }

                // If all the sources are bool, it's better to use short-circuiting
                // logical operations, rather than forcing a cast to int and doing
                // a bitwise operation with the value, as it is likely to be used as
                // a bool in the end.
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
                        inst |= Instruction.FP32;
                    }

                    dest.VarType = type;
                }
                else
                {
                    dest.VarType = InstructionInfo.GetDestVarType(inst);
                }

                IAstNode source;

                if (operation is TextureOperation texOp)
                {
                    if (texOp.Inst == Instruction.ImageLoad || texOp.Inst == Instruction.ImageStore)
                    {
                        dest.VarType = texOp.Format.GetComponentType();
                    }

                    AstTextureOperation astTexOp = GetAstTextureOperation(texOp);

                    if (texOp.Inst == Instruction.ImageLoad)
                    {
                        context.Info.Images.Add(astTexOp);
                    }
                    else
                    {
                        context.Info.Samplers.Add(astTexOp);
                    }

                    source = astTexOp;
                }
                else if (!isCopy)
                {
                    source = new AstOperation(inst, operation.Index, sources, operation.SourcesCount);
                }
                else
                {
                    source = sources[0];
                }

                context.AddNode(new AstAssignment(dest, source));
            }
            else if (operation.Inst == Instruction.Comment)
            {
                context.AddNode(new AstComment(((CommentNode)operation).Comment));
            }
            else if (operation is TextureOperation texOp)
            {
                AstTextureOperation astTexOp = GetAstTextureOperation(texOp);

                context.Info.Images.Add(astTexOp);

                context.AddNode(astTexOp);
            }
            else
            {
                if (UsesStorage(inst))
                {
                    AddSBufferUse(context.Info.SBuffers, operation);
                }

                context.AddNode(new AstOperation(inst, operation.Index, sources, operation.SourcesCount));
            }

            // Those instructions needs to be emulated by using helper functions,
            // because they are NVIDIA specific. Those flags helps the backend to
            // decide which helper functions are needed on the final generated code.
            switch (operation.Inst)
            {
                case Instruction.AtomicMaxS32 | Instruction.MrShared:
                case Instruction.AtomicMinS32 | Instruction.MrShared:
                    context.Info.HelperFunctionsMask |= HelperFunctionsMask.AtomicMinMaxS32Shared;
                    break;
                case Instruction.AtomicMaxS32 | Instruction.MrStorage:
                case Instruction.AtomicMinS32 | Instruction.MrStorage:
                    context.Info.HelperFunctionsMask |= HelperFunctionsMask.AtomicMinMaxS32Storage;
                    break;
                case Instruction.MultiplyHighS32:
                    context.Info.HelperFunctionsMask |= HelperFunctionsMask.MultiplyHighS32;
                    break;
                case Instruction.MultiplyHighU32:
                    context.Info.HelperFunctionsMask |= HelperFunctionsMask.MultiplyHighU32;
                    break;
                case Instruction.Shuffle:
                    context.Info.HelperFunctionsMask |= HelperFunctionsMask.Shuffle;
                    break;
                case Instruction.ShuffleDown:
                    context.Info.HelperFunctionsMask |= HelperFunctionsMask.ShuffleDown;
                    break;
                case Instruction.ShuffleUp:
                    context.Info.HelperFunctionsMask |= HelperFunctionsMask.ShuffleUp;
                    break;
                case Instruction.ShuffleXor:
                    context.Info.HelperFunctionsMask |= HelperFunctionsMask.ShuffleXor;
                    break;
                case Instruction.SwizzleAdd:
                    context.Info.HelperFunctionsMask |= HelperFunctionsMask.SwizzleAdd;
                    break;
            }
        }

        private static void AddSBufferUse(HashSet<int> sBuffers, Operation operation)
        {
            Operand slot = operation.GetSource(0);

            if (slot.Type == OperandType.Constant)
            {
                sBuffers.Add(slot.Value);
            }
            else
            {
                // If the value is not constant, then we don't know
                // how many storage buffers are used, so we assume
                // all of them are used.
                for (int index = 0; index < GlobalMemory.StorageMaxCount; index++)
                {
                    sBuffers.Add(index);
                }
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

        private static bool UsesStorage(Instruction inst)
        {
            if (inst == Instruction.LoadStorage || inst == Instruction.StoreStorage)
            {
                return true;
            }

            return inst.IsAtomic() && (inst & Instruction.MrMask) == Instruction.MrStorage;
        }
    }
}