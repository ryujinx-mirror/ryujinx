using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;
using System.Collections.Generic;
using System.Linq;

using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Bra(EmitterContext context)
        {
            EmitBranch(context, context.CurrBlock.Branch.Address);
        }

        public static void Exit(EmitterContext context)
        {
            OpCodeExit op = (OpCodeExit)context.CurrOp;

            //TODO: Figure out how this is supposed to work in the
            //presence of other condition codes.
            if (op.Condition == Condition.Always)
            {
                context.Return();
            }
        }

        public static void Kil(EmitterContext context)
        {
            context.Discard();
        }

        public static void Ssy(EmitterContext context)
        {
            OpCodeSsy op = (OpCodeSsy)context.CurrOp;

            foreach (KeyValuePair<OpCodeSync, Operand> kv in op.Syncs)
            {
                OpCodeSync opSync = kv.Key;

                Operand local = kv.Value;

                int ssyIndex = opSync.Targets[op];

                context.Copy(local, Const(ssyIndex));
            }
        }

        public static void Sync(EmitterContext context)
        {
            OpCodeSync op = (OpCodeSync)context.CurrOp;

            if (op.Targets.Count == 1)
            {
                //If we have only one target, then the SSY is basically
                //a branch, we can produce better codegen for this case.
                OpCodeSsy opSsy = op.Targets.Keys.First();

                EmitBranch(context, opSsy.GetAbsoluteAddress());
            }
            else
            {
                foreach (KeyValuePair<OpCodeSsy, int> kv in op.Targets)
                {
                    OpCodeSsy opSsy = kv.Key;

                    Operand label = context.GetLabel(opSsy.GetAbsoluteAddress());

                    Operand local = opSsy.Syncs[op];

                    int ssyIndex = kv.Value;

                    context.BranchIfTrue(label, context.ICompareEqual(local, Const(ssyIndex)));
                }
            }
        }

        private static void EmitBranch(EmitterContext context, ulong address)
        {
            //If we're branching to the next instruction, then the branch
            //is useless and we can ignore it.
            if (address == context.CurrOp.Address + 8)
            {
                return;
            }

            Operand label = context.GetLabel(address);

            Operand pred = Register(context.CurrOp.Predicate);

            if (context.CurrOp.Predicate.IsPT)
            {
                context.Branch(label);
            }
            else if (context.CurrOp.InvertPredicate)
            {
                context.BranchIfFalse(label, pred);
            }
            else
            {
                context.BranchIfTrue(label, pred);
            }
        }
    }
}