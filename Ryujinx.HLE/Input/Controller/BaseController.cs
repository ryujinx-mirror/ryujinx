using static Ryujinx.HLE.Input.Hid;

namespace Ryujinx.HLE.Input
{
    public abstract class BaseController : IHidDevice
    {
        protected ControllerStatus HidControllerType;
        protected ControllerId     ControllerId;

        private long _currentLayoutOffset;
        private long _mainLayoutOffset;

        protected long DeviceStateOffset => Offset + 0x4188;

        protected Switch Device { get; }

        public long Offset    { get; private set; }
        public bool Connected { get; protected set; }

        public ControllerHeader            Header             { get; private set; }
        public ControllerStateHeader       CurrentStateHeader { get; private set; }
        public ControllerDeviceState       DeviceState        { get; private set; }
        public ControllerLayouts           CurrentLayout      { get; private set; }
        public ControllerState             LastInputState     { get; set; }
        public ControllerConnectionState   ConnectionState    { get; protected set; }

        public BaseController(Switch device, ControllerStatus controllerType)
        {
            Device            = device;
            HidControllerType = controllerType;
        }

        protected void Initialize(
            bool isHalf,
            (NpadColor left, NpadColor right) bodyColors,
            (NpadColor left, NpadColor right) buttonColors,
            ControllerColorDescription        singleColorDesc   = 0,
            ControllerColorDescription        splitColorDesc    = 0,
            NpadColor                         singleBodyColor   = 0,
            NpadColor                         singleButtonColor = 0
            )
        {
            Header = new ControllerHeader()
            {
                IsJoyConHalf           = isHalf ? 1 : 0,
                LeftBodyColor          = bodyColors.left,
                LeftButtonColor        = buttonColors.left,
                RightBodyColor         = bodyColors.right,
                RightButtonColor       = buttonColors.right,
                Status                 = HidControllerType,
                SingleBodyColor        = singleBodyColor,
                SingleButtonColor      = singleButtonColor,
                SplitColorDescription  = splitColorDesc,
                SingleColorDescription = singleColorDesc,
            };

            CurrentStateHeader = new ControllerStateHeader
            {
                EntryCount        = HidEntryCount,
                MaxEntryCount     = HidEntryCount - 1,
                CurrentEntryIndex = -1
            };

            DeviceState = new ControllerDeviceState()
            {
                PowerInfo0BatteryState = BatteryState.Percent100,
                PowerInfo1BatteryState = BatteryState.Percent100,
                PowerInfo2BatteryState = BatteryState.Percent100,
                DeviceType             = ControllerDeviceType.NPadLeftController | ControllerDeviceType.NPadRightController,
                DeviceFlags            = DeviceFlags.PowerInfo0Connected
                                            | DeviceFlags.PowerInfo1Connected
                                            | DeviceFlags.PowerInfo2Connected
            };

            LastInputState = new ControllerState()
            {
                SamplesTimestamp  = -1,
                SamplesTimestamp2 = -1
            };
        }

        public virtual void Connect(ControllerId controllerId)
        {
            ControllerId = controllerId;

            Offset = Device.Hid.HidPosition + HidControllersOffset + (int)controllerId * HidControllerSize;

            _mainLayoutOffset = Offset + HidControllerHeaderSize
                + ((int)ControllerLayouts.Main * HidControllerLayoutsSize);

            Device.Memory.FillWithZeros(Offset, 0x5000);
            Device.Memory.WriteStruct(Offset, Header);
            Device.Memory.WriteStruct(DeviceStateOffset, DeviceState);

            Connected = true;
        }

        public void SetLayout(ControllerLayouts controllerLayout)
        {
            CurrentLayout = controllerLayout;

            _currentLayoutOffset = Offset + HidControllerHeaderSize
                + ((int)controllerLayout * HidControllerLayoutsSize);
        }

        public void SendInput(
            ControllerButtons buttons,
            JoystickPosition  leftStick,
            JoystickPosition  rightStick)
        {
            ControllerState currentInput = new ControllerState()
            {
                SamplesTimestamp  = (long)LastInputState.SamplesTimestamp + 1,
                SamplesTimestamp2 = (long)LastInputState.SamplesTimestamp + 1,
                ButtonState       = buttons,
                ConnectionState   = ConnectionState,
                LeftStick         = leftStick,
                RightStick        = rightStick
            };

            ControllerStateHeader newInputStateHeader = new ControllerStateHeader
            {
                EntryCount        = HidEntryCount,
                MaxEntryCount     = HidEntryCount - 1,
                CurrentEntryIndex = (CurrentStateHeader.CurrentEntryIndex + 1) % HidEntryCount,
                Timestamp         = GetTimestamp(),
            };

            Device.Memory.WriteStruct(_currentLayoutOffset, newInputStateHeader);
            Device.Memory.WriteStruct(_mainLayoutOffset,    newInputStateHeader);

            long currentInputStateOffset = HidControllersLayoutHeaderSize
                + newInputStateHeader.CurrentEntryIndex * HidControllersInputEntrySize;

            Device.Memory.WriteStruct(_currentLayoutOffset + currentInputStateOffset, currentInput);
            Device.Memory.WriteStruct(_mainLayoutOffset + currentInputStateOffset,    currentInput);

            LastInputState     = currentInput;
            CurrentStateHeader = newInputStateHeader;
        }
    }
}
