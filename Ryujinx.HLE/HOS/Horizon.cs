using LibHac;
using LibHac.Fs;
using LibHac.Fs.NcaUtils;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.FileSystem.Content;
using Ryujinx.HLE.HOS.Font;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Sm;
using Ryujinx.HLE.HOS.Services.Time.Clock;
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
        internal const int IirsSize = 0x8000;
        internal const int TimeSize = 0x1000;

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

        private long _kipId;
        private long _processId;
        private long _threadUid;

        internal CountdownEvent ThreadCounter;

        internal SortedDictionary<long, KProcess> Processes;

        internal ConcurrentDictionary<string, KAutoObject> AutoObjectNames;

        internal bool EnableVersionChecks { get; private set; }

        internal AppletStateMgr AppletState { get; private set; }

        internal KSharedMemory HidSharedMem  { get; private set; }
        internal KSharedMemory FontSharedMem { get; private set; }
        internal KSharedMemory IirsSharedMem { get; private set; }
        internal KSharedMemory TimeSharedMem { get; private set; }

        internal SharedFontManager Font { get; private set; }

        internal ContentManager ContentManager { get; private set; }

        internal KEvent VsyncEvent { get; private set; }

        internal Keyset KeySet { get; private set; }

        private bool _hasStarted;

        public Nacp ControlData { get; set; }

        public string CurrentTitle { get; private set; }

        public string TitleName { get; private set; }

        public string TitleID { get; private set; }

        public IntegrityCheckLevel FsIntegrityCheckLevel { get; set; }

        public int GlobalAccessLogMode { get; set; }

        internal long HidBaseAddress { get; private set; }

        public Horizon(Switch device)
        {
            ControlData = new Nacp();

            Device = device;

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

            _kipId     = InitialKipId;
            _processId = InitialProcessId;

            Scheduler.StartAutoPreemptionThread();

            KernelInitialized = true;

            ThreadCounter = new CountdownEvent(1);

            Processes = new SortedDictionary<long, KProcess>();

            AutoObjectNames = new ConcurrentDictionary<string, KAutoObject>();

            // Note: This is not really correct, but with HLE of services, the only memory
            // region used that is used is Application, so we can use the other ones for anything.
            KMemoryRegionManager region = MemoryRegions[(int)MemoryRegion.NvServices];

            ulong hidPa  = region.Address;
            ulong fontPa = region.Address + HidSize;
            ulong iirsPa = region.Address + HidSize + FontSize;
            ulong timePa = region.Address + HidSize + FontSize + IirsSize;

            HidBaseAddress = (long)(hidPa - DramMemoryMap.DramBase);

            KPageList hidPageList  = new KPageList();
            KPageList fontPageList = new KPageList();
            KPageList iirsPageList = new KPageList();
            KPageList timePageList = new KPageList();

            hidPageList .AddRange(hidPa,  HidSize  / KMemoryManager.PageSize);
            fontPageList.AddRange(fontPa, FontSize / KMemoryManager.PageSize);
            iirsPageList.AddRange(iirsPa, IirsSize / KMemoryManager.PageSize);
            timePageList.AddRange(timePa, TimeSize / KMemoryManager.PageSize);

            HidSharedMem  = new KSharedMemory(this, hidPageList,  0, 0, MemoryPermission.Read);
            FontSharedMem = new KSharedMemory(this, fontPageList, 0, 0, MemoryPermission.Read);
            IirsSharedMem = new KSharedMemory(this, iirsPageList, 0, 0, MemoryPermission.Read);
            TimeSharedMem = new KSharedMemory(this, timePageList, 0, 0, MemoryPermission.Read);

            AppletState = new AppletStateMgr(this);

            AppletState.SetFocus(true);

            Font = new SharedFontManager(device, (long)(fontPa - DramMemoryMap.DramBase));

            IUserInterface.InitializePort(this);

            VsyncEvent = new KEvent(this);

            LoadKeySet();

            ContentManager = new ContentManager(device);

            // NOTE: Now we set the default internal offset of the steady clock like Nintendo does... even if it's strange this is accurate.
            // TODO: use bpc:r and set:sys (and set external clock source id from settings)
            DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            SteadyClockCore.Instance.SetInternalOffset(new TimeSpanType(((ulong)(DateTime.Now.ToUniversalTime() - UnixEpoch).TotalSeconds) * 1000000000));
        }

        public void LoadCart(string exeFsDir, string romFsFile = null)
        {
            if (romFsFile != null)
            {
                Device.FileSystem.LoadRomFs(romFsFile);
            }

            LocalFileSystem codeFs = new LocalFileSystem(exeFsDir);

            LoadExeFs(codeFs, out _);
        }

        public void LoadXci(string xciFile)
        {
            FileStream file = new FileStream(xciFile, FileMode.Open, FileAccess.Read);

            Xci xci = new Xci(KeySet, file.AsStorage());

            (Nca mainNca, Nca patchNca, Nca controlNca) = GetXciGameData(xci);

            if (mainNca == null)
            {
                Logger.PrintError(LogClass.Loader, "Unable to load XCI");

                return;
            }

            ContentManager.LoadEntries();

            LoadNca(mainNca, patchNca, controlNca);
        }

        public void LoadKip(string kipFile)
        {
            using (FileStream fs = new FileStream(kipFile, FileMode.Open))
            {
                ProgramLoader.LoadKernelInitalProcess(this, new KernelInitialProcess(fs));
            }
        }

        private (Nca Main, Nca patch, Nca Control) GetXciGameData(Xci xci)
        {
            if (!xci.HasPartition(XciPartitionType.Secure))
            {
                throw new InvalidDataException("Could not find XCI secure partition");
            }

            Nca mainNca    = null;
            Nca patchNca   = null;
            Nca controlNca = null;

            XciPartition securePartition = xci.OpenPartition(XciPartitionType.Secure);

            foreach (DirectoryEntry ticketEntry in securePartition.EnumerateEntries("*.tik"))
            {
                Ticket ticket = new Ticket(securePartition.OpenFile(ticketEntry.FullPath, OpenMode.Read).AsStream());

                if (!KeySet.TitleKeys.ContainsKey(ticket.RightsId))
                {
                    KeySet.TitleKeys.Add(ticket.RightsId, ticket.GetTitleKey(KeySet));
                }
            }

            foreach (DirectoryEntry fileEntry in securePartition.EnumerateEntries("*.nca"))
            {
                IStorage ncaStorage = securePartition.OpenFile(fileEntry.FullPath, OpenMode.Read).AsStorage();

                Nca nca = new Nca(KeySet, ncaStorage);

                if (nca.Header.ContentType == ContentType.Program)
                {
                    int dataIndex = Nca.GetSectionIndexFromType(NcaSectionType.Data, ContentType.Program);

                    if (nca.Header.GetFsHeader(dataIndex).IsPatchSection())
                    {
                        patchNca = nca;
                    }
                    else
                    {
                        mainNca = nca;
                    }
                }
                else if (nca.Header.ContentType == ContentType.Control)
                {
                    controlNca = nca;
                }
            }

            if (mainNca == null)
            {
                Logger.PrintError(LogClass.Loader, "Could not find an Application NCA in the provided XCI file");
            }

            if (controlNca != null)
            {
                ReadControlData(controlNca);
            }

            return (mainNca, patchNca, controlNca);
        }

        public void ReadControlData(Nca controlNca)
        {
            IFileSystem controlFs = controlNca.OpenFileSystem(NcaSectionType.Data, FsIntegrityCheckLevel);

            IFile controlFile = controlFs.OpenFile("/control.nacp", OpenMode.Read);

            ControlData = new Nacp(controlFile.AsStream());

            TitleName = CurrentTitle = ControlData.Descriptions[(int)State.DesiredTitleLanguage].Title;
        }

        public void LoadNca(string ncaFile)
        {
            FileStream file = new FileStream(ncaFile, FileMode.Open, FileAccess.Read);

            Nca nca = new Nca(KeySet, file.AsStorage(false));

            LoadNca(nca, null, null);
        }

        public void LoadNsp(string nspFile)
        {
            FileStream file = new FileStream(nspFile, FileMode.Open, FileAccess.Read);

            PartitionFileSystem nsp = new PartitionFileSystem(file.AsStorage());

            foreach (DirectoryEntry ticketEntry in nsp.EnumerateEntries("*.tik"))
            {
                Ticket ticket = new Ticket(nsp.OpenFile(ticketEntry.FullPath, OpenMode.Read).AsStream());

                if (!KeySet.TitleKeys.ContainsKey(ticket.RightsId))
                {
                    KeySet.TitleKeys.Add(ticket.RightsId, ticket.GetTitleKey(KeySet));
                }
            }

            Nca mainNca    = null;
            Nca patchNca   = null;
            Nca controlNca = null;

            foreach (DirectoryEntry fileEntry in nsp.EnumerateEntries("*.nca"))
            {
                IStorage ncaStorage = nsp.OpenFile(fileEntry.FullPath, OpenMode.Read).AsStorage();

                Nca nca = new Nca(KeySet, ncaStorage);

                if (nca.Header.ContentType == ContentType.Program)
                {
                    int dataIndex = Nca.GetSectionIndexFromType(NcaSectionType.Data, ContentType.Program);

                    if (nca.Header.GetFsHeader(dataIndex).IsPatchSection())
                    {
                        patchNca = nca;
                    }
                    else
                    {
                        mainNca = nca;
                    }
                }
                else if (nca.Header.ContentType == ContentType.Control)
                {
                    controlNca = nca;
                }
            }

            if (mainNca != null)
            {
                LoadNca(mainNca, patchNca, controlNca);

                return;
            }

            // This is not a normal NSP, it's actually a ExeFS as a NSP
            LoadExeFs(nsp, out _);
        }

        public void LoadNca(Nca mainNca, Nca patchNca, Nca controlNca)
        {
            if (mainNca.Header.ContentType != ContentType.Program)
            {
                Logger.PrintError(LogClass.Loader, "Selected NCA is not a \"Program\" NCA");

                return;
            }

            IStorage    dataStorage = null;
            IFileSystem codeFs      = null;

            if (patchNca == null)
            {
                if (mainNca.CanOpenSection(NcaSectionType.Data))
                {
                    dataStorage = mainNca.OpenStorage(NcaSectionType.Data, FsIntegrityCheckLevel);
                }

                if (mainNca.CanOpenSection(NcaSectionType.Code))
                {
                    codeFs = mainNca.OpenFileSystem(NcaSectionType.Code, FsIntegrityCheckLevel);
                }
            }
            else
            {
                if (patchNca.CanOpenSection(NcaSectionType.Data))
                {
                    dataStorage = mainNca.OpenStorageWithPatch(patchNca, NcaSectionType.Data, FsIntegrityCheckLevel);
                }

                if (patchNca.CanOpenSection(NcaSectionType.Code))
                {
                    codeFs = mainNca.OpenFileSystemWithPatch(patchNca, NcaSectionType.Code, FsIntegrityCheckLevel);
                }
            }

            if (codeFs == null)
            {
                Logger.PrintError(LogClass.Loader, "No ExeFS found in NCA");

                return;
            }

            if (dataStorage == null)
            {
                Logger.PrintWarning(LogClass.Loader, "No RomFS found in NCA");
            }
            else
            {
                Device.FileSystem.SetRomFs(dataStorage.AsStream(FileAccess.Read));
            }

            LoadExeFs(codeFs, out Npdm metaData);

            Nacp ReadControlData()
            {
                IFileSystem controlRomfs = controlNca.OpenFileSystem(NcaSectionType.Data, FsIntegrityCheckLevel);

                IFile controlFile = controlRomfs.OpenFile("/control.nacp", OpenMode.Read);

                Nacp controlData = new Nacp(controlFile.AsStream());

                TitleName = CurrentTitle = controlData.Descriptions[(int)State.DesiredTitleLanguage].Title;
                TitleID = metaData.Aci0.TitleId.ToString("x16");

                CurrentTitle = controlData.Descriptions[(int)State.DesiredTitleLanguage].Title;

                if (string.IsNullOrWhiteSpace(CurrentTitle))
                {
                    TitleName = CurrentTitle = controlData.Descriptions.ToList().Find(x => !string.IsNullOrWhiteSpace(x.Title)).Title;
                }

                return controlData;
            }

            if (controlNca != null)
            {
                ReadControlData();
            }
            else
            {
                TitleID = CurrentTitle = metaData.Aci0.TitleId.ToString("x16");
            }
        }

        private void LoadExeFs(IFileSystem codeFs, out Npdm metaData)
        {
            if (codeFs.FileExists("/main.npdm"))
            {
                Logger.PrintInfo(LogClass.Loader, "Loading main.npdm...");

                metaData = new Npdm(codeFs.OpenFile("/main.npdm", OpenMode.Read).AsStream());
            }
            else
            {
                Logger.PrintWarning(LogClass.Loader, "NPDM file not found, using default values!");

                metaData = GetDefaultNpdm();
            }

            List<IExecutable> staticObjects = new List<IExecutable>();

            void LoadNso(string filename)
            {
                foreach (DirectoryEntry file in codeFs.EnumerateEntries($"{filename}*"))
                {
                    if (Path.GetExtension(file.Name) != string.Empty)
                    {
                        continue;
                    }

                    Logger.PrintInfo(LogClass.Loader, $"Loading {file.Name}...");

                    NxStaticObject staticObject = new NxStaticObject(codeFs.OpenFile(file.FullPath, OpenMode.Read).AsStream());

                    staticObjects.Add(staticObject);
                }
            }

            TitleID = CurrentTitle = metaData.Aci0.TitleId.ToString("x16");

            LoadNso("rtld");
            LoadNso("main");
            LoadNso("subsdk");
            LoadNso("sdk");

            ContentManager.LoadEntries();

            ProgramLoader.LoadStaticObjects(this, metaData, staticObjects.ToArray());
        }

        public void LoadProgram(string filePath)
        {
            Npdm metaData = GetDefaultNpdm();

            bool isNro = Path.GetExtension(filePath).ToLower() == ".nro";

            FileStream input = new FileStream(filePath, FileMode.Open);

            IExecutable staticObject;

            if (isNro)
            {
                NxRelocatableObject obj = new NxRelocatableObject(input);
                staticObject = obj;

                // homebrew NRO can actually have some data after the actual NRO
                if (input.Length > obj.FileSize)
                {
                    input.Position = obj.FileSize;

                    BinaryReader reader = new BinaryReader(input);

                    uint asetMagic = reader.ReadUInt32();

                    if (asetMagic == 0x54455341)
                    {
                        uint asetVersion = reader.ReadUInt32();
                        if (asetVersion == 0)
                        {
                            ulong iconOffset = reader.ReadUInt64();
                            ulong iconSize = reader.ReadUInt64();

                            ulong nacpOffset = reader.ReadUInt64();
                            ulong nacpSize = reader.ReadUInt64();

                            ulong romfsOffset = reader.ReadUInt64();
                            ulong romfsSize = reader.ReadUInt64();

                            if (romfsSize != 0)
                            {
                                Device.FileSystem.SetRomFs(new HomebrewRomFsStream(input, obj.FileSize + (long)romfsOffset));
                            }
                        }
                        else
                        {
                            Logger.PrintWarning(LogClass.Loader, $"Unsupported ASET header version found \"{asetVersion}\"");
                        }
                    }
                }
            }
            else
            {
                staticObject = new NxStaticObject(input);
            }

            ContentManager.LoadEntries();

            TitleID = CurrentTitle = metaData.Aci0.TitleId.ToString("x16");
            TitleName = metaData.TitleName;

            ProgramLoader.LoadStaticObjects(this, metaData, new IExecutable[] { staticObject });
        }

        private Npdm GetDefaultNpdm()
        {
            Assembly asm = Assembly.GetCallingAssembly();

            using (Stream npdmStream = asm.GetManifestResourceStream("Ryujinx.HLE.Homebrew.npdm"))
            {
                return new Npdm(npdmStream);
            }
        }

        public void LoadKeySet()
        {
            string keyFile        = null;
            string titleKeyFile   = null;
            string consoleKeyFile = null;

            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            LoadSetAtPath(Path.Combine(home, ".switch"));
            LoadSetAtPath(Device.FileSystem.GetSystemPath());

            KeySet = ExternalKeys.ReadKeyFile(keyFile, titleKeyFile, consoleKeyFile);

            void LoadSetAtPath(string basePath)
            {
                string localKeyFile        = Path.Combine(basePath,    "prod.keys");
                string localTitleKeyFile   = Path.Combine(basePath,   "title.keys");
                string localConsoleKeyFile = Path.Combine(basePath, "console.keys");

                if (File.Exists(localKeyFile))
                {
                    keyFile = localKeyFile;
                }

                if (File.Exists(localTitleKeyFile))
                {
                    titleKeyFile = localTitleKeyFile;
                }

                if (File.Exists(localConsoleKeyFile))
                {
                    consoleKeyFile = localConsoleKeyFile;
                }
            }
        }

        public void SignalVsync()
        {
            VsyncEvent.ReadableEvent.Signal();
        }

        internal long GetThreadUid()
        {
            return Interlocked.Increment(ref _threadUid) - 1;
        }

        internal long GetKipId()
        {
            return Interlocked.Increment(ref _kipId) - 1;
        }

        internal long GetProcessId()
        {
            return Interlocked.Increment(ref _processId) - 1;
        }

        public void EnableMultiCoreScheduling()
        {
            if (!_hasStarted)
            {
                Scheduler.MultiCoreScheduling = true;
            }
        }

        public void DisableMultiCoreScheduling()
        {
            if (!_hasStarted)
            {
                Scheduler.MultiCoreScheduling = false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Force all threads to exit.
                lock (Processes)
                {
                    foreach (KProcess process in Processes.Values)
                    {
                        process.StopAllThreads();
                    }
                }

                // It's only safe to release resources once all threads
                // have exited.
                ThreadCounter.Signal();
                ThreadCounter.Wait();

                Scheduler.Dispose();

                TimeManager.Dispose();

                Device.Unload();
            }
        }
    }
}