using Ryujinx.Horizon.Sdk.Ns;

namespace Ryujinx.Horizon.Sdk.Arp.Detail
{
    class ApplicationInstance
    {
        public ulong Pid { get; set; }
        public ApplicationLaunchProperty? LaunchProperty { get; set; }
        public ApplicationProcessProperty? ProcessProperty { get; set; }
        public ApplicationControlProperty? ControlProperty { get; set; }
        public ApplicationCertificate? Certificate { get; set; }
    }
}
