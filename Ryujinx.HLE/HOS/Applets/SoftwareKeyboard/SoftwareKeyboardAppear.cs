using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// A structure with appearance configurations for the software keyboard when running in inline mode.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct SoftwareKeyboardAppear
    {
        private const int OkTextLength = 8;

        /// <summary>
        /// Some games send a Calc without intention of showing the keyboard, a
        /// common trend observed is that this field will be != 0 in such cases.
        /// </summary>
        public uint ShouldBeHidden;

        /// <summary>
        /// The string displayed in the Submit button.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = OkTextLength + 1)]
        public string OkText;

        /// <summary>
        /// The character displayed in the left button of the numeric keyboard.
        /// </summary>
        public char LeftOptionalSymbolKey;

        /// <summary>
        /// The character displayed in the right button of the numeric keyboard.
        /// </summary>
        public char RightOptionalSymbolKey;

        /// <summary>
        /// When set, predictive typing is enabled making use of the system dictionary, and any custom user dictionary.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool PredictionEnabled;

        public byte Empty;

        /// <summary>
        /// Specifies prohibited characters that cannot be input into the text entry area.
        /// </summary>
        public InvalidCharFlags InvalidCharFlag;

        public int Padding1;
        public int Padding2;

        /// <summary>
        /// Indicates the return button is enabled in the keyboard. This allows for input with multiple lines.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool UseNewLine;

        /// <summary>
        /// [10.0.0+] If value is 1 or 2, then keytopAsFloating=0 and footerScalable=1 in Calc.
        /// </summary>
        public byte Unknown1;

        public byte Padding4;
        public byte Padding5;

        /// <summary>
        /// Bitmask 0x1000 of the Calc and DirectionalButtonAssignEnabled in bitmask 0x10000000.
        /// </summary>
        public uint CalcFlags;

        public uint Padding6;
        public uint Padding7;
        public uint Padding8;
        public uint Padding9;
        public uint Padding10;
        public uint Padding11;
    }
}
