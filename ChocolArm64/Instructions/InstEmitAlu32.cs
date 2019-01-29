using ChocolArm64.Decoders;
using ChocolArm64.State;
using ChocolArm64.Translation;
using System.Reflection.Emit;

using static ChocolArm64.Instructions.InstEmit32Helper;
using static ChocolArm64.Instructions.InstEmitAluHelper;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit32
    {
        public static void Add(ILEmitterCtx context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            EmitAluLoadOpers(context, setCarry: false);

            context.Emit(OpCodes.Add);

            if (op.SetFlags)
            {
                context.EmitZnFlagCheck();

                EmitAddsCCheck(context);
                EmitAddsVCheck(context);
            }

            EmitAluStore(context);
        }

        public static void Cmp(ILEmitterCtx context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            EmitAluLoadOpers(context, setCarry: false);

            context.Emit(OpCodes.Sub);

            context.EmitZnFlagCheck();

            EmitSubsCCheck(context);
            EmitSubsVCheck(context);

            context.Emit(OpCodes.Pop);
        }

        public static void Mov(ILEmitterCtx context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            EmitAluLoadOper2(context);

            if (op.SetFlags)
            {
                context.EmitZnFlagCheck();
            }

            EmitAluStore(context);
        }

        public static void Sub(ILEmitterCtx context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            EmitAluLoadOpers(context, setCarry: false);

            context.Emit(OpCodes.Sub);

            if (op.SetFlags)
            {
                context.EmitZnFlagCheck();

                EmitSubsCCheck(context);
                EmitSubsVCheck(context);
            }

            EmitAluStore(context);
        }

        private static void EmitAluStore(ILEmitterCtx context)
        {
            IOpCode32Alu op = (IOpCode32Alu)context.CurrOp;

            if (op.Rd == RegisterAlias.Aarch32Pc)
            {
                if (op.SetFlags)
                {
                    //TODO: Load SPSR etc.

                    context.EmitLdflg((int)PState.TBit);

                    ILLabel lblThumb = new ILLabel();
                    ILLabel lblEnd   = new ILLabel();

                    context.Emit(OpCodes.Brtrue_S, lblThumb);

                    context.EmitLdc_I4(~3);

                    context.Emit(OpCodes.Br_S, lblEnd);

                    context.MarkLabel(lblThumb);

                    context.EmitLdc_I4(~1);

                    context.MarkLabel(lblEnd);

                    context.Emit(OpCodes.And);
                    context.Emit(OpCodes.Conv_U8);
                    context.Emit(OpCodes.Ret);
                }
                else
                {
                    EmitAluWritePc(context);
                }
            }
            else
            {
                context.EmitStint(GetRegisterAlias(context.Mode, op.Rd));
            }
        }

        private static void EmitAluWritePc(ILEmitterCtx context)
        {
            context.EmitStoreState();

            if (IsThumb(context.CurrOp))
            {
                context.EmitLdc_I4(~1);

                context.Emit(OpCodes.And);
                context.Emit(OpCodes.Conv_U8);
                context.Emit(OpCodes.Ret);
            }
            else
            {
                EmitBxWritePc(context);
            }
        }
    }
}