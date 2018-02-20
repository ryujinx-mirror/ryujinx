using ChocolArm64.Decoder;
using ChocolArm64.Translation;
using System.Reflection.Emit;

using static ChocolArm64.Instruction.AInstEmitMemoryHelper;

namespace ChocolArm64.Instruction
{
    static partial class AInstEmit
    {
        public static void Adr(AILEmitterCtx Context)
        {
            AOpCodeAdr Op = (AOpCodeAdr)Context.CurrOp;

            Context.EmitLdc_I(Op.Position + Op.Imm);
            Context.EmitStintzr(Op.Rd);
        }

        public static void Adrp(AILEmitterCtx Context)
        {
            AOpCodeAdr Op = (AOpCodeAdr)Context.CurrOp;

            Context.EmitLdc_I((Op.Position & ~0xfffL) + (Op.Imm << 12));
            Context.EmitStintzr(Op.Rd);
        }

        public static void Ldr(AILEmitterCtx Context)  => EmitLdr(Context, false);
        public static void Ldrs(AILEmitterCtx Context) => EmitLdr(Context, true);

        public static void EmitLdr(AILEmitterCtx Context, bool Signed)
        {
            AOpCodeMem Op = (AOpCodeMem)Context.CurrOp;

            Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);

            EmitLoadAddress(Context);

            if (Signed && Op.Extend64)
            {
                EmitReadSx64Call(Context, Op.Size);
            }
            else if (Signed)
            {
                EmitReadSx32Call(Context, Op.Size);
            }
            else
            {
                EmitReadZxCall(Context, Op.Size);
            }

            if (Op is IAOpCodeSimd)
            {
                Context.EmitStvec(Op.Rt);
            }
            else
            {
                Context.EmitStintzr(Op.Rt);
            }

            EmitWBackIfNeeded(Context);
        }

        public static void LdrLit(AILEmitterCtx Context)
        {
            IAOpCodeLit Op = (IAOpCodeLit)Context.CurrOp;

            if (Op.Prefetch)
            {
                return;
            }

            Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
            Context.EmitLdc_I8(Op.Imm);

            if (Op.Signed)
            {
                EmitReadSx64Call(Context, Op.Size);
            }
            else
            {
                EmitReadZxCall(Context, Op.Size);
            }

            if (Op is IAOpCodeSimd)
            {
                Context.EmitStvec(Op.Rt);
            }
            else
            {
                Context.EmitStint(Op.Rt);
            }
        }

        public static void Ldp(AILEmitterCtx Context)
        {
            AOpCodeMemPair Op = (AOpCodeMemPair)Context.CurrOp;

            void EmitReadAndStore(int Rt)
            {
                if (Op.Extend64)
                {
                    EmitReadSx64Call(Context, Op.Size);
                }
                else
                {
                    EmitReadZxCall(Context, Op.Size);
                }

                if (Op is IAOpCodeSimd)
                {
                    Context.EmitStvec(Rt);
                }
                else
                {
                    Context.EmitStintzr(Rt);
                }
            }

            Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);

            EmitLoadAddress(Context);

            EmitReadAndStore(Op.Rt);

            Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
            Context.EmitLdtmp();
            Context.EmitLdc_I8(1 << Op.Size);

            Context.Emit(OpCodes.Add);

            EmitReadAndStore(Op.Rt2);

            EmitWBackIfNeeded(Context);
        }        

        public static void Str(AILEmitterCtx Context)
        {
            AOpCodeMem Op = (AOpCodeMem)Context.CurrOp;

            Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);

            EmitLoadAddress(Context);

            if (Op is IAOpCodeSimd)
            {
                Context.EmitLdvec(Op.Rt);
            }
            else
            {
                Context.EmitLdintzr(Op.Rt);
            }

            EmitWriteCall(Context, Op.Size);

            EmitWBackIfNeeded(Context);
        }

        public static void Stp(AILEmitterCtx Context)
        {
            AOpCodeMemPair Op = (AOpCodeMemPair)Context.CurrOp;

            Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);

            EmitLoadAddress(Context);

            if (Op is IAOpCodeSimd)
            {
                Context.EmitLdvec(Op.Rt);
            }
            else
            {
                Context.EmitLdintzr(Op.Rt);
            }

            EmitWriteCall(Context, Op.Size);

            Context.EmitLdarg(ATranslatedSub.MemoryArgIdx);
            Context.EmitLdtmp();
            Context.EmitLdc_I8(1 << Op.Size);

            Context.Emit(OpCodes.Add);

            if (Op is IAOpCodeSimd)
            {
                Context.EmitLdvec(Op.Rt2);
            }
            else
            {
                Context.EmitLdintzr(Op.Rt2);
            }

            EmitWriteCall(Context, Op.Size);

            EmitWBackIfNeeded(Context);
        }

        private static void EmitLoadAddress(AILEmitterCtx Context)
        {
            switch (Context.CurrOp)
            {
                case AOpCodeMemImm Op:
                    Context.EmitLdint(Op.Rn);

                    if (!Op.PostIdx)
                    {
                        //Pre-indexing.
                        Context.EmitLdc_I(Op.Imm);

                        Context.Emit(OpCodes.Add);
                    }
                    break;

                case AOpCodeMemReg Op:
                    Context.EmitLdint(Op.Rn);
                    Context.EmitLdintzr(Op.Rm);
                    Context.EmitCast(Op.IntType);

                    if (Op.Shift)
                    {
                        Context.EmitLsl(Op.Size);
                    }

                    Context.Emit(OpCodes.Add);
                    break;
            }

            //Save address to Scratch var since the register value may change.
            Context.Emit(OpCodes.Dup);

            Context.EmitSttmp();
        }

        private static void EmitWBackIfNeeded(AILEmitterCtx Context)
        {
            //Check whenever the current OpCode has post-indexed write back, if so write it.
            //Note: AOpCodeMemPair inherits from AOpCodeMemImm, so this works for both.
            if (Context.CurrOp is AOpCodeMemImm Op && Op.WBack)
            {
                Context.EmitLdtmp();

                if (Op.PostIdx)
                {
                    Context.EmitLdc_I(Op.Imm);

                    Context.Emit(OpCodes.Add);
                }

                Context.EmitStint(Op.Rn);
            }
        }
    }
}