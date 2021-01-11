using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets.SoftwareKeyboard
{
    /// <summary>
    /// A structure that defines the configuration options of the software keyboard.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack=1, CharSet = CharSet.Unicode)]
    struct SoftwareKeyboardCalc
    {
        private const int InputTextLength = 505;

        public uint Unknown;

        public ushort Size;

        public byte Unknown1;
        public byte Unknown2;

        public ulong Flags;

        public SoftwareKeyboardInitialize Initialize;

        public float Volume;

        public int CursorPos;

        public SoftwareKeyboardAppear Appear;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = InputTextLength + 1)]
        public string InputText;

        public byte Utf8Mode;

        public byte Unknown3;

        [MarshalAs(UnmanagedType.I1)]
        public bool BackspaceEnabled;

        public short Unknown4;
        public byte Unknown5;

        [MarshalAs(UnmanagedType.I1)]
        public byte KeytopAsFloating;

        [MarshalAs(UnmanagedType.I1)]
        public byte FooterScalable;

        [MarshalAs(UnmanagedType.I1)]
        public byte AlphaEnabledInInputMode;

        [MarshalAs(UnmanagedType.I1)]
        public byte InputModeFadeType;

        [MarshalAs(UnmanagedType.I1)]
        public byte TouchDisabled;

        [MarshalAs(UnmanagedType.I1)]
        public byte HardwareKeyboardDisabled;

        public uint Unknown6;
        public uint Unknown7;

        public float KeytopScale0;
        public float KeytopScale1;
        public float KeytopTranslate0;
        public float KeytopTranslate1;
        public float KeytopBgAlpha;
        public float FooterBgAlpha;
        public float BalloonScale;

        public float Unknown8;
        public uint Unknown9;
        public uint Unknown10;
        public uint Unknown11;

        public byte SeGroup;

        public byte TriggerFlag;
        public byte Trigger;

        public byte Padding;
    }
}
