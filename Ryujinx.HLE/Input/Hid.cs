using Ryujinx.Common;
using Ryujinx.Configuration.Hid;
using Ryujinx.HLE.HOS;
using System;

namespace Ryujinx.HLE.Input
{
    public partial class Hid
    {
        private Switch _device;

        private long _touchScreenOffset;
        private long _touchEntriesOffset;
        private long _keyboardOffset;

        private TouchHeader    _currentTouchHeader;
        private KeyboardHeader _currentKeyboardHeader;
        private KeyboardEntry  _currentKeyboardEntry;

        public BaseController PrimaryController { get; private set; }

        internal long HidPosition;

        public Hid(Switch device, long hidPosition)
        {
            _device     = device;
            HidPosition = hidPosition;

            device.Memory.FillWithZeros(hidPosition, Horizon.HidSize);

            _currentTouchHeader = new TouchHeader()
            {
                CurrentEntryIndex = -1,
            };

            _currentKeyboardHeader = new KeyboardHeader()
            {
                CurrentEntryIndex = -1,
            };

            _currentKeyboardEntry = new KeyboardEntry()
            {
                SamplesTimestamp  = -1,
                SamplesTimestamp2 = -1
            };

            _touchScreenOffset  = HidPosition + HidTouchScreenOffset;
            _touchEntriesOffset = _touchScreenOffset + HidTouchHeaderSize;
            _keyboardOffset     = HidPosition + HidKeyboardOffset;
        }

        private static ControllerStatus ConvertControllerTypeToState(ControllerType controllerType)
        {
            switch (controllerType)
            {
                case ControllerType.Handheld:      return ControllerStatus.Handheld;
                case ControllerType.NpadLeft:      return ControllerStatus.NpadLeft;
                case ControllerType.NpadRight:     return ControllerStatus.NpadRight;
                case ControllerType.NpadPair:      return ControllerStatus.NpadPair;
                case ControllerType.ProController: return ControllerStatus.ProController;
                default:                           throw new NotImplementedException();
            }
        }

        public void InitializePrimaryController(ControllerType controllerType)
        {
            ControllerId controllerId = controllerType == ControllerType.Handheld ?
                ControllerId.ControllerHandheld : ControllerId.ControllerPlayer1;

            if (controllerType == ControllerType.ProController)
            {
                PrimaryController = new ProController(_device, NpadColor.Black, NpadColor.Black);
            }
            else
            {
                PrimaryController = new NpadController(ConvertControllerTypeToState(controllerType),
                     _device,
                     (NpadColor.BodyNeonRed,     NpadColor.BodyNeonRed),
                     (NpadColor.ButtonsNeonBlue, NpadColor.ButtonsNeonBlue));
            }

            PrimaryController.Connect(controllerId);
        }

        public ControllerButtons UpdateStickButtons(
            JoystickPosition leftStick,
            JoystickPosition rightStick)
        {
            ControllerButtons result = 0;

            if (rightStick.Dx < 0)
            {
                result |= ControllerButtons.RStickLeft;
            }

            if (rightStick.Dx > 0)
            {
                result |= ControllerButtons.RStickRight;
            }

            if (rightStick.Dy < 0)
            {
                result |= ControllerButtons.RStickDown;
            }

            if (rightStick.Dy > 0)
            {
                result |= ControllerButtons.RStickUp;
            }

            if (leftStick.Dx < 0)
            {
                result |= ControllerButtons.LStickLeft;
            }

            if (leftStick.Dx > 0)
            {
                result |= ControllerButtons.LStickRight;
            }

            if (leftStick.Dy < 0)
            {
                result |= ControllerButtons.LStickDown;
            }

            if (leftStick.Dy > 0)
            {
                result |= ControllerButtons.LStickUp;
            }

            return result;
        }
        public void SetTouchPoints(params TouchPoint[] points)
        {
            long timestamp     = GetTimestamp();
            long sampleCounter = _currentTouchHeader.SamplesTimestamp + 1;

            var newTouchHeader = new TouchHeader
            {
                CurrentEntryIndex = (_currentTouchHeader.CurrentEntryIndex + 1) % HidEntryCount,
                EntryCount        = HidEntryCount,
                MaxEntries        = HidEntryCount - 1,
                SamplesTimestamp  = sampleCounter,
                Timestamp         = timestamp,
            };

            long currentTouchEntryOffset = _touchEntriesOffset + newTouchHeader.CurrentEntryIndex * HidTouchEntrySize;

            TouchEntry touchEntry = new TouchEntry()
            {
                SamplesTimestamp = sampleCounter,
                TouchCount       = points.Length
            };

            _device.Memory.WriteStruct(currentTouchEntryOffset, touchEntry);

            currentTouchEntryOffset += HidTouchEntryHeaderSize;

            for (int i = 0; i < points.Length; i++)
            {
                TouchData touch = new TouchData()
                {
                    Angle           = points[i].Angle,
                    DiameterX       = points[i].DiameterX,
                    DiameterY       = points[i].DiameterY,
                    Index           = i,
                    SampleTimestamp = sampleCounter,
                    X               = points[i].X,
                    Y               = points[i].Y
                };

                _device.Memory.WriteStruct(currentTouchEntryOffset, touch);

                currentTouchEntryOffset += HidTouchEntryTouchSize;
            }

            _device.Memory.WriteStruct(_touchScreenOffset, newTouchHeader);

            _currentTouchHeader = newTouchHeader;
        }

        public unsafe void WriteKeyboard(Keyboard keyboard)
        {
            long timestamp = GetTimestamp();

            var newKeyboardHeader = new KeyboardHeader()
            {
                CurrentEntryIndex = (_currentKeyboardHeader.CurrentEntryIndex + 1) % HidEntryCount,
                EntryCount        = HidEntryCount,
                MaxEntries        = HidEntryCount - 1,
                Timestamp         = timestamp,
            };

            _device.Memory.WriteStruct(_keyboardOffset, newKeyboardHeader);

            long keyboardEntryOffset = _keyboardOffset + HidKeyboardHeaderSize;
            keyboardEntryOffset += newKeyboardHeader.CurrentEntryIndex * HidKeyboardEntrySize;

            var newkeyboardEntry = new KeyboardEntry()
            {
                SamplesTimestamp  = _currentKeyboardEntry.SamplesTimestamp + 1,
                SamplesTimestamp2 = _currentKeyboardEntry.SamplesTimestamp2 + 1,
                Keys              = keyboard.Keys,
                Modifier          = keyboard.Modifier,
            };

            _device.Memory.WriteStruct(keyboardEntryOffset, newkeyboardEntry);

            _currentKeyboardEntry  = newkeyboardEntry;
            _currentKeyboardHeader = newKeyboardHeader;
        }

        internal static long GetTimestamp()
        {
            return PerformanceCounter.ElapsedMilliseconds * 19200;
        }
    }
}
