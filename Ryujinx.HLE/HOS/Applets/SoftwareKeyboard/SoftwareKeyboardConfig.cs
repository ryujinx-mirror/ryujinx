using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    // TODO(jduncanator): Define all fields
    [StructLayout(LayoutKind.Explicit)]
    struct SoftwareKeyboardConfig
    {
        /// <summary>
        /// Type of keyboard.
        /// </summary>
        [FieldOffset(0x0)]
        public SoftwareKeyboardType Type;

        /// <summary>
        /// When non-zero, specifies the max string length. When the input is too long, swkbd will stop accepting more input until text is deleted via the B button (Backspace).
        /// </summary>
        [FieldOffset(0x3AC)]
        public uint StringLengthMax;

        /// <summary>
        /// When non-zero, specifies the max string length. When the input is too long, swkbd will display an icon and disable the ok-button.
        /// </summary>
        [FieldOffset(0x3B0)]
        public uint StringLengthMaxExtended;

        /// <summary>
        /// When set, the application will validate the entered text whilst the swkbd is still on screen.
        /// </summary>
        [FieldOffset(0x3D0), MarshalAs(UnmanagedType.I1)]
        public bool CheckText;
    }
}
