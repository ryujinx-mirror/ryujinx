using ChocolArm64.Memory;
using Ryujinx.Loaders.Executables;
using Ryujinx.OsHle.Handles;
using Ryujinx.OsHle.Utilities;
using System.Collections.Concurrent;
using System.IO;

namespace Ryujinx.OsHle
{
    class Horizon
    {
        internal const int HidSize  = 0x40000;
        internal const int FontSize = 0x50;

        internal int HidHandle  { get; private set; }
        internal int FontHandle { get; private set; }

        public long HidOffset  { get; private set; }
        public long FontOffset { get; private set; }

        internal IdPool IdGen    { get; private set; }
        internal IdPool NvMapIds { get; private set; }

        internal IdPoolWithObj Handles  { get; private set; }
        internal IdPoolWithObj Fds      { get; private set; }
        internal IdPoolWithObj Displays { get; private set; }

        public ConcurrentDictionary<long, Mutex>   Mutexes  { get; private set; }
        public ConcurrentDictionary<long, CondVar> CondVars { get; private set; }

        private ConcurrentDictionary<int, Process> Processes;

        private AMemoryAlloc Allocator;

        private Switch Ns;

        public Horizon(Switch Ns)
        {
            this.Ns = Ns;

            IdGen    = new IdPool();
            NvMapIds = new IdPool();

            Handles  = new IdPoolWithObj();
            Fds      = new IdPoolWithObj();
            Displays = new IdPoolWithObj();

            Mutexes  = new ConcurrentDictionary<long, Mutex>();
            CondVars = new ConcurrentDictionary<long, CondVar>();

            Processes = new ConcurrentDictionary<int, Process>();

            Allocator = new AMemoryAlloc();

            HidOffset  = Allocator.Alloc(HidSize);
            FontOffset = Allocator.Alloc(FontSize);

            HidHandle  = Handles.GenerateId(new HSharedMem(HidOffset));
            FontHandle = Handles.GenerateId(new HSharedMem(FontOffset));
        }

        public void LoadCart(string ExeFsDir, string RomFsFile = null)
        {
            if (RomFsFile != null)
            {
                Ns.VFs.LoadRomFs(RomFsFile);
            }

            int ProcessId = IdGen.GenerateId();

            Process MainProcess = new Process(Ns, Allocator, ProcessId);

            void LoadNso(string FileName)
            {
                foreach (string File in Directory.GetFiles(ExeFsDir, FileName))
                {
                    if (Path.GetExtension(File) != string.Empty)
                    {
                        continue;
                    }

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

            MainProcess.InitializeHeap();
            MainProcess.Run();

            Processes.TryAdd(ProcessId, MainProcess);
        }

         public void LoadProgram(string FileName)
        {
            int ProcessId = IdGen.GenerateId();

            Process MainProcess = new Process(Ns, Allocator, ProcessId);

            using (FileStream Input = new FileStream(FileName, FileMode.Open))
            {
                if (Path.GetExtension(FileName).ToLower() == ".nro")
                {
                    MainProcess.LoadProgram(new Nro(Input));
                }
                else
                {
                    MainProcess.LoadProgram(new Nso(Input));
                }
            }

            MainProcess.SetEmptyArgs();
            MainProcess.InitializeHeap();
            MainProcess.Run();

            Processes.TryAdd(ProcessId, MainProcess);
        }

        public void FinalizeAllProcesses()
        {
            foreach (Process Process in Processes.Values)
            {
                Process.StopAllThreads();
                Process.Dispose();
            }
        }

        internal bool TryGetProcess(int ProcessId, out Process Process)
        {
            if (!Processes.TryGetValue(ProcessId, out Process))
            {
                return false;
            }

            return true;
        }

        internal void CloseHandle(int Handle)
        {
            object HndData = Handles.GetData<object>(Handle);

            if (HndData is HTransferMem TransferMem)
            {
                TransferMem.Memory.Manager.Reprotect(
                    TransferMem.Position,
                    TransferMem.Size,
                    TransferMem.Perm);
            }

            Handles.Delete(Handle);
        }

        public long GetVirtHidOffset()
        {
            HSharedMem HidSharedMem = Handles.GetData<HSharedMem>(HidHandle);

            return HidSharedMem.VirtPos;
        }
    }
}