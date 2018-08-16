using Ryujinx.HLE.HOS.Services;
using System;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KSession : IDisposable
    {
        public IpcService Service { get; private set; }

        public string ServiceName { get; private set; }

        public KSession(IpcService Service, string ServiceName)
        {
            this.Service     = Service;
            this.ServiceName = ServiceName;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool Disposing)
        {
            if (Disposing && Service is IDisposable DisposableService)
            {
                DisposableService.Dispose();
            }
        }
    }
}