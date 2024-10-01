using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// A structure that defines the configuration options of the software keyboard.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct SoftwareKeyboardConfig
    {
        private const int SubmitTextLength = 8;
        private const int HeaderTextLength = 64;
        private const int SubtitleTextLength = 128;
        private const int GuideTextLength = 256;

        /// <summary>
        /// Type of keyboard.
        /// </summary>
        public KeyboardMode Mode;

        /// <summary>
        /// The string displayed in the Submit button.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SubmitTextLength + 1)]
        public string SubmitText;

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

        /// <summary>
        /// Specifies prohibited characters that cannot be input into the text entry area.
        /// </summary>
        public InvalidCharFlags InvalidCharFlag;

        /// <summary>
        /// The initial position of the text cursor displayed in the text entry area.
        /// </summary>
        public InitialCursorPosition InitialCursorPosition;

        /// <summary>
        /// The string displayed in the header area of the keyboard.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = HeaderTextLength + 1)]
        public string HeaderText;

        /// <summary>
        /// The string displayed in the subtitle area of the keyboard.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SubtitleTextLength + 1)]
        public string SubtitleText;

        /// <summary>
        /// The placeholder string displayed in the text entry area when no text is entered.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = GuideTextLength + 1)]
        public string GuideText;

        /// <summary>
        /// When non-zero, specifies the maximum allowed length of the string entered into the text entry area.
        /// </summary>
        public int StringLengthMax;

        /// <summary>
        /// When non-zero, specifies the minimum allowed length of the string entered into the text entry area.
        /// </summary>
        public int StringLengthMin;

        /// <summary>
        /// When enabled, hides input characters as dots in the text entry area.
        /// </summary>
        public PasswordMode PasswordMode;

        /// <summary>
        /// Specifies whether the text entry area is displayed as a single-line entry, or a multi-line entry field.
        /// </summary>
        public InputFormMode InputFormMode;

        /// <summary>
        /// When set, enables or disables the return key. This value is ignored when single-line entry is specified as the InputFormMode.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool UseNewLine;

        /// <summary>
        /// When set, the software keyboard will return a UTF-8 encoded string, rather than UTF-16.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool UseUtf8;

        /// <summary>
        /// When set, the software keyboard will blur the game application rendered behind the keyboard.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool UseBlurBackground;

        /// <summary>
        /// Offset into the work buffer of the initial text when the keyboard is first displayed.
        /// </summary>
        public int InitialStringOffset;

        /// <summary>
        /// Length of the initial text.
        /// </summary>
        public int InitialStringLength;

        /// <summary>
        /// Offset into the work buffer of the custom user dictionary.
        /// </summary>
        public int CustomDictionaryOffset;

        /// <summary>
        /// Number of entries in the custom user dictionary.
        /// </summary>
        public int CustomDictionaryCount;

        /// <summary>
        /// When set, the text entered will be validated on the application side after the keyboard has been submitted.
        /// </summary>
        [MarshalAs(UnmanagedType.I1)]
        public bool CheckText;
    }
}
