using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets
{
#pragma warning disable CS0649
    // (1.0.0+ version)
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    unsafe struct ControllerSupportArgVPre7
    {
        public ControllerSupportArgHeader Header;
        public fixed uint IdentificationColor[4];
        public byte EnableExplainText;
        public fixed byte ExplainText[4 * 0x81];
    }
#pragma warning restore CS0649
}