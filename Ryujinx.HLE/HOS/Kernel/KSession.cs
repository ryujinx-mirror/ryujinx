using Ryujinx.HLE.HOS.Services;
using System;

namespace Ryujinx.HLE.HOS.Kernel
{
    class KSession : IDisposable
    {
        public IpcService Service { get; }

        public string ServiceName { get; }

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