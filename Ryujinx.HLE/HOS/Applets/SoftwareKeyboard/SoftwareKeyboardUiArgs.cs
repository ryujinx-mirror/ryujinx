namespace Ryujinx.HLE.HOS.Applets
{
    public struct SoftwareKeyboardUiArgs
    {
        public string HeaderText;
        public string SubtitleText;
        public string InitialText;
        public string GuideText;
        public string SubmitText;
        public int StringLengthMin;
        public int StringLengthMax;
    }
}