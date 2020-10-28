using System;
using System.Collections.Generic;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Memory;
using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    public class NpadDevices : BaseDevice
    {
        private const BatteryCharge DefaultBatteryCharge = BatteryCharge.Percent100;

        private const int NoMatchNotifyFrequencyMs = 2000;
        private int _activeCount;
        private long _lastNotifyTimestamp;

        public const int MaxControllers = 9; // Players 1-8 and Handheld
        private ControllerType[] _configuredTypes;
        private KEvent[] _styleSetUpdateEvents;
        private bool[] _supportedPlayers;

        internal NpadJoyHoldType JoyHold { get; set; }
        internal bool SixAxisActive = false; // TODO: link to hidserver when implemented
        internal ControllerType SupportedStyleSets { get; set; }

        public NpadDevices(Switch device, bool active = true) : base(device, active)
        {
            _configuredTypes = new ControllerType[MaxControllers];

            SupportedStyleSets = ControllerType.Handheld | ControllerType.JoyconPair |
                                 ControllerType.JoyconLeft | ControllerType.JoyconRight |
                                 ControllerType.ProController;

            _supportedPlayers = new bool[MaxControllers];
            _supportedPlayers.AsSpan().Fill(true);

            _styleSetUpdateEvents = new KEvent[MaxControllers];
            for (int i = 0; i < _styleSetUpdateEvents.Length; ++i)
            {
                _styleSetUpdateEvents[i] = new KEvent(_device.System.KernelContext);
            }

            _activeCount = 0;

            JoyHold = NpadJoyHoldType.Vertical;
        }

        internal ref KEvent GetStyleSetUpdateEvent(PlayerIndex player)
        {
            return ref _styleSetUpdateEvents[(int)player];
        }

        internal void ClearSupportedPlayers()
        {
            _supportedPlayers.AsSpan().Clear();
        }

        internal void SetSupportedPlayer(PlayerIndex player, bool supported = true)
        {
            _supportedPlayers[(int)player] = supported;
        }

        internal IEnumerable<PlayerIndex> GetSupportedPlayers()
        {
            for (int i = 0; i < _supportedPlayers.Length; ++i)
            {
                if (_supportedPlayers[i])
                {
                    yield return (PlayerIndex)i;
                }
            }
        }

        public bool Validate(int playerMin, int playerMax, ControllerType acceptedTypes, out int configuredCount, out PlayerIndex primaryIndex)
        {
            primaryIndex = PlayerIndex.Unknown;
            configuredCount = 0;

            for (int i = 0; i < MaxControllers; ++i)
            {
                ControllerType npad = _configuredTypes[i];

                if (npad == ControllerType.Handheld && _device.System.State.DockedMode)
                {
                    continue;
                }

                ControllerType currentType = _device.Hid.SharedMemory.Npads[i].Header.Type;

                if (currentType != ControllerType.None && (npad & acceptedTypes) != 0 && _supportedPlayers[i])
                {
                    configuredCount++;
                    if (primaryIndex == PlayerIndex.Unknown)
                    {
                        primaryIndex = (PlayerIndex)i;
                    }
                }
            }

            if (configuredCount < playerMin || configuredCount > playerMax || primaryIndex == PlayerIndex.Unknown)
            {
                return false;
            }

            return true;
        }

        public void Configure(params ControllerConfig[] configs)
        {
            _configuredTypes = new ControllerType[MaxControllers];

            for (int i = 0; i < configs.Length; ++i)
            {
                PlayerIndex player = configs[i].Player;
                ControllerType controllerType = configs[i].Type;

                if (player > PlayerIndex.Handheld)
                {
                    throw new ArgumentOutOfRangeException("Player must be Player1-8 or Handheld");
                }

                if (controllerType == ControllerType.Handheld)
                {
                    player = PlayerIndex.Handheld;
                }

                _configuredTypes[(int)player] = controllerType;

                Logger.Info?.Print(LogClass.Hid, $"Configured Controller {controllerType} to {player}");
            }
        }

        public void Update(IList<GamepadInput> states)
        {
            Remap();

            UpdateAllEntries();

            // Update configured inputs
            for (int i = 0; i < states.Count; ++i)
            {
                UpdateInput(states[i]);
            }
        }

        private void Remap()
        {
            // Remap/Init if necessary
            for (int i = 0; i < MaxControllers; ++i)
            {
                ControllerType config = _configuredTypes[i];

                // Remove Handheld config when Docked
                if (config == ControllerType.Handheld && _device.System.State.DockedMode)
                {
                    config = ControllerType.None;
                }

                // Auto-remap ProController and JoyconPair
                if (config == ControllerType.JoyconPair && (SupportedStyleSets & ControllerType.JoyconPair) == 0 && (SupportedStyleSets & ControllerType.ProController) != 0)
                {
                    config = ControllerType.ProController;
                }
                else if (config == ControllerType.ProController && (SupportedStyleSets & ControllerType.ProController) == 0 && (SupportedStyleSets & ControllerType.JoyconPair) != 0)
                {
                    config = ControllerType.JoyconPair;
                }

                // Check StyleSet and PlayerSet
                if ((config & SupportedStyleSets) == 0 || !_supportedPlayers[i])
                {
                    config = ControllerType.None;
                }

                SetupNpad((PlayerIndex)i, config);
            }

            if (_activeCount == 0 && PerformanceCounter.ElapsedMilliseconds > _lastNotifyTimestamp + NoMatchNotifyFrequencyMs)
            {
                Logger.Warning?.Print(LogClass.Hid, $"No matching controllers found. Application requests '{SupportedStyleSets}' on '{string.Join(", ", GetSupportedPlayers())}'");
                _lastNotifyTimestamp = PerformanceCounter.ElapsedMilliseconds;
            }
        }

        private void SetupNpad(PlayerIndex player, ControllerType type)
        {
            ref ShMemNpad controller = ref _device.Hid.SharedMemory.Npads[(int)player];

            ControllerType oldType = controller.Header.Type;

            if (oldType == type)
            {
                return; // Already configured
            }

            controller = new ShMemNpad(); // Zero it

            if (type == ControllerType.None)
            {
                _styleSetUpdateEvents[(int)player].ReadableEvent.Signal(); // Signal disconnect
                _activeCount--;

                Logger.Info?.Print(LogClass.Hid, $"Disconnected Controller {oldType} from {player}");

                return;
            }

            // TODO: Allow customizing colors at config
            NpadStateHeader defaultHeader = new NpadStateHeader
            {
                IsHalf             = false,
                SingleColorBody    = NpadColor.BodyGray,
                SingleColorButtons = NpadColor.ButtonGray,
                LeftColorBody      = NpadColor.BodyNeonBlue,
                LeftColorButtons   = NpadColor.ButtonGray,
                RightColorBody     = NpadColor.BodyNeonRed,
                RightColorButtons  = NpadColor.ButtonGray
            };

            controller.SystemProperties = NpadSystemProperties.PowerInfo0Connected |
                                          NpadSystemProperties.PowerInfo1Connected |
                                          NpadSystemProperties.PowerInfo2Connected;

            controller.BatteryState.ToSpan().Fill(DefaultBatteryCharge);

            switch (type)
            {
                case ControllerType.ProController:
                    defaultHeader.Type           = ControllerType.ProController;
                    controller.DeviceType        = DeviceType.FullKey;
                    controller.SystemProperties |= NpadSystemProperties.AbxyButtonOriented |
                                                   NpadSystemProperties.PlusButtonCapability |
                                                   NpadSystemProperties.MinusButtonCapability;
                    break;
                case ControllerType.Handheld:
                    defaultHeader.Type           = ControllerType.Handheld;
                    controller.DeviceType        = DeviceType.HandheldLeft |
                                                   DeviceType.HandheldRight;
                    controller.SystemProperties |= NpadSystemProperties.AbxyButtonOriented |
                                                   NpadSystemProperties.PlusButtonCapability |
                                                   NpadSystemProperties.MinusButtonCapability;
                    break;
                case ControllerType.JoyconPair:
                    defaultHeader.Type           = ControllerType.JoyconPair;
                    controller.DeviceType        = DeviceType.JoyLeft |
                                                   DeviceType.JoyRight;
                    controller.SystemProperties |= NpadSystemProperties.AbxyButtonOriented |
                                                   NpadSystemProperties.PlusButtonCapability |
                                                   NpadSystemProperties.MinusButtonCapability;
                    break;
                case ControllerType.JoyconLeft:
                    defaultHeader.Type           = ControllerType.JoyconLeft;
                    defaultHeader.IsHalf         = true;
                    controller.DeviceType        = DeviceType.JoyLeft;
                    controller.SystemProperties |= NpadSystemProperties.SlSrButtonOriented |
                                                   NpadSystemProperties.MinusButtonCapability;
                    break;
                case ControllerType.JoyconRight:
                    defaultHeader.Type           = ControllerType.JoyconRight;
                    defaultHeader.IsHalf         = true;
                    controller.DeviceType        = DeviceType.JoyRight;
                    controller.SystemProperties |= NpadSystemProperties.SlSrButtonOriented |
                                                   NpadSystemProperties.PlusButtonCapability;
                    break;
                case ControllerType.Pokeball:
                    defaultHeader.Type    = ControllerType.Pokeball;
                    controller.DeviceType = DeviceType.Palma;
                    break;
            }

            controller.Header = defaultHeader;

            _styleSetUpdateEvents[(int)player].ReadableEvent.Signal();
            _activeCount++;

            Logger.Info?.Print(LogClass.Hid, $"Connected Controller {type} to {player}");
        }

        private static NpadLayoutsIndex ControllerTypeToNpadLayout(ControllerType controllerType)
        => controllerType switch
        {
            ControllerType.ProController => NpadLayoutsIndex.ProController,
            ControllerType.Handheld      => NpadLayoutsIndex.Handheld,
            ControllerType.JoyconPair    => NpadLayoutsIndex.JoyDual,
            ControllerType.JoyconLeft    => NpadLayoutsIndex.JoyLeft,
            ControllerType.JoyconRight   => NpadLayoutsIndex.JoyRight,
            ControllerType.Pokeball      => NpadLayoutsIndex.Pokeball,
            _                            => NpadLayoutsIndex.SystemExternal
        };

        private void UpdateInput(GamepadInput state)
        {
            if (state.PlayerId == PlayerIndex.Unknown)
            {
                return;
            }

            ref ShMemNpad currentNpad = ref _device.Hid.SharedMemory.Npads[(int)state.PlayerId];

            if (currentNpad.Header.Type == ControllerType.None)
            {
                return;
            }

            ref NpadLayout currentLayout = ref currentNpad.Layouts[(int)ControllerTypeToNpadLayout(currentNpad.Header.Type)];
            ref NpadState  currentEntry  = ref currentLayout.Entries[(int)currentLayout.Header.LatestEntry];

            currentEntry.Buttons = state.Buttons;
            currentEntry.LStickX = state.LStick.Dx;
            currentEntry.LStickY = state.LStick.Dy;
            currentEntry.RStickX = state.RStick.Dx;
            currentEntry.RStickY = state.RStick.Dy;

            // Mirror data to Default layout just in case
            ref NpadLayout mainLayout = ref currentNpad.Layouts[(int)NpadLayoutsIndex.SystemExternal];
            mainLayout.Entries[(int)mainLayout.Header.LatestEntry] = currentEntry;
        }

        private static SixAxixLayoutsIndex ControllerTypeToSixAxisLayout(ControllerType controllerType)
        => controllerType switch
        {
            ControllerType.ProController => SixAxixLayoutsIndex.ProController,
            ControllerType.Handheld      => SixAxixLayoutsIndex.Handheld,
            ControllerType.JoyconPair    => SixAxixLayoutsIndex.JoyDualLeft,
            ControllerType.JoyconLeft    => SixAxixLayoutsIndex.JoyLeft,
            ControllerType.JoyconRight   => SixAxixLayoutsIndex.JoyRight,
            ControllerType.Pokeball      => SixAxixLayoutsIndex.Pokeball,
            _                            => SixAxixLayoutsIndex.SystemExternal
        };

        public void UpdateSixAxis(IList<SixAxisInput> states)
        {
            for (int i = 0; i < states.Count; ++i)
            {
                if (SetSixAxisState(states[i]))
                {
                    i++;

                    if (i >= states.Count)
                    {
                        return;
                    }

                    SetSixAxisState(states[i], true);
                }
            }
        }

        private bool SetSixAxisState(SixAxisInput state, bool isRightPair = false)
        {
            if (state.PlayerId == PlayerIndex.Unknown)
            {
                return false;
            }

            ref ShMemNpad currentNpad = ref _device.Hid.SharedMemory.Npads[(int)state.PlayerId];

            if (currentNpad.Header.Type == ControllerType.None)
            {
                return false;
            }

            HidVector accel = new HidVector()
            {
                X = state.Accelerometer.X,
                Y = state.Accelerometer.Y,
                Z = state.Accelerometer.Z
            };

            HidVector gyro = new HidVector()
            {
                X = state.Gyroscope.X,
                Y = state.Gyroscope.Y,
                Z = state.Gyroscope.Z
            };

            HidVector rotation = new HidVector()
            {
                X = state.Rotation.X,
                Y = state.Rotation.Y,
                Z = state.Rotation.Z
            };

            ref NpadSixAxis currentLayout = ref currentNpad.Sixaxis[(int)ControllerTypeToSixAxisLayout(currentNpad.Header.Type) + (isRightPair ? 1 : 0)];
            ref SixAxisState currentEntry = ref currentLayout.Entries[(int)currentLayout.Header.LatestEntry];

            int previousEntryIndex = (int)(currentLayout.Header.LatestEntry == 0 ?
                                           currentLayout.Header.MaxEntryIndex : currentLayout.Header.LatestEntry - 1);

            ref SixAxisState previousEntry = ref currentLayout.Entries[previousEntryIndex];

            currentEntry.Accelerometer = accel;
            currentEntry.Gyroscope     = gyro;
            currentEntry.Rotations     = rotation;

            unsafe
            {
                for (int i = 0; i < 9; i++)
                {
                    currentEntry.Orientation[i] = state.Orientation[i];
                }
            }

            return currentNpad.Header.Type == ControllerType.JoyconPair && !isRightPair;
        }

        private void UpdateAllEntries()
        {
            ref Array10<ShMemNpad> controllers = ref _device.Hid.SharedMemory.Npads;
            for (int i = 0; i < controllers.Length; ++i)
            {
                ref Array7<NpadLayout> layouts = ref controllers[i].Layouts;
                for (int l = 0; l < layouts.Length; ++l)
                {
                    ref NpadLayout currentLayout = ref layouts[l];
                    int currentIndex = UpdateEntriesHeader(ref currentLayout.Header, out int previousIndex);

                    ref NpadState currentEntry = ref currentLayout.Entries[currentIndex];
                    NpadState previousEntry    = currentLayout.Entries[previousIndex];

                    currentEntry.SampleTimestamp  = previousEntry.SampleTimestamp + 1;
                    currentEntry.SampleTimestamp2 = previousEntry.SampleTimestamp2 + 1;

                    if (controllers[i].Header.Type == ControllerType.None)
                    {
                        continue;
                    }

                    currentEntry.ConnectionState = NpadConnectionState.ControllerStateConnected;

                    switch (controllers[i].Header.Type)
                    {
                        case ControllerType.Handheld:
                        case ControllerType.ProController:
                            currentEntry.ConnectionState |= NpadConnectionState.ControllerStateWired;
                            break;
                        case ControllerType.JoyconPair:
                            currentEntry.ConnectionState |= NpadConnectionState.JoyLeftConnected |
                                                            NpadConnectionState.JoyRightConnected;
                            break;
                        case ControllerType.JoyconLeft:
                            currentEntry.ConnectionState |= NpadConnectionState.JoyLeftConnected;
                            break;
                        case ControllerType.JoyconRight:
                            currentEntry.ConnectionState |= NpadConnectionState.JoyRightConnected;
                            break;
                    }
                }

                ref Array6<NpadSixAxis> sixaxis = ref controllers[i].Sixaxis;
                for (int l = 0; l < sixaxis.Length; ++l)
                {
                    ref NpadSixAxis currentLayout = ref sixaxis[l];
                    int currentIndex = UpdateEntriesHeader(ref currentLayout.Header, out int previousIndex);

                    ref SixAxisState currentEntry = ref currentLayout.Entries[currentIndex];
                    SixAxisState previousEntry = currentLayout.Entries[previousIndex];

                    currentEntry.SampleTimestamp  = previousEntry.SampleTimestamp + 1;
                    currentEntry.SampleTimestamp2 = previousEntry.SampleTimestamp2 + 1;

                    currentEntry._unknown2 = 1;
                }
            }
        }
    }
}