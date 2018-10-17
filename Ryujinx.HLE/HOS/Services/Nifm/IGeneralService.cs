using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

using static Ryujinx.HLE.HOS.ErrorCode;

namespace Ryujinx.HLE.HOS.Services.Nifm
{
    class IGeneralService : IpcService
    {
        private Dictionary<int, ServiceProcessRequest> m_Commands;

        public override IReadOnlyDictionary<int, ServiceProcessRequest> Commands => m_Commands;

        public IGeneralService()
        {
            m_Commands = new Dictionary<int, ServiceProcessRequest>()
            {
                { 4, CreateRequest        },
                { 12, GetCurrentIpAddress }
            };
        }

        public long CreateRequest(ServiceCtx Context)
        {
            int Unknown = Context.RequestData.ReadInt32();

            MakeObject(Context, new IRequest(Context.Device.System));

            Logger.PrintStub(LogClass.ServiceNifm, "Stubbed.");

            return 0;
        }

        public long GetCurrentIpAddress(ServiceCtx Context)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return MakeError(ErrorModule.Nifm, NifmErr.NoInternetConnection);
            }

            IPHostEntry Host = Dns.GetHostEntry(Dns.GetHostName());

            IPAddress Address = Host.AddressList.FirstOrDefault(A => A.AddressFamily == AddressFamily.InterNetwork);

            Context.ResponseData.Write(BitConverter.ToUInt32(Address.GetAddressBytes()));

            Logger.PrintInfo(LogClass.ServiceNifm, $"Console's local IP is \"{Address}\".");

            return 0;
        }
    }
}
