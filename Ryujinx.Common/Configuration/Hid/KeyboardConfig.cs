namespace Ryujinx.Common.Configuration.Hid
{
    public class KeyboardConfig : InputConfig
    {
        // DO NOT MODIFY
        public const uint AllKeyboardsIndex = 0;

        /// <summary>
        /// Left JoyCon Keyboard Bindings
        /// </summary>
        public NpadKeyboardLeft LeftJoycon { get; set; }

        /// <summary>
        /// Right JoyCon Keyboard Bindings
        /// </summary>
        public NpadKeyboardRight RightJoycon { get; set; }
    }
}