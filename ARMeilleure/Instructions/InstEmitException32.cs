using ARMeilleure.Decoders;
using ARMeilleure.Translation;

using static ARMeilleure.IntermediateRepresentation.OperandHelper;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit32
    {
        public static void Svc(ArmEmitterContext context)
        {
            EmitExceptionCall(context, NativeInterface.SupervisorCall);
        }

        public static void Trap(ArmEmitterContext context)
        {
            EmitExceptionCall(context, NativeInterface.Break);
        }

        private static void EmitExceptionCall(ArmEmitterContext context, _Void_U64_S32 func)
        {
            OpCode32Exception op = (OpCode32Exception)context.CurrOp;

            context.StoreToContext();

            context.Call(func, Const(op.Address), Const(op.Id));

            context.LoadFromContext();

            if (context.CurrBlock.Next == null)
            {
                context.Return(Const(op.Address + 4));
            }
        }
    }
}
