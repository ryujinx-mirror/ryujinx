using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Ro
{
    class NrrInfo
    {
        public NrrHeader Header { get; private set; }
        public List<byte[]> Hashes { get; private set; }
        public ulong NrrAddress { get; private set; }

        public NrrInfo(ulong nrrAddress, NrrHeader header, List<byte[]> hashes)
        {
            NrrAddress = nrrAddress;
            Header = header;
            Hashes = hashes;
        }
    }
}
