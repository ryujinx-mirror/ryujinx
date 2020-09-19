using Ryujinx.Common;
using Ryujinx.HLE.Exceptions;
using Ryujinx.Common.Configuration.Hid;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    public class Hid
    {
        private readonly Switch _device;

        private readonly ulong _hidMemoryAddress;

        internal ref HidSharedMemory SharedMemory => ref _device.Memory.GetRef<HidSharedMemory>(_hidMemoryAddress);

        internal const int SharedMemEntryCount = 17;

        public DebugPadDevice DebugPad;
        public TouchDevice    Touchscreen;
        public MouseDevice    Mouse;
        public KeyboardDevice Keyboard;
        public NpadDevices    Npads;

        static Hid()
        {
            if (Unsafe.SizeOf<ShMemDebugPad>() != 0x400)
            {
                throw new InvalidStructLayoutException<ShMemDebugPad>(0x400);
            }
            if (Unsafe.SizeOf<ShMemTouchScreen>() != 0x3000)
            {
                throw new InvalidStructLayoutException<ShMemTouchScreen>(0x3000);
            }
            if (Unsafe.SizeOf<ShMemKeyboard>() != 0x400)
            {
                throw new InvalidStructLayoutException<ShMemKeyboard>(0x400);
            }
            if (Unsafe.SizeOf<ShMemMouse>() != 0x400)
            {
                throw new InvalidStructLayoutException<ShMemMouse>(0x400);
            }
            if (Unsafe.SizeOf<ShMemNpad>() != 0x5000)
            {
                throw new InvalidStructLayoutException<ShMemNpad>(0x5000);
            }
            if (Unsafe.SizeOf<HidSharedMemory>() != Horizon.HidSize)
            {
                throw new InvalidStructLayoutException<HidSharedMemory>(Horizon.HidSize);
            }
        }

        public Hid(in Switch device, ulong sharedHidMemoryAddress)
        {
            _device           = device;
            _hidMemoryAddress = sharedHidMemoryAddress;

            device.Memory.ZeroFill(sharedHidMemoryAddress, Horizon.HidSize);
        }

        public void InitDevices()
        {
            DebugPad    = new DebugPadDevice(_device, true);
            Touchscreen = new TouchDevice(_device, true);
            Mouse       = new MouseDevice(_device, false);
            Keyboard    = new KeyboardDevice(_device, false);
            Npads       = new NpadDevices(_device, true);
        }

        internal void RefreshInputConfig(List<InputConfig> inputConfig)
        {
            ControllerConfig[] npadConfig = new ControllerConfig[inputConfig.Count];

            for (int i = 0; i < npadConfig.Length; ++i)
            {
                npadConfig[i].Player = (PlayerIndex)inputConfig[i].PlayerIndex;
                npadConfig[i].Type = (ControllerType)inputConfig[i].ControllerType;
            }

            _device.Hid.Npads.Configure(npadConfig);
        }

        internal void RefreshInputConfigEvent(object _, ReactiveEventArgs<List<InputConfig>> args)
        {
            RefreshInputConfig(args.NewValue);
        }

        public ControllerKeys UpdateStickButtons(JoystickPosition leftStick, JoystickPosition rightStick)
        {
            const int stickButtonThreshold = short.MaxValue / 2;
            ControllerKeys result = 0;

            result |= (leftStick.Dx < -stickButtonThreshold) ? ControllerKeys.LStickLeft  : result;
            result |= (leftStick.Dx > stickButtonThreshold)  ? ControllerKeys.LStickRight : result;
            result |= (leftStick.Dy < -stickButtonThreshold) ? ControllerKeys.LStickDown  : result;
            result |= (leftStick.Dy > stickButtonThreshold)  ? ControllerKeys.LStickUp    : result;

            result |= (rightStick.Dx < -stickButtonThreshold) ? ControllerKeys.RStickLeft  : result;
            result |= (rightStick.Dx > stickButtonThreshold)  ? ControllerKeys.RStickRight : result;
            result |= (rightStick.Dy < -stickButtonThreshold) ? ControllerKeys.RStickDown  : result;
            result |= (rightStick.Dy > stickButtonThreshold)  ? ControllerKeys.RStickUp    : result;

            return result;
        }

        internal static ulong GetTimestampTicks()
        {
            return (ulong)PerformanceCounter.ElapsedMilliseconds * 19200;
        }
    }
}
