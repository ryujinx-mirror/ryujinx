using Ryujinx.HLE.HOS.Applets.SoftwareKeyboard;

namespace Ryujinx.HLE.HOS.Applets
{
    public struct SoftwareKeyboardUiArgs
    {
        public KeyboardMode KeyboardMode;
        public string HeaderText;
        public string SubtitleText;
        public string InitialText;
        public string GuideText;
        public string SubmitText;
        public int StringLengthMin;
        public int StringLengthMax;
    }
}