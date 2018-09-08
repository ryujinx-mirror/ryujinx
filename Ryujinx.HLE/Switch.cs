using Ryujinx.Audio;
using Ryujinx.Graphics;
using Ryujinx.Graphics.Gal;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.Input;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.Memory;
using System;

namespace Ryujinx.HLE
{
    public class Switch : IDisposable
    {
        internal IAalOutput AudioOut { get; private set; }

        public Logger Log { get; private set; }

        internal DeviceMemory Memory { get; private set; }

        internal NvGpu Gpu { get; private set; }

        internal VirtualFileSystem FileSystem { get; private set; }

        public Horizon System { get; private set; }

        public PerformanceStatistics Statistics { get; private set; }

        public Hid Hid { get; private set; }

        public Switch(IGalRenderer Renderer, IAalOutput AudioOut)
        {
            if (Renderer == null)
            {
                throw new ArgumentNullException(nameof(Renderer));
            }

            if (AudioOut == null)
            {
                throw new ArgumentNullException(nameof(AudioOut));
            }

            this.AudioOut = AudioOut;

            Log = new Logger();

            Memory = new DeviceMemory();

            Gpu = new NvGpu(Renderer);

            FileSystem = new VirtualFileSystem();

            System = new Horizon(this);

            Statistics = new PerformanceStatistics();

            Hid = new Hid(this, System.HidSharedMem.PA);
        }

        public void LoadCart(string ExeFsDir, string RomFsFile = null)
        {
            System.LoadCart(ExeFsDir, RomFsFile);
        }

        public void LoadXci(string XciFile)
        {
            System.LoadXci(XciFile);
        }

        public void LoadNca(string NcaFile)
        {
            System.LoadNca(NcaFile);
        }

        public void LoadNsp(string NspFile)
        {
            System.LoadNsp(NspFile);
        }

        public void LoadProgram(string FileName)
        {
            System.LoadProgram(FileName);
        }

        public bool WaitFifo()
        {
            return Gpu.Fifo.Event.WaitOne(8);
        }

        public void ProcessFrame()
        {
            Gpu.Fifo.DispatchCalls();
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

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                System.Dispose();
            }
        }
    }
}
