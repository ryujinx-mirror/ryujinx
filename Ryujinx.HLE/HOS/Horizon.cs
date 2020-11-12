using LibHac;
using LibHac.Bcat;
using LibHac.Fs;
using LibHac.FsSystem;
using Ryujinx.Audio.Renderer;
using Ryujinx.Audio.Renderer.Device;
using Ryujinx.Audio.Renderer.Integration;
using Ryujinx.Audio.Renderer.Server;
using Ryujinx.Common;
using Ryujinx.Configuration;
using Ryujinx.HLE.FileSystem.Content;
using Ryujinx.HLE.HOS.Font;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy;
using Ryujinx.HLE.HOS.Services.Apm;
using Ryujinx.HLE.HOS.Services.Arp;
using Ryujinx.HLE.HOS.Services.Audio.AudioRenderer;
using Ryujinx.HLE.HOS.Services.Mii;
using Ryujinx.HLE.HOS.Services.Nv;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvHostCtrl;
using Ryujinx.HLE.HOS.Services.Pcv.Bpc;
using Ryujinx.HLE.HOS.Services.Settings;
using Ryujinx.HLE.HOS.Services.Sm;
using Ryujinx.HLE.HOS.Services.SurfaceFlinger;
using Ryujinx.HLE.HOS.Services.Time.Clock;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Loaders.Executables;
using Ryujinx.HLE.Utilities;
using System;
using System.IO;
using System.Threading;

namespace Ryujinx.HLE.HOS
{
    using TimeServiceManager = Services.Time.TimeManager;

    public class Horizon : IDisposable
    {
        internal const int HidSize  = 0x40000;
        internal const int FontSize = 0x1100000;
        internal const int IirsSize = 0x8000;
        internal const int TimeSize = 0x1000;

        internal KernelContext KernelContext { get; }

        internal Switch Device { get; private set; }

        internal SurfaceFlinger SurfaceFlinger { get; private set; }
        internal AudioRendererManager AudioRendererManager { get; private set; }
        internal VirtualDeviceSessionRegistry AudioDeviceSessionRegistry { get; private set; }

        public SystemStateMgr State { get; private set; }

        internal PerformanceState PerformanceState { get; private set; }

        internal AppletStateMgr AppletState { get; private set; }

        internal KSharedMemory HidSharedMem  { get; private set; }
        internal KSharedMemory FontSharedMem { get; private set; }
        internal KSharedMemory IirsSharedMem { get; private set; }
        internal SharedFontManager Font { get; private set; }

        internal ContentManager ContentManager { get; private set; }

        internal KEvent VsyncEvent { get; private set; }

        internal KEvent DisplayResolutionChangeEvent { get; private set; }

        public Keyset KeySet => Device.FileSystem.KeySet;

#pragma warning disable CS0649
        private bool _hasStarted;
#pragma warning restore CS0649
        private bool _isDisposed;

        public bool EnablePtc { get; set; }

        public IntegrityCheckLevel FsIntegrityCheckLevel { get; set; }

        public int GlobalAccessLogMode { get; set; }

        internal ulong HidBaseAddress { get; private set; }

        internal NvHostSyncpt HostSyncpoint { get; private set; }

        internal LibHac.Horizon LibHacHorizonServer { get; private set; }
        internal HorizonClient LibHacHorizonClient { get; private set; }

        public Horizon(Switch device, ContentManager contentManager)
        {
            KernelContext = new KernelContext(device, device.Memory);

            Device = device;

            State = new SystemStateMgr();

            PerformanceState = new PerformanceState();

            // Note: This is not really correct, but with HLE of services, the only memory
            // region used that is used is Application, so we can use the other ones for anything.
            KMemoryRegionManager region = KernelContext.MemoryRegions[(int)MemoryRegion.NvServices];

            ulong hidPa  = region.Address;
            ulong fontPa = region.Address + HidSize;
            ulong iirsPa = region.Address + HidSize + FontSize;
            ulong timePa = region.Address + HidSize + FontSize + IirsSize;

            HidBaseAddress = hidPa - DramMemoryMap.DramBase;

            KPageList hidPageList  = new KPageList();
            KPageList fontPageList = new KPageList();
            KPageList iirsPageList = new KPageList();
            KPageList timePageList = new KPageList();

            hidPageList .AddRange(hidPa,  HidSize  / KMemoryManager.PageSize);
            fontPageList.AddRange(fontPa, FontSize / KMemoryManager.PageSize);
            iirsPageList.AddRange(iirsPa, IirsSize / KMemoryManager.PageSize);
            timePageList.AddRange(timePa, TimeSize / KMemoryManager.PageSize);

            HidSharedMem  = new KSharedMemory(KernelContext, hidPageList,  0, 0, MemoryPermission.Read);
            FontSharedMem = new KSharedMemory(KernelContext, fontPageList, 0, 0, MemoryPermission.Read);
            IirsSharedMem = new KSharedMemory(KernelContext, iirsPageList, 0, 0, MemoryPermission.Read);

            KSharedMemory timeSharedMemory = new KSharedMemory(KernelContext, timePageList, 0, 0, MemoryPermission.Read);

            TimeServiceManager.Instance.Initialize(device, this, timeSharedMemory, timePa - DramMemoryMap.DramBase, TimeSize);

            AppletState = new AppletStateMgr(this);

            AppletState.SetFocus(true);

            Font = new SharedFontManager(device, fontPa - DramMemoryMap.DramBase);

            IUserInterface.InitializePort(this);

            VsyncEvent = new KEvent(KernelContext);

            DisplayResolutionChangeEvent = new KEvent(KernelContext);

            ContentManager = contentManager;

            // TODO: use set:sys (and get external clock source id from settings)
            // TODO: use "time!standard_steady_clock_rtc_update_interval_minutes" and implement a worker thread to be accurate.
            UInt128 clockSourceId = new UInt128(Guid.NewGuid().ToByteArray());
            IRtcManager.GetExternalRtcValue(out ulong rtcValue);

            // We assume the rtc is system time.
            TimeSpanType systemTime = TimeSpanType.FromSeconds((long)rtcValue);

            // Configure and setup internal offset
            TimeSpanType internalOffset = TimeSpanType.FromSeconds(ConfigurationState.Instance.System.SystemTimeOffset);

            TimeSpanType systemTimeOffset = new TimeSpanType(systemTime.NanoSeconds + internalOffset.NanoSeconds);

            if (systemTime.IsDaylightSavingTime() && !systemTimeOffset.IsDaylightSavingTime())
            {
                internalOffset = internalOffset.AddSeconds(3600L);
            }
            else if (!systemTime.IsDaylightSavingTime() && systemTimeOffset.IsDaylightSavingTime())
            {
                internalOffset = internalOffset.AddSeconds(-3600L);
            }

            internalOffset = new TimeSpanType(-internalOffset.NanoSeconds);

            // First init the standard steady clock
            TimeServiceManager.Instance.SetupStandardSteadyClock(null, clockSourceId, systemTime, internalOffset, TimeSpanType.Zero, false);
            TimeServiceManager.Instance.SetupStandardLocalSystemClock(null, new SystemClockContext(), systemTime.ToSeconds());

            if (NxSettings.Settings.TryGetValue("time!standard_network_clock_sufficient_accuracy_minutes", out object standardNetworkClockSufficientAccuracyMinutes))
            {
                TimeSpanType standardNetworkClockSufficientAccuracy = new TimeSpanType((int)standardNetworkClockSufficientAccuracyMinutes * 60000000000);

                // The network system clock needs a valid system clock, as such we setup this system clock using the local system clock.
                TimeServiceManager.Instance.StandardLocalSystemClock.GetClockContext(null, out SystemClockContext localSytemClockContext);
                TimeServiceManager.Instance.SetupStandardNetworkSystemClock(localSytemClockContext, standardNetworkClockSufficientAccuracy);
            }

            TimeServiceManager.Instance.SetupStandardUserSystemClock(null, false, SteadyClockTimePoint.GetRandom());

            // FIXME: TimeZone shoud be init here but it's actually done in ContentManager

            TimeServiceManager.Instance.SetupEphemeralNetworkSystemClock();

            DatabaseImpl.Instance.InitializeDatabase(device);

            HostSyncpoint = new NvHostSyncpt(device);

            SurfaceFlinger = new SurfaceFlinger(device);

            ConfigurationState.Instance.System.EnableDockedMode.Event += OnDockedModeChange;

            InitLibHacHorizon();
            InitializeAudioRenderer();
        }

        private void InitializeAudioRenderer()
        {
            AudioRendererManager = new AudioRendererManager();
            AudioDeviceSessionRegistry = new VirtualDeviceSessionRegistry();

            IWritableEvent[] writableEvents = new IWritableEvent[RendererConstants.AudioRendererSessionCountMax];

            for (int i = 0; i < writableEvents.Length; i++)
            {
                KEvent systemEvent = new KEvent(KernelContext);

                writableEvents[i] = new AudioKernelEvent(systemEvent);
            }

            HardwareDevice[] devices = new HardwareDevice[RendererConstants.AudioRendererSessionCountMax];

            // TODO: don't hardcode those values.
            // TODO: keep the device somewhere and dispose it when exiting.
            // TODO: This is kind of wrong, we should have an high level API for that and mix all buffers between them.
            for (int i = 0; i < devices.Length; i++)
            {
                devices[i] = new AalHardwareDevice(i, Device.AudioOut, 2, RendererConstants.TargetSampleRate);
            }

            AudioRendererManager.Initialize(writableEvents, devices);
        }

        public void LoadKip(string kipPath)
        {
            using IStorage kipFile = new LocalStorage(kipPath, FileAccess.Read);

            ProgramLoader.LoadKip(KernelContext, new KipExecutable(kipFile));
        }

        private void InitLibHacHorizon()
        {
            LibHac.Horizon horizon = new LibHac.Horizon(null, Device.FileSystem.FsServer);

            horizon.CreateHorizonClient(out HorizonClient ryujinxClient).ThrowIfFailure();
            horizon.CreateHorizonClient(out HorizonClient bcatClient).ThrowIfFailure();

            ryujinxClient.Sm.RegisterService(new LibHacIReader(this), "arp:r").ThrowIfFailure();
            new BcatServer(bcatClient);

            LibHacHorizonServer = horizon;
            LibHacHorizonClient = ryujinxClient;
        }

        private void OnDockedModeChange(object sender, ReactiveEventArgs<bool> e)
        {
            if (e.NewValue != State.DockedMode)
            {
                State.DockedMode = e.NewValue;
                PerformanceState.PerformanceMode = State.DockedMode ? PerformanceMode.Boost : PerformanceMode.Default;

                AppletState.EnqueueMessage(MessageInfo.OperationModeChanged);
                AppletState.EnqueueMessage(MessageInfo.PerformanceModeChanged);
                SignalDisplayResolutionChange();

                // Reconfigure controllers
                Device.Hid.RefreshInputConfig(ConfigurationState.Instance.Hid.InputConfig.Value);
            }
        }

        public void SignalDisplayResolutionChange()
        {
            DisplayResolutionChangeEvent.ReadableEvent.Signal();
        }

        public void SignalVsync()
        {
            VsyncEvent.ReadableEvent.Signal();
        }

        public void EnableMultiCoreScheduling()
        {
            if (!_hasStarted)
            {
                KernelContext.Scheduler.MultiCoreScheduling = true;
            }
        }

        public void DisableMultiCoreScheduling()
        {
            if (!_hasStarted)
            {
                KernelContext.Scheduler.MultiCoreScheduling = false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed && disposing)
            {
                ConfigurationState.Instance.System.EnableDockedMode.Event -= OnDockedModeChange;

                _isDisposed = true;

                SurfaceFlinger.Dispose();

                KProcess terminationProcess = new KProcess(KernelContext);
                KThread terminationThread = new KThread(KernelContext);

                terminationThread.Initialize(0, 0, 0, 3, 0, terminationProcess, ThreadType.Kernel, () =>
                {
                    // Force all threads to exit.
                    lock (KernelContext.Processes)
                    {
                        foreach (KProcess process in KernelContext.Processes.Values)
                        {
                            process.Terminate();
                        }
                    }

                    // Exit ourself now!
                    KernelContext.Scheduler.ExitThread(terminationThread);
                    KernelContext.Scheduler.GetCurrentThread().Exit();
                    KernelContext.Scheduler.RemoveThread(terminationThread);
                });

                terminationThread.Start();

                // Wait until the thread is actually started.
                while (terminationThread.HostThread.ThreadState == ThreadState.Unstarted)
                {
                    Thread.Sleep(10);
                }

                // Wait until the termination thread is done terminating all the other threads.
                terminationThread.HostThread.Join();

                // Destroy nvservices channels as KThread could be waiting on some user events.
                // This is safe as KThread that are likely to call ioctls are going to be terminated by the post handler hook on the SVC facade.
                INvDrvServices.Destroy();

                AudioRendererManager.Dispose();

                KernelContext.Dispose();
            }
        }
    }
}
