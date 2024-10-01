using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System;
using System.Diagnostics;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.CodeGen.Optimizations
{
    static class Optimizer
    {
        public static void RunPass(ControlFlowGraph cfg)
        {
            // Scratch buffer used to store uses.
            Span<Operation> buffer = default;

            bool modified;

            do
            {
                modified = false;

                for (BasicBlock block = cfg.Blocks.Last; block != null; block = block.ListPrevious)
                {
                    Operation node;
                    Operation prevNode;

                    for (node = block.Operations.Last; node != default; node = prevNode)
                    {
                        prevNode = node.ListPrevious;

                        if (IsUnused(node))
                        {
                            RemoveNode(block, node);

                            modified = true;

                            continue;
                        }
                        else if (node.Instruction == Instruction.Phi)
                        {
                            continue;
                        }

                        ConstantFolding.RunPass(node);
                        Simplification.RunPass(node);

                        if (DestIsSingleLocalVar(node))
                        {
                            if (IsPropagableCompare(node))
                            {
                                modified |= PropagateCompare(ref buffer, node);

                                if (modified && IsUnused(node))
                                {
                                    RemoveNode(block, node);
                                }
                            }
                            else if (IsPropagableCopy(node))
                            {
                                PropagateCopy(ref buffer, node);

                                RemoveNode(block, node);

                                modified = true;
                            }
                        }
                    }
                }
            }
            while (modified);
        }

        public static void RemoveUnusedNodes(ControlFlowGraph cfg)
        {
            bool modified;

            do
            {
                modified = false;

                for (BasicBlock block = cfg.Blocks.Last; block != null; block = block.ListPrevious)
                {
                    Operation node;
                    Operation prevNode;

                    for (node = block.Operations.Last; node != default; node = prevNode)
                    {
                        prevNode = node.ListPrevious;

                        if (IsUnused(node))
                        {
                            RemoveNode(block, node);

                            modified = true;
                        }
                    }
                }
            }
            while (modified);
        }

        private static bool PropagateCompare(ref Span<Operation> buffer, Operation compOp)
        {
            // Try to propagate Compare operations into their BranchIf uses, when these BranchIf uses are in the form
            // of:
            //
            // - BranchIf %x, 0x0, Equal        ;; i.e BranchIfFalse %x
            // - BranchIf %x, 0x0, NotEqual     ;; i.e BranchIfTrue %x
            //
            // The commutative property of Equal and NotEqual is taken into consideration as well.
            //
            // For example:
            //
            //  %x = Compare %a, %b, comp
            //  BranchIf %x, 0x0, NotEqual
            //
            // =>
            //
            //  BranchIf %a, %b, comp

            static bool IsZeroBranch(Operation operation, out Comparison compType)
            {
                compType = Comparison.Equal;

                if (operation.Instruction != Instruction.BranchIf)
                {
                    return false;
                }

                Operand src1 = operation.GetSource(0);
                Operand src2 = operation.GetSource(1);
                Operand comp = operation.GetSource(2);

                compType = (Comparison)comp.AsInt32();

                return (src1.Kind == OperandKind.Constant && src1.Value == 0) ||
                       (src2.Kind == OperandKind.Constant && src2.Value == 0);
            }

            bool modified = false;

            Operand dest = compOp.Destination;
            Operand src1 = compOp.GetSource(0);
            Operand src2 = compOp.GetSource(1);
            Operand comp = compOp.GetSource(2);

            Comparison compType = (Comparison)comp.AsInt32();

            Span<Operation> uses = dest.GetUses(ref buffer);

            foreach (Operation use in uses)
            {
                // If operation is a BranchIf and has a constant value 0 in its RHS or LHS source operands.
                if (IsZeroBranch(use, out Comparison otherCompType))
                {
                    Comparison propCompType;

                    if (otherCompType == Comparison.NotEqual)
                    {
                        propCompType = compType;
                    }
                    else if (otherCompType == Comparison.Equal)
                    {
                        propCompType = compType.Invert();
                    }
                    else
                    {
                        continue;
                    }

                    use.SetSource(0, src1);
                    use.SetSource(1, src2);
                    use.SetSource(2, Const((int)propCompType));

                    modified = true;
                }
            }

            return modified;
        }

        private static void PropagateCopy(ref Span<Operation> buffer, Operation copyOp)
        {
            // Propagate copy source operand to all uses of the destination operand.
            Operand dest = copyOp.Destination;
            Operand source = copyOp.GetSource(0);

            Span<Operation> uses = dest.GetUses(ref buffer);

            foreach (Operation use in uses)
            {
                for (int index = 0; index < use.SourcesCount; index++)
                {
                    if (use.GetSource(index) == dest)
                    {
                        use.SetSource(index, source);
                    }
                }
            }
        }

        private static void RemoveNode(BasicBlock block, Operation node)
        {
            // Remove a node from the nodes list, and also remove itself
            // from all the use lists on the operands that this node uses.
            block.Operations.Remove(node);

            for (int index = 0; index < node.SourcesCount; index++)
            {
                node.SetSource(index, default);
            }

            Debug.Assert(node.Destination == default || node.Destination.UsesCount == 0);

            node.Destination = default;
        }

        private static bool IsUnused(Operation node)
        {
            return DestIsSingleLocalVar(node) && node.Destination.UsesCount == 0 && !HasSideEffects(node);
        }

        private static bool DestIsSingleLocalVar(Operation node)
        {
            return node.DestinationsCount == 1 && node.Destination.Kind == OperandKind.LocalVariable;
        }

        private static bool HasSideEffects(Operation node)
        {
            return node.Instruction == Instruction.Call
                || node.Instruction == Instruction.Tailcall
                || node.Instruction == Instruction.CompareAndSwap
                || node.Instruction == Instruction.CompareAndSwap16
                || node.Instruction == Instruction.CompareAndSwap8;
        }

        private static bool IsPropagableCompare(Operation operation)
        {
            return operation.Instruction == Instruction.Compare;
        }

        private static bool IsPropagableCopy(Operation operation)
        {
            if (operation.Instruction != Instruction.Copy)
            {
                return false;
            }

            return operation.Destination.Type == operation.GetSource(0).Type;
        }
    }
}
