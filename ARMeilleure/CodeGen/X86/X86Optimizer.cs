using ARMeilleure.CodeGen.Optimizations;
using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;

using static ARMeilleure.IntermediateRepresentation.OperandHelper;
using static ARMeilleure.IntermediateRepresentation.OperationHelper;

namespace ARMeilleure.CodeGen.X86
{
    static class X86Optimizer
    {
        public static void RunPass(ControlFlowGraph cfg)
        {
            for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
            {
                Node nextNode;

                for (Node node = block.Operations.First; node != null; node = nextNode)
                {
                    nextNode = node.ListNext;

                    if (!(node is Operation operation))
                    {
                        continue;
                    }

                    // Insert copies for constants that can't fit on a 32-bits immediate.
                    // Doing this early unblocks a few optimizations.
                    if (operation.Instruction == Instruction.Add)
                    {
                        Operand src1 = operation.GetSource(0);
                        Operand src2 = operation.GetSource(1);

                        if (src1.Kind == OperandKind.Constant && (src1.Relocatable || CodeGenCommon.IsLongConst(src1)))
                        {
                            Operand temp = Local(src1.Type);

                            Operation copyOp = Operation(Instruction.Copy, temp, src1);

                            block.Operations.AddBefore(operation, copyOp);

                            operation.SetSource(0, temp);
                        }

                        if (src2.Kind == OperandKind.Constant && (src2.Relocatable || CodeGenCommon.IsLongConst(src2)))
                        {
                            Operand temp = Local(src2.Type);

                            Operation copyOp = Operation(Instruction.Copy, temp, src2);

                            block.Operations.AddBefore(operation, copyOp);

                            operation.SetSource(1, temp);
                        }
                    }

                    // Try to fold something like:
                    //  shl rbx, 2
                    //  add rax, rbx
                    //  add rax, 0xcafe
                    //  mov rax, [rax]
                    // Into:
                    //  mov rax, [rax+rbx*4+0xcafe]
                    if (IsMemoryLoadOrStore(operation.Instruction))
                    {
                        OperandType type;

                        if (operation.Destination != null)
                        {
                            type = operation.Destination.Type;
                        }
                        else
                        {
                            type = operation.GetSource(1).Type;
                        }

                        MemoryOperand memOp = GetMemoryOperandOrNull(operation.GetSource(0), type);

                        if (memOp != null)
                        {
                            operation.SetSource(0, memOp);
                        }
                    }
                }
            }

            Optimizer.RemoveUnusedNodes(cfg);
        }

        private static MemoryOperand GetMemoryOperandOrNull(Operand addr, OperandType type)
        {
            Operand baseOp = addr;

            // First we check if the address is the result of a local X with 32-bits immediate
            // addition. If that is the case, then the baseOp is X, and the memory operand immediate
            // becomes the addition immediate. Otherwise baseOp keeps being the address.
            int imm = GetConstOp(ref baseOp);

            // Now we check if the baseOp is the result of a local Y with a local Z addition.
            // If that is the case, we now set baseOp to Y and indexOp to Z. We further check
            // if Z is the result of a left shift of local W by a value >= 0 and <= 3, if that
            // is the case, we set indexOp to W and adjust the scale value of the memory operand
            // to match that of the left shift.
            // There is one missed case, which is the address being a shift result, but this is
            // probably not worth optimizing as it should never happen.
            (Operand indexOp, Multiplier scale) = GetIndexOp(ref baseOp);

            // If baseOp is still equal to address, then there's nothing that can be optimized.
            if (baseOp == addr)
            {
                return null;
            }

            return MemoryOp(type, baseOp, indexOp, scale, imm);
        }

        private static int GetConstOp(ref Operand baseOp)
        {
            Operation operation = GetAsgOpWithInst(baseOp, Instruction.Add);

            if (operation == null)
            {
                return 0;
            }

            Operand src1 = operation.GetSource(0);
            Operand src2 = operation.GetSource(1);

            Operand constOp;
            Operand otherOp;

            if (src1.Kind == OperandKind.Constant && src2.Kind == OperandKind.LocalVariable)
            {
                constOp = src1;
                otherOp = src2;
            }
            else if (src1.Kind == OperandKind.LocalVariable && src2.Kind == OperandKind.Constant)
            {
                constOp = src2;
                otherOp = src1;
            }
            else
            {
                return 0;
            }

            // If we have addition by 64-bits constant, then we can't optimize it further,
            // as we can't encode a 64-bits immediate on the memory operand.
            if (CodeGenCommon.IsLongConst(constOp))
            {
                return 0;
            }

            baseOp = otherOp;

            return constOp.AsInt32();
        }

        private static (Operand, Multiplier) GetIndexOp(ref Operand baseOp)
        {
            Operand indexOp = null;

            Multiplier scale = Multiplier.x1;

            Operation addOp = GetAsgOpWithInst(baseOp, Instruction.Add);

            if (addOp == null)
            {
                return (indexOp, scale);
            }

            Operand src1 = addOp.GetSource(0);
            Operand src2 = addOp.GetSource(1);

            if (src1.Kind != OperandKind.LocalVariable || src2.Kind != OperandKind.LocalVariable)
            {
                return (indexOp, scale);
            }

            baseOp = src1;
            indexOp = src2;

            Operation shlOp = GetAsgOpWithInst(src1, Instruction.ShiftLeft);

            bool indexOnSrc2 = false;

            if (shlOp == null)
            {
                shlOp = GetAsgOpWithInst(src2, Instruction.ShiftLeft);

                indexOnSrc2 = true;
            }

            if (shlOp != null)
            {
                Operand shSrc = shlOp.GetSource(0);
                Operand shift = shlOp.GetSource(1);

                if (shSrc.Kind == OperandKind.LocalVariable && shift.Kind == OperandKind.Constant && shift.Value <= 3)
                {
                    scale = shift.Value switch
                    {
                        1 => Multiplier.x2,
                        2 => Multiplier.x4,
                        3 => Multiplier.x8,
                        _ => Multiplier.x1
                    };

                    baseOp = indexOnSrc2 ? src1 : src2;
                    indexOp = shSrc;
                }
            }

            return (indexOp, scale);
        }

        private static Operation GetAsgOpWithInst(Operand op, Instruction inst)
        {
            // If we have multiple assignments, folding is not safe
            // as the value may be different depending on the
            // control flow path.
            if (op.Assignments.Count != 1)
            {
                return null;
            }

            Node asgOp = op.Assignments[0];

            if (!(asgOp is Operation operation))
            {
                return null;
            }

            if (operation.Instruction != inst)
            {
                return null;
            }

            return operation;
        }

        private static bool IsMemoryLoadOrStore(Instruction inst)
        {
            return inst == Instruction.Load ||
                   inst == Instruction.Load16 ||
                   inst == Instruction.Load8 ||
                   inst == Instruction.Store ||
                   inst == Instruction.Store16 ||
                   inst == Instruction.Store8;
        }
    }
}
