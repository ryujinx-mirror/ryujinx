namespace Ryujinx.Common.Configuration.Hid.Keyboard
{
    public class GenericKeyboardInputConfig<TKey> : GenericInputConfigurationCommon<TKey> where TKey : unmanaged
    {
        /// <summary>
        /// Left JoyCon Controller Stick Bindings
        /// </summary>
        public JoyconConfigKeyboardStick<TKey> LeftJoyconStick { get; set; }

        /// <summary>
        /// Right JoyCon Controller Stick Bindings
        /// </summary>
        public JoyconConfigKeyboardStick<TKey> RightJoyconStick { get; set; }
    }
}
