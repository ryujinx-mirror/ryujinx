using Ryujinx.Core.Loaders.Executables;
using Ryujinx.Core.Logging;
using Ryujinx.Core.OsHle.Handles;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace Ryujinx.Core.OsHle
{
    public class Horizon : IDisposable
    {
        internal const int HidSize  = 0x40000;
        internal const int FontSize = 0x50;

        private Switch Ns;

        private KProcessScheduler Scheduler;

        private ConcurrentDictionary<int, Process> Processes;

        internal SystemStateMgr SystemState { get; private set; }

        internal HSharedMem HidSharedMem  { get; private set; }
        internal HSharedMem FontSharedMem { get; private set; }

        internal KEvent VsyncEvent { get; private set; }

        public Horizon(Switch Ns)
        {
            this.Ns = Ns;

            Scheduler = new KProcessScheduler(Ns.Log);

            Processes = new ConcurrentDictionary<int, Process>();

            SystemState = new SystemStateMgr();

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

        public void LoadProgram(string FileName)
        {
            bool IsNro = Path.GetExtension(FileName).ToLower() == ".nro";

            string Name = Path.GetFileNameWithoutExtension(FileName);

            Process MainProcess = MakeProcess();

            using (FileStream Input = new FileStream(FileName, FileMode.Open))
            {
                MainProcess.LoadProgram(IsNro
                    ? (IExecutable)new Nro(Input, Name)
                    : (IExecutable)new Nso(Input, Name));
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