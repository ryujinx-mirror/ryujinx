using System;
using System.Threading;

namespace Ryujinx.Core.OsHle.Handles
{
    class KSynchronizationObject : IDisposable
    {
        public ManualResetEvent Handle { get; private set; }

        public KSynchronizationObject()
        {
            Handle = new ManualResetEvent(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing)
            {
                Handle.Dispose();
            }
        }
    }
}