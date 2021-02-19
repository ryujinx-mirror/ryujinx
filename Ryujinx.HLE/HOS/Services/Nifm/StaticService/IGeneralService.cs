using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Nifm.StaticService.GeneralService;
using Ryujinx.HLE.HOS.Services.Nifm.StaticService.Types;
using System;
using System.Net.NetworkInformation;

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

            context.Response.PtrBuff[0] = context.Response.PtrBuff[0].WithSize(4);

            context.Memory.Write((ulong)position, _generalServiceDetail.ClientId);

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

            Logger.Stub?.PrintStub(LogClass.ServiceNifm, new { version });

            return ResultCode.Success;
        }

        [Command(12)]
        // GetCurrentIpAddress() -> nn::nifm::IpV4Address
        public ResultCode GetCurrentIpAddress(ServiceCtx context)
        {
            (_, UnicastIPAddressInformation unicastAddress) = GetLocalInterface();

            if (unicastAddress == null)
            {
                return ResultCode.NoInternetConnection;
            }

            context.ResponseData.WriteStruct(new IpV4Address(unicastAddress.Address));

            Logger.Info?.Print(LogClass.ServiceNifm, $"Console's local IP is \"{unicastAddress.Address}\".");

            return ResultCode.Success;
        }

        [Command(15)]
        // GetCurrentIpConfigInfo() -> (nn::nifm::IpAddressSetting, nn::nifm::DnsSetting)
        public ResultCode GetCurrentIpConfigInfo(ServiceCtx context)
        {
            (IPInterfaceProperties interfaceProperties, UnicastIPAddressInformation unicastAddress) = GetLocalInterface();

            if (interfaceProperties == null)
            {
                return ResultCode.NoInternetConnection;
            }

            Logger.Info?.Print(LogClass.ServiceNifm, $"Console's local IP is \"{unicastAddress.Address}\".");

            context.ResponseData.WriteStruct(new IpAddressSetting(interfaceProperties, unicastAddress));
            context.ResponseData.WriteStruct(new DnsSetting(interfaceProperties));

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

            int clientId = context.Memory.Read<int>((ulong)position);

            context.ResponseData.Write(GeneralServiceManager.Get(clientId).IsAnyInternetRequestAccepted);

            return ResultCode.Success;
        }

        private (IPInterfaceProperties, UnicastIPAddressInformation) GetLocalInterface()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return (null, null);
            }

            IPInterfaceProperties       targetProperties  = null;
            UnicastIPAddressInformation targetAddressInfo = null;

            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface adapter in interfaces)
            {
                // Ignore loopback and non IPv4 capable interface.
                if (adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback && adapter.Supports(NetworkInterfaceComponent.IPv4))
                {
                    IPInterfaceProperties properties = adapter.GetIPProperties();

                    if (properties.GatewayAddresses.Count > 0 && properties.DnsAddresses.Count > 1)
                    {
                        foreach (UnicastIPAddressInformation info in properties.UnicastAddresses)
                        {
                            // Only accept an IPv4 address
                            if (info.Address.GetAddressBytes().Length == 4)
                            {
                                targetProperties  = properties;
                                targetAddressInfo = info;

                                break;
                            }
                        }
                    }

                    // Found the target interface, stop here.
                    if (targetProperties != null)
                    {
                        break;
                    }
                }
            }

            return (targetProperties, targetAddressInfo);
        }

        public void Dispose()
        {
            GeneralServiceManager.Remove(_generalServiceDetail.ClientId);
        }
    }
}