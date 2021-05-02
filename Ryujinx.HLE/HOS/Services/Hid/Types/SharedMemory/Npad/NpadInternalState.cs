using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;

namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Npad
{
    struct NpadInternalState
    {
        public NpadStyleTag StyleSet;
        public NpadJoyAssignmentMode JoyAssignmentMode;
        public NpadFullKeyColorState FullKeyColor;
        public NpadJoyColorState JoyColor;
        public RingLifo<NpadCommonState> FullKey;
        public RingLifo<NpadCommonState> Handheld;
        public RingLifo<NpadCommonState> JoyDual;
        public RingLifo<NpadCommonState> JoyLeft;
        public RingLifo<NpadCommonState> JoyRight;
        public RingLifo<NpadCommonState> Palma;
        public RingLifo<NpadCommonState> SystemExt;
        public RingLifo<SixAxisSensorState> FullKeySixAxisSensor;
        public RingLifo<SixAxisSensorState> HandheldSixAxisSensor;
        public RingLifo<SixAxisSensorState> JoyDualSixAxisSensor;
        public RingLifo<SixAxisSensorState> JoyDualRightSixAxisSensor;
        public RingLifo<SixAxisSensorState> JoyLeftSixAxisSensor;
        public RingLifo<SixAxisSensorState> JoyRightSixAxisSensor;
        public DeviceType DeviceType;
        private uint _reserved1;
        public NpadSystemProperties SystemProperties;
        public NpadSystemButtonProperties SystemButtonProperties;
        public NpadBatteryLevel BatteryLevelJoyDual;
        public NpadBatteryLevel BatteryLevelJoyLeft;
        public NpadBatteryLevel BatteryLevelJoyRight;
        public uint AppletFooterUiAttributes;
        public byte AppletFooterUiType;
        private unsafe fixed byte _reserved2[0x7B];
        public RingLifo<NpadGcTriggerState> GcTrigger;
        public NpadLarkType LarkTypeLeftAndMain;
        public NpadLarkType LarkTypeRight;
        public NpadLuciaType LuciaType;
        public uint Unknown43EC;

        public static NpadInternalState Create()
        {
            return new NpadInternalState
            {
                FullKey = RingLifo<NpadCommonState>.Create(),
                Handheld = RingLifo<NpadCommonState>.Create(),
                JoyDual = RingLifo<NpadCommonState>.Create(),
                JoyLeft = RingLifo<NpadCommonState>.Create(),
                JoyRight = RingLifo<NpadCommonState>.Create(),
                Palma = RingLifo<NpadCommonState>.Create(),
                SystemExt = RingLifo<NpadCommonState>.Create(),
                FullKeySixAxisSensor = RingLifo<SixAxisSensorState>.Create(),
                HandheldSixAxisSensor = RingLifo<SixAxisSensorState>.Create(),
                JoyDualSixAxisSensor = RingLifo<SixAxisSensorState>.Create(),
                JoyDualRightSixAxisSensor = RingLifo<SixAxisSensorState>.Create(),
                JoyLeftSixAxisSensor = RingLifo<SixAxisSensorState>.Create(),
                JoyRightSixAxisSensor = RingLifo<SixAxisSensorState>.Create(),
                GcTrigger = RingLifo<NpadGcTriggerState>.Create(),
            };
        }
    }
}