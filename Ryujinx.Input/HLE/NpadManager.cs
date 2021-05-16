using Ryujinx.Common.Configuration.Hid;
using Ryujinx.Common.Configuration.Hid.Controller;
using Ryujinx.Common.Configuration.Hid.Keyboard;
using Ryujinx.HLE.HOS.Services.Hid;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using CemuHookClient = Ryujinx.Input.Motion.CemuHook.Client;
using Switch = Ryujinx.HLE.Switch;

namespace Ryujinx.Input.HLE
{
    public class NpadManager : IDisposable
    {
        private CemuHookClient _cemuHookClient;

        private object _lock = new object();

        private bool _blockInputUpdates;

        private const int MaxControllers = 9;

        private NpadController[] _controllers;

        private readonly IGamepadDriver _keyboardDriver;
        private readonly IGamepadDriver _gamepadDriver;

        private bool _isDisposed;

        private List<InputConfig> _inputConfig;
        private bool _enableKeyboard;
        private Switch _device;

        public NpadManager(IGamepadDriver keyboardDriver, IGamepadDriver gamepadDriver)
        {
            _controllers = new NpadController[MaxControllers];
            _cemuHookClient = new CemuHookClient(this);

            _keyboardDriver = keyboardDriver;
            _gamepadDriver = gamepadDriver;
            _inputConfig = new List<InputConfig>();
            _enableKeyboard = false;

            _gamepadDriver.OnGamepadConnected += HandleOnGamepadConnected;
            _gamepadDriver.OnGamepadDisconnected += HandleOnGamepadDisconnected;
        }

        private void RefreshInputConfigForHLE()
        {
            lock (_lock)
            {
                _device.Hid.RefreshInputConfig(_inputConfig);
            }
        }

        private void HandleOnGamepadDisconnected(string obj)
        {
            // Force input reload
            ReloadConfiguration(_inputConfig, _enableKeyboard);
        }

        private void HandleOnGamepadConnected(string id)
        {
            // Force input reload
            ReloadConfiguration(_inputConfig, _enableKeyboard);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool DriverConfigurationUpdate(ref NpadController controller, InputConfig config)
        {
            IGamepadDriver targetDriver = _gamepadDriver;

            if (config is StandardControllerInputConfig)
            {
                targetDriver = _gamepadDriver;
            }
            else if (config is StandardKeyboardInputConfig)
            {
                targetDriver = _keyboardDriver;
            }

            Debug.Assert(targetDriver != null, "Unknown input configuration!");

            if (controller.GamepadDriver != targetDriver || controller.Id != config.Id)
            {
                return controller.UpdateDriverConfiguration(targetDriver, config);
            }
            else
            {
                return controller.GamepadDriver != null;
            }
        }

        public void ReloadConfiguration(List<InputConfig> inputConfig, bool enableKeyboard)
        {
            lock (_lock)
            {
                for (int i = 0; i < _controllers.Length; i++)
                {
                    _controllers[i]?.Dispose();
                    _controllers[i] = null;
                }

                foreach (InputConfig inputConfigEntry in inputConfig)
                {
                    NpadController controller = new NpadController(_cemuHookClient);

                    bool isValid = DriverConfigurationUpdate(ref controller, inputConfigEntry);

                    if (!isValid)
                    {
                        controller.Dispose();
                    }
                    else
                    {
                        _controllers[(int)inputConfigEntry.PlayerIndex] = controller;
                    }
                }

                _inputConfig = inputConfig;
                _enableKeyboard = enableKeyboard;

                _device.Hid.RefreshInputConfig(inputConfig);
            }
        }

        public void UnblockInputUpdates()
        {
            lock (_lock)
            {
                _blockInputUpdates = false;
            }
        }

        public void BlockInputUpdates()
        {
            lock (_lock)
            {
                _blockInputUpdates = true;
            }
        }

        public void Initialize(Switch device, List<InputConfig> inputConfig, bool enableKeyboard)
        {
            _device = device;
            _device.Configuration.RefreshInputConfig = RefreshInputConfigForHLE;

            ReloadConfiguration(inputConfig, enableKeyboard);
        }

        public void Update()
        {
            lock (_lock)
            {
                List<GamepadInput> hleInputStates = new List<GamepadInput>();
                List<SixAxisInput> hleMotionStates = new List<SixAxisInput>(NpadDevices.MaxControllers);

                KeyboardInput? hleKeyboardInput = null;

                foreach (InputConfig inputConfig in _inputConfig)
                {
                    GamepadInput inputState = default;
                    SixAxisInput motionState = default;

                    NpadController controller = _controllers[(int)inputConfig.PlayerIndex];

                    // Do we allow input updates and is a controller connected?
                    if (!_blockInputUpdates && controller != null)
                    {
                        DriverConfigurationUpdate(ref controller, inputConfig);

                        controller.UpdateUserConfiguration(inputConfig);
                        controller.Update();

                        inputState = controller.GetHLEInputState();

                        inputState.Buttons |= _device.Hid.UpdateStickButtons(inputState.LStick, inputState.RStick);

                        motionState = controller.GetHLEMotionState();

                        if (_enableKeyboard)
                        {
                            hleKeyboardInput = controller.GetHLEKeyboardInput();
                        }
                    }
                    else
                    {
                        // Ensure that orientation isn't null
                        motionState.Orientation = new float[9];
                    }

                    inputState.PlayerId = (Ryujinx.HLE.HOS.Services.Hid.PlayerIndex)inputConfig.PlayerIndex;
                    motionState.PlayerId = (Ryujinx.HLE.HOS.Services.Hid.PlayerIndex)inputConfig.PlayerIndex;

                    hleInputStates.Add(inputState);
                    hleMotionStates.Add(motionState);
                }

                _device.Hid.Npads.Update(hleInputStates);
                _device.Hid.Npads.UpdateSixAxis(hleMotionStates);

                if (hleKeyboardInput.HasValue)
                {
                    _device.Hid.Keyboard.Update(hleKeyboardInput.Value);
                }

                _device.TamperMachine.UpdateInput(hleInputStates);
            }
        }

        internal InputConfig GetPlayerInputConfigByIndex(int index)
        {
            lock (_lock)
            {
                return _inputConfig.Find(x => x.PlayerIndex == (Ryujinx.Common.Configuration.Hid.PlayerIndex)index);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_lock)
                {
                    if (!_isDisposed)
                    {
                        _cemuHookClient.Dispose();

                        _gamepadDriver.OnGamepadConnected -= HandleOnGamepadConnected;
                        _gamepadDriver.OnGamepadDisconnected -= HandleOnGamepadDisconnected;

                        for (int i = 0; i < _controllers.Length; i++)
                        {
                            _controllers[i]?.Dispose();
                        }

                        _isDisposed = true;
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
