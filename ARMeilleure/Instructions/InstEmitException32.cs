using ARMeilleure.Decoders;
using ARMeilleure.Translation;

using static ARMeilleure.Instructions.InstEmitFlowHelper;
using static ARMeilleure.IntermediateRepresentation.OperandHelper;

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
            OpCode32Exception op = (OpCode32Exception)context.CurrOp;

            context.StoreToContext();

            context.Call(typeof(NativeInterface).GetMethod(name), Const(op.Address), Const(op.Id));

            context.LoadFromContext();

            Translator.EmitSynchronization(context);
        }
    }
}
