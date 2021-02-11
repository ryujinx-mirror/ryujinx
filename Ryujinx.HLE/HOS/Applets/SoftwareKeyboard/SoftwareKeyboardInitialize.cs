using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// A structure that mirrors the parameters used to initialize the keyboard applet.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct SoftwareKeyboardInitialize
    {
        public uint Unknown;

        /// <summary>
        /// The applet mode used when launching the swkb. The bits regarding the background vs foreground mode can be wrong.
        /// </summary>
        public byte LibMode;

        /// <summary>
        /// [5.0.0+] Set to 0x1 to indicate a firmware version >= 5.0.0.
        /// </summary>
        public byte FivePlus;

        public byte Padding1;
        public byte Padding2;
    }
}
