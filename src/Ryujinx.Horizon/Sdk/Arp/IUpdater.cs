using Ryujinx.Horizon.Common;

namespace Ryujinx.Horizon.Sdk.Arp
{
    public interface IUpdater
    {
        public Result Issue();
        public Result SetApplicationProcessProperty(ulong pid, ApplicationProcessProperty applicationProcessProperty);
        public Result DeleteApplicationProcessProperty();
        public Result SetApplicationCertificate(ApplicationCertificate applicationCertificate);
    }
}
