using Ryujinx.Common.Memory;

namespace Ryujinx.HLE.HOS.Applets.Browser
{
    public struct WebCommonReturnValue
    {
        public WebExitReason ExitReason;
        public uint Padding;
        public ByteArray4096 LastUrl;
        public ulong LastUrlSize;
    }
}
