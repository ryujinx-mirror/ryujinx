using LibHac.FsSystem;
using Ryujinx.Audio;
using Ryujinx.Configuration;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu;
using Ryujinx.Graphics.Host1x;
using Ryujinx.Graphics.Nvdec;
using Ryujinx.Graphics.Vic;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.FileSystem.Content;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services;
using Ryujinx.HLE.HOS.Services.Hid;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.Memory;
using System;

namespace Ryujinx.HLE
{
    public class Switch : IDisposable
    {
        public IAalOutput AudioOut { get; private set; }

        internal MemoryBlock Memory { get; private set; }

        public GpuContext Gpu { get; private set; }

        internal Host1xDevice Host1x { get; }

        public VirtualFileSystem FileSystem { get; private set; }

        public Horizon System { get; private set; }

        public ApplicationLoader Application { get; }

        public PerformanceStatistics Statistics { get; private set; }

        public UserChannelPersistence UserChannelPersistence { get; }

        public Hid Hid { get; private set; }

        public IHostUiHandler UiHandler { get; set; }

        public bool EnableDeviceVsync { get; set; } = true;

        public Switch(VirtualFileSystem fileSystem, ContentManager contentManager, UserChannelPersistence userChannelPersistence, IRenderer renderer, IAalOutput audioOut)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (audioOut == null)
            {
                throw new ArgumentNullException(nameof(audioOut));
            }

            if (userChannelPersistence == null)
            {
                throw new ArgumentNullException(nameof(userChannelPersistence));
            }

            UserChannelPersistence = userChannelPersistence;

            AudioOut = audioOut;

            Memory = new MemoryBlock(1UL << 32);

            Gpu = new GpuContext(renderer);

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

            FileSystem = fileSystem;

            System = new Horizon(this, contentManager);

            Statistics = new PerformanceStatistics();

            Hid = new Hid(this, System.HidBaseAddress);
            Hid.InitDevices();

            Application = new ApplicationLoader(this, fileSystem, contentManager);
        }

        public void Initialize()
        {
            System.State.SetLanguage((SystemLanguage)ConfigurationState.Instance.System.Language.Value);

            System.State.SetRegion((RegionCode)ConfigurationState.Instance.System.Region.Value);

            EnableDeviceVsync = ConfigurationState.Instance.Graphics.EnableVsync;

            System.State.DockedMode = ConfigurationState.Instance.System.EnableDockedMode;

            if (ConfigurationState.Instance.System.EnableMulticoreScheduling)
            {
                System.EnableMultiCoreScheduling();
            }

            System.EnablePtc = ConfigurationState.Instance.System.EnablePtc;

            System.FsIntegrityCheckLevel = GetIntegrityCheckLevel();

            System.GlobalAccessLogMode = ConfigurationState.Instance.System.FsGlobalAccessLogMode;

            ServiceConfiguration.IgnoreMissingServices = ConfigurationState.Instance.System.IgnoreMissingServices;

            // Configure controllers
            Hid.RefreshInputConfig(ConfigurationState.Instance.Hid.InputConfig.Value);
            ConfigurationState.Instance.Hid.InputConfig.Event += Hid.RefreshInputConfigEvent;
        }

        public static IntegrityCheckLevel GetIntegrityCheckLevel()
        {
            return ConfigurationState.Instance.System.EnableFsIntegrityChecks
                ? IntegrityCheckLevel.ErrorOnInvalid
                : IntegrityCheckLevel.None;
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
                ConfigurationState.Instance.Hid.InputConfig.Event -= Hid.RefreshInputConfigEvent;

                System.Dispose();
                Host1x.Dispose();
                AudioOut.Dispose();
                FileSystem.Unload();
                Memory.Dispose();
            }
        }
    }
}
