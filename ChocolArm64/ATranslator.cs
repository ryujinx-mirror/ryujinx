using ChocolArm64.Decoder;
using ChocolArm64.Events;
using ChocolArm64.Instruction;
using ChocolArm64.Memory;
using ChocolArm64.Translation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ChocolArm64
{
    public class ATranslator
    {
        private ConcurrentDictionary<long, ATranslatedSub> CachedSubs;

        private ConcurrentDictionary<long, string> SymbolTable;

        public event EventHandler<ACpuTraceEventArgs> CpuTrace;

        public bool EnableCpuTrace { get; set; }

        private bool KeepRunning;

        public ATranslator(IReadOnlyDictionary<long, string> SymbolTable = null)
        {
            CachedSubs = new ConcurrentDictionary<long, ATranslatedSub>();

            if (SymbolTable != null)
            {
                this.SymbolTable = new ConcurrentDictionary<long, string>(SymbolTable);
            }
            else
            {
                this.SymbolTable = new ConcurrentDictionary<long, string>();
            }

            KeepRunning = true;
        }

        public void StopExecution() => KeepRunning = false;

        public void ExecuteSubroutine(AThread Thread, long Position)
        {
            do
            {
                if (EnableCpuTrace)
                {
                    if (!SymbolTable.TryGetValue(Position, out string SubName))
                    {
                        SubName = string.Empty;
                    }

                    CpuTrace?.Invoke(this, new ACpuTraceEventArgs(Position, SubName));
                }

                if (!CachedSubs.TryGetValue(Position, out ATranslatedSub Sub) || Sub.NeedsReJit)
                {
                    Sub = TranslateSubroutine(Thread.Memory, Position);
                }

                Position = Sub.Execute(Thread.ThreadState, Thread.Memory);
            }
            while (Position != 0 && KeepRunning);
        }

        internal bool TryGetCachedSub(AOpCode OpCode, out ATranslatedSub Sub)
        {
            if (OpCode.Emitter != AInstEmit.Bl)
            {
                Sub = null;

                return false;
            }

            return TryGetCachedSub(((AOpCodeBImmAl)OpCode).Imm, out Sub);
        }

        internal bool TryGetCachedSub(long Position, out ATranslatedSub Sub)
        {
            return CachedSubs.TryGetValue(Position, out Sub);
        }

        internal bool HasCachedSub(long Position)
        {
            return CachedSubs.ContainsKey(Position);
        }

        private ATranslatedSub TranslateSubroutine(AMemory Memory, long Position)
        {
            (ABlock[] Graph, ABlock Root) Cfg = ADecoder.DecodeSubroutine(this, Memory, Position);

            string SubName = SymbolTable.GetOrAdd(Position, $"Sub{Position:x16}");

            PropagateName(Cfg.Graph, SubName);

            AILEmitterCtx Context = new AILEmitterCtx(
                this,
                Cfg.Graph,
                Cfg.Root,
                SubName);

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

            CachedSubs.AddOrUpdate(Position, Subroutine, (Key, OldVal) => Subroutine);

            return Subroutine;
        }

        private void PropagateName(ABlock[] Graph, string Name)
        {
            foreach (ABlock Block in Graph)
            {
                AOpCode LastOp = Block.GetLastOp();

                if (LastOp != null &&
                   (LastOp.Emitter == AInstEmit.Bl ||
                    LastOp.Emitter == AInstEmit.Blr))
                {
                    SymbolTable.TryAdd(LastOp.Position + 4, Name);
                }
            }
        }
    }
}