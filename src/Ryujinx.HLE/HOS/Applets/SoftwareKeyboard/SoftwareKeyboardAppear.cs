using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// A structure with appearance configurations for the software keyboard when running in inline mode.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Unicode)]
    struct SoftwareKeyboardAppear
    {
        public const int OkTextLength = SoftwareKeyboardAppearEx.OkTextLength;

        public KeyboardMode KeyboardMode;

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

        /// <summary>
        /// When set, there is only the option to accept the input.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool CancelButtonDisabled;

        /// <summary>
        /// Specifies prohibited characters that cannot be input into the text entry area.
        /// </summary>
        public InvalidCharFlags InvalidChars;

        /// <summary>
        /// Maximum text length allowed.
        /// </summary>
        public int TextMaxLength;

        /// <summary>
        /// Minimum text length allowed.
        /// </summary>
        public int TextMinLength;

        /// <summary>
        /// Indicates the return button is enabled in the keyboard. This allows for input with multiple lines.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool UseNewLine;

        /// <summary>
        /// [10.0.0+] If value is 1 or 2, then keytopAsFloating=0 and footerScalable=1 in Calc.
        /// </summary>
        public KeyboardMiniaturizationMode MiniaturizationMode;

        public byte Reserved1;
        public byte Reserved2;

        /// <summary>
        /// Bit field with invalid buttons for the keyboard.
        /// </summary>
        public InvalidButtonFlags InvalidButtons;

        [MarshalAs(UnmanagedType.I1)]
        public bool UseSaveData;

        public uint   Reserved3;
        public ushort Reserved4;
        public byte   Reserved5;
        public ulong  Reserved6;
        public ulong  Reserved7;

        public SoftwareKeyboardAppearEx ToExtended()
        {
            SoftwareKeyboardAppearEx appear = new SoftwareKeyboardAppearEx();

            appear.KeyboardMode           = KeyboardMode;
            appear.OkText                 = OkText;
            appear.LeftOptionalSymbolKey  = LeftOptionalSymbolKey;
            appear.RightOptionalSymbolKey = RightOptionalSymbolKey;
            appear.PredictionEnabled      = PredictionEnabled;
            appear.CancelButtonDisabled   = CancelButtonDisabled;
            appear.InvalidChars           = InvalidChars;
            appear.TextMaxLength          = TextMaxLength;
            appear.TextMinLength          = TextMinLength;
            appear.UseNewLine             = UseNewLine;
            appear.MiniaturizationMode    = MiniaturizationMode;
            appear.Reserved1              = Reserved1;
            appear.Reserved2              = Reserved2;
            appear.InvalidButtons         = InvalidButtons;
            appear.UseSaveData            = UseSaveData;
            appear.Reserved3              = Reserved3;
            appear.Reserved4              = Reserved4;
            appear.Reserved5              = Reserved5;
            appear.Uid0                   = Reserved6;
            appear.Uid1                   = Reserved7;
            appear.SamplingNumber         = 0;
            appear.Reserved6              = 0;
            appear.Reserved7              = 0;
            appear.Reserved8              = 0;
            appear.Reserved9              = 0;

            return appear;
        }
    }
}
