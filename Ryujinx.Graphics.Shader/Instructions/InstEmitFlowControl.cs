using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;
using System.Collections.Generic;
using System.Linq;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Bra(EmitterContext context)
        {
            InstBra op = context.GetOp<InstBra>();

            EmitBranch(context, context.CurrBlock.Successors[^1].Address);
        }

        public static void Brk(EmitterContext context)
        {
            InstBrk op = context.GetOp<InstBrk>();

            EmitBrkOrSync(context);
        }

        public static void Brx(EmitterContext context)
        {
            InstBrx op = context.GetOp<InstBrx>();
            InstOp currOp = context.CurrOp;
            int startIndex = context.CurrBlock.HasNext() ? 1 : 0;

            if (context.CurrBlock.Successors.Count <= startIndex)
            {
                context.Config.GpuAccessor.Log($"Failed to find targets for BRX instruction at 0x{currOp.Address:X}.");
                return;
            }

            int offset = (int)currOp.GetAbsoluteAddress();

            Operand address = context.IAdd(Register(op.SrcA, RegisterType.Gpr), Const(offset));

            // Sorting the target addresses in descending order improves the code,
            // since it will always check the most distant targets first, then the
            // near ones. This can be easily transformed into if/else statements.
            var sortedTargets = context.CurrBlock.Successors.Skip(startIndex).OrderByDescending(x => x.Address);

            Block lastTarget = sortedTargets.LastOrDefault();

            foreach (Block possibleTarget in sortedTargets)
            {
                Operand label = context.GetLabel(possibleTarget.Address);

                if (possibleTarget != lastTarget)
                {
                    context.BranchIfTrue(label, context.ICompareEqual(address, Const((int)possibleTarget.Address)));
                }
                else
                {
                    context.Branch(label);
                }
            }
        }

        public static void Cal(EmitterContext context)
        {
            InstCal op = context.GetOp<InstCal>();

            DecodedFunction function = context.Program.GetFunctionByAddress(context.CurrOp.GetAbsoluteAddress());

            if (function.IsCompilerGenerated)
            {
                switch (function.Type)
                {
                    case FunctionType.BuiltInFSIBegin:
                        context.FSIBegin();
                        break;
                    case FunctionType.BuiltInFSIEnd:
                        context.FSIEnd();
                        break;
                }
            }
            else
            {
                context.Call(function.Id, false);
            }
        }

        public static void Exit(EmitterContext context)
        {
            InstExit op = context.GetOp<InstExit>();

            if (context.IsNonMain)
            {
                context.Config.GpuAccessor.Log("Invalid exit on non-main function.");
                return;
            }

            // TODO: Figure out how this is supposed to work in the
            // presence of other condition codes.
            if (op.Ccc == Ccc.T)
            {
                context.Return();
            }
        }

        public static void Kil(EmitterContext context)
        {
            InstKil op = context.GetOp<InstKil>();

            context.Discard();
        }

        public static void Pbk(EmitterContext context)
        {
            InstPbk op = context.GetOp<InstPbk>();

            EmitPbkOrSsy(context);
        }

        public static void Ret(EmitterContext context)
        {
            InstRet op = context.GetOp<InstRet>();

            if (context.IsNonMain)
            {
                context.Return();
            }
            else
            {
                context.Config.GpuAccessor.Log("Invalid return on main function.");
            }
        }

        public static void Ssy(EmitterContext context)
        {
            InstSsy op = context.GetOp<InstSsy>();

            EmitPbkOrSsy(context);
        }

        public static void Sync(EmitterContext context)
        {
            InstSync op = context.GetOp<InstSync>();

            EmitBrkOrSync(context);
        }

        private static void EmitPbkOrSsy(EmitterContext context)
        {
            var consumers = context.CurrBlock.PushOpCodes.First(x => x.Op.Address == context.CurrOp.Address).Consumers;

            foreach (KeyValuePair<Block, Operand> kv in consumers)
            {
                Block consumerBlock = kv.Key;
                Operand local = kv.Value;

                int id = consumerBlock.SyncTargets[context.CurrOp.Address].PushOpId;

                context.Copy(local, Const(id));
            }
        }

        private static void EmitBrkOrSync(EmitterContext context)
        {
            var targets = context.CurrBlock.SyncTargets;

            if (targets.Count == 1)
            {
                // If we have only one target, then the SSY/PBK is basically
                // a branch, we can produce better codegen for this case.
                EmitBranch(context, targets.Values.First().PushOpInfo.Op.GetAbsoluteAddress());
            }
            else
            {
                // TODO: Support CC here aswell (condition).
                foreach (SyncTarget target in targets.Values)
                {
                    PushOpInfo pushOpInfo = target.PushOpInfo;

                    Operand label = context.GetLabel(pushOpInfo.Op.GetAbsoluteAddress());
                    Operand local = pushOpInfo.Consumers[context.CurrBlock];

                    context.BranchIfTrue(label, context.ICompareEqual(local, Const(target.PushOpId)));
                }
            }
        }

        private static void EmitBranch(EmitterContext context, ulong address)
        {
            InstOp op = context.CurrOp;
            InstConditional opCond = new InstConditional(op.RawOpCode);

            // If we're branching to the next instruction, then the branch
            // is useless and we can ignore it.
            if (address == op.Address + 8)
            {
                return;
            }

            Operand label = context.GetLabel(address);

            Operand pred = Register(opCond.Pred, RegisterType.Predicate);

            if (opCond.Ccc != Ccc.T)
            {
                Operand cond = GetCondition(context, opCond.Ccc);

                if (opCond.Pred == RegisterConsts.PredicateTrueIndex)
                {
                    pred = cond;
                }
                else if (opCond.PredInv)
                {
                    pred = context.BitwiseAnd(context.BitwiseNot(pred), cond);
                }
                else
                {
                    pred = context.BitwiseAnd(pred, cond);
                }

                context.BranchIfTrue(label, pred);
            }
            else if (opCond.Pred == RegisterConsts.PredicateTrueIndex)
            {
                context.Branch(label);
            }
            else if (opCond.PredInv)
            {
                context.BranchIfFalse(label, pred);
            }
            else
            {
                context.BranchIfTrue(label, pred);
            }
        }

        private static Operand GetCondition(EmitterContext context, Ccc cond)
        {
            // TODO: More condition codes, figure out how they work.
            switch (cond)
            {
                case Ccc.Eq:
                case Ccc.Equ:
                    return GetZF();
                case Ccc.Ne:
                case Ccc.Neu:
                    return context.BitwiseNot(GetZF());
            }

            return Const(IrConsts.True);
        }
    }
}