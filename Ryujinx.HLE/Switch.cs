using Ryujinx.Audio.Backends.CompatLayer;
using Ryujinx.Audio.Integration;
using Ryujinx.Graphics.Gpu;
using Ryujinx.Graphics.Host1x;
using Ryujinx.Graphics.Nvdec;
using Ryujinx.Graphics.Vic;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services.Apm;
using Ryujinx.HLE.HOS.Services.Hid;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices;
using Ryujinx.Memory;
using System;

namespace Ryujinx.HLE
{
    public class Switch : IDisposable
    {
        public HLEConfiguration Configuration { get; }

        public IHardwareDeviceDriver AudioDeviceDriver { get; }

        internal MemoryBlock Memory { get; }

        public GpuContext Gpu { get; }

        internal NvMemoryAllocator MemoryAllocator { get; }

        internal Host1xDevice Host1x { get; }

        public VirtualFileSystem FileSystem => Configuration.VirtualFileSystem;

        public Horizon System { get; }

        public ApplicationLoader Application { get; }

        public PerformanceStatistics Statistics { get; }

        public Hid Hid { get; }

        public TamperMachine TamperMachine { get; }

        public IHostUiHandler UiHandler { get; }

        public bool EnableDeviceVsync { get; set; } = true;

        public Switch(HLEConfiguration configuration)
        {
            if (configuration.GpuRenderer == null)
            {
                throw new ArgumentNullException(nameof(configuration.GpuRenderer));
            }

            if (configuration.AudioDeviceDriver == null)
            {
                throw new ArgumentNullException(nameof(configuration.AudioDeviceDriver));
            }

            if (configuration.UserChannelPersistence== null)
            {
                throw new ArgumentNullException(nameof(configuration.UserChannelPersistence));
            }

            Configuration = configuration;

            UiHandler = configuration.HostUiHandler;

            AudioDeviceDriver = new CompatLayerHardwareDeviceDriver(configuration.AudioDeviceDriver);

            Memory = new MemoryBlock(configuration.MemoryConfiguration.ToDramSize(), MemoryAllocationFlags.Reserve);

            Gpu = new GpuContext(configuration.GpuRenderer);

            MemoryAllocator = new NvMemoryAllocator();

            Host1x = new Host1xDevice(Gpu.Synchronization);
            var nvdec = new NvdecDevice(Gpu.MemoryManager);
            var vic = new VicDevice(Gpu.MemoryManager);
            Host1x.RegisterDevice(ClassId.Nvdec, nvdec);
            Host1x.RegisterDevice(ClassId.Vic, vic);

            nvdec.FrameDecoded += (FrameDecodedEventArgs e) =>
            {
                // FIXME:
                // Figure out what is causing frame ordering issues on H264.
                // For now this is needed as workaround.
                if (e.CodecId == CodecId.H264)
                {
                    vic.SetSurfaceOverride(e.LumaOffset, e.ChromaOffset, 0);
                }
                else
                {
                    vic.DisableSurfaceOverride();
                }
            };

            System = new Horizon(this);
            System.InitializeServices();

            Statistics = new PerformanceStatistics();

            Hid = new Hid(this, System.HidStorage);
            Hid.InitDevices();

            Application = new ApplicationLoader(this);

            TamperMachine = new TamperMachine();

            Initialize();
        }

        private void Initialize()
        {
            System.State.SetLanguage(Configuration.SystemLanguage);

            System.State.SetRegion(Configuration.Region);

            EnableDeviceVsync = Configuration.EnableVsync;

            System.State.DockedMode = Configuration.EnableDockedMode;

            System.PerformanceState.PerformanceMode = System.State.DockedMode ? PerformanceMode.Boost : PerformanceMode.Default;

            System.EnablePtc = Configuration.EnablePtc;

            System.FsIntegrityCheckLevel = Configuration.FsIntegrityCheckLevel;

            System.GlobalAccessLogMode = Configuration.FsGlobalAccessLogMode;
        }

        public void LoadCart(string exeFsDir, string romFsFile = null)
        {
            Application.LoadCart(exeFsDir, romFsFile);
        }

        public void LoadXci(string xciFile)
        {
            Application.LoadXci(xciFile);
        }

        public void LoadNca(string ncaFile)
        {
            Application.LoadNca(ncaFile);
        }

        public void LoadNsp(string nspFile)
        {
            Application.LoadNsp(nspFile);
        }

        public void LoadProgram(string fileName)
        {
            Application.LoadProgram(fileName);
        }

        public bool WaitFifo()
        {
            return Gpu.GPFifo.WaitForCommands();
        }

        public void ProcessFrame()
        {
            Gpu.Renderer.PreFrame();

            Gpu.GPFifo.DispatchCalls();
        }

        public bool ConsumeFrameAvailable()
        {
            return Gpu.Window.ConsumeFrameAvailable();
        }

        public void PresentFrame(Action swapBuffersCallback)
        {
            Gpu.Window.Present(swapBuffersCallback);
        }

        public void DisposeGpu()
        {
            Gpu.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                System.Dispose();
                Host1x.Dispose();
                AudioDeviceDriver.Dispose();
                FileSystem.Unload();
                Memory.Dispose();
            }
        }
    }
}
