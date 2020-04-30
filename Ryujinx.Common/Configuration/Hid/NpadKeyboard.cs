namespace Ryujinx.UI.Input
{
    public class NpadKeyboard
    {
        /// <summary>
        /// Left JoyCon Keyboard Bindings
        /// </summary>
        public Configuration.Hid.NpadKeyboardLeft LeftJoycon { get; set; }

        /// <summary>
        /// Right JoyCon Keyboard Bindings
        /// </summary>
        public Configuration.Hid.NpadKeyboardRight RightJoycon { get; set; }

        /// <summary>
        /// Hotkey Keyboard Bindings
        /// </summary>
        public Configuration.Hid.KeyboardHotkeys Hotkeys { get; set; }
    }
}
