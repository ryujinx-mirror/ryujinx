using Ryujinx.HLE.HOS.Services.Ldn.Types;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x4FC)]
    struct ConnectRequest
    {
        public SecurityConfig SecurityConfig;
        public UserConfig UserConfig;
        public uint LocalCommunicationVersion;
        public uint OptionUnknown;
        public NetworkInfo NetworkInfo;
    }
}
