using System.Runtime.InteropServices;

namespace Ryujinx.Modules.Motion
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ControllerInfoResponse
    {
        public  SharedResponse Shared;
        private byte           _zero;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ControllerInfoRequest
    {
        public MessageType Type;
        public int         PortsCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] PortIndices;
    }
}