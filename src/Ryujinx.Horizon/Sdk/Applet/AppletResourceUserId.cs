using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Applet
{
    [StructLayout(LayoutKind.Sequential, Size = 0x8, Pack = 0x8)]
    readonly struct AppletResourceUserId
    {
        public readonly ulong Id;

        public AppletResourceUserId(ulong id)
        {
            Id = id;
        }
    }
}
