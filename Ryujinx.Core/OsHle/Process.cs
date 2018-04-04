using ChocolArm64;
using ChocolArm64.Events;
using ChocolArm64.Memory;
using Ryujinx.Core.Loaders;
using Ryujinx.Core.Loaders.Executables;
using Ryujinx.Core.OsHle.Exceptions;
using Ryujinx.Core.OsHle.Handles;
using Ryujinx.Core.OsHle.Services.Nv;
using Ryujinx.Core.OsHle.Svc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ryujinx.Core.OsHle
{
    class Process : IDisposable
    {
        private const int TlsSize       = 0x200;
        private const int TotalTlsSlots = 32;

        private const int TickFreq = 19_200_000;

        private Switch Ns;

        public bool NeedsHbAbi { get; private set; }

        public long HbAbiDataPosition { get; private set; }

        public int ProcessId { get; private set; }

        private ATranslator Translator;

        public AMemory Memory { get; private set; }

        public KProcessScheduler Scheduler { get; private set; }

        public KProcessHandleTable HandleTable { get; private set; }

        public AppletStateMgr AppletState { get; private set; }

        private SvcHandler SvcHandler;

        private ConcurrentDictionary<int, AThread> TlsSlots;

        private ConcurrentDictionary<long, KThread> ThreadsByTpidr;

        private List<Executable> Executables;

        private KThread MainThread;

        private long ImageBase;

        private bool ShouldDispose;

        private bool Disposed;

        public Process(Switch Ns, int ProcessId)
        {
            this.Ns        = Ns;
            this.ProcessId = ProcessId;

            Memory = new AMemory();

            HandleTable = new KProcessHandleTable();

            Scheduler = new KProcessScheduler();

            AppletState = new AppletStateMgr();

            SvcHandler = new SvcHandler(Ns, this);

            TlsSlots = new ConcurrentDictionary<int, AThread>();

            ThreadsByTpidr = new ConcurrentDictionary<long, KThread>();

            Executables = new List<Executable>();

            ImageBase = MemoryRegions.AddrSpaceStart;

            MapRWMemRegion(
                MemoryRegions.TlsPagesAddress,
                MemoryRegions.TlsPagesSize,
                MemoryType.ThreadLocal);
        }

        public void LoadProgram(IExecutable Program)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(nameof(Process));
            }

            Logging.Info($"Image base at 0x{ImageBase:x16}.");

            Executable Executable = new Executable(Program, Memory, ImageBase);

            Executables.Add(Executable);

            ImageBase = AMemoryHelper.PageRoundUp(Executable.ImageEnd);
        }

        public void SetEmptyArgs()
        {
            //TODO: This should be part of Run.
            ImageBase += AMemoryMgr.PageSize;
        }

        public bool Run(bool NeedsHbAbi = false)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(nameof(Process));
            }

            this.NeedsHbAbi = NeedsHbAbi;

            if (Executables.Count == 0)
            {
                return false;
            }

            MapRWMemRegion(
                MemoryRegions.MainStackAddress,
                MemoryRegions.MainStackSize,
                MemoryType.Normal);

            long StackTop = MemoryRegions.MainStackAddress + MemoryRegions.MainStackSize;

            int Handle = MakeThread(Executables[0].ImageBase, StackTop, 0, 0, 0);

            if (Handle == -1)
            {
                return false;
            }

            MainThread = HandleTable.GetData<KThread>(Handle);

            if (NeedsHbAbi)
            {
                HbAbiDataPosition = AMemoryHelper.PageRoundUp(Executables[0].ImageEnd);

                Homebrew.WriteHbAbiData(Memory, HbAbiDataPosition, Handle);

                MainThread.Thread.ThreadState.X0 = (ulong)HbAbiDataPosition;
                MainThread.Thread.ThreadState.X1 = ulong.MaxValue;
            }

            Scheduler.StartThread(MainThread);

            return true;
        }

        private void MapRWMemRegion(long Position, long Size, MemoryType Type)
        {
            Memory.Manager.Map(Position, Size, (int)Type, AMemoryPerm.RW);
        }

        public void StopAllThreadsAsync()
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(nameof(Process));
            }

            if (MainThread != null)
            {
                MainThread.Thread.StopExecution();
            }

            foreach (AThread Thread in TlsSlots.Values)
            {
                Thread.StopExecution();
            }
        }

        public int MakeThread(
            long EntryPoint,
            long StackTop,
            long ArgsPtr,
            int  Priority,
            int  ProcessorId)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(nameof(Process));
            }

            AThread Thread = new AThread(GetTranslator(), Memory, EntryPoint);

            KThread ThreadHnd = new KThread(Thread, ProcessorId, Priority);

            int Handle = HandleTable.OpenHandle(ThreadHnd);

            int ThreadId = GetFreeTlsSlot(Thread);

            long Tpidr = MemoryRegions.TlsPagesAddress + ThreadId * TlsSize;

            Thread.ThreadState.Break     += BreakHandler;
            Thread.ThreadState.SvcCall   += SvcHandler.SvcCall;
            Thread.ThreadState.Undefined += UndefinedHandler;
            Thread.ThreadState.ProcessId  = ProcessId;
            Thread.ThreadState.ThreadId   = ThreadId;
            Thread.ThreadState.CntfrqEl0  = TickFreq;
            Thread.ThreadState.Tpidr      = Tpidr;
            Thread.ThreadState.X0         = (ulong)ArgsPtr;
            Thread.ThreadState.X1         = (ulong)Handle;
            Thread.ThreadState.X31        = (ulong)StackTop;

            Thread.WorkFinished += ThreadFinished;

            ThreadsByTpidr.TryAdd(Thread.ThreadState.Tpidr, ThreadHnd);

            return Handle;
        }

        private void BreakHandler(object sender, AInstExceptionEventArgs e)
        {
            throw new GuestBrokeExecutionException();
        }

        private void UndefinedHandler(object sender, AInstUndefinedEventArgs e)
        {
            throw new UndefinedInstructionException(e.Position, e.RawOpCode);
        }

        private ATranslator GetTranslator()
        {
            if (Translator == null)
            {
                Dictionary<long, string> SymbolTable = new Dictionary<long, string>();

                foreach (Executable Exe in Executables)
                {
                    foreach (KeyValuePair<long, string> KV in Exe.SymbolTable)
                    {
                        SymbolTable.TryAdd(Exe.ImageBase + KV.Key, KV.Value);
                    }
                }

                Translator = new ATranslator(SymbolTable);

                Translator.CpuTrace += CpuTraceHandler;
            }

            return Translator;
        }

        private void CpuTraceHandler(object sender, ACpuTraceEventArgs e)
        {
            string NsoName = string.Empty;

            for (int Index = Executables.Count - 1; Index >= 0; Index--)
            {
                if (e.Position >= Executables[Index].ImageBase)
                {
                    NsoName = $"{(e.Position - Executables[Index].ImageBase):x16}";

                    break;
                }
            }

            Logging.Trace($"Executing at 0x{e.Position:x16} {e.SubName} {NsoName}");
        }

        public void EnableCpuTracing()
        {
            Translator.EnableCpuTrace = true;
        }

        public void DisableCpuTracing()
        {
            Translator.EnableCpuTrace = false;
        }

        private int GetFreeTlsSlot(AThread Thread)
        {
            for (int Index = 1; Index < TotalTlsSlots; Index++)
            {
                if (TlsSlots.TryAdd(Index, Thread))
                {
                    return Index;
                }
            }

            throw new InvalidOperationException();
        }

        private void ThreadFinished(object sender, EventArgs e)
        {
            if (sender is AThread Thread)
            {
                Logging.Info($"Thread {Thread.ThreadId} exiting...");

                TlsSlots.TryRemove(GetTlsSlot(Thread.ThreadState.Tpidr), out _);
            }

            if (TlsSlots.Count == 0)
            {
                if (ShouldDispose)
                {
                    Dispose();
                }

                Logging.Info($"No threads running, now exiting Process {ProcessId}...");

                Ns.Os.ExitProcess(ProcessId);
            }
        }

        private int GetTlsSlot(long Position)
        {
            return (int)((Position - MemoryRegions.TlsPagesAddress) / TlsSize);
        }

        public KThread GetThread(long Tpidr)
        {
            if (!ThreadsByTpidr.TryGetValue(Tpidr, out KThread Thread))
            {
                Logging.Error($"Thread with TPIDR 0x{Tpidr:x16} not found!");
            }

            return Thread;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing && !Disposed)
            {
                //If there is still some thread running, disposing the objects is not
                //safe as the thread may try to access those resources. Instead, we set
                //the flag to have the Process disposed when all threads finishes.
                //Note: This may not happen if the guest code gets stuck on a infinite loop.
                if (TlsSlots.Count > 0)
                {
                    ShouldDispose = true;

                    Logging.Info($"Process {ProcessId} waiting all threads terminate...");

                    return;
                }

                Disposed = true;

                foreach (object Obj in HandleTable.Clear())
                {
                    if (Obj is KSession Session)
                    {
                        Session.Dispose();
                    }
                }

                ServiceNvDrv.Fds.DeleteProcess(this);

                ServiceNvDrv.NvMaps    .DeleteProcess(this);
                ServiceNvDrv.NvMapsById.DeleteProcess(this);
                ServiceNvDrv.NvMapsFb  .DeleteProcess(this);

                Scheduler.Dispose();

                AppletState.Dispose();

                SvcHandler.Dispose();

                Memory.Dispose();

                Logging.Info($"Process {ProcessId} exiting...");
            }
        }
    }
}