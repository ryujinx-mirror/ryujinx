namespace Ryujinx.Common.Configuration.Hid
{
    public struct NpadControllerRight
    {
        public ControllerInputId StickX      { get; set; }
        public bool InvertStickX             { get; set; }
        public ControllerInputId StickY      { get; set; }
        public bool InvertStickY             { get; set; }
        public ControllerInputId StickButton { get; set; }
        public ControllerInputId ButtonA     { get; set; }
        public ControllerInputId ButtonB     { get; set; }
        public ControllerInputId ButtonX     { get; set; }
        public ControllerInputId ButtonY     { get; set; }
        public ControllerInputId ButtonPlus  { get; set; }
        public ControllerInputId ButtonR     { get; set; }
        public ControllerInputId ButtonZr    { get; set; }
        public ControllerInputId ButtonSl    { get; set; }
        public ControllerInputId ButtonSr    { get; set; }
    }
}