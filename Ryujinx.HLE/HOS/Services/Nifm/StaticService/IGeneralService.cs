using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Nifm.StaticService.GeneralService;
using Ryujinx.HLE.HOS.Services.Nifm.StaticService.Types;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Ryujinx.HLE.HOS.Services.Nifm.StaticService
{
    class IGeneralService : IpcService, IDisposable
    {
        private GeneralServiceDetail _generalServiceDetail;

        public IGeneralService()
        {
            _generalServiceDetail = new GeneralServiceDetail
            {
                ClientId                     = GeneralServiceManager.Count,
                IsAnyInternetRequestAccepted = true // NOTE: Why not accept any internet request?
            };

            GeneralServiceManager.Add(_generalServiceDetail);
        }

        [Command(1)]
        // GetClientId() -> buffer<nn::nifm::ClientId, 0x1a, 4>
        public ResultCode GetClientId(ServiceCtx context)
        {
            long position = context.Request.RecvListBuff[0].Position;
            long size     = context.Request.RecvListBuff[0].Size;

            context.Memory.WriteInt32(position, _generalServiceDetail.ClientId);

            return ResultCode.Success;
        }

        [Command(4)]
        // CreateRequest(u32 version) -> object<nn::nifm::detail::IRequest>
        public ResultCode CreateRequest(ServiceCtx context)
        {
            uint version = context.RequestData.ReadUInt32();

            MakeObject(context, new IRequest(context.Device.System, version));

            // Doesn't occur in our case.
            // return ResultCode.ObjectIsNull;

            Logger.PrintStub(LogClass.ServiceNifm, new { version });

            return ResultCode.Success;
        }

        [Command(12)]
        // GetCurrentIpAddress() -> nn::nifm::IpV4Address
        public ResultCode GetCurrentIpAddress(ServiceCtx context)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return ResultCode.NoInternetConnection;
            }

            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            IPAddress address = host.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

            context.ResponseData.Write(BitConverter.ToUInt32(address.GetAddressBytes()));

            Logger.PrintInfo(LogClass.ServiceNifm, $"Console's local IP is \"{address}\".");

            return ResultCode.Success;
        }

        [Command(18)]
        // GetInternetConnectionStatus() -> nn::nifm::detail::sf::InternetConnectionStatus
        public ResultCode GetInternetConnectionStatus(ServiceCtx context)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return ResultCode.NoInternetConnection;
            }

            InternetConnectionStatus internetConnectionStatus = new InternetConnectionStatus
            {
                Type         = InternetConnectionType.WiFi,
                WifiStrength = 3,
                State        = InternetConnectionState.Connected,
            };

            context.ResponseData.WriteStruct(internetConnectionStatus);

            return ResultCode.Success;
        }

        [Command(21)]
        // IsAnyInternetRequestAccepted(buffer<nn::nifm::ClientId, 0x19, 4>) -> bool
        public ResultCode IsAnyInternetRequestAccepted(ServiceCtx context)
        {
            long position = context.Request.PtrBuff[0].Position;
            long size     = context.Request.PtrBuff[0].Size;

            int clientId = context.Memory.ReadInt32(position);

            context.ResponseData.Write(GeneralServiceManager.Get(clientId).IsAnyInternetRequestAccepted);

            return ResultCode.Success;
        }

        public void Dispose()
        {
            GeneralServiceManager.Remove(_generalServiceDetail.ClientId);
        }
    }
}