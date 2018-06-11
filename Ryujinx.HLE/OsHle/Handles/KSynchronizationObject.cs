using System;
using System.Threading;

namespace Ryujinx.HLE.OsHle.Handles
{
    class KSynchronizationObject : IDisposable
    {
        public ManualResetEvent WaitEvent { get; private set; }

        public KSynchronizationObject()
        {
            WaitEvent = new ManualResetEvent(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                WaitEvent.Dispose();
            }
        }
    }
}