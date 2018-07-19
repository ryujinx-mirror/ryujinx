using Ryujinx.HLE.Logging;
using Ryujinx.HLE.OsHle.Ipc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace Ryujinx.HLE.OsHle.Services.Nifm
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

        public const int NoInternetConnection = 0x2586e;

        //CreateRequest(i32)
        public long CreateRequest(ServiceCtx Context)
        {
            int Unknown = Context.RequestData.ReadInt32();

            MakeObject(Context, new IRequest());

            Context.Ns.Log.PrintStub(LogClass.ServiceNifm, "Stubbed.");

            return 0;
        }

        public long GetCurrentIpAddress(ServiceCtx Context)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                return NoInternetConnection;
            }

            IPHostEntry Host    = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress   Address = Host.AddressList.FirstOrDefault(A => A.AddressFamily == AddressFamily.InterNetwork);

            Context.ResponseData.Write(BitConverter.ToUInt32(Address.GetAddressBytes()));

            Context.Ns.Log.PrintInfo(LogClass.ServiceNifm, $"Console's local IP is {Address.ToString()}");

            return 0;
        }
    }
}
