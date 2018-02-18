using ChocolArm64.Decoder;
using ChocolArm64.Instruction;
using ChocolArm64.Translation;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ChocolArm64
{
    class ATranslator
    {
        public AThread Thread { get; private set; }

        private Dictionary<long, ATranslatedSub> CachedSubs;

        private bool KeepRunning;

        public ATranslator(AThread Parent)
        {
            this.Thread = Parent;

            CachedSubs = new Dictionary<long, ATranslatedSub>();

            KeepRunning = true;
        }

        public void StopExecution() => KeepRunning = false;

        public void ExecuteSubroutine(long Position)
        {
            do
            {
                if (CachedSubs.TryGetValue(Position, out ATranslatedSub Sub) && !Sub.NeedsReJit)
                {
                    Position = Sub.Execute(Thread.Registers, Thread.Memory);
                }
                else
                {
                    Position = TranslateSubroutine(Position).Execute(Thread.Registers, Thread.Memory);
                }
            }
            while (Position != 0 && KeepRunning);
        }

        public bool TryGetCachedSub(AOpCode OpCode, out ATranslatedSub Sub)
        {
            if (OpCode.Emitter != AInstEmit.Bl)
            {
                Sub = null;

                return false;
            }

            return TryGetCachedSub(((AOpCodeBImmAl)OpCode).Imm, out Sub);
        }

        public bool TryGetCachedSub(long Position, out ATranslatedSub Sub)
        {
            return CachedSubs.TryGetValue(Position, out Sub);
        }

        public bool HasCachedSub(long Position)
        {
            return CachedSubs.ContainsKey(Position);
        }

        private ATranslatedSub TranslateSubroutine(long Position)
        {
            (ABlock[] Graph, ABlock Root) Cfg = ADecoder.DecodeSubroutine(this, Position);

            AILEmitterCtx Context = new AILEmitterCtx(
                this,
                Cfg.Graph,
                Cfg.Root);

            if (Context.CurrBlock.Position != Position)
            {
                Context.Emit(OpCodes.Br, Context.GetLabel(Position));
            }

            do
            {
                Context.EmitOpCode();
            }
            while (Context.AdvanceOpCode());

            //Mark all methods that calls this method for ReJiting,
            //since we can now call it directly which is faster.
            foreach (ATranslatedSub TS in CachedSubs.Values)
            {
                if (TS.SubCalls.Contains(Position))
                {
                    TS.MarkForReJit();
                }
            }

            ATranslatedSub Subroutine = Context.GetSubroutine();

            if (!CachedSubs.TryAdd(Position, Subroutine))
            {
                CachedSubs[Position] = Subroutine;
            }

            return Subroutine;
        }
    }
}