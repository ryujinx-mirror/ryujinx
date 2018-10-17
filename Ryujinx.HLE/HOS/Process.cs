using ChocolArm64;
using ChocolArm64.Events;
using ChocolArm64.Memory;
using ChocolArm64.State;
using LibHac;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Diagnostics.Demangler;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Services.Nv;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Loaders;
using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Loaders.Npdm;
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

            int HandleTableSize = 1024;

            if (MetaData != null)
            {
                foreach (KernelAccessControlItem Item in MetaData.ACI0.KernelAccessControl.Items)
                {
                    if (Item.HasHandleTableSize)
                    {
                        HandleTableSize = Item.HandleTableSize;

                        break;
                    }
                }
            }

            HandleTable = new KProcessHandleTable(Device.System, HandleTableSize);

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

            long ImageEnd = LoadProgram(Program, ImageBase);

            ImageBase = IntUtils.AlignUp(ImageEnd, KMemoryManager.PageSize);
        }

        public long LoadProgram(IExecutable Program, long ExecutableBase)
        {
            if (Disposed)
            {
                throw new ObjectDisposedException(nameof(Process));
            }

            Logger.PrintInfo(LogClass.Loader, $"Image base at 0x{ExecutableBase:x16}.");

            Executable Executable = new Executable(Program, MemoryManager, Memory, ExecutableBase);

            Executables.Add(Executable);

            return Executable.ImageEnd;
        }

        public void RemoveProgram(long ExecutableBase)
        {
            foreach (Executable Executable in Executables)
            {
                if (Executable.ImageBase == ExecutableBase)
                {
                    Executables.Remove(Executable);
                    break;
                }
            }
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

            KThread MainThread = HandleTable.GetKThread(Handle);

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

            HandleTable.GenerateHandle(Thread, out int Handle);

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
            Executable Exe = GetExecutable(e.Position);

            if (Exe == null)
            {
                return;
            }

            if (!TryGetSubName(Exe, e.Position, out string SubName))
            {
                SubName = string.Empty;
            }

            long Offset = e.Position - Exe.ImageBase;

            string ExeNameWithAddr = $"{Exe.Name}:0x{Offset:x8}";

            Logger.PrintDebug(LogClass.Cpu, ExeNameWithAddr + " " + SubName);
        }

        private ATranslator GetTranslator()
        {
            if (Translator == null)
            {
                Translator = new ATranslator();

                Translator.CpuTrace += CpuTraceHandler;
            }

            return Translator;
        }

        public void PrintStackTrace(AThreadState ThreadState)
        {
            StringBuilder Trace = new StringBuilder();

            Trace.AppendLine("Guest stack trace:");

            void AppendTrace(long Position)
            {
                Executable Exe = GetExecutable(Position);

                if (Exe == null)
                {
                    return;
                }

                if (!TryGetSubName(Exe, Position, out string SubName))
                {
                    SubName = $"Sub{Position:x16}";
                }
                else if (SubName.StartsWith("_Z"))
                {
                    SubName = Demangler.Parse(SubName);
                }

                long Offset = Position - Exe.ImageBase;

                string ExeNameWithAddr = $"{Exe.Name}:0x{Offset:x8}";

                Trace.AppendLine(" " + ExeNameWithAddr + " " + SubName);
            }

            long FramePointer = (long)ThreadState.X29;

            while (FramePointer != 0)
            {
                AppendTrace(Memory.ReadInt64(FramePointer + 8));

                FramePointer = Memory.ReadInt64(FramePointer);
            }

            Logger.PrintInfo(LogClass.Cpu, Trace.ToString());
        }

        private bool TryGetSubName(Executable Exe, long Position, out string Name)
        {
            Position -= Exe.ImageBase;

            int Left  = 0;
            int Right = Exe.SymbolTable.Count - 1;

            while (Left <= Right)
            {
                int Size = Right - Left;

                int Middle = Left + (Size >> 1);

                ElfSym Symbol = Exe.SymbolTable[Middle];

                long EndPosition = Symbol.Value + Symbol.Size;

                if ((ulong)Position >= (ulong)Symbol.Value && (ulong)Position < (ulong)EndPosition)
                {
                    Name = Symbol.Name;

                    return true;
                }

                if ((ulong)Position < (ulong)Symbol.Value)
                {
                    Right = Middle - 1;
                }
                else
                {
                    Left = Middle + 1;
                }
            }

            Name = null;

            return false;
        }

        private Executable GetExecutable(long Position)
        {
            string Name = string.Empty;

            for (int Index = Executables.Count - 1; Index >= 0; Index--)
            {
                if ((ulong)Position >= (ulong)Executables[Index].ImageBase)
                {
                    return Executables[Index];
                }
            }

            return null;
        }

        private void ThreadFinished(object sender, EventArgs e)
        {
            if (sender is AThread Thread)
            {
                if (Threads.TryRemove(Thread.ThreadState.Tpidr, out KThread KernelThread))
                {
                    Device.System.Scheduler.RemoveThread(KernelThread);
                }
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

            HandleTable.Destroy();

            INvDrvServices.UnloadProcess(this);

            if (NeedsHbAbi && Executables.Count > 0 && Executables[0].FilePath.EndsWith(Homebrew.TemporaryNroSuffix))
            {
                File.Delete(Executables[0].FilePath);
            }

            Logger.PrintInfo(LogClass.Loader, $"Process {ProcessId} exiting...");
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