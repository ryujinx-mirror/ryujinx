namespace Ryujinx.Common.Configuration.Hid
{
    public class LeftJoyconCommonConfig<TButton>
    {
        public TButton ButtonMinus { get; set; }
        public TButton ButtonL { get; set; }
        public TButton ButtonZl { get; set; }
        public TButton ButtonSl { get; set; }
        public TButton ButtonSr { get; set; }
        public TButton DpadUp { get; set; }
        public TButton DpadDown { get; set; }
        public TButton DpadLeft { get; set; }
        public TButton DpadRight { get; set; }
    }
}
