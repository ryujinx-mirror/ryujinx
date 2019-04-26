using ChocolArm64.IntermediateRepresentation;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System.Reflection;
using System.Reflection.Emit;

namespace ChocolArm64.Instructions
{
    static class InstEmitFlowHelper
    {
        public static void EmitCall(ILEmitterCtx context, long imm)
        {
            if (context.Tier == TranslationTier.Tier0)
            {
                context.EmitStoreContext();

                context.TranslateAhead(imm);

                context.EmitLdc_I8(imm);

                context.Emit(OpCodes.Ret);

                return;
            }

            if (!context.TryOptEmitSubroutineCall())
            {
                context.HasSlowCall = true;

                context.EmitStoreContext();

                context.TranslateAhead(imm);

                context.EmitLdarg(TranslatedSub.StateArgIdx);

                context.EmitLdfld(typeof(CpuThreadState).GetField(nameof(CpuThreadState.CurrentTranslator),
                    BindingFlags.Instance |
                    BindingFlags.NonPublic));

                context.EmitLdarg(TranslatedSub.StateArgIdx);
                context.EmitLdc_I8(imm);
                context.EmitLdc_I4((int)CallType.Call);

                context.EmitPrivateCall(typeof(Translator), nameof(Translator.GetOrTranslateSubroutine));

                context.EmitLdarg(TranslatedSub.StateArgIdx);
                context.EmitLdarg(TranslatedSub.MemoryArgIdx);

                context.EmitCall(typeof(TranslatedSub), nameof(TranslatedSub.Execute));
            }

            EmitContinueOrReturnCheck(context);
        }

        public static void EmitVirtualCall(ILEmitterCtx context)
        {
            EmitVirtualCallOrJump(context, isJump: false);
        }

        public static void EmitVirtualJump(ILEmitterCtx context)
        {
            EmitVirtualCallOrJump(context, isJump: true);
        }

        private static void EmitVirtualCallOrJump(ILEmitterCtx context, bool isJump)
        {
            if (context.Tier == TranslationTier.Tier0)
            {
                context.Emit(OpCodes.Ret);
            }
            else
            {
                context.EmitSttmp();
                context.EmitLdarg(TranslatedSub.StateArgIdx);

                context.EmitLdfld(typeof(CpuThreadState).GetField(nameof(CpuThreadState.CurrentTranslator),
                    BindingFlags.Instance |
                    BindingFlags.NonPublic));

                context.EmitLdarg(TranslatedSub.StateArgIdx);
                context.EmitLdtmp();
                context.EmitLdc_I4(isJump
                    ? (int)CallType.VirtualJump
                    : (int)CallType.VirtualCall);

                context.EmitPrivateCall(typeof(Translator), nameof(Translator.GetOrTranslateSubroutine));

                context.EmitLdarg(TranslatedSub.StateArgIdx);
                context.EmitLdarg(TranslatedSub.MemoryArgIdx);

                if (isJump)
                {
                    //The tail prefix allows the JIT to jump to the next function,
                    //while releasing the stack space used by the current one.
                    //This is ideal for BR ARM instructions, which are
                    //basically indirect tail calls.
                    context.Emit(OpCodes.Tailcall);
                }

                MethodInfo mthdInfo = typeof(ArmSubroutine).GetMethod("Invoke");

                context.EmitCall(mthdInfo, isVirtual: true);

                if (!isJump)
                {
                    EmitContinueOrReturnCheck(context);
                }
                else
                {
                    context.Emit(OpCodes.Ret);
                }
            }
        }

        private static void EmitContinueOrReturnCheck(ILEmitterCtx context)
        {
            //Note: The return value of the called method will be placed
            //at the Stack, the return value is always a Int64 with the
            //return address of the function. We check if the address is
            //correct, if it isn't we keep returning until we reach the dispatcher.
            if (context.CurrBlock.Next != null)
            {
                context.Emit(OpCodes.Dup);

                context.EmitLdc_I8(context.CurrOp.Position + 4);

                ILLabel lblContinue = new ILLabel();

                context.Emit(OpCodes.Beq_S, lblContinue);
                context.Emit(OpCodes.Ret);

                context.MarkLabel(lblContinue);

                context.Emit(OpCodes.Pop);

                context.EmitLoadContext();
            }
            else
            {
                context.Emit(OpCodes.Ret);
            }
        }
    }
}
