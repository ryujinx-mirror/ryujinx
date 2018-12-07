namespace Ryujinx.HLE.Input
{
    public class HidProController : HidControllerBase
    {
        bool _wired = false;

        public HidProController(Switch device) : base(HidControllerType.ProController, device)
        {
            _wired = true;
        }

        public override void Connect(HidControllerId controllerId)
        {
            base.Connect(controllerId);

            HidControllerColorDesc singleColorDesc =
                HidControllerColorDesc.ColorDescColorsNonexistent;

            HidControllerColorDesc splitColorDesc = 0;

            NpadColor singleColorBody    = NpadColor.Black;
            NpadColor singleColorButtons = NpadColor.Black;

            Device.Memory.WriteInt32(Offset + 0x08, (int)singleColorDesc);
            Device.Memory.WriteInt32(Offset + 0x0c, (int)singleColorBody);
            Device.Memory.WriteInt32(Offset + 0x10, (int)singleColorButtons);
            Device.Memory.WriteInt32(Offset + 0x14, (int)splitColorDesc);

            Connected = true;
        }

        public override void SendInput(
            HidControllerButtons buttons,
            HidJoystickPosition  leftStick,
            HidJoystickPosition  rightStick)
        {
            long controllerOffset = WriteInput(buttons, leftStick, rightStick, HidControllerLayouts.ProController);

            Device.Memory.WriteInt64(controllerOffset + 0x28,
              (Connected ? (uint)HidControllerConnState.ControllerStateConnected : 0) |
              (_wired ? (uint)HidControllerConnState.ControllerStateWired : 0));

            controllerOffset = WriteInput(buttons, leftStick, rightStick, HidControllerLayouts.Main);

            Device.Memory.WriteInt64(controllerOffset + 0x28,
              (Connected ? (uint)HidControllerConnState.ControllerStateWired : 0) |
              (uint)HidControllerConnState.ControllerStateWired);
        }
    }
}
