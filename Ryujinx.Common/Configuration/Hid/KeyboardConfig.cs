namespace Ryujinx.Common.Configuration.Hid
{
    public class KeyboardConfig : InputConfig
    {
        /// <summary>
        /// Left JoyCon Keyboard Bindings
        /// </summary>
        public NpadKeyboardLeft LeftJoycon { get; set; }

        /// <summary>
        /// Right JoyCon Keyboard Bindings
        /// </summary>
        public NpadKeyboardRight RightJoycon { get; set; }

        /// <summary>
        /// Hotkey Keyboard Bindings
        /// </summary>
        public KeyboardHotkeys Hotkeys { get; set; }
    }
}