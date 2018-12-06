using Ryujinx.Audio;
using Ryujinx.Graphics;
using Ryujinx.Graphics.Gal;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.Input;
using System;
using System.Threading;

namespace Ryujinx.HLE
{
    public class Switch : IDisposable
    {
        internal IAalOutput AudioOut { get; private set; }

        internal DeviceMemory Memory { get; private set; }

        internal NvGpu Gpu { get; private set; }

        internal VirtualFileSystem FileSystem { get; private set; }

        public Horizon System { get; private set; }

        public PerformanceStatistics Statistics { get; private set; }

        public Hid Hid { get; private set; }

        public bool EnableDeviceVsync { get; set; } = true;

        public AutoResetEvent VsyncEvent { get; private set; }

        public event EventHandler Finish;

        public Switch(IGalRenderer renderer, IAalOutput audioOut)
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

            Gpu = new NvGpu(renderer);

            FileSystem = new VirtualFileSystem();

            System = new Horizon(this);

            Statistics = new PerformanceStatistics();

            Hid = new Hid(this, System.HidBaseAddress);

            VsyncEvent = new AutoResetEvent(true);
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
            return Gpu.Pusher.WaitForCommands();
        }

        public void ProcessFrame()
        {
            Gpu.Pusher.DispatchCalls();
        }

        internal void Unload()
        {
            FileSystem.Dispose();

            Memory.Dispose();
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
