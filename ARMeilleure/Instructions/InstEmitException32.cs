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
            IOpCode32Exception op = (IOpCode32Exception)context.CurrOp;

            string name = nameof(NativeInterface.SupervisorCall);

            context.StoreToContext();

            context.Call(typeof(NativeInterface).GetMethod(name), Const(((IOpCode)op).Address), Const(op.Id));

            context.LoadFromContext();

            Translator.EmitSynchronization(context);
        }

        public static void Trap(ArmEmitterContext context)
        {
            IOpCode32Exception op = (IOpCode32Exception)context.CurrOp;

            string name = nameof(NativeInterface.Break);

            context.StoreToContext();

            context.Call(typeof(NativeInterface).GetMethod(name), Const(((IOpCode)op).Address), Const(op.Id));

            context.LoadFromContext();

            context.Return(Const(context.CurrOp.Address));
        }
    }
}
