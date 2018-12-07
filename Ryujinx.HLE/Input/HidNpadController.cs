namespace Ryujinx.HLE.Input
{
    public class HidNpadController : HidControllerBase
    {
        private (NpadColor Left, NpadColor Right) _npadBodyColors;
        private (NpadColor Left, NpadColor Right) _npadButtonColors;

        private HidControllerLayouts _currentLayout;

        private bool _isHalf;

        public HidNpadController(
            HidControllerType      controllerType,
            Switch                 device,
            (NpadColor, NpadColor) npadBodyColors,
            (NpadColor, NpadColor) npadButtonColors) : base(controllerType, device)
        {
            _npadBodyColors   = npadBodyColors;
            _npadButtonColors = npadButtonColors;

            _currentLayout = HidControllerLayouts.HandheldJoined;

            switch (controllerType)
            {
                case HidControllerType.NpadLeft:
                    _currentLayout = HidControllerLayouts.Left;
                    break;
                case HidControllerType.NpadRight:
                    _currentLayout = HidControllerLayouts.Right;
                    break;
                case HidControllerType.NpadPair:
                    _currentLayout = HidControllerLayouts.Joined;
                    break;
            }
        }

        public override void Connect(HidControllerId controllerId)
        {
            if(HidControllerType != HidControllerType.NpadLeft && HidControllerType != HidControllerType.NpadRight)
            {
                _isHalf = false;
            }

            base.Connect(_currentLayout == HidControllerLayouts.HandheldJoined ? HidControllerId.ControllerHandheld : controllerId);

            HidControllerColorDesc singleColorDesc =
                HidControllerColorDesc.ColorDescColorsNonexistent;

            HidControllerColorDesc splitColorDesc = 0;

            NpadColor singleColorBody    = NpadColor.Black;
            NpadColor singleColorButtons = NpadColor.Black;

            Device.Memory.WriteInt32(Offset + 0x04, _isHalf ? 1 : 0);

            if (_isHalf)
            {
                Device.Memory.WriteInt32(Offset + 0x08, (int)singleColorDesc);
                Device.Memory.WriteInt32(Offset + 0x0c, (int)singleColorBody);
                Device.Memory.WriteInt32(Offset + 0x10, (int)singleColorButtons);
                Device.Memory.WriteInt32(Offset + 0x14, (int)splitColorDesc);
            }
            else
            {
                Device.Memory.WriteInt32(Offset + 0x18, (int)_npadBodyColors.Left);
                Device.Memory.WriteInt32(Offset + 0x1c, (int)_npadButtonColors.Left);
                Device.Memory.WriteInt32(Offset + 0x20, (int)_npadBodyColors.Right);
                Device.Memory.WriteInt32(Offset + 0x24, (int)_npadButtonColors.Right);
            }

            Connected = true;
        }

        public override void SendInput
            (HidControllerButtons buttons,
            HidJoystickPosition   leftStick,
            HidJoystickPosition   rightStick)
        {
            long controllerOffset = WriteInput(buttons, leftStick, rightStick, _currentLayout);

            Device.Memory.WriteInt64(controllerOffset + 0x28,
              (Connected ? (uint)HidControllerConnState.ControllerStateConnected : 0) |
              (_currentLayout == HidControllerLayouts.HandheldJoined ? (uint)HidControllerConnState.ControllerStateWired : 0));

            controllerOffset = WriteInput(buttons, leftStick, rightStick, HidControllerLayouts.Main);

            Device.Memory.WriteInt64(controllerOffset + 0x28,
              (Connected ? (uint)HidControllerConnState.ControllerStateWired : 0) |
              (uint)HidControllerConnState.ControllerStateWired);
        }
    }
}
