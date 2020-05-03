using Ryujinx.Configuration.Hid;

namespace Ryujinx.Common.Configuration.Hid
{
    public struct NpadKeyboardLeft
    {
        public Key StickUp     { get; set; }
        public Key StickDown   { get; set; }
        public Key StickLeft   { get; set; }
        public Key StickRight  { get; set; }
        public Key StickButton { get; set; }
        public Key DPadUp      { get; set; }
        public Key DPadDown    { get; set; }
        public Key DPadLeft    { get; set; }
        public Key DPadRight   { get; set; }
        public Key ButtonMinus { get; set; }
        public Key ButtonL     { get; set; }
        public Key ButtonZl    { get; set; }
        public Key ButtonSl    { get; set; }
        public Key ButtonSr    { get; set; }
    }
}