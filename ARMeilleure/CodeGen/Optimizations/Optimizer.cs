using ARMeilleure.IntermediateRepresentation;
using ARMeilleure.Translation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

                        if (DestIsLocalVar(operation) && IsPropagableCopy(operation))
                        {
                            PropagateCopy(operation);

                            RemoveNode(block, node);

                            modified = true;
                        }

                        node = nextNode;
                    }
                }
            }
            while (modified);
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
            return (node is Operation operation) && operation.Instruction == Instruction.Call;
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