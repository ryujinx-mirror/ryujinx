using Ryujinx.Common.Configuration.Hid;
using Ryujinx.Common.Configuration.Hid.Controller;
using Ryujinx.Common.Configuration.Hid.Keyboard;
using Ryujinx.Configuration;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services.Hid;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using CemuHookClient = Ryujinx.Input.Motion.CemuHook.Client;

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

        public NpadManager(IGamepadDriver keyboardDriver, IGamepadDriver gamepadDriver)
        {
            _controllers = new NpadController[MaxControllers];
            _cemuHookClient = new CemuHookClient();

            _keyboardDriver = keyboardDriver;
            _gamepadDriver = gamepadDriver;
            _inputConfig = ConfigurationState.Instance.Hid.InputConfig.Value;

            _gamepadDriver.OnGamepadConnected += HandleOnGamepadConnected;
            _gamepadDriver.OnGamepadDisconnected += HandleOnGamepadDisconnected;
        }

        private void HandleOnGamepadDisconnected(string obj)
        {
            // Force input reload
            ReloadConfiguration(ConfigurationState.Instance.Hid.InputConfig.Value);
        }

        private void HandleOnGamepadConnected(string id)
        {
            // Force input reload
            ReloadConfiguration(ConfigurationState.Instance.Hid.InputConfig.Value);
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

        public void ReloadConfiguration(List<InputConfig> inputConfig)
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

                // Enforce an update of the property that will be updated by HLE.
                // TODO: Move that in the input manager maybe?
                ConfigurationState.Instance.Hid.InputConfig.Value = inputConfig;
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

        public void Update(Hid hleHid, TamperMachine tamperMachine)
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

                        inputState.Buttons |= hleHid.UpdateStickButtons(inputState.LStick, inputState.RStick);

                        motionState = controller.GetHLEMotionState();

                        if (ConfigurationState.Instance.Hid.EnableKeyboard)
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

                hleHid.Npads.Update(hleInputStates);
                hleHid.Npads.UpdateSixAxis(hleMotionStates);

                if (hleKeyboardInput.HasValue)
                {
                    hleHid.Keyboard.Update(hleKeyboardInput.Value);
                }

                tamperMachine.UpdateInput(hleInputStates);
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
