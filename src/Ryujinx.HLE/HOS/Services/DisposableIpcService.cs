using System;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services
{
    abstract class DisposableIpcService : IpcService, IDisposable
    {
        private int _disposeState;

        public DisposableIpcService(ServerBase server = null) : base(server) { }

        protected abstract void Dispose(bool isDisposing);

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposeState, 1, 0) == 0)
            {
                Dispose(true);
            }
        }
    }
}
