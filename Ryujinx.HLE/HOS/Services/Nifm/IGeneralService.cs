using Ryujinx.Common.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Nifm
{
    class IGeneralService : IpcService
    {
        public IGeneralService() { }

        [Command(4)]
        // CreateRequest(u32) -> object<nn::nifm::detail::IRequest>
        public long CreateRequest(ServiceCtx context)
        {
            int unknown = context.RequestData.ReadInt32();

            MakeObject(context, new IRequest(context.Device.System));

            Logger.PrintStub(LogClass.ServiceNifm);

            return 0;
        }

        [Command(12)]
        // GetCurrentIpAddress() -> nn::nifm::IpV4Address
        public long GetCurrentIpAddress(ServiceCtx context)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return MakeError(ErrorModule.Nifm, NifmErr.NoInternetConnection);
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            IPAddress address = host.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

            context.ResponseData.Write(BitConverter.ToUInt32(address.GetAddressBytes()));

            Logger.PrintInfo(LogClass.ServiceNifm, $"Console's local IP is \"{address}\".");

            return 0;
        }
    }
}