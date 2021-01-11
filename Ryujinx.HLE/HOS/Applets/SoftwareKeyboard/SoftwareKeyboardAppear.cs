using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct SoftwareKeyboardAppear
    {
        private const int OkTextLength = 8;

        // Some games send a Calc without intention of showing the keyboard, a
        // common trend observed is that this field will be != 0 in such cases.
        public uint ShouldBeHidden;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = OkTextLength + 1)]
        public string OkText;

        /// <summary>
        /// The character displayed in the left button of the numeric keyboard.
        /// This is ignored when Mode is not set to NumbersOnly.
        /// </summary>
        public char LeftOptionalSymbolKey;

        /// <summary>
        /// The character displayed in the right button of the numeric keyboard.
        /// This is ignored when Mode is not set to NumbersOnly.
        /// </summary>
        public char RightOptionalSymbolKey;

        /// <summary>
        /// When set, predictive typing is enabled making use of the system dictionary,
        /// and any custom user dictionary.
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

        public byte EnableReturnButton;

        public byte Padding3;
        public byte Padding4;
        public byte Padding5;

        public uint CalcArgFlags;

        public uint Padding6;
        public uint Padding7;
        public uint Padding8;
        public uint Padding9;
        public uint Padding10;
        public uint Padding11;
    }
}
