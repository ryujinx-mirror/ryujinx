using LibHac;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.FileSystem.Content;
using Ryujinx.HLE.HOS.Font;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Loaders.Npdm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using NxStaticObject = Ryujinx.HLE.Loaders.Executables.NxStaticObject;

namespace Ryujinx.HLE.HOS
{
    public class Horizon : IDisposable
    {
        internal const int InitialKipId     = 1;
        internal const int InitialProcessId = 0x51;

        internal const int HidSize  = 0x40000;
        internal const int FontSize = 0x1100000;

        private const int MemoryBlockAllocatorSize = 0x2710;

        private const ulong UserSlabHeapBase     = DramMemoryMap.SlabHeapBase;
        private const ulong UserSlabHeapItemSize = KMemoryManager.PageSize;
        private const ulong UserSlabHeapSize     = 0x3de000;

        internal long PrivilegedProcessLowestId  { get; set; } = 1;
        internal long PrivilegedProcessHighestId { get; set; } = 8;

        internal Switch Device { get; private set; }

        public SystemStateMgr State { get; private set; }

        internal bool KernelInitialized { get; private set; }

        internal KResourceLimit ResourceLimit { get; private set; }

        internal KMemoryRegionManager[] MemoryRegions { get; private set; }

        internal KMemoryBlockAllocator LargeMemoryBlockAllocator { get; private set; }
        internal KMemoryBlockAllocator SmallMemoryBlockAllocator { get; private set; }

        internal KSlabHeap UserSlabHeapPages { get; private set; }

        internal KCriticalSection CriticalSection { get; private set; }

        internal KScheduler Scheduler { get; private set; }

        internal KTimeManager TimeManager { get; private set; }

        internal KSynchronization Synchronization { get; private set; }

        internal KContextIdManager ContextIdManager { get; private set; }

        private long KipId;
        private long ProcessId;
        private long ThreadUid;

        internal CountdownEvent ThreadCounter;

        internal SortedDictionary<long, KProcess> Processes;

        internal ConcurrentDictionary<string, KAutoObject> AutoObjectNames;

        internal bool EnableVersionChecks { get; private set; }

        internal AppletStateMgr AppletState { get; private set; }

        internal KSharedMemory HidSharedMem  { get; private set; }
        internal KSharedMemory FontSharedMem { get; private set; }

        internal SharedFontManager Font { get; private set; }

        internal ContentManager ContentManager { get; private set; }

        internal KEvent VsyncEvent { get; private set; }

        internal Keyset KeySet { get; private set; }

        private bool HasStarted;

        public Nacp ControlData { get; set; }

        public string CurrentTitle { get; private set; }

        public IntegrityCheckLevel FsIntegrityCheckLevel { get; set; }

        internal long HidBaseAddress { get; private set; }

        public Horizon(Switch Device)
        {
            this.Device = Device;

            State = new SystemStateMgr();

            ResourceLimit = new KResourceLimit(this);

            KernelInit.InitializeResourceLimit(ResourceLimit);

            MemoryRegions = KernelInit.GetMemoryRegions();

            LargeMemoryBlockAllocator = new KMemoryBlockAllocator(MemoryBlockAllocatorSize * 2);
            SmallMemoryBlockAllocator = new KMemoryBlockAllocator(MemoryBlockAllocatorSize);

            UserSlabHeapPages = new KSlabHeap(
                UserSlabHeapBase,
                UserSlabHeapItemSize,
                UserSlabHeapSize);

            CriticalSection = new KCriticalSection(this);

            Scheduler = new KScheduler(this);

            TimeManager = new KTimeManager();

            Synchronization = new KSynchronization(this);

            ContextIdManager = new KContextIdManager();

            KipId     = InitialKipId;
            ProcessId = InitialProcessId;

            Scheduler.StartAutoPreemptionThread();

            KernelInitialized = true;

            ThreadCounter = new CountdownEvent(1);

            Processes = new SortedDictionary<long, KProcess>();

            AutoObjectNames = new ConcurrentDictionary<string, KAutoObject>();

            //Note: This is not really correct, but with HLE of services, the only memory
            //region used that is used is Application, so we can use the other ones for anything.
            KMemoryRegionManager Region = MemoryRegions[(int)MemoryRegion.NvServices];

            ulong HidPa  = Region.Address;
            ulong FontPa = Region.Address + HidSize;

            HidBaseAddress = (long)(HidPa - DramMemoryMap.DramBase);

            KPageList HidPageList  = new KPageList();
            KPageList FontPageList = new KPageList();

            HidPageList .AddRange(HidPa,  HidSize  / KMemoryManager.PageSize);
            FontPageList.AddRange(FontPa, FontSize / KMemoryManager.PageSize);

            HidSharedMem  = new KSharedMemory(HidPageList,  0, 0, MemoryPermission.Read);
            FontSharedMem = new KSharedMemory(FontPageList, 0, 0, MemoryPermission.Read);

            AppletState = new AppletStateMgr(this);

            AppletState.SetFocus(true);

            Font = new SharedFontManager(Device, (long)(FontPa - DramMemoryMap.DramBase));

            VsyncEvent = new KEvent(this);

            LoadKeySet();

            ContentManager = new ContentManager(Device);
        }

        public void LoadCart(string ExeFsDir, string RomFsFile = null)
        {
            if (RomFsFile != null)
            {
                Device.FileSystem.LoadRomFs(RomFsFile);
            }

            string NpdmFileName = Path.Combine(ExeFsDir, "main.npdm");

            Npdm MetaData = null;

            if (File.Exists(NpdmFileName))
            {
                Logger.PrintInfo(LogClass.Loader, $"Loading main.npdm...");

                using (FileStream Input = new FileStream(NpdmFileName, FileMode.Open))
                {
                    MetaData = new Npdm(Input);
                }
            }
            else
            {
                Logger.PrintWarning(LogClass.Loader, $"NPDM file not found, using default values!");

                MetaData = GetDefaultNpdm();
            }

            List<IExecutable> StaticObjects = new List<IExecutable>();

            void LoadNso(string SearchPattern)
            {
                foreach (string File in Directory.GetFiles(ExeFsDir, SearchPattern))
                {
                    if (Path.GetExtension(File) != string.Empty)
                    {
                        continue;
                    }

                    Logger.PrintInfo(LogClass.Loader, $"Loading {Path.GetFileNameWithoutExtension(File)}...");

                    using (FileStream Input = new FileStream(File, FileMode.Open))
                    {
                        NxStaticObject StaticObject = new NxStaticObject(Input);

                        StaticObjects.Add(StaticObject);
                    }
                }
            }

            if (!MetaData.Is64Bits)
            {
                throw new NotImplementedException("32-bit titles are unsupported!");
            }

            CurrentTitle = MetaData.ACI0.TitleId.ToString("x16");

            LoadNso("rtld");
            LoadNso("main");
            LoadNso("subsdk*");
            LoadNso("sdk");

            ContentManager.LoadEntries();

            ProgramLoader.LoadStaticObjects(this, MetaData, StaticObjects.ToArray());
        }

        public void LoadXci(string XciFile)
        {
            FileStream File = new FileStream(XciFile, FileMode.Open, FileAccess.Read);

            Xci Xci = new Xci(KeySet, File);

            (Nca MainNca, Nca ControlNca) = GetXciGameData(Xci);

            if (MainNca == null)
            {
                Logger.PrintError(LogClass.Loader, "Unable to load XCI");

                return;
            }

            ContentManager.LoadEntries();

            LoadNca(MainNca, ControlNca);
        }

        private (Nca Main, Nca Control) GetXciGameData(Xci Xci)
        {
            if (Xci.SecurePartition == null)
            {
                throw new InvalidDataException("Could not find XCI secure partition");
            }

            Nca MainNca    = null;
            Nca PatchNca   = null;
            Nca ControlNca = null;

            foreach (PfsFileEntry TicketEntry in Xci.SecurePartition.Files.Where(x => x.Name.EndsWith(".tik")))
            {
                Ticket ticket = new Ticket(Xci.SecurePartition.OpenFile(TicketEntry));

                if (!KeySet.TitleKeys.ContainsKey(ticket.RightsId))
                {
                    KeySet.TitleKeys.Add(ticket.RightsId, ticket.GetTitleKey(KeySet));
                }
            }

            foreach (PfsFileEntry FileEntry in Xci.SecurePartition.Files.Where(x => x.Name.EndsWith(".nca")))
            {
                Stream NcaStream = Xci.SecurePartition.OpenFile(FileEntry);

                Nca Nca = new Nca(KeySet, NcaStream, true);

                if (Nca.Header.ContentType == ContentType.Program)
                {
                    if (Nca.Sections.Any(x => x?.Type == SectionType.Romfs))
                    {
                        MainNca = Nca;
                    }
                    else if (Nca.Sections.Any(x => x?.Type == SectionType.Bktr))
                    {
                        PatchNca = Nca;
                    }
                }
                else if (Nca.Header.ContentType == ContentType.Control)
                {
                    ControlNca = Nca;
                }
            }

            if (MainNca == null)
            {
                Logger.PrintError(LogClass.Loader, "Could not find an Application NCA in the provided XCI file");
            }

            MainNca.SetBaseNca(PatchNca);

            if (ControlNca != null)
            {
                ReadControlData(ControlNca);
            }

            if (PatchNca != null)
            {
                PatchNca.SetBaseNca(MainNca);

                return (PatchNca, ControlNca);
            }

            return (MainNca, ControlNca);
        }

        public void ReadControlData(Nca ControlNca)
        {
            Romfs ControlRomfs = new Romfs(ControlNca.OpenSection(0, false, FsIntegrityCheckLevel));

            byte[] ControlFile = ControlRomfs.GetFile("/control.nacp");

            BinaryReader Reader = new BinaryReader(new MemoryStream(ControlFile));

            ControlData = new Nacp(Reader);
        }

        public void LoadNca(string NcaFile)
        {
            FileStream File = new FileStream(NcaFile, FileMode.Open, FileAccess.Read);

            Nca Nca = new Nca(KeySet, File, true);

            LoadNca(Nca, null);
        }

        public void LoadNsp(string NspFile)
        {
            FileStream File = new FileStream(NspFile, FileMode.Open, FileAccess.Read);

            Pfs Nsp = new Pfs(File);

            PfsFileEntry TicketFile = Nsp.Files.FirstOrDefault(x => x.Name.EndsWith(".tik"));

            // Load title key from the NSP's ticket in case the user doesn't have a title key file
            if (TicketFile != null)
            {
                Ticket Ticket = new Ticket(Nsp.OpenFile(TicketFile));

                KeySet.TitleKeys[Ticket.RightsId] = Ticket.GetTitleKey(KeySet);
            }

            Nca MainNca    = null;
            Nca ControlNca = null;

            foreach (PfsFileEntry NcaFile in Nsp.Files.Where(x => x.Name.EndsWith(".nca")))
            {
                Nca Nca = new Nca(KeySet, Nsp.OpenFile(NcaFile), true);

                if (Nca.Header.ContentType == ContentType.Program)
                {
                    MainNca = Nca;
                }
                else if (Nca.Header.ContentType == ContentType.Control)
                {
                    ControlNca = Nca;
                }
            }

            if (MainNca != null)
            {
                LoadNca(MainNca, ControlNca);

                return;
            }

            Logger.PrintError(LogClass.Loader, "Could not find an Application NCA in the provided NSP file");
        }

        public void LoadNca(Nca MainNca, Nca ControlNca)
        {
            if (MainNca.Header.ContentType != ContentType.Program)
            {
                Logger.PrintError(LogClass.Loader, "Selected NCA is not a \"Program\" NCA");

                return;
            }

            Stream RomfsStream = MainNca.OpenSection(ProgramPartitionType.Data, false, FsIntegrityCheckLevel);
            Stream ExefsStream = MainNca.OpenSection(ProgramPartitionType.Code, false, FsIntegrityCheckLevel);

            if (ExefsStream == null)
            {
                Logger.PrintError(LogClass.Loader, "No ExeFS found in NCA");

                return;
            }

            if (RomfsStream == null)
            {
                Logger.PrintWarning(LogClass.Loader, "No RomFS found in NCA");
            }
            else
            {
                Device.FileSystem.SetRomFs(RomfsStream);
            }

            Pfs Exefs = new Pfs(ExefsStream);

            Npdm MetaData = null;

            if (Exefs.FileExists("main.npdm"))
            {
                Logger.PrintInfo(LogClass.Loader, "Loading main.npdm...");

                MetaData = new Npdm(Exefs.OpenFile("main.npdm"));
            }
            else
            {
                Logger.PrintWarning(LogClass.Loader, $"NPDM file not found, using default values!");

                MetaData = GetDefaultNpdm();
            }

            List<IExecutable> StaticObjects = new List<IExecutable>();

            void LoadNso(string Filename)
            {
                foreach (PfsFileEntry File in Exefs.Files.Where(x => x.Name.StartsWith(Filename)))
                {
                    if (Path.GetExtension(File.Name) != string.Empty)
                    {
                        continue;
                    }

                    Logger.PrintInfo(LogClass.Loader, $"Loading {Filename}...");

                    NxStaticObject StaticObject = new NxStaticObject(Exefs.OpenFile(File));

                    StaticObjects.Add(StaticObject);
                }
            }

            Nacp ReadControlData()
            {
                Romfs ControlRomfs = new Romfs(ControlNca.OpenSection(0, false, FsIntegrityCheckLevel));

                byte[] ControlFile = ControlRomfs.GetFile("/control.nacp");

                BinaryReader Reader = new BinaryReader(new MemoryStream(ControlFile));

                Nacp ControlData = new Nacp(Reader);

                CurrentTitle = ControlData.Languages[(int)State.DesiredTitleLanguage].Title;

                if (string.IsNullOrWhiteSpace(CurrentTitle))
                {
                    CurrentTitle = ControlData.Languages.ToList().Find(x => !string.IsNullOrWhiteSpace(x.Title)).Title;
                }

                return ControlData;
            }

            if (ControlNca != null)
            {
                ReadControlData();
            }
            else
            {
                CurrentTitle = MetaData.ACI0.TitleId.ToString("x16");
            }

            if (!MetaData.Is64Bits)
            {
                throw new NotImplementedException("32-bit titles are not supported!");
            }

            LoadNso("rtld");
            LoadNso("main");
            LoadNso("subsdk");
            LoadNso("sdk");

            ContentManager.LoadEntries();

            ProgramLoader.LoadStaticObjects(this, MetaData, StaticObjects.ToArray());
        }

        public void LoadProgram(string FilePath)
        {
            Npdm MetaData = GetDefaultNpdm();

            bool IsNro = Path.GetExtension(FilePath).ToLower() == ".nro";

            using (FileStream Input = new FileStream(FilePath, FileMode.Open))
            {
                IExecutable StaticObject = IsNro
                    ? (IExecutable)new NxRelocatableObject(Input)
                    : (IExecutable)new NxStaticObject(Input);

                ProgramLoader.LoadStaticObjects(this, MetaData, new IExecutable[] { StaticObject });
            }
        }

        private Npdm GetDefaultNpdm()
        {
            Assembly Asm = Assembly.GetCallingAssembly();

            using (Stream NpdmStream = Asm.GetManifestResourceStream("Ryujinx.HLE.Homebrew.npdm"))
            {
                return new Npdm(NpdmStream);
            }
        }

        public void LoadKeySet()
        {
            string KeyFile        = null;
            string TitleKeyFile   = null;
            string ConsoleKeyFile = null;

            string Home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            LoadSetAtPath(Path.Combine(Home, ".switch"));
            LoadSetAtPath(Device.FileSystem.GetSystemPath());

            KeySet = ExternalKeys.ReadKeyFile(KeyFile, TitleKeyFile, ConsoleKeyFile);

            void LoadSetAtPath(string BasePath)
            {
                string LocalKeyFile        = Path.Combine(BasePath,    "prod.keys");
                string LocalTitleKeyFile   = Path.Combine(BasePath,   "title.keys");
                string LocalConsoleKeyFile = Path.Combine(BasePath, "console.keys");

                if (File.Exists(LocalKeyFile))
                {
                    KeyFile = LocalKeyFile;
                }

                if (File.Exists(LocalTitleKeyFile))
                {
                    TitleKeyFile = LocalTitleKeyFile;
                }

                if (File.Exists(LocalConsoleKeyFile))
                {
                    ConsoleKeyFile = LocalConsoleKeyFile;
                }
            }
        }

        public void SignalVsync()
        {
            VsyncEvent.ReadableEvent.Signal();
        }

        internal long GetThreadUid()
        {
            return Interlocked.Increment(ref ThreadUid) - 1;
        }

        internal long GetKipId()
        {
            return Interlocked.Increment(ref KipId) - 1;
        }

        internal long GetProcessId()
        {
            return Interlocked.Increment(ref ProcessId) - 1;
        }

        public void EnableMultiCoreScheduling()
        {
            if (!HasStarted)
            {
                Scheduler.MultiCoreScheduling = true;
            }
        }

        public void DisableMultiCoreScheduling()
        {
            if (!HasStarted)
            {
                Scheduler.MultiCoreScheduling = false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                //Force all threads to exit.
                lock (Processes)
                {
                    foreach (KProcess Process in Processes.Values)
                    {
                        Process.StopAllThreads();
                    }
                }

                //It's only safe to release resources once all threads
                //have exited.
                ThreadCounter.Signal();
                ThreadCounter.Wait();

                Scheduler.Dispose();

                TimeManager.Dispose();

                Device.Unload();
            }
        }
    }
}
