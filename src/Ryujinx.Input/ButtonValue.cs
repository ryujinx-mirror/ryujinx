using System.Diagnostics;

namespace Ryujinx.Input
{
    public enum ButtonValueType { Key, GamepadButtonInputId, StickId }

    public readonly struct ButtonValue
    {
        private readonly ButtonValueType _type;
        private readonly uint _rawValue;

        public ButtonValue(Key key)
        {
            _type = ButtonValueType.Key;
            _rawValue = (uint)key;
        }

        public ButtonValue(GamepadButtonInputId gamepad)
        {
            _type = ButtonValueType.GamepadButtonInputId;
            _rawValue = (uint)gamepad;
        }

        public ButtonValue(StickInputId stick)
        {
            _type = ButtonValueType.StickId;
            _rawValue = (uint)stick;
        }

        public Common.Configuration.Hid.Key AsKey()
        {
            Debug.Assert(_type == ButtonValueType.Key);
            return (Common.Configuration.Hid.Key)_rawValue;
        }

        public Common.Configuration.Hid.Controller.GamepadInputId AsGamepadButtonInputId()
        {
            Debug.Assert(_type == ButtonValueType.GamepadButtonInputId);
            return (Common.Configuration.Hid.Controller.GamepadInputId)_rawValue;
        }

        public Common.Configuration.Hid.Controller.StickInputId AsGamepadStickId()
        {
            Debug.Assert(_type == ButtonValueType.StickId);
            return (Common.Configuration.Hid.Controller.StickInputId)_rawValue;
        }
    }
}
