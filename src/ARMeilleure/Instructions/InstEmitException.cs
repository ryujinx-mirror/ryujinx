using ARMeilleure.Decoders;
using ARMeilleure.Translation;

using static ARMeilleure.IntermediateRepresentation.Operand.Factory;

namespace ARMeilleure.Instructions
{
    static partial class InstEmit
    {
        public static void Brk(ArmEmitterContext context)
        {
            OpCodeException op = (OpCodeException)context.CurrOp;

            string name = nameof(NativeInterface.Break);

            context.StoreToContext();

            context.Call(typeof(NativeInterface).GetMethod(name), Const(op.Address), Const(op.Id));

            context.LoadFromContext();

            context.Return(Const(op.Address));
        }

        public static void Svc(ArmEmitterContext context)
        {
            OpCodeException op = (OpCodeException)context.CurrOp;

            string name = nameof(NativeInterface.SupervisorCall);

            context.StoreToContext();

            context.Call(typeof(NativeInterface).GetMethod(name), Const(op.Address), Const(op.Id));

            context.LoadFromContext();

            Translator.EmitSynchronization(context);
        }

        public static void Und(ArmEmitterContext context)
        {
            OpCode op = context.CurrOp;

            string name = nameof(NativeInterface.Undefined);

            context.StoreToContext();

            context.Call(typeof(NativeInterface).GetMethod(name), Const(op.Address), Const(op.RawOpCode));

            context.LoadFromContext();

            context.Return(Const(op.Address));
        }
    }
}
