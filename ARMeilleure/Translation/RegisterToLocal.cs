using ARMeilleure.IntermediateRepresentation;
using System.Collections.Generic;

using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Translation
{
    static class RegisterToLocal
    {
        public static void Rename(ControlFlowGraph cfg)
        {
            Dictionary<Register, Operand> registerToLocalMap = new Dictionary<Register, Operand>();

            Operand GetLocal(Operand op)
            {
                Register register = op.GetRegister();

                if (!registerToLocalMap.TryGetValue(register, out Operand local))
                {
                    local = Local(op.Type);

                    registerToLocalMap.Add(register, local);
                }

                return local;
            }

            foreach (BasicBlock block in cfg.Blocks)
            {
                foreach (Node node in block.Operations)
                {
                    Operand dest = node.Destination;

                    if (dest != null && dest.Kind == OperandKind.Register)
                    {
                        node.Destination = GetLocal(dest);
                    }

                    for (int index = 0; index < node.SourcesCount; index++)
                    {
                        Operand source = node.GetSource(index);

                        if (source.Kind == OperandKind.Register)
                        {
                            node.SetSource(index, GetLocal(source));
                        }
                    }
                }
            }
        }
    }
}