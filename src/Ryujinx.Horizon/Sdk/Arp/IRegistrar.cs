using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Ns;

namespace Ryujinx.Horizon.Sdk.Arp
{
    public interface IRegistrar
    {
        public Result Issue(out ulong applicationInstanceId);
        public Result SetApplicationLaunchProperty(ApplicationLaunchProperty applicationLaunchProperty);
        public Result SetApplicationControlProperty(in ApplicationControlProperty applicationControlProperty);
    }
}
