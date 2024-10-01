using Ryujinx.Common.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator.LdnMitm.Proxy
{
    internal class LdnProxyTcpClient : NetCoreServer.TcpClient, ILdnTcpSocket
    {
        private readonly LanProtocol _protocol;
        private byte[] _buffer;
        private int _bufferEnd;

        public LdnProxyTcpClient(LanProtocol protocol, IPAddress address, int port) : base(address, port)
        {
            _protocol = protocol;
            _buffer = new byte[LanProtocol.BufferSize];
            OptionSendBufferSize = LanProtocol.TcpTxBufferSize;
            OptionReceiveBufferSize = LanProtocol.TcpRxBufferSize;
            OptionSendBufferLimit = LanProtocol.TxBufferSizeMax;
            OptionReceiveBufferLimit = LanProtocol.RxBufferSizeMax;
        }

        protected override void OnConnected()
        {
            Logger.Info?.PrintMsg(LogClass.ServiceLdn, $"LdnProxyTCPClient connected!");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            _protocol.Read(ref _buffer, ref _bufferEnd, buffer, (int)offset, (int)size);
        }

        public void DisconnectAndStop()
        {
            DisconnectAsync();

            while (IsConnected)
            {
                Thread.Yield();
            }
        }

        public bool SendPacketAsync(EndPoint endPoint, byte[] data)
        {
            if (endPoint != null)
            {
                Logger.Warning?.PrintMsg(LogClass.ServiceLdn, "LdnProxyTcpClient is sending a packet but endpoint is not null.");
            }

            if (IsConnecting && !IsConnected)
            {
                Logger.Info?.PrintMsg(LogClass.ServiceLdn, "LdnProxyTCPClient needs to connect before sending packets. Waiting...");

                while (IsConnecting && !IsConnected)
                {
                    Thread.Yield();
                }
            }

            return SendAsync(data);
        }

        protected override void OnError(SocketError error)
        {
            Logger.Error?.PrintMsg(LogClass.ServiceLdn, $"LdnProxyTCPClient caught an error with code {error}");
        }

        protected override void Dispose(bool disposingManagedResources)
        {
            DisconnectAndStop();
            base.Dispose(disposingManagedResources);
        }

        public override bool Connect()
        {
            // TODO: NetCoreServer has a Connect() method, but it currently leads to weird issues.
            base.ConnectAsync();

            while (IsConnecting)
            {
                Thread.Sleep(1);
            }

            return IsConnected;
        }

        public bool Start()
        {
            throw new InvalidOperationException("Start was called.");
        }

        public bool Stop()
        {
            throw new InvalidOperationException("Stop was called.");
        }
    }
}
