using ChocolArm64.Decoders;
using ChocolArm64.Translation;
using System.Reflection.Emit;

using static ChocolArm64.Instructions.InstEmitMemoryHelper;

namespace ChocolArm64.Instructions
{
    static partial class InstEmit
    {
        public static void Adr(ILEmitterCtx context)
        {
            OpCodeAdr64 op = (OpCodeAdr64)context.CurrOp;

            context.EmitLdc_I(op.Position + op.Imm);
            context.EmitStintzr(op.Rd);
        }

        public static void Adrp(ILEmitterCtx context)
        {
            OpCodeAdr64 op = (OpCodeAdr64)context.CurrOp;

            context.EmitLdc_I((op.Position & ~0xfffL) + (op.Imm << 12));
            context.EmitStintzr(op.Rd);
        }

        public static void Ldr(ILEmitterCtx context)  => EmitLdr(context, false);
        public static void Ldrs(ILEmitterCtx context) => EmitLdr(context, true);

        private static void EmitLdr(ILEmitterCtx context, bool signed)
        {
            OpCodeMem64 op = (OpCodeMem64)context.CurrOp;

            context.EmitLdarg(TranslatedSub.MemoryArgIdx);

            EmitLoadAddress(context);

            if (signed && op.Extend64)
            {
                EmitReadSx64Call(context, op.Size);
            }
            else if (signed)
            {
                EmitReadSx32Call(context, op.Size);
            }
            else
            {
                EmitReadZxCall(context, op.Size);
            }

            if (op is IOpCodeSimd64)
            {
                context.EmitStvec(op.Rt);
            }
            else
            {
                context.EmitStintzr(op.Rt);
            }

            EmitWBackIfNeeded(context);
        }

        public static void Ldr_Literal(ILEmitterCtx context)
        {
            IOpCodeLit64 op = (IOpCodeLit64)context.CurrOp;

            if (op.Prefetch)
            {
                return;
            }

            context.EmitLdarg(TranslatedSub.MemoryArgIdx);
            context.EmitLdc_I8(op.Imm);

            if (op.Signed)
            {
                EmitReadSx64Call(context, op.Size);
            }
            else
            {
                EmitReadZxCall(context, op.Size);
            }

            if (op is IOpCodeSimd64)
            {
                context.EmitStvec(op.Rt);
            }
            else
            {
                context.EmitStint(op.Rt);
            }
        }

        public static void Ldp(ILEmitterCtx context)
        {
            OpCodeMemPair64 op = (OpCodeMemPair64)context.CurrOp;

            void EmitReadAndStore(int rt)
            {
                if (op.Extend64)
                {
                    EmitReadSx64Call(context, op.Size);
                }
                else
                {
                    EmitReadZxCall(context, op.Size);
                }

                if (op is IOpCodeSimd64)
                {
                    context.EmitStvec(rt);
                }
                else
                {
                    context.EmitStintzr(rt);
                }
            }

            context.EmitLdarg(TranslatedSub.MemoryArgIdx);

            EmitLoadAddress(context);

            EmitReadAndStore(op.Rt);

            context.EmitLdarg(TranslatedSub.MemoryArgIdx);
            context.EmitLdtmp();
            context.EmitLdc_I8(1 << op.Size);

            context.Emit(OpCodes.Add);

            EmitReadAndStore(op.Rt2);

            EmitWBackIfNeeded(context);
        }

        public static void Str(ILEmitterCtx context)
        {
            OpCodeMem64 op = (OpCodeMem64)context.CurrOp;

            context.EmitLdarg(TranslatedSub.MemoryArgIdx);

            EmitLoadAddress(context);

            if (op is IOpCodeSimd64)
            {
                context.EmitLdvec(op.Rt);
            }
            else
            {
                context.EmitLdintzr(op.Rt);
            }

            EmitWriteCall(context, op.Size);

            EmitWBackIfNeeded(context);
        }

        public static void Stp(ILEmitterCtx context)
        {
            OpCodeMemPair64 op = (OpCodeMemPair64)context.CurrOp;

            context.EmitLdarg(TranslatedSub.MemoryArgIdx);

            EmitLoadAddress(context);

            if (op is IOpCodeSimd64)
            {
                context.EmitLdvec(op.Rt);
            }
            else
            {
                context.EmitLdintzr(op.Rt);
            }

            EmitWriteCall(context, op.Size);

            context.EmitLdarg(TranslatedSub.MemoryArgIdx);
            context.EmitLdtmp();
            context.EmitLdc_I8(1 << op.Size);

            context.Emit(OpCodes.Add);

            if (op is IOpCodeSimd64)
            {
                context.EmitLdvec(op.Rt2);
            }
            else
            {
                context.EmitLdintzr(op.Rt2);
            }

            EmitWriteCall(context, op.Size);

            EmitWBackIfNeeded(context);
        }

        private static void EmitLoadAddress(ILEmitterCtx context)
        {
            switch (context.CurrOp)
            {
                case OpCodeMemImm64 op:
                    context.EmitLdint(op.Rn);

                    if (!op.PostIdx)
                    {
                        //Pre-indexing.
                        context.EmitLdc_I(op.Imm);

                        context.Emit(OpCodes.Add);
                    }
                    break;

                case OpCodeMemReg64 op:
                    context.EmitLdint(op.Rn);
                    context.EmitLdintzr(op.Rm);
                    context.EmitCast(op.IntType);

                    if (op.Shift)
                    {
                        context.EmitLsl(op.Size);
                    }

                    context.Emit(OpCodes.Add);
                    break;
            }

            //Save address to Scratch var since the register value may change.
            context.Emit(OpCodes.Dup);

            context.EmitSttmp();
        }

        private static void EmitWBackIfNeeded(ILEmitterCtx context)
        {
            //Check whenever the current OpCode has post-indexed write back, if so write it.
            //Note: AOpCodeMemPair inherits from AOpCodeMemImm, so this works for both.
            if (context.CurrOp is OpCodeMemImm64 op && op.WBack)
            {
                context.EmitLdtmp();

                if (op.PostIdx)
                {
                    context.EmitLdc_I(op.Imm);

                    context.Emit(OpCodes.Add);
                }

                context.EmitStint(op.Rn);
            }
        }
    }
}