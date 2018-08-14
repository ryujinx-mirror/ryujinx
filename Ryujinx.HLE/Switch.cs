using Ryujinx.Audio;
using Ryujinx.Graphics.Gal;
using Ryujinx.HLE.Font;
using Ryujinx.HLE.Gpu;
using Ryujinx.HLE.Input;
using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle;
using System;

namespace Ryujinx.HLE
{
    public class Switch : IDisposable
    {
        internal IAalOutput AudioOut { get; private set; }

        public Logger Log { get; private set; }

        internal NvGpu Gpu { get; private set; }

        internal VirtualFileSystem VFs { get; private set; }

        public Horizon Os { get; private set; }

        public PerformanceStatistics Statistics { get; private set; }

        public Hid Hid { get; private set; }

        public SharedFontManager Font { get; private set; }

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

            Statistics = new PerformanceStatistics();

            Hid = new Hid(Log);

            Font = new SharedFontManager(Log, VFs.GetSystemPath());

            Os.HidSharedMem.MemoryMapped    += Hid.ShMemMap;
            Os.HidSharedMem.MemoryUnmapped  += Hid.ShMemUnmap;

            Os.FontSharedMem.MemoryMapped   += Font.ShMemMap;
            Os.FontSharedMem.MemoryUnmapped += Font.ShMemUnmap;
        }

        public void LoadCart(string ExeFsDir, string RomFsFile = null)
        {
            Os.LoadCart(ExeFsDir, RomFsFile);
        }

        public void LoadProgram(string FileName)
        {
            Os.LoadProgram(FileName);
        }

        public bool WaitFifo()
        {
            return Gpu.Fifo.Event.WaitOne(8);
        }

        public void ProcessFrame()
        {
            Gpu.Fifo.DispatchCalls();
        }

        public virtual void OnFinish(EventArgs e)
        {
            Os.Dispose();
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
