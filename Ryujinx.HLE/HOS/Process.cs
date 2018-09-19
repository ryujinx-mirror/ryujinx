using ChocolArm64;
using ChocolArm64.Events;
using ChocolArm64.Memory;
using ChocolArm64.State;
using LibHac;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Diagnostics.Demangler;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Services.Nv;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Loaders;
using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Loaders.Npdm;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Ryujinx.HLE.HOS
{
    class Process : IDisposable
    {
        private const int TickFreq = 19_200_000;

        public Switch Device { get; private set; }

        public bool NeedsHbAbi { get; private set; }

        public long HbAbiDataPosition { get; private set; }

        public int ProcessId { get; private set; }

        private ATranslator Translator;

        public AMemory Memory { get; private set; }

        public KMemoryManager MemoryManager { get; private set; }

        private List<KTlsPageManager> TlsPages;

        public Npdm MetaData { get; private set; }

        public Nacp ControlData { get; set; }

        public KProcessHandleTable HandleTable { get; private set; }

        public AppletStateMgr AppletState { get; private set; }

        private SvcHandler SvcHandler;

        private ConcurrentDictionary<long, KThread> Threads;

        private List<Executable> Executables;

        private Dictionary<long, string> SymbolTable;

        private long ImageBase;

        private bool Disposed;

        public Process(Switch Device, int ProcessId, Npdm MetaData)
        {
            this.Device    = Device;
            this.MetaData  = MetaData;
            this.ProcessId = ProcessId;

            Memory = new AMemory(Device.Memory.RamPointer);

            MemoryManager = new KMemoryManager(this);

            TlsPages = new List<KTlsPageManager>();

            HandleTable = new KProcessHandleTable();

            AppletState = new AppletStateMgr(Device.System);

            SvcHandler = new SvcHandler(Device, this);

            Threads = new ConcurrentDictionary<long, KThread>();

            Executables = new List<Executable>();

            ImageBase = MemoryManager.CodeRegionStart;
        }

        public void LoadProgram(IExecutable Program)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(nameof(Process));
            }

            Device.Log.PrintInfo(LogClass.Loader, $"Image base at 0x{ImageBase:x16}.");

            Executable Executable = new Executable(Program, MemoryManager, Memory, ImageBase);

            Executables.Add(Executable);

            ImageBase = IntUtils.AlignUp(Executable.ImageEnd, KMemoryManager.PageSize);
        }

        public void SetEmptyArgs()
        {
            //TODO: This should be part of Run.
            ImageBase += KMemoryManager.PageSize;
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

            MakeSymbolTable();

            long MainStackTop = MemoryManager.CodeRegionEnd - KMemoryManager.PageSize;

            long MainStackSize = 1 * 1024 * 1024;

            long MainStackBottom = MainStackTop - MainStackSize;

            MemoryManager.HleMapCustom(
                MainStackBottom,
                MainStackSize,
                MemoryState.MappedMemory,
                MemoryPermission.ReadAndWrite);

            int Handle = MakeThread(Executables[0].ImageBase, MainStackTop, 0, 44, 0);

            if (Handle == -1)
            {
                return false;
            }

            KThread MainThread = HandleTable.GetData<KThread>(Handle);

            if (NeedsHbAbi)
            {
                HbAbiDataPosition = IntUtils.AlignUp(Executables[0].ImageEnd, KMemoryManager.PageSize);

                const long HbAbiDataSize = KMemoryManager.PageSize;

                MemoryManager.HleMapCustom(
                    HbAbiDataPosition,
                    HbAbiDataSize,
                    MemoryState.MappedMemory,
                    MemoryPermission.ReadAndWrite);

                string SwitchPath = Device.FileSystem.SystemPathToSwitchPath(Executables[0].FilePath);

                Homebrew.WriteHbAbiData(Memory, HbAbiDataPosition, Handle, SwitchPath);

                MainThread.Context.ThreadState.X0 = (ulong)HbAbiDataPosition;
                MainThread.Context.ThreadState.X1 = ulong.MaxValue;
            }

            MainThread.TimeUp();

            return true;
        }

        private int ThreadIdCtr = 1;

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

            AThread CpuThread = new AThread(GetTranslator(), Memory, EntryPoint);

            long Tpidr = GetFreeTls();

            int ThreadId = ThreadIdCtr++; //(int)((Tpidr - MemoryManager.TlsIoRegionStart) / 0x200) + 1;

            KThread Thread = new KThread(CpuThread, this, Device.System, ProcessorId, Priority, ThreadId);

            Thread.LastPc = EntryPoint;

            int Handle = HandleTable.OpenHandle(Thread);

            CpuThread.ThreadState.CntfrqEl0 = TickFreq;
            CpuThread.ThreadState.Tpidr     = Tpidr;

            CpuThread.ThreadState.X0  = (ulong)ArgsPtr;
            CpuThread.ThreadState.X1  = (ulong)Handle;
            CpuThread.ThreadState.X31 = (ulong)StackTop;

            CpuThread.ThreadState.Interrupt += InterruptHandler;
            CpuThread.ThreadState.Break     += BreakHandler;
            CpuThread.ThreadState.SvcCall   += SvcHandler.SvcCall;
            CpuThread.ThreadState.Undefined += UndefinedHandler;

            CpuThread.WorkFinished += ThreadFinished;

            Threads.TryAdd(CpuThread.ThreadState.Tpidr, Thread);

            return Handle;
        }

        private long GetFreeTls()
        {
            long Position;

            lock (TlsPages)
            {
                for (int Index = 0; Index < TlsPages.Count; Index++)
                {
                    if (TlsPages[Index].TryGetFreeTlsAddr(out Position))
                    {
                        return Position;
                    }
                }

                long PagePosition = MemoryManager.HleMapTlsPage();

                KTlsPageManager TlsPage = new KTlsPageManager(PagePosition);

                TlsPages.Add(TlsPage);

                TlsPage.TryGetFreeTlsAddr(out Position);
            }

            return Position;
        }

        private void InterruptHandler(object sender, EventArgs e)
        {
            Device.System.Scheduler.ContextSwitch();
        }

        private void BreakHandler(object sender, AInstExceptionEventArgs e)
        {
            throw new GuestBrokeExecutionException();
        }

        private void UndefinedHandler(object sender, AInstUndefinedEventArgs e)
        {
            throw new UndefinedInstructionException(e.Position, e.RawOpCode);
        }

        private void MakeSymbolTable()
        {
            SymbolTable = new Dictionary<long, string>();

            foreach (Executable Exe in Executables)
            {
                foreach (KeyValuePair<long, string> KV in Exe.SymbolTable)
                {
                    SymbolTable.TryAdd(Exe.ImageBase + KV.Key, KV.Value);
                }
            }
        }

        private ATranslator GetTranslator()
        {
            if (Translator == null)
            {
                Translator = new ATranslator(SymbolTable);

                Translator.CpuTrace += CpuTraceHandler;
            }

            return Translator;
        }

        public void EnableCpuTracing()
        {
            Translator.EnableCpuTrace = true;
        }

        public void DisableCpuTracing()
        {
            Translator.EnableCpuTrace = false;
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

            Device.Log.PrintDebug(LogClass.Cpu, $"Executing at 0x{e.Position:x16} {e.SubName} {NsoName}");
        }

        public void PrintStackTrace(AThreadState ThreadState)
        {
            long[] Positions = ThreadState.GetCallStack();

            StringBuilder Trace = new StringBuilder();

            Trace.AppendLine("Guest stack trace:");

            foreach (long Position in Positions)
            {
                if (!SymbolTable.TryGetValue(Position, out string SubName))
                {
                    SubName = $"Sub{Position:x16}";
                }
                else if (SubName.StartsWith("_Z"))
                {
                    SubName = Demangler.Parse(SubName);
                }

                Trace.AppendLine(" " + SubName + " (" + GetNsoNameAndAddress(Position) + ")");
            }

            Device.Log.PrintInfo(LogClass.Cpu, Trace.ToString());
        }

        private string GetNsoNameAndAddress(long Position)
        {
            string Name = string.Empty;

            for (int Index = Executables.Count - 1; Index >= 0; Index--)
            {
                if (Position >= Executables[Index].ImageBase)
                {
                    long Offset = Position - Executables[Index].ImageBase;

                    Name = $"{Executables[Index].Name}:{Offset:x8}";

                    break;
                }
            }

            return Name;
        }

        private void ThreadFinished(object sender, EventArgs e)
        {
            if (sender is AThread Thread)
            {
                Threads.TryRemove(Thread.ThreadState.Tpidr, out KThread KernelThread);
            }

            if (Threads.Count == 0)
            {
                Device.System.ExitProcess(ProcessId);
            }
        }

        public KThread GetThread(long Tpidr)
        {
            if (!Threads.TryGetValue(Tpidr, out KThread Thread))
            {
                throw new InvalidOperationException();
            }

            return Thread;
        }

        private void Unload()
        {
            if (Disposed || Threads.Count > 0)
            {
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

            INvDrvServices.UnloadProcess(this);

            if (NeedsHbAbi && Executables.Count > 0 && Executables[0].FilePath.EndsWith(Homebrew.TemporaryNroSuffix))
            {
                File.Delete(Executables[0].FilePath);
            }

            Device.Log.PrintInfo(LogClass.Loader, $"Process {ProcessId} exiting...");
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                if (Threads.Count > 0)
                {
                    foreach (KThread Thread in Threads.Values)
                    {
                        Device.System.Scheduler.StopThread(Thread);
                    }
                }
                else
                {
                    Unload();
                }
            }
        }
    }
}