using NetCoreServer;
using Ryujinx.Common.Logging;
using System;
using System.Net;
using System.Net.Sockets;

namespace Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator.LdnMitm.Proxy
{
    internal class LdnProxyTcpServer : TcpServer, ILdnTcpSocket
    {
        private readonly LanProtocol _protocol;

        public LdnProxyTcpServer(LanProtocol protocol, IPAddress address, int port) : base(address, port)
        {
            _protocol = protocol;
            OptionReuseAddress = true;
            OptionSendBufferSize = LanProtocol.TcpTxBufferSize;
            OptionReceiveBufferSize = LanProtocol.TcpRxBufferSize;

            Logger.Info?.PrintMsg(LogClass.ServiceLdn, $"LdnProxyTCPServer created a server for this address: {address}:{port}");
        }

        protected override TcpSession CreateSession()
        {
            return new LdnProxyTcpSession(this, _protocol);
        }

        protected override void OnError(SocketError error)
        {
            Logger.Error?.PrintMsg(LogClass.ServiceLdn, $"LdnProxyTCPServer caught an error with code {error}");
        }

        protected override void Dispose(bool disposingManagedResources)
        {
            Stop();
            base.Dispose(disposingManagedResources);
        }

        public bool Connect()
        {
            throw new InvalidOperationException("Connect was called.");
        }

        public void DisconnectAndStop()
        {
            Stop();
        }

        public bool SendPacketAsync(EndPoint endpoint, byte[] buffer)
        {
            throw new InvalidOperationException("SendPacketAsync was called.");
        }
    }
}
