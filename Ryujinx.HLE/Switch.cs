using LibHac.FsSystem;
using Ryujinx.Audio;
using Ryujinx.Configuration;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Input;
using System;
using System.Threading;

namespace Ryujinx.HLE
{
    public class Switch : IDisposable
    {
        internal IAalOutput AudioOut { get; private set; }

        internal DeviceMemory Memory { get; private set; }

        internal GpuContext Gpu { get; private set; }

        public VirtualFileSystem FileSystem { get; private set; }

        public Horizon System { get; private set; }

        public PerformanceStatistics Statistics { get; private set; }

        public Hid Hid { get; private set; }

        public bool EnableDeviceVsync { get; set; } = true;

        public AutoResetEvent VsyncEvent { get; private set; }

        public event EventHandler Finish;

        public Switch(IRenderer renderer, IAalOutput audioOut)
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

            Memory = new DeviceMemory();

            Gpu = new GpuContext(renderer);

            FileSystem = new VirtualFileSystem();

            System = new Horizon(this);

            Statistics = new PerformanceStatistics();

            Hid = new Hid(this, System.HidBaseAddress);

            VsyncEvent = new AutoResetEvent(true);
        }

        public void Initialize()
        {
            System.State.SetLanguage((SystemLanguage)ConfigurationState.Instance.System.Language.Value);

            EnableDeviceVsync = ConfigurationState.Instance.Graphics.EnableVsync;

            // TODO: Make this reloadable and implement Docking/Undocking logic.
            System.State.DockedMode = ConfigurationState.Instance.System.EnableDockedMode;

            if (ConfigurationState.Instance.System.EnableMulticoreScheduling)
            {
                System.EnableMultiCoreScheduling();
            }

            System.FsIntegrityCheckLevel = ConfigurationState.Instance.System.EnableFsIntegrityChecks
                ? IntegrityCheckLevel.ErrorOnInvalid
                : IntegrityCheckLevel.None;

            System.GlobalAccessLogMode = ConfigurationState.Instance.System.FsGlobalAccessLogMode;

            ServiceConfiguration.IgnoreMissingServices = ConfigurationState.Instance.System.IgnoreMissingServices;
        }

        public void LoadCart(string exeFsDir, string romFsFile = null)
        {
            System.LoadCart(exeFsDir, romFsFile);
        }

        public void LoadXci(string xciFile)
        {
            System.LoadXci(xciFile);
        }

        public void LoadNca(string ncaFile)
        {
            System.LoadNca(ncaFile);
        }

        public void LoadNsp(string nspFile)
        {
            System.LoadNsp(nspFile);
        }

        public void LoadProgram(string fileName)
        {
            System.LoadProgram(fileName);
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
            FileSystem.Dispose();

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
                VsyncEvent.Dispose();
            }
        }
    }
}
