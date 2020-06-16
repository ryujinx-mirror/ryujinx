using LibHac.FsSystem;
using Ryujinx.Audio;
using Ryujinx.Configuration;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.FileSystem.Content;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services;
using Ryujinx.HLE.HOS.Services.Hid;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.Memory;
using System;
using System.Threading;

namespace Ryujinx.HLE
{
    public class Switch : IDisposable
    {
        public IAalOutput AudioOut { get; private set; }

        internal MemoryBlock Memory { get; private set; }

        public GpuContext Gpu { get; private set; }

        public VirtualFileSystem FileSystem { get; private set; }

        public Horizon System { get; private set; }

        public ApplicationLoader Application { get; }

        public PerformanceStatistics Statistics { get; private set; }

        public Hid Hid { get; private set; }

        public bool EnableDeviceVsync { get; set; } = true;

        public Switch(VirtualFileSystem fileSystem, ContentManager contentManager, IRenderer renderer, IAalOutput audioOut)
        {
            if (renderer == null)
            {
                throw new ArgumentNullException(nameof(renderer));
            }

            if (audioOut == null)
            {
                throw new ArgumentNullException(nameof(audioOut));
            }

            AudioOut = audioOut;

            Memory = new MemoryBlock(1UL << 32);

            Gpu = new GpuContext(renderer);

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
            return Gpu.DmaPusher.WaitForCommands();
        }

        public void ProcessFrame()
        {
            Gpu.DmaPusher.DispatchCalls();
        }

        public void PresentFrame(Action swapBuffersCallback)
        {
            Gpu.Window.Present(swapBuffersCallback);
        }

        internal void Unload()
        {
            FileSystem.Unload();

            Memory.Dispose();
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
                AudioOut.Dispose();
            }
        }
    }
}
