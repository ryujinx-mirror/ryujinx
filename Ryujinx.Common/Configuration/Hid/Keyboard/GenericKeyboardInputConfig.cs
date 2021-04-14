namespace Ryujinx.Common.Configuration.Hid.Keyboard
{
    public class GenericKeyboardInputConfig<Key> : GenericInputConfigurationCommon<Key> where Key : unmanaged
    {
        /// <summary>
        /// Left JoyCon Controller Stick Bindings
        /// </summary>
        public JoyconConfigKeyboardStick<Key> LeftJoyconStick { get; set; }

        /// <summary>
        /// Right JoyCon Controller Stick Bindings
        /// </summary>
        public JoyconConfigKeyboardStick<Key> RightJoyconStick { get; set; }
    }
}
