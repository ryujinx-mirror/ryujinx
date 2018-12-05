namespace Ryujinx.HLE.Input
{
    public class HidProController : HidControllerBase
    {
        bool Wired = false;

        public HidProController(Switch Device) : base(HidControllerType.ProController, Device)
        {
            Wired = true;
        }

        public override void Connect(HidControllerId ControllerId)
        {
            base.Connect(ControllerId);

            HidControllerColorDesc SingleColorDesc =
                HidControllerColorDesc.ColorDesc_ColorsNonexistent;

            HidControllerColorDesc SplitColorDesc = 0;

            NpadColor SingleColorBody    = NpadColor.Black;
            NpadColor SingleColorButtons = NpadColor.Black;

            Device.Memory.WriteInt32(Offset + 0x08, (int)SingleColorDesc);
            Device.Memory.WriteInt32(Offset + 0x0c, (int)SingleColorBody);
            Device.Memory.WriteInt32(Offset + 0x10, (int)SingleColorButtons);
            Device.Memory.WriteInt32(Offset + 0x14, (int)SplitColorDesc);

            Connected = true;
        }

        public override void SendInput(
            HidControllerButtons Buttons,
            HidJoystickPosition  LeftStick,
            HidJoystickPosition  RightStick)
        {
            long ControllerOffset = WriteInput(Buttons, LeftStick, RightStick, HidControllerLayouts.Pro_Controller);

            Device.Memory.WriteInt64(ControllerOffset + 0x28,
              (Connected ? (uint)HidControllerConnState.Controller_State_Connected : 0) |
              (Wired ? (uint)HidControllerConnState.Controller_State_Wired : 0));
        }
    }
}
