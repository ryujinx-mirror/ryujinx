using LibHac;
using LibHac.Account;
using LibHac.Common;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.FsSystem.NcaUtils;
using LibHac.Ncm;
using LibHac.Ns;
using LibHac.Spl;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.FileSystem.Content;
using Ryujinx.HLE.HOS.Font;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Pcv.Bpc;
using Ryujinx.HLE.HOS.Services.Settings;
using Ryujinx.HLE.HOS.Services.Sm;
using Ryujinx.HLE.HOS.Services.Time.Clock;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Loaders.Npdm;
using Ryujinx.HLE.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using TimeServiceManager = Ryujinx.HLE.HOS.Services.Time.TimeManager;
using NxStaticObject     = Ryujinx.HLE.Loaders.Executables.NxStaticObject;

using static LibHac.Fs.ApplicationSaveDataManagement;

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
        internal SharedFontManager Font { get; private set; }

        internal ContentManager ContentManager { get; private set; }

        internal KEvent VsyncEvent { get; private set; }

        public Keyset KeySet => Device.FileSystem.KeySet;

        private bool _hasStarted;

        public BlitStruct<ApplicationControlProperty> ControlData { get; set; }

        public string TitleName { get; private set; }

        public ulong  TitleId { get; private set; }
        public string TitleIdText => TitleId.ToString("x16");

        public IntegrityCheckLevel FsIntegrityCheckLevel { get; set; }

        public int GlobalAccessLogMode { get; set; }

        internal long HidBaseAddress { get; private set; }

        public Horizon(Switch device, ContentManager contentManager)
        {
            ControlData = new BlitStruct<ApplicationControlProperty>(1);

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

            KSharedMemory timeSharedMemory = new KSharedMemory(this, timePageList, 0, 0, MemoryPermission.Read);

            TimeServiceManager.Instance.Initialize(device, this, timeSharedMemory, (long)(timePa - DramMemoryMap.DramBase), TimeSize);

            AppletState = new AppletStateMgr(this);

            AppletState.SetFocus(true);

            Font = new SharedFontManager(device, (long)(fontPa - DramMemoryMap.DramBase));

            IUserInterface.InitializePort(this);

            VsyncEvent = new KEvent(this);

            ContentManager = contentManager;

            // TODO: use set:sys (and get external clock source id from settings)
            // TODO: use "time!standard_steady_clock_rtc_update_interval_minutes" and implement a worker thread to be accurate.
            UInt128 clockSourceId = new UInt128(Guid.NewGuid().ToByteArray());
            IRtcManager.GetExternalRtcValue(out ulong rtcValue);

            // We assume the rtc is system time.
            TimeSpanType systemTime = TimeSpanType.FromSeconds((long)rtcValue);

            // First init the standard steady clock
            TimeServiceManager.Instance.SetupStandardSteadyClock(null, clockSourceId, systemTime, TimeSpanType.Zero, TimeSpanType.Zero, false);
            TimeServiceManager.Instance.SetupStandardLocalSystemClock(null, new SystemClockContext(), systemTime.ToSeconds());

            if (NxSettings.Settings.TryGetValue("time!standard_network_clock_sufficient_accuracy_minutes", out object standardNetworkClockSufficientAccuracyMinutes))
            {
                TimeSpanType standardNetworkClockSufficientAccuracy = new TimeSpanType((int)standardNetworkClockSufficientAccuracyMinutes * 60000000000);

                TimeServiceManager.Instance.SetupStandardNetworkSystemClock(new SystemClockContext(), standardNetworkClockSufficientAccuracy);
            }

            TimeServiceManager.Instance.SetupStandardUserSystemClock(null, false, SteadyClockTimePoint.GetRandom());

            // FIXME: TimeZone shoud be init here but it's actually done in ContentManager

            TimeServiceManager.Instance.SetupEphemeralNetworkSystemClock();
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

            ContentManager.LoadEntries(Device);

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

            foreach (DirectoryEntryEx ticketEntry in securePartition.EnumerateEntries("/", "*.tik"))
            {
                Result result = securePartition.OpenFile(out IFile ticketFile, ticketEntry.FullPath, OpenMode.Read);

                if (result.IsSuccess())
                {
                    Ticket ticket = new Ticket(ticketFile.AsStream());

                    KeySet.ExternalKeySet.Add(new RightsId(ticket.RightsId), new AccessKey(ticket.GetTitleKey(KeySet)));
                }
            }

            foreach (DirectoryEntryEx fileEntry in securePartition.EnumerateEntries("/", "*.nca"))
            {
                Result result = securePartition.OpenFile(out IFile ncaFile, fileEntry.FullPath, OpenMode.Read);
                if (result.IsFailure())
                {
                    continue;
                }

                Nca nca = new Nca(KeySet, ncaFile.AsStorage());

                if (nca.Header.ContentType == NcaContentType.Program)
                {
                    int dataIndex = Nca.GetSectionIndexFromType(NcaSectionType.Data, NcaContentType.Program);

                    if (nca.Header.GetFsHeader(dataIndex).IsPatchSection())
                    {
                        patchNca = nca;
                    }
                    else
                    {
                        mainNca = nca;
                    }
                }
                else if (nca.Header.ContentType == NcaContentType.Control)
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
            else
            {
                ControlData.ByteSpan.Clear();
            }

            return (mainNca, patchNca, controlNca);
        }

        public void ReadControlData(Nca controlNca)
        {
            IFileSystem controlFs = controlNca.OpenFileSystem(NcaSectionType.Data, FsIntegrityCheckLevel);

            Result result = controlFs.OpenFile(out IFile controlFile, "/control.nacp", OpenMode.Read);

            if (result.IsSuccess())
            {
                result = controlFile.Read(out long bytesRead, 0, ControlData.ByteSpan, ReadOption.None);

                if (result.IsSuccess() && bytesRead == ControlData.ByteSpan.Length)
                {
                    TitleName = ControlData.Value
                        .Titles[(int) State.DesiredTitleLanguage].Name.ToString();

                    if (string.IsNullOrWhiteSpace(TitleName))
                    {
                        TitleName = ControlData.Value.Titles.ToArray()
                            .FirstOrDefault(x => x.Name[0] != 0).Name.ToString();
                    }
                }
            }
            else
            {
                ControlData.ByteSpan.Clear();
            }
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

            foreach (DirectoryEntryEx ticketEntry in nsp.EnumerateEntries("/", "*.tik"))
            {
                Result result = nsp.OpenFile(out IFile ticketFile, ticketEntry.FullPath, OpenMode.Read);

                if (result.IsSuccess())
                {
                    Ticket ticket = new Ticket(ticketFile.AsStream());

                    KeySet.ExternalKeySet.Add(new RightsId(ticket.RightsId), new AccessKey(ticket.GetTitleKey(KeySet)));
                }
            }

            Nca mainNca    = null;
            Nca patchNca   = null;
            Nca controlNca = null;

            foreach (DirectoryEntryEx fileEntry in nsp.EnumerateEntries("/", "*.nca"))
            {
                nsp.OpenFile(out IFile ncaFile, fileEntry.FullPath, OpenMode.Read).ThrowIfFailure();

                Nca nca = new Nca(KeySet, ncaFile.AsStorage());

                if (nca.Header.ContentType == NcaContentType.Program)
                {
                    int dataIndex = Nca.GetSectionIndexFromType(NcaSectionType.Data, NcaContentType.Program);

                    if (nca.Header.GetFsHeader(dataIndex).IsPatchSection())
                    {
                        patchNca = nca;
                    }
                    else
                    {
                        mainNca = nca;
                    }
                }
                else if (nca.Header.ContentType == NcaContentType.Control)
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
            if (mainNca.Header.ContentType != NcaContentType.Program)
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
            
            TitleId = metaData.Aci0.TitleId;

            if (controlNca != null)
            {
                ReadControlData(controlNca);
            }
            else
            {
                ControlData.ByteSpan.Clear();
            }

            if (TitleId != 0)
            {
                EnsureSaveData(new TitleId(TitleId));
            }
        }

        private void LoadExeFs(IFileSystem codeFs, out Npdm metaData)
        {
            Result result = codeFs.OpenFile(out IFile npdmFile, "/main.npdm", OpenMode.Read);

            if (result == ResultFs.PathNotFound)
            {
                Logger.PrintWarning(LogClass.Loader, "NPDM file not found, using default values!");

                metaData = GetDefaultNpdm();
            }
            else
            {
                metaData = new Npdm(npdmFile.AsStream());
            }

            List<IExecutable> staticObjects = new List<IExecutable>();

            void LoadNso(string filename)
            {
                foreach (DirectoryEntryEx file in codeFs.EnumerateEntries("/", $"{filename}*"))
                {
                    if (Path.GetExtension(file.Name) != string.Empty)
                    {
                        continue;
                    }

                    Logger.PrintInfo(LogClass.Loader, $"Loading {file.Name}...");

                    codeFs.OpenFile(out IFile nsoFile, file.FullPath, OpenMode.Read).ThrowIfFailure();

                    NxStaticObject staticObject = new NxStaticObject(nsoFile.AsStream());

                    staticObjects.Add(staticObject);
                }
            }

            TitleId = metaData.Aci0.TitleId;

            LoadNso("rtld");
            LoadNso("main");
            LoadNso("subsdk");
            LoadNso("sdk");

            ContentManager.LoadEntries(Device);

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
                            ulong iconSize   = reader.ReadUInt64();

                            ulong nacpOffset = reader.ReadUInt64();
                            ulong nacpSize   = reader.ReadUInt64();

                            ulong romfsOffset = reader.ReadUInt64();
                            ulong romfsSize   = reader.ReadUInt64();

                            if (romfsSize != 0)
                            {
                                Device.FileSystem.SetRomFs(new HomebrewRomFsStream(input, obj.FileSize + (long)romfsOffset));
                            }

                            if (nacpSize != 0)
                            {
                                input.Seek(obj.FileSize + (long)nacpOffset, SeekOrigin.Begin);

                                reader.Read(ControlData.ByteSpan);

                                ref ApplicationControlProperty nacp = ref ControlData.Value;

                                metaData.TitleName = nacp.Titles[(int)State.DesiredTitleLanguage].Name.ToString();

                                if (string.IsNullOrWhiteSpace(metaData.TitleName))
                                {
                                    metaData.TitleName = nacp.Titles.ToArray().FirstOrDefault(x => x.Name[0] != 0).Name.ToString();
                                }

                                if (nacp.PresenceGroupId != 0)
                                {
                                    metaData.Aci0.TitleId = nacp.PresenceGroupId;
                                }
                                else if (nacp.SaveDataOwnerId.Value != 0)
                                {
                                    metaData.Aci0.TitleId = nacp.SaveDataOwnerId.Value;
                                }
                                else if (nacp.AddOnContentBaseId != 0)
                                {
                                    metaData.Aci0.TitleId = nacp.AddOnContentBaseId - 0x1000;
                                }
                                else
                                {
                                    metaData.Aci0.TitleId = 0000000000000000;
                                }
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

            ContentManager.LoadEntries(Device);

            TitleName = metaData.TitleName;
            TitleId   = metaData.Aci0.TitleId;

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

        private Result EnsureSaveData(TitleId titleId)
        {
            Logger.PrintInfo(LogClass.Application, "Ensuring required savedata exists.");

            UInt128 lastOpenedUser = State.Account.LastOpenedUser.UserId;
            Uid user = new Uid((ulong)lastOpenedUser.Low, (ulong)lastOpenedUser.High);

            ref ApplicationControlProperty control = ref ControlData.Value;

            if (LibHac.Util.IsEmpty(ControlData.ByteSpan))
            {
                // If the current application doesn't have a loaded control property, create a dummy one
                // and set the savedata sizes so a user savedata will be created.
                control = ref new BlitStruct<ApplicationControlProperty>(1).Value;

                // The set sizes don't actually matter as long as they're non-zero because we use directory savedata.
                control.UserAccountSaveDataSize = 0x4000;
                control.UserAccountSaveDataJournalSize = 0x4000;

                Logger.PrintWarning(LogClass.Application,
                    "No control file was found for this game. Using a dummy one instead. This may cause inaccuracies in some games.");
            }

            Result rc = EnsureApplicationSaveData(Device.FileSystem.FsClient, out _, titleId, ref ControlData.Value, ref user);

            if (rc.IsFailure())
            {
                Logger.PrintError(LogClass.Application, $"Error calling EnsureApplicationSaveData. Result code {rc.ToStringWithName()}");
            }

            return rc;
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
                KProcess terminationProcess = new KProcess(this);

                KThread terminationThread = new KThread(this);

                terminationThread.Initialize(0, 0, 0, 3, 0, terminationProcess, ThreadType.Kernel, () =>
                {
                    // Force all threads to exit.
                    lock (Processes)
                    {
                        foreach (KProcess process in Processes.Values)
                        {
                            process.Terminate();
                        }
                    }

                    // Exit ourself now!
                    Scheduler.ExitThread(terminationThread);
                    Scheduler.GetCurrentThread().Exit();
                    Scheduler.RemoveThread(terminationThread);
                });

                terminationThread.Start();

                // Signal the vsync event to avoid issues of KThread waiting on it.
                if (Device.EnableDeviceVsync)
                {
                    Device.VsyncEvent.Set();
                }

                // This is needed as the IPC Dummy KThread is also counted in the ThreadCounter.
                ThreadCounter.Signal();

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