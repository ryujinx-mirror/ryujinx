namespace Ryujinx.Common.Configuration.Hid
{
    public struct NpadControllerLeft
    {
        public ControllerInputId Stick       { get; set; }
        public ControllerInputId StickButton { get; set; }
        public ControllerInputId ButtonMinus { get; set; }
        public ControllerInputId ButtonL     { get; set; }
        public ControllerInputId ButtonZl    { get; set; }
        public ControllerInputId DPadUp      { get; set; }
        public ControllerInputId DPadDown    { get; set; }
        public ControllerInputId DPadLeft    { get; set; }
        public ControllerInputId DPadRight   { get; set; }
    }
}
