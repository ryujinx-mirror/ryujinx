using Ryujinx.HLE.HOS.Services;
using System;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KSession : IDisposable
    {
        public IpcService Service { get; private set; }

        public string ServiceName { get; private set; }

        public KSession(IpcService service, string serviceName)
        {
            Service     = service;
            ServiceName = serviceName;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && Service is IDisposable disposableService)
            {
                disposableService.Dispose();
            }
        }
    }
}