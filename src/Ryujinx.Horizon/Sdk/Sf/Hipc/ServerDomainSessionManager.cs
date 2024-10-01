using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf.Cmif;
using System;

namespace Ryujinx.Horizon.Sdk.Sf.Hipc
{
    class ServerDomainSessionManager : ServerSessionManager
    {
        public ServerDomainManager Domain { get; }

        public ServerDomainSessionManager(int entryCount, int maxDomains)
        {
            Domain = new ServerDomainManager(entryCount, maxDomains);
        }

        protected override Result DispatchManagerRequest(ServerSession session, Span<byte> inMessage, Span<byte> outMessage)
        {
            HipcManager hipcManager = new(this, session);

            return DispatchRequest(new ServiceObjectHolder(hipcManager), session, inMessage, outMessage);
        }
    }
}
