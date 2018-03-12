using Ryujinx.Core.Input;
using Ryujinx.Core.OsHle;
using Ryujinx.Core.Settings;
using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Gpu;
using System;

namespace Ryujinx.Core
{
    public class Switch : IDisposable
    {
        internal NsGpu     Gpu { get; private set; }
        internal Horizon   Os  { get; private set; }
        internal VirtualFs VFs { get; private set; }

        public Hid    Hid                       { get; private set; }        
        public SetSys Settings                  { get; private set; }
        public PerformanceStatistics Statistics { get; private set; }

        public event EventHandler Finish;

        public Switch(IGalRenderer Renderer)
        {
            Gpu = new NsGpu(Renderer);

            VFs = new VirtualFs();

            Hid = new Hid();

            Statistics = new PerformanceStatistics();

            Os = new Horizon(this);

            Os.HidSharedMem.MemoryMapped   += Hid.ShMemMap;
            Os.HidSharedMem.MemoryUnmapped += Hid.ShMemUnmap;

            Settings = new SetSys();
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