using ChocolArm64.Decoders;
using ChocolArm64.IntermediateRepresentation;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System.Reflection.Emit;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        public static void Brk(ILEmitterCtx context)
        {
            EmitExceptionCall(context, nameof(CpuThreadState.OnBreak));
        }

        public static void Svc(ILEmitterCtx context)
        {
            EmitExceptionCall(context, nameof(CpuThreadState.OnSvcCall));
        }

        private static void EmitExceptionCall(ILEmitterCtx context, string mthdName)
        {
            OpCodeException64 op = (OpCodeException64)context.CurrOp;

            context.EmitStoreContext();

            context.EmitLdarg(TranslatedSub.StateArgIdx);

            context.EmitLdc_I8(op.Position);
            context.EmitLdc_I4(op.Id);

            context.EmitPrivateCall(typeof(CpuThreadState), mthdName);

            // Check if the thread should still be running, if it isn't then we return 0
            // to force a return to the dispatcher and then exit the thread.
            context.EmitLdarg(TranslatedSub.StateArgIdx);

            context.EmitCallPropGet(typeof(CpuThreadState), nameof(CpuThreadState.Running));

            ILLabel lblEnd = new ILLabel();

            context.Emit(OpCodes.Brtrue_S, lblEnd);

            context.EmitLdc_I8(0);

            context.Emit(OpCodes.Ret);

            context.MarkLabel(lblEnd);

            if (context.CurrBlock.Next != null)
            {
                context.EmitLoadContext();
            }
            else
            {
                context.EmitLdc_I8(op.Position + 4);

                context.Emit(OpCodes.Ret);
            }
        }

        public static void Und(ILEmitterCtx context)
        {
            OpCode64 op = context.CurrOp;

            context.EmitStoreContext();

            context.EmitLdarg(TranslatedSub.StateArgIdx);

            context.EmitLdc_I8(op.Position);
            context.EmitLdc_I4(op.RawOpCode);

            context.EmitPrivateCall(typeof(CpuThreadState), nameof(CpuThreadState.OnUndefined));

            if (context.CurrBlock.Next != null)
            {
                context.EmitLoadContext();
            }
            else
            {
                context.EmitLdc_I8(op.Position + 4);

                context.Emit(OpCodes.Ret);
            }
        }
    }
}