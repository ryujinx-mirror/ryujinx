using ARMeilleure.Decoders;
using ARMeilleure.Translation;
using System;

using static ARMeilleure.Instructions.InstEmitFlowHelper;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
        public static void Brk(ArmEmitterContext context)
        {
            EmitExceptionCall(context, NativeInterface.Break);
        }

        public static void Svc(ArmEmitterContext context)
        {
            EmitExceptionCall(context, NativeInterface.SupervisorCall);
        }

        private static void EmitExceptionCall(ArmEmitterContext context, _Void_U64_S32 func)
        {
            OpCodeException op = (OpCodeException)context.CurrOp;

            context.StoreToContext();

            context.Call(func, Const(op.Address), Const(op.Id));

            context.LoadFromContext();

            if (context.CurrBlock.Next == null)
            {
                EmitTailContinue(context, Const(op.Address + 4));
            }
        }

        public static void Und(ArmEmitterContext context)
        {
            OpCode op = context.CurrOp;

            Delegate dlg = new _Void_U64_S32(NativeInterface.Undefined);

            context.StoreToContext();

            context.Call(dlg, Const(op.Address), Const(op.RawOpCode));

            context.LoadFromContext();

            if (context.CurrBlock.Next == null)
            {
                EmitTailContinue(context, Const(op.Address + 4));
            }
        }
    }
}