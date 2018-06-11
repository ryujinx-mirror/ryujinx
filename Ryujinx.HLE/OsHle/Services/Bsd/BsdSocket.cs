using System.Net;
using System.Net.Sockets;

namespace Ryujinx.HLE.OsHle.Services.Bsd
{
    class BsdSocket
    {
        public int Family;
        public int Type;
        public int Protocol;

        public IPAddress IpAddress;

        public IPEndPoint RemoteEP;

        public Socket Handle;
    }
}