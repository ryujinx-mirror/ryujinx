using Ryujinx.HLE.HOS.Services.Ldn.Types;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator.Types
{
    /// <remarks>
    /// Advertise data is appended separately (remaining data in the buffer).
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Size = 0x94, CharSet = CharSet.Ansi)]
    struct CreateAccessPointRequest
    {
        public SecurityConfig SecurityConfig;
        public UserConfig UserConfig;
        public NetworkConfig NetworkConfig;
    }
}
