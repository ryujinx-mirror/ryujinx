using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System.Diagnostics;

using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.CodeGen.Optimizations
{
    static class Optimizer
    {
        public static void RunPass(ControlFlowGraph cfg)
        {
            bool modified;

            do
            {
                modified = false;

                for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
                {
                    Node node = block.Operations.First;

                    while (node != null)
                    {
                        Node nextNode = node.ListNext;

                        bool isUnused = IsUnused(node);

                        if (!(node is Operation operation) || isUnused)
                        {
                            if (isUnused)
                            {
                                RemoveNode(block, node);

                                modified = true;
                            }

                            node = nextNode;

                            continue;
                        }

                        ConstantFolding.RunPass(operation);

                        Simplification.RunPass(operation);

                        if (DestIsLocalVar(operation))
                        {   
                            if (IsPropagableCompare(operation))
                            {
                                modified |= PropagateCompare(operation);

                                if (modified && IsUnused(operation))
                                {
                                    RemoveNode(block, node);
                                }
                            }
                            else if (IsPropagableCopy(operation))
                            {
                                PropagateCopy(operation);

                                RemoveNode(block, node);

                                modified = true;
                            }
                        }

                        node = nextNode;
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

                for (BasicBlock block = cfg.Blocks.First; block != null; block = block.ListNext)
                {
                    Node node = block.Operations.First;

                    while (node != null)
                    {
                        Node nextNode = node.ListNext;

                        if (IsUnused(node))
                        {
                            RemoveNode(block, node);

                            modified = true;
                        }

                        node = nextNode;
                    }
                }
            }
            while (modified);
        }

        private static bool PropagateCompare(Operation compOp)
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

            Node[] uses = dest.Uses.ToArray();

            foreach (Node use in uses)
            {
                if (!(use is Operation operation))
                {
                    continue;
                }

                // If operation is a BranchIf and has a constant value 0 in its RHS or LHS source operands.
                if (IsZeroBranch(operation, out Comparison otherCompType))
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

                    operation.SetSource(0, src1);
                    operation.SetSource(1, src2);
                    operation.SetSource(2, Const((int)propCompType));

                    modified = true;
                }
            }

            return modified;
        }

        private static void PropagateCopy(Operation copyOp)
        {
            // Propagate copy source operand to all uses of the destination operand.
            Operand dest   = copyOp.Destination;
            Operand source = copyOp.GetSource(0);

            Node[] uses = dest.Uses.ToArray();

            foreach (Node use in uses)
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

        private static void RemoveNode(BasicBlock block, Node node)
        {
            // Remove a node from the nodes list, and also remove itself
            // from all the use lists on the operands that this node uses.
            block.Operations.Remove(node);

            for (int index = 0; index < node.SourcesCount; index++)
            {
                node.SetSource(index, null);
            }

            Debug.Assert(node.Destination == null || node.Destination.Uses.Count == 0);

            node.Destination = null;
        }

        private static bool IsUnused(Node node)
        {
            return DestIsLocalVar(node) && node.Destination.Uses.Count == 0 && !HasSideEffects(node);
        }

        private static bool DestIsLocalVar(Node node)
        {
            return node.Destination != null && node.Destination.Kind == OperandKind.LocalVariable;
        }

        private static bool HasSideEffects(Node node)
        {
            return (node is Operation operation) && (operation.Instruction == Instruction.Call
                || operation.Instruction == Instruction.Tailcall
                || operation.Instruction == Instruction.CompareAndSwap
                || operation.Instruction == Instruction.CompareAndSwap16
                || operation.Instruction == Instruction.CompareAndSwap8);
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