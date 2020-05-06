using Ryujinx.Graphics.Shader.Decoders;
using Ryujinx.Graphics.Shader.IntermediateRepresentation;
using Ryujinx.Graphics.Shader.Translation;

using static Ryujinx.Graphics.Shader.Instructions.InstEmitHelper;
using static Ryujinx.Graphics.Shader.IntermediateRepresentation.OperandHelper;

namespace Ryujinx.Graphics.Shader.Instructions
{
    static partial class InstEmit
    {
        public static void Vote(EmitterContext context)
        {
            OpCodeVote op = (OpCodeVote)context.CurrOp;

            Operand pred = GetPredicate39(context);

            Operand res = null;

            switch (op.VoteOp)
            {
                case VoteOp.All:
                    res = context.VoteAll(pred);
                    break;
                case VoteOp.Any:
                    res = context.VoteAny(pred);
                    break;
                case VoteOp.AllEqual:
                    res = context.VoteAllEqual(pred);
                    break;
            }

            if (res != null)
            {
                context.Copy(Register(op.Predicate45), res);
            }
            else
            {
                context.Config.GpuAccessor.Log($"Invalid vote operation: {op.VoteOp}.");
            }

            if (!op.Rd.IsRZ)
            {
                context.Copy(Register(op.Rd), context.Ballot(pred));
            }
        }
    }
}