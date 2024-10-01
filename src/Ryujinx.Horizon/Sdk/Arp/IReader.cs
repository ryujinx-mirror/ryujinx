using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Ns;
using System;

namespace Ryujinx.Horizon.Sdk.Arp
{
    public interface IReader
    {
        public Result GetApplicationLaunchProperty(out ApplicationLaunchProperty applicationLaunchProperty, ulong applicationInstanceId);
        public Result GetApplicationControlProperty(out ApplicationControlProperty applicationControlProperty, ulong applicationInstanceId);
        public Result GetApplicationProcessProperty(out ApplicationProcessProperty applicationControlProperty, ulong applicationInstanceId);
        public Result GetApplicationInstanceId(out ulong applicationInstanceId, ulong pid);
        public Result GetApplicationInstanceUnregistrationNotifier(out IUnregistrationNotifier unregistrationNotifier);
        public Result ListApplicationInstanceId(out int count, Span<ulong> applicationInstanceIdList);
        public Result GetMicroApplicationInstanceId(out ulong MicroApplicationInstanceId, ulong pid);
        public Result GetApplicationCertificate(out ApplicationCertificate applicationCertificate, ulong applicationInstanceId);
    }
}
