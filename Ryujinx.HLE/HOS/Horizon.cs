using LibHac;
using LibHac.Bcat;
using LibHac.Fs;
using LibHac.FsSystem;
using Ryujinx.Audio;
using Ryujinx.Audio.Input;
using Ryujinx.Audio.Integration;
using Ryujinx.Audio.Output;
using Ryujinx.Audio.Renderer.Device;
using Ryujinx.Audio.Renderer.Server;
using Ryujinx.Common;
using Ryujinx.Configuration;
using Ryujinx.HLE.FileSystem.Content;
using Ryujinx.HLE.HOS.Font;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services;
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
using System.Linq;
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
        internal AudioManager AudioManager { get; private set; }
        internal AudioOutputManager AudioOutputManager { get; private set; }
        internal AudioInputManager AudioInputManager { get; private set; }
        internal AudioRendererManager AudioRendererManager { get; private set; }
        internal VirtualDeviceSessionRegistry AudioDeviceSessionRegistry { get; private set; }

        public SystemStateMgr State { get; private set; }

        internal PerformanceState PerformanceState { get; private set; }

        internal AppletStateMgr AppletState { get; private set; }

        internal ServerBase BsdServer { get; private set; }
        internal ServerBase AudRenServer { get; private set; }
        internal ServerBase AudOutServer { get; private set; }
        internal ServerBase HidServer { get; private set; }
        internal ServerBase NvDrvServer { get; private set; }
        internal ServerBase TimeServer { get; private set; }
        internal ServerBase ViServer { get; private set; }
        internal ServerBase ViServerM { get; private set; }
        internal ServerBase ViServerS { get; private set; }

        internal KSharedMemory HidSharedMem  { get; private set; }
        internal KSharedMemory FontSharedMem { get; private set; }
        internal KSharedMemory IirsSharedMem { get; private set; }
        internal SharedFontManager Font { get; private set; }

        internal ContentManager ContentManager { get; private set; }

        internal KEvent VsyncEvent { get; private set; }

        internal KEvent DisplayResolutionChangeEvent { get; private set; }

        public Keyset KeySet => Device.FileSystem.KeySet;

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

            HidSharedMem  = new KSharedMemory(KernelContext, hidPageList,  0, 0, KMemoryPermission.Read);
            FontSharedMem = new KSharedMemory(KernelContext, fontPageList, 0, 0, KMemoryPermission.Read);
            IirsSharedMem = new KSharedMemory(KernelContext, iirsPageList, 0, 0, KMemoryPermission.Read);

            KSharedMemory timeSharedMemory = new KSharedMemory(KernelContext, timePageList, 0, 0, KMemoryPermission.Read);

            TimeServiceManager.Instance.Initialize(device, this, timeSharedMemory, timePa - DramMemoryMap.DramBase, TimeSize);

            AppletState = new AppletStateMgr(this);

            AppletState.SetFocus(true);

            Font = new SharedFontManager(device, fontPa - DramMemoryMap.DramBase);

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
            AudioManager = new AudioManager();
            AudioOutputManager = new AudioOutputManager();
            AudioInputManager = new AudioInputManager();
            AudioRendererManager = new AudioRendererManager();
            AudioDeviceSessionRegistry = new VirtualDeviceSessionRegistry();

            IWritableEvent[] audioOutputRegisterBufferEvents = new IWritableEvent[Constants.AudioOutSessionCountMax];

            for (int i = 0; i < audioOutputRegisterBufferEvents.Length; i++)
            {
                KEvent registerBufferEvent = new KEvent(KernelContext);

                audioOutputRegisterBufferEvents[i] = new AudioKernelEvent(registerBufferEvent);
            }

            AudioOutputManager.Initialize(Device.AudioDeviceDriver, audioOutputRegisterBufferEvents);

            IWritableEvent[] audioInputRegisterBufferEvents = new IWritableEvent[Constants.AudioInSessionCountMax];

            for (int i = 0; i < audioInputRegisterBufferEvents.Length; i++)
            {
                KEvent registerBufferEvent = new KEvent(KernelContext);

                audioInputRegisterBufferEvents[i] = new AudioKernelEvent(registerBufferEvent);
            }

            AudioInputManager.Initialize(Device.AudioDeviceDriver, audioInputRegisterBufferEvents);

            IWritableEvent[] systemEvents = new IWritableEvent[Constants.AudioRendererSessionCountMax];

            for (int i = 0; i < systemEvents.Length; i++)
            {
                KEvent systemEvent = new KEvent(KernelContext);

                systemEvents[i] = new AudioKernelEvent(systemEvent);
            }

            AudioManager.Initialize(Device.AudioDeviceDriver.GetUpdateRequiredEvent(), AudioOutputManager.Update, AudioInputManager.Update);

            AudioRendererManager.Initialize(systemEvents, Device.AudioDeviceDriver);

            AudioManager.Start();
        }

        public void InitializeServices()
        {
            IUserInterface sm = new IUserInterface(KernelContext);

            // Wait until SM server thread is done with initialization,
            // only then doing connections to SM is safe.
            sm.Server.InitDone.WaitOne();
            sm.Server.InitDone.Dispose();

            BsdServer = new ServerBase(KernelContext, "BsdServer");
            AudRenServer = new ServerBase(KernelContext, "AudioRendererServer");
            AudOutServer = new ServerBase(KernelContext, "AudioOutServer");
            HidServer = new ServerBase(KernelContext, "HidServer");
            NvDrvServer = new ServerBase(KernelContext, "NvservicesServer");
            TimeServer = new ServerBase(KernelContext, "TimeServer");
            ViServer = new ServerBase(KernelContext, "ViServerU");
            ViServerM = new ServerBase(KernelContext, "ViServerM");
            ViServerS = new ServerBase(KernelContext, "ViServerS");
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

                AppletState.Messages.Enqueue(MessageInfo.OperationModeChanged);
                AppletState.Messages.Enqueue(MessageInfo.PerformanceModeChanged);
                AppletState.MessageEvent.ReadableEvent.Signal();

                SignalDisplayResolutionChange();

                // Reconfigure controllers
                Device.Hid.RefreshInputConfig(ConfigurationState.Instance.Hid.InputConfig.Value);
            }
        }

        public void SimulateWakeUpMessage()
        {
            AppletState.Messages.Enqueue(MessageInfo.Resume);
            AppletState.MessageEvent.ReadableEvent.Signal();
        }

        public void SignalDisplayResolutionChange()
        {
            DisplayResolutionChangeEvent.ReadableEvent.Signal();
        }

        public void SignalVsync()
        {
            VsyncEvent.ReadableEvent.Signal();
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

                KProcess terminationProcess = new KProcess(KernelContext);
                KThread terminationThread = new KThread(KernelContext);

                terminationThread.Initialize(0, 0, 0, 3, 0, terminationProcess, ThreadType.Kernel, () =>
                {
                    // Force all threads to exit.
                    lock (KernelContext.Processes)
                    {
                        // Terminate application.
                        foreach (KProcess process in KernelContext.Processes.Values.Where(x => x.Flags.HasFlag(ProcessCreationFlags.IsApplication)))
                        {
                            process.Terminate();
                        }

                        // The application existed, now surface flinger can exit too.
                        SurfaceFlinger.Dispose();

                        // Terminate HLE services (must be done after the application is already terminated,
                        // otherwise the application will receive errors due to service termination.
                        foreach (KProcess process in KernelContext.Processes.Values.Where(x => !x.Flags.HasFlag(ProcessCreationFlags.IsApplication)))
                        {
                            process.Terminate();
                        }
                    }

                    // Exit ourself now!
                    KernelStatic.GetCurrentThread().Exit();
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

                AudioManager.Dispose();
                AudioOutputManager.Dispose();
                AudioInputManager.Dispose();

                AudioRendererManager.Dispose();

                KernelContext.Dispose();
            }
        }
    }
}
