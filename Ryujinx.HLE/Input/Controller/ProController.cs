namespace Ryujinx.HLE.Input
{
    public class ProController : BaseController
    {
        private bool _wired = false;

        private NpadColor _bodyColor;
        private NpadColor _buttonColor;

        public ProController(Switch    device,
                             NpadColor bodyColor,
                             NpadColor buttonColor) : base(device, ControllerStatus.ProController)
        {
            _wired = true;

            _bodyColor   = bodyColor;
            _buttonColor = buttonColor;
        }

        public override void Connect(ControllerId controllerId)
        {
            ControllerColorDescription singleColorDesc =
                ControllerColorDescription.ColorDescriptionColorsNonexistent;

            ControllerColorDescription splitColorDesc = 0;

            ConnectionState = ControllerConnectionState.ControllerStateConnected | ControllerConnectionState.ControllerStateWired;

            Initialize(false,
                (0, 0),
                (0, 0),
                singleColorDesc,
                splitColorDesc,
                _bodyColor,
                _buttonColor);

            base.Connect(controllerId);

            SetLayout(ControllerLayouts.ProController);
        }
    }
}
