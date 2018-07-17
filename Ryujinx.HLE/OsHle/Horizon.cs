using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Handles;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace Ryujinx.HLE.OsHle
{
    public class Horizon : IDisposable
    {
        internal const int HidSize  = 0x40000;
        internal const int FontSize = 0x50;

        private Switch Ns;

        private KProcessScheduler Scheduler;

        private ConcurrentDictionary<int, Process> Processes;

        public SystemStateMgr SystemState { get; private set; }

        internal MemoryAllocator Allocator { get; private set; }

        internal HSharedMem HidSharedMem  { get; private set; }
        internal HSharedMem FontSharedMem { get; private set; }

        internal KEvent VsyncEvent { get; private set; }

        public Horizon(Switch Ns)
        {
            this.Ns = Ns;

            Scheduler = new KProcessScheduler(Ns.Log);

            Processes = new ConcurrentDictionary<int, Process>();

            SystemState = new SystemStateMgr();

            Allocator = new MemoryAllocator();

            HidSharedMem  = new HSharedMem();
            FontSharedMem = new HSharedMem();

            VsyncEvent = new KEvent();
        }

        public void LoadCart(string ExeFsDir, string RomFsFile = null)
        {
            if (RomFsFile != null)
            {
                Ns.VFs.LoadRomFs(RomFsFile);
            }

            Process MainProcess = MakeProcess();

            void LoadNso(string FileName)
            {
                foreach (string File in Directory.GetFiles(ExeFsDir, FileName))
                {
                    if (Path.GetExtension(File) != string.Empty)
                    {
                        continue;
                    }

                    Ns.Log.PrintInfo(LogClass.Loader, $"Loading {Path.GetFileNameWithoutExtension(File)}...");

                    using (FileStream Input = new FileStream(File, FileMode.Open))
                    {
                        string Name = Path.GetFileNameWithoutExtension(File);

                        Nso Program = new Nso(Input, Name);

                        MainProcess.LoadProgram(Program);
                    }
                }
            }

            LoadNso("rtld");

            MainProcess.SetEmptyArgs();

            LoadNso("main");
            LoadNso("subsdk*");
            LoadNso("sdk");

            MainProcess.Run();
        }

        public void LoadProgram(string FilePath)
        {
            bool IsNro = Path.GetExtension(FilePath).ToLower() == ".nro";

            string Name = Path.GetFileNameWithoutExtension(FilePath);
            string SwitchFilePath = Ns.VFs.SystemPathToSwitchPath(FilePath);

            if (IsNro && (SwitchFilePath == null || !SwitchFilePath.StartsWith("sdmc:/")))
            {
                // TODO: avoid copying the file if we are already inside a sdmc directory
                string SwitchPath = $"sdmc:/switch/{Name}{Homebrew.TemporaryNroSuffix}";
                string TempPath = Ns.VFs.SwitchPathToSystemPath(SwitchPath);

                string SwitchDir = Path.GetDirectoryName(TempPath);
                if (!Directory.Exists(SwitchDir))
                {
                    Directory.CreateDirectory(SwitchDir);
                }
                File.Copy(FilePath, TempPath, true);

                FilePath = TempPath;
            }

            Process MainProcess = MakeProcess();

            using (FileStream Input = new FileStream(FilePath, FileMode.Open))
            {
                MainProcess.LoadProgram(IsNro
                    ? (IExecutable)new Nro(Input, FilePath)
                    : (IExecutable)new Nso(Input, FilePath));
            }

            MainProcess.SetEmptyArgs();
            MainProcess.Run(IsNro);
        }

        public void SignalVsync() => VsyncEvent.WaitEvent.Set();

        private Process MakeProcess()
        {
            Process Process;

            lock (Processes)
            {
                int ProcessId = 0;

                while (Processes.ContainsKey(ProcessId))
                {
                    ProcessId++;
                }

                Process = new Process(Ns, Scheduler, ProcessId);

                Processes.TryAdd(ProcessId, Process);
            }

            InitializeProcess(Process);

            return Process;
        }

        private void InitializeProcess(Process Process)
        {
            Process.AppletState.SetFocus(true);
        }

        internal void ExitProcess(int ProcessId)
        {
            if (Processes.TryGetValue(ProcessId, out Process Process) && Process.NeedsHbAbi)
            {
                string NextNro = Homebrew.ReadHbAbiNextLoadPath(Process.Memory, Process.HbAbiDataPosition);

                Ns.Log.PrintInfo(LogClass.Loader, $"HbAbi NextLoadPath {NextNro}");

                if (NextNro == string.Empty)
                {
                    NextNro = "sdmc:/hbmenu.nro";
                }

                NextNro = NextNro.Replace("sdmc:", string.Empty);

                NextNro = Ns.VFs.GetFullPath(Ns.VFs.GetSdCardPath(), NextNro);

                if (File.Exists(NextNro))
                {
                    LoadProgram(NextNro);
                }
            }

            if (Processes.TryRemove(ProcessId, out Process))
            {
                Process.StopAllThreadsAsync();
                Process.Dispose();

                if (Processes.Count == 0)
                {
                    Ns.OnFinish(EventArgs.Empty);
                }
            }
        }

        internal bool TryGetProcess(int ProcessId, out Process Process)
        {
            return Processes.TryGetValue(ProcessId, out Process);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                foreach (Process Process in Processes.Values)
                {
                    Process.StopAllThreadsAsync();
                    Process.Dispose();
                }

                VsyncEvent.Dispose();

                Scheduler.Dispose();
            }
        }
    }
}