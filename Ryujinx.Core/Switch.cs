using Ryujinx.Audio;
using Ryujinx.Core.Input;
using Ryujinx.Core.Logging;
using Ryujinx.Core.OsHle;
using Ryujinx.Core.Settings;
using Ryujinx.Graphics.Gal;
using Ryujinx.Core.Gpu;
using System;

namespace Ryujinx.Core
{
    public class Switch : IDisposable
    {
        internal IAalOutput AudioOut { get; private set; }

        public Logger Log { get; private set; }

        internal NvGpu Gpu { get; private set; }

        internal VirtualFileSystem VFs { get; private set; }

        public Horizon Os { get; private set; }

        public SystemSettings Settings { get; private set; }

        public PerformanceStatistics Statistics { get; private set; }

        public Hid Hid { get; private set; }

        public event EventHandler Finish;

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

            Gpu = new NvGpu(Renderer);

            VFs = new VirtualFileSystem();

            Os = new Horizon(this);

            Settings = new SystemSettings();

            Statistics = new PerformanceStatistics();

            Hid = new Hid(Log);

            Os.HidSharedMem.MemoryMapped   += Hid.ShMemMap;
            Os.HidSharedMem.MemoryUnmapped += Hid.ShMemUnmap;
        }

        public void LoadCart(string ExeFsDir, string RomFsFile = null)
        {
            Os.LoadCart(ExeFsDir, RomFsFile);
        }

        public void LoadProgram(string FileName)
        {
            Os.LoadProgram(FileName);
        }

        internal virtual void OnFinish(EventArgs e)
        {
            Finish?.Invoke(this, e);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                Os.Dispose();
                VFs.Dispose();
            }
        }
    }
}