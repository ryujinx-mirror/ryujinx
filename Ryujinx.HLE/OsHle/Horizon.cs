using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Loaders.Npdm;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Font;
using Ryujinx.HLE.OsHle.Handles;
using Ryujinx.HLE.OsHle.SystemState;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace Ryujinx.HLE.OsHle
{
    public class Horizon : IDisposable
    {
        internal const int HidSize  = 0x40000;
        internal const int FontSize = 0x1100000;

        private Switch Ns;

        private KProcessScheduler Scheduler;

        private ConcurrentDictionary<int, Process> Processes;

        public SystemStateMgr SystemState { get; private set; }

        internal KSharedMemory HidSharedMem  { get; private set; }
        internal KSharedMemory FontSharedMem { get; private set; }

        internal SharedFontManager Font { get; private set; }

        internal KEvent VsyncEvent { get; private set; }

        public Horizon(Switch Ns)
        {
            this.Ns = Ns;

            Scheduler = new KProcessScheduler(Ns.Log);

            Processes = new ConcurrentDictionary<int, Process>();

            SystemState = new SystemStateMgr();

            if (!Ns.Memory.Allocator.TryAllocate(HidSize,  out long HidPA) ||
                !Ns.Memory.Allocator.TryAllocate(FontSize, out long FontPA))
            {
                throw new InvalidOperationException();
            }

            HidSharedMem  = new KSharedMemory(HidPA, HidSize);
            FontSharedMem = new KSharedMemory(FontPA, FontSize);

            Font = new SharedFontManager(Ns, FontSharedMem.PA);

            VsyncEvent = new KEvent();
        }

        public void LoadCart(string ExeFsDir, string RomFsFile = null)
        {
            if (RomFsFile != null)
            {
                Ns.VFs.LoadRomFs(RomFsFile);
            }

            string NpdmFileName = Path.Combine(ExeFsDir, "main.npdm");

            Npdm MetaData = null;

            if (File.Exists(NpdmFileName))
            {
                Ns.Log.PrintInfo(LogClass.Loader, $"Loading main.npdm...");

                using (FileStream Input = new FileStream(NpdmFileName, FileMode.Open))
                {
                    MetaData = new Npdm(Input);
                }
            }
            else
            {
                Ns.Log.PrintWarning(LogClass.Loader, $"NPDM file not found, using default values!");
            }

            Process MainProcess = MakeProcess(MetaData);

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

            if (!MainProcess.MetaData.Is64Bits)
            {
                throw new NotImplementedException("32-bit titles are unsupported!");
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

        private Process MakeProcess(Npdm MetaData = null)
        {
            Process Process;

            lock (Processes)
            {
                int ProcessId = 0;

                while (Processes.ContainsKey(ProcessId))
                {
                    ProcessId++;
                }

                Process = new Process(Ns, Scheduler, ProcessId, MetaData);

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
