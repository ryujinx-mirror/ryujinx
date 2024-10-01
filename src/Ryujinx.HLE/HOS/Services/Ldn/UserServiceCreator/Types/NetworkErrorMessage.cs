using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ldn.UserServiceCreator.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x4)]
    struct NetworkErrorMessage
    {
        public NetworkError Error;
    }
}
