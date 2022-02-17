using ARMeilleure.Decoders;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitFlowHelper;
using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit32
    {
        public static void Svc(ArmEmitterContext context)
        {
            EmitExceptionCall(context, nameof(NativeInterface.SupervisorCall));
        }

        public static void Trap(ArmEmitterContext context)
        {
            EmitExceptionCall(context, nameof(NativeInterface.Break));
        }

        private static void EmitExceptionCall(ArmEmitterContext context, string name)
        {
            IOpCode32Exception op = (IOpCode32Exception)context.CurrOp;

            context.StoreToContext();

            context.Call(typeof(NativeInterface).GetMethod(name), Const(((IOpCode)op).Address), Const(op.Id));

            context.LoadFromContext();

            Translator.EmitSynchronization(context);
        }
    }
}
