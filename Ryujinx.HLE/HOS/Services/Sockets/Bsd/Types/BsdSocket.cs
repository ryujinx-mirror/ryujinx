using System.Net.Sockets;

namespace Ryujinx.HLE.HOS.Services.Sockets.Bsd
{
    class BsdSocket
    {
        public int Family;
        public int Type;
        public int Protocol;

        public Socket Handle;
    }
}