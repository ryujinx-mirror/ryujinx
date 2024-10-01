using Ryujinx.HLE.HOS.Services.Ldn.Types;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0xBC)]
    struct ConnectPrivateRequest
    {
        public SecurityConfig SecurityConfig;
        public SecurityParameter SecurityParameter;
        public UserConfig UserConfig;
        public uint LocalCommunicationVersion;
        public uint OptionUnknown;
        public NetworkConfig NetworkConfig;
    }
}
