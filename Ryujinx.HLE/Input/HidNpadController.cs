namespace Ryujinx.HLE.Input
{
    public class HidNpadController : HidControllerBase
    {
        private (NpadColor Left, NpadColor Right) NpadBodyColors;
        private (NpadColor Left, NpadColor Right) NpadButtonColors;

        private HidControllerLayouts CurrentLayout;

        private bool IsHalf;

        public HidNpadController(
            HidControllerType      ControllerType,
            Switch                 Device,
            (NpadColor, NpadColor) NpadBodyColors,
            (NpadColor, NpadColor) NpadButtonColors) : base(ControllerType, Device)
        {
            this.NpadBodyColors   = NpadBodyColors;
            this.NpadButtonColors = NpadButtonColors;

            CurrentLayout = HidControllerLayouts.Handheld_Joined;

            switch (ControllerType)
            {
                case HidControllerType.NpadLeft:
                    CurrentLayout = HidControllerLayouts.Left;
                    break;
                case HidControllerType.NpadRight:
                    CurrentLayout = HidControllerLayouts.Right;
                    break;
                case HidControllerType.NpadPair:
                    CurrentLayout = HidControllerLayouts.Joined;
                    break;
            }
        }

        public override void Connect(HidControllerId ControllerId)
        {
            if(HidControllerType != HidControllerType.NpadLeft && HidControllerType != HidControllerType.NpadRight)
            {
                IsHalf = false;
            }

            base.Connect(CurrentLayout == HidControllerLayouts.Handheld_Joined ? HidControllerId.CONTROLLER_HANDHELD : ControllerId);

            HidControllerColorDesc SingleColorDesc =
                HidControllerColorDesc.ColorDesc_ColorsNonexistent;

            HidControllerColorDesc SplitColorDesc = 0;

            NpadColor SingleColorBody    = NpadColor.Black;
            NpadColor SingleColorButtons = NpadColor.Black;

            Device.Memory.WriteInt32(Offset + 0x04, IsHalf ? 1 : 0);

            if (IsHalf)
            {
                Device.Memory.WriteInt32(Offset + 0x08, (int)SingleColorDesc);
                Device.Memory.WriteInt32(Offset + 0x0c, (int)SingleColorBody);
                Device.Memory.WriteInt32(Offset + 0x10, (int)SingleColorButtons);
                Device.Memory.WriteInt32(Offset + 0x14, (int)SplitColorDesc);
            }
            else
            {
                Device.Memory.WriteInt32(Offset + 0x18, (int)NpadBodyColors.Left);
                Device.Memory.WriteInt32(Offset + 0x1c, (int)NpadButtonColors.Left);
                Device.Memory.WriteInt32(Offset + 0x20, (int)NpadBodyColors.Right);
                Device.Memory.WriteInt32(Offset + 0x24, (int)NpadButtonColors.Right);
            }

            Connected = true;
        }

        public override void SendInput
            (HidControllerButtons Buttons,
            HidJoystickPosition   LeftStick,
            HidJoystickPosition   RightStick)
        {
            long ControllerOffset = WriteInput(Buttons, LeftStick, RightStick, CurrentLayout);

            Device.Memory.WriteInt64(ControllerOffset + 0x28,
              (Connected ? (uint)HidControllerConnState.Controller_State_Connected : 0) |
              (CurrentLayout == HidControllerLayouts.Handheld_Joined ? (uint)HidControllerConnState.Controller_State_Wired : 0));
        }
    }
}
