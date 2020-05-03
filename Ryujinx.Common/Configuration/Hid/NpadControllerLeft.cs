namespace Ryujinx.Common.Configuration.Hid
{
    public struct NpadControllerLeft
    {
        public ControllerInputId StickX      { get; set; }
        public bool InvertStickX             { get; set; }
        public ControllerInputId StickY      { get; set; }
        public bool InvertStickY             { get; set; }
        public ControllerInputId StickButton { get; set; }
        public ControllerInputId ButtonMinus { get; set; }
        public ControllerInputId ButtonL     { get; set; }
        public ControllerInputId ButtonZl    { get; set; }
        public ControllerInputId ButtonSl    { get; set; }
        public ControllerInputId ButtonSr    { get; set; }
        public ControllerInputId DPadUp      { get; set; }
        public ControllerInputId DPadDown    { get; set; }
        public ControllerInputId DPadLeft    { get; set; }
        public ControllerInputId DPadRight   { get; set; }
    }
}