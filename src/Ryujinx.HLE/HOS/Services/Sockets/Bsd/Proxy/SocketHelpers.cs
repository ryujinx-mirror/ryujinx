using Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator.LdnRyu.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd.Proxy
{
    static class SocketHelpers
    {
        private static LdnProxy _proxy;

        public static void Select(List<ISocketImpl> readEvents, List<ISocketImpl> writeEvents, List<ISocketImpl> errorEvents, int timeout)
        {
            var readDefault = readEvents.Select(x => (x as DefaultSocket)?.BaseSocket).Where(x => x != null).ToList();
            var writeDefault = writeEvents.Select(x => (x as DefaultSocket)?.BaseSocket).Where(x => x != null).ToList();
            var errorDefault = errorEvents.Select(x => (x as DefaultSocket)?.BaseSocket).Where(x => x != null).ToList();

            Socket.Select(readDefault, writeDefault, errorDefault, timeout);

            void FilterSockets(List<ISocketImpl> removeFrom, List<Socket> selectedSockets, Func<LdnProxySocket, bool> ldnCheck)
            {
                removeFrom.RemoveAll(socket =>
                {
                    switch (socket)
                    {
                        case DefaultSocket dsocket:
                            return !selectedSockets.Contains(dsocket.BaseSocket);
                        case LdnProxySocket psocket:
                            return !ldnCheck(psocket);
                        default:
                            throw new NotImplementedException();
                    }
                });
            };

            FilterSockets(readEvents, readDefault, (socket) => socket.Readable);
            FilterSockets(writeEvents, writeDefault, (socket) => socket.Writable);
            FilterSockets(errorEvents, errorDefault, (socket) => socket.Error);
        }

        public static void RegisterProxy(LdnProxy proxy)
        {
            if (_proxy != null)
            {
                UnregisterProxy();
            }

            _proxy = proxy;
        }

        public static void UnregisterProxy()
        {
            _proxy?.Dispose();
            _proxy = null;
        }

        public static ISocketImpl CreateSocket(AddressFamily domain, SocketType type, ProtocolType protocol, string lanInterfaceId)
        {
            if (_proxy != null)
            {
                if (_proxy.Supported(domain, type, protocol))
                {
                    return new LdnProxySocket(domain, type, protocol, _proxy);
                }
            }

            return new DefaultSocket(domain, type, protocol, lanInterfaceId);
        }
    }
}
