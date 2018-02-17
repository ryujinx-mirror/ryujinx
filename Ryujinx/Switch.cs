using ChocolArm64.Memory;
using Gal;
using Ryujinx.Gpu;
using Ryujinx.OsHle;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx
{
    public class Switch : IDisposable
    {
        public IntPtr Ram {get; private set; }

        internal NsGpu     Gpu { get; private set; }
        internal Horizon   Os  { get; private set; }
        internal VirtualFs VFs { get; private set; }

        public event EventHandler Finish;

        public Switch(IGalRenderer Renderer)
        {
            Ram = Marshal.AllocHGlobal((IntPtr)AMemoryMgr.RamSize);

            Gpu = new NsGpu(Renderer);
            Os  = new Horizon(this);
            VFs = new VirtualFs();
        }

        internal virtual void OnFinish(EventArgs e)
        {
            if (Finish != null)
            {
                Finish(this, e);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                VFs.Dispose();
            }

            Marshal.FreeHGlobal(Ram);
        }
    }
}