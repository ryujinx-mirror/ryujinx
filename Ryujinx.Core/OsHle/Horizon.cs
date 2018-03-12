using Ryujinx.Core.Loaders.Executables;
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

        internal ConcurrentDictionary<long, Mutex>   Mutexes  { get; private set; }
        internal ConcurrentDictionary<long, CondVar> CondVars { get; private set; }

        private ConcurrentDictionary<int, Process> Processes;

        internal HSharedMem HidSharedMem;
        internal HSharedMem FontSharedMem;

        private Switch Ns;

        public Horizon(Switch Ns)
        {
            this.Ns = Ns;

            Mutexes  = new ConcurrentDictionary<long, Mutex>();
            CondVars = new ConcurrentDictionary<long, CondVar>();

            Processes = new ConcurrentDictionary<int, Process>();

            HidSharedMem  = new HSharedMem();
            FontSharedMem = new HSharedMem();
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

                    Logging.Info($"Loading {Path.GetFileNameWithoutExtension(File)}...");

                    using (FileStream Input = new FileStream(File, FileMode.Open))
                    {
                        Nso Program = new Nso(Input);

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

            Process MainProcess = MakeProcess();

            using (FileStream Input = new FileStream(FileName, FileMode.Open))
            {
                MainProcess.LoadProgram(IsNro
                    ? (IExecutable)new Nro(Input)
                    : (IExecutable)new Nso(Input));
            }

            MainProcess.SetEmptyArgs();
            MainProcess.Run(IsNro);
        }

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

                Process = new Process(Ns, ProcessId);

                Processes.TryAdd(ProcessId, Process);
            }

            return Process;
        }

        internal void ExitProcess(int ProcessId)
        {
            if (Processes.TryGetValue(ProcessId, out Process Process) && Process.NeedsHbAbi)
            {
                string NextNro = Homebrew.ReadHbAbiNextLoadPath(Process.Memory, Process.HbAbiDataPosition);

                Logging.Info($"HbAbi NextLoadPath {NextNro}");

                if (NextNro == string.Empty)
                {
                    NextNro = "sdmc:/hbmenu.nro";
                }

                NextNro = NextNro.Replace("sdmc:", string.Empty);

                NextNro = Ns.VFs.GetFullPath(Ns.VFs.GetSdCardPath(), NextNro);

                if (File.Exists(NextNro))
                {
                    //TODO: Those dictionaries shouldn't even exist,
                    //the Mutex and CondVar helper classes should be static.
                    Mutexes.Clear();
                    CondVars.Clear();

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
            }
        }
    }
}