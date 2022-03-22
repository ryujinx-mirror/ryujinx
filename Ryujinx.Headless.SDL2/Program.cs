using ARMeilleure.Translation;
using ARMeilleure.Translation.PTC;
using CommandLine;
using LibHac.Tools.FsSystem;
using Ryujinx.Audio.Backends.SDL2;
using Ryujinx.Common;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Configuration.Hid;
using Ryujinx.Common.Configuration.Hid.Controller;
using Ryujinx.Common.Configuration.Hid.Controller.Motion;
using Ryujinx.Common.Configuration.Hid.Keyboard;
using Ryujinx.Common.Logging;
using Ryujinx.Common.System;
using Ryujinx.Common.Utilities;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.GAL.Multithreading;
using Ryujinx.Graphics.Gpu;
using Ryujinx.Graphics.Gpu.Shader;
using Ryujinx.Graphics.OpenGL;
using Ryujinx.Headless.SDL2.OpenGL;
using Ryujinx.HLE;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.Input;
using Ryujinx.Input.HLE;
using Ryujinx.Input.SDL2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;

using ConfigGamepadInputId = Ryujinx.Common.Configuration.Hid.Controller.GamepadInputId;
using ConfigStickInputId = Ryujinx.Common.Configuration.Hid.Controller.StickInputId;
using Key = Ryujinx.Common.Configuration.Hid.Key;

namespace Ryujinx.Headless.SDL2
{
    class Program
    {
        public static string Version { get; private set; }

        private static VirtualFileSystem _virtualFileSystem;
        private static ContentManager _contentManager;
        private static AccountManager _accountManager;
        private static LibHacHorizonManager _libHacHorizonManager;
        private static UserChannelPersistence _userChannelPersistence;
        private static InputManager _inputManager;
        private static Switch _emulationContext;
        private static WindowBase _window;
        private static WindowsMultimediaTimerResolution _windowsMultimediaTimerResolution;
        private static List<InputConfig> _inputConfiguration;
        private static bool _enableKeyboard;
        private static bool _enableMouse;

        static void Main(string[] args)
        {
            Version = ReleaseInformations.GetVersion();

            Console.Title = $"Ryujinx Console {Version} (Headless SDL2)";

            AppDataManager.Initialize(null);

            _virtualFileSystem = VirtualFileSystem.CreateInstance();
            _libHacHorizonManager = new LibHacHorizonManager();

            _libHacHorizonManager.InitializeFsServer(_virtualFileSystem);
            _libHacHorizonManager.InitializeArpServer();
            _libHacHorizonManager.InitializeBcatServer();
            _libHacHorizonManager.InitializeSystemClients();

            _contentManager = new ContentManager(_virtualFileSystem);
            _accountManager = new AccountManager(_libHacHorizonManager.RyujinxClient);
            _userChannelPersistence = new UserChannelPersistence();

            _inputManager = new InputManager(new SDL2KeyboardDriver(), new SDL2GamepadDriver());

            GraphicsConfig.EnableShaderCache = true;

            Parser.Default.ParseArguments<Options>(args)
            .WithParsed(options => Load(options))
            .WithNotParsed(errors => errors.Output());

            _inputManager.Dispose();
        }

        private static InputConfig HandlePlayerConfiguration(string inputProfileName, string inputId, PlayerIndex index)
        {
            if (inputId == null)
            {
                if (index == PlayerIndex.Player1)
                {
                    Logger.Info?.Print(LogClass.Application, $"{index} not configured, defaulting to default keyboard.");

                    // Default to keyboard
                    inputId = "0";
                }
                else
                {
                    Logger.Info?.Print(LogClass.Application, $"{index} not configured");

                    return null;
                }
            }

            IGamepad gamepad;

            bool isKeyboard = true;

            gamepad = _inputManager.KeyboardDriver.GetGamepad(inputId);

            if (gamepad == null)
            {
                gamepad = _inputManager.GamepadDriver.GetGamepad(inputId);
                isKeyboard = false;

                if (gamepad == null)
                {
                    Logger.Error?.Print(LogClass.Application, $"{index} gamepad not found (\"{inputId}\")");

                    return null;
                }
            }

            string gamepadName = gamepad.Name;

            gamepad.Dispose();

            InputConfig config;

            if (inputProfileName == null || inputProfileName.Equals("default"))
            {
                if (isKeyboard)
                {
                    config = new StandardKeyboardInputConfig
                    {
                        Version          = InputConfig.CurrentVersion,
                        Backend          = InputBackendType.WindowKeyboard,
                        Id               = null,
                        ControllerType   = ControllerType.JoyconPair,
                        LeftJoycon       = new LeftJoyconCommonConfig<Key>
                        {
                            DpadUp       = Key.Up,
                            DpadDown     = Key.Down,
                            DpadLeft     = Key.Left,
                            DpadRight    = Key.Right,
                            ButtonMinus  = Key.Minus,
                            ButtonL      = Key.E,
                            ButtonZl     = Key.Q,
                            ButtonSl     = Key.Unbound,
                            ButtonSr     = Key.Unbound
                        },

                        LeftJoyconStick  = new JoyconConfigKeyboardStick<Key>
                        {
                            StickUp      = Key.W,
                            StickDown    = Key.S,
                            StickLeft    = Key.A,
                            StickRight   = Key.D,
                            StickButton  = Key.F,
                        },

                        RightJoycon      = new RightJoyconCommonConfig<Key>
                        {
                            ButtonA      = Key.Z,
                            ButtonB      = Key.X,
                            ButtonX      = Key.C,
                            ButtonY      = Key.V,
                            ButtonPlus   = Key.Plus,
                            ButtonR      = Key.U,
                            ButtonZr     = Key.O,
                            ButtonSl     = Key.Unbound,
                            ButtonSr     = Key.Unbound
                        },

                        RightJoyconStick = new JoyconConfigKeyboardStick<Key>
                        {
                            StickUp      = Key.I,
                            StickDown    = Key.K,
                            StickLeft    = Key.J,
                            StickRight   = Key.L,
                            StickButton  = Key.H,
                        }
                    };
                }
                else
                {
                    bool isNintendoStyle = gamepadName.Contains("Nintendo");

                    config = new StandardControllerInputConfig
                    {
                        Version          = InputConfig.CurrentVersion,
                        Backend          = InputBackendType.GamepadSDL2,
                        Id               = null,
                        ControllerType   = ControllerType.JoyconPair,
                        DeadzoneLeft     = 0.1f,
                        DeadzoneRight    = 0.1f,
                        RangeLeft        = 1.0f,
                        RangeRight       = 1.0f,
                        TriggerThreshold = 0.5f,
                        LeftJoycon = new LeftJoyconCommonConfig<ConfigGamepadInputId>
                        {
                            DpadUp       = ConfigGamepadInputId.DpadUp,
                            DpadDown     = ConfigGamepadInputId.DpadDown,
                            DpadLeft     = ConfigGamepadInputId.DpadLeft,
                            DpadRight    = ConfigGamepadInputId.DpadRight,
                            ButtonMinus  = ConfigGamepadInputId.Minus,
                            ButtonL      = ConfigGamepadInputId.LeftShoulder,
                            ButtonZl     = ConfigGamepadInputId.LeftTrigger,
                            ButtonSl     = ConfigGamepadInputId.Unbound,
                            ButtonSr     = ConfigGamepadInputId.Unbound,
                        },

                        LeftJoyconStick = new JoyconConfigControllerStick<ConfigGamepadInputId, ConfigStickInputId>
                        {
                            Joystick     = ConfigStickInputId.Left,
                            StickButton  = ConfigGamepadInputId.LeftStick,
                            InvertStickX = false,
                            InvertStickY = false,
                            Rotate90CW   = false,
                        },

                        RightJoycon = new RightJoyconCommonConfig<ConfigGamepadInputId>
                        {
                            ButtonA      = isNintendoStyle ? ConfigGamepadInputId.A : ConfigGamepadInputId.B,
                            ButtonB      = isNintendoStyle ? ConfigGamepadInputId.B : ConfigGamepadInputId.A,
                            ButtonX      = isNintendoStyle ? ConfigGamepadInputId.X : ConfigGamepadInputId.Y,
                            ButtonY      = isNintendoStyle ? ConfigGamepadInputId.Y : ConfigGamepadInputId.X,
                            ButtonPlus   = ConfigGamepadInputId.Plus,
                            ButtonR      = ConfigGamepadInputId.RightShoulder,
                            ButtonZr     = ConfigGamepadInputId.RightTrigger,
                            ButtonSl     = ConfigGamepadInputId.Unbound,
                            ButtonSr     = ConfigGamepadInputId.Unbound,
                        },

                        RightJoyconStick = new JoyconConfigControllerStick<ConfigGamepadInputId, ConfigStickInputId>
                        {
                            Joystick     = ConfigStickInputId.Right,
                            StickButton  = ConfigGamepadInputId.RightStick,
                            InvertStickX = false,
                            InvertStickY = false,
                            Rotate90CW   = false,
                        },

                        Motion = new StandardMotionConfigController
                        {
                            MotionBackend = MotionInputBackendType.GamepadDriver,
                            EnableMotion = true,
                            Sensitivity  = 100,
                            GyroDeadzone = 1,
                        },
                        Rumble = new RumbleConfigController
                        {
                            StrongRumble = 1f,
                            WeakRumble = 1f,
                            EnableRumble = false
                        }
                    };
                }
            }
            else
            {
                string profileBasePath;

                if (isKeyboard)
                {
                    profileBasePath = Path.Combine(AppDataManager.ProfilesDirPath, "keyboard");
                }
                else
                {
                    profileBasePath = Path.Combine(AppDataManager.ProfilesDirPath, "controller");
                }

                string path = Path.Combine(profileBasePath, inputProfileName + ".json");

                if (!File.Exists(path))
                {
                    Logger.Error?.Print(LogClass.Application, $"Input profile \"{inputProfileName}\" not found for \"{inputId}\"");

                    return null;
                }

                try
                {
                    using (Stream stream = File.OpenRead(path))
                    {
                        config = JsonHelper.Deserialize<InputConfig>(stream);
                    }
                }
                catch (JsonException)
                {
                    Logger.Error?.Print(LogClass.Application, $"Input profile \"{inputProfileName}\" parsing failed for \"{inputId}\"");

                    return null;
                }
            }

            config.Id = inputId;
            config.PlayerIndex = index;

            string inputTypeName = isKeyboard ? "Keyboard" : "Gamepad";

            Logger.Info?.Print(LogClass.Application, $"{config.PlayerIndex} configured with {inputTypeName} \"{config.Id}\"");

            // If both stick ranges are 0 (usually indicative of an outdated profile load) then both sticks will be set to 1.0.
            if (config is StandardControllerInputConfig controllerConfig)
            {
                if (controllerConfig.RangeLeft <= 0.0f && controllerConfig.RangeRight <= 0.0f)
                {
                    controllerConfig.RangeLeft  = 1.0f;
                    controllerConfig.RangeRight = 1.0f;

                    Logger.Info?.Print(LogClass.Application, $"{config.PlayerIndex} stick range reset. Save the profile now to update your configuration");
                }
            }

            return config;
        }

        static void Load(Options option)
        {
            IGamepad gamepad;

            if (option.ListInputIds)
            {
                Logger.Info?.Print(LogClass.Application, "Input Ids:");

                foreach (string id in _inputManager.KeyboardDriver.GamepadsIds)
                {
                    gamepad = _inputManager.KeyboardDriver.GetGamepad(id);

                    Logger.Info?.Print(LogClass.Application, $"- {id} (\"{gamepad.Name}\")");

                    gamepad.Dispose();
                }

                foreach (string id in _inputManager.GamepadDriver.GamepadsIds)
                {
                    gamepad = _inputManager.GamepadDriver.GetGamepad(id);

                    Logger.Info?.Print(LogClass.Application, $"- {id} (\"{gamepad.Name}\")");

                    gamepad.Dispose();
                }

                return;
            }

            if (option.InputPath == null)
            {
                Logger.Error?.Print(LogClass.Application, "Please provide a file to load");

                return;
            }

            _inputConfiguration = new List<InputConfig>();
            _enableKeyboard = (bool)option.EnableKeyboard;
            _enableMouse = (bool)option.EnableMouse;

            void LoadPlayerConfiguration(string inputProfileName, string inputId, PlayerIndex index)
            {
                InputConfig inputConfig = HandlePlayerConfiguration(inputProfileName, inputId, index);

                if (inputConfig != null)
                {
                    _inputConfiguration.Add(inputConfig);
                }
            }

            LoadPlayerConfiguration(option.InputProfile1Name, option.InputId1, PlayerIndex.Player1);
            LoadPlayerConfiguration(option.InputProfile2Name, option.InputId2, PlayerIndex.Player2);
            LoadPlayerConfiguration(option.InputProfile3Name, option.InputId3, PlayerIndex.Player3);
            LoadPlayerConfiguration(option.InputProfile4Name, option.InputId4, PlayerIndex.Player4);
            LoadPlayerConfiguration(option.InputProfile5Name, option.InputId5, PlayerIndex.Player5);
            LoadPlayerConfiguration(option.InputProfile6Name, option.InputId6, PlayerIndex.Player6);
            LoadPlayerConfiguration(option.InputProfile7Name, option.InputId7, PlayerIndex.Player7);
            LoadPlayerConfiguration(option.InputProfile8Name, option.InputId8, PlayerIndex.Player8);
            LoadPlayerConfiguration(option.InputProfileHandheldName, option.InputIdHandheld, PlayerIndex.Handheld);

            if (_inputConfiguration.Count == 0)
            {
                return;
            }

            // Setup logging level
            Logger.SetEnable(LogLevel.Debug, (bool)option.LoggingEnableDebug);
            Logger.SetEnable(LogLevel.Stub, (bool)option.LoggingEnableStub);
            Logger.SetEnable(LogLevel.Info, (bool)option.LoggingEnableInfo);
            Logger.SetEnable(LogLevel.Warning, (bool)option.LoggingEnableWarning);
            Logger.SetEnable(LogLevel.Error, (bool)option.LoggingEnableError);
            Logger.SetEnable(LogLevel.Trace, (bool)option.LoggingEnableTrace);
            Logger.SetEnable(LogLevel.Guest, (bool)option.LoggingEnableGuest);
            Logger.SetEnable(LogLevel.AccessLog, (bool)option.LoggingEnableFsAccessLog);

            if ((bool)option.EnableFileLog)
            {
                Logger.AddTarget(new AsyncLogTargetWrapper(
                    new FileLogTarget(ReleaseInformations.GetBaseApplicationDirectory(), "file"),
                    1000,
                    AsyncLogTargetOverflowAction.Block
                ));
            }

            // Setup graphics configuration
            GraphicsConfig.EnableShaderCache = (bool)option.EnableShaderCache;
            GraphicsConfig.ResScale = option.ResScale;
            GraphicsConfig.MaxAnisotropy = option.MaxAnisotropy;
            GraphicsConfig.ShadersDumpPath = option.GraphicsShadersDumpPath;

            while (true)
            {
                LoadApplication(option);

                if (_userChannelPersistence.PreviousIndex == -1 || !_userChannelPersistence.ShouldRestart)
                {
                    break;
                }

                _userChannelPersistence.ShouldRestart = false;
            }
        }

        private static void SetupProgressHandler()
        {
            Ptc.PtcStateChanged -= ProgressHandler;
            Ptc.PtcStateChanged += ProgressHandler;

            _emulationContext.Gpu.ShaderCacheStateChanged -= ProgressHandler;
            _emulationContext.Gpu.ShaderCacheStateChanged += ProgressHandler;
        }

        private static void ProgressHandler<T>(T state, int current, int total) where T : Enum
        {
            string label;

            switch (state)
            {
                case PtcLoadingState ptcState:
                    label = $"PTC : {current}/{total}";
                    break;
                case ShaderCacheState shaderCacheState:
                    label = $"Shaders : {current}/{total}";
                    break;
                default:
                    throw new ArgumentException($"Unknown Progress Handler type {typeof(T)}");
            }

            Logger.Info?.Print(LogClass.Application, label);
        }

        private static Switch InitializeEmulationContext(WindowBase window, Options options)
        {
            IRenderer renderer = new Renderer();

            BackendThreading threadingMode = options.BackendThreading;

            bool threadedGAL = threadingMode == BackendThreading.On || (threadingMode == BackendThreading.Auto && renderer.PreferThreading);

            if (threadedGAL)
            {
                renderer = new ThreadedRenderer(renderer);
            }

            HLEConfiguration configuration = new HLEConfiguration(_virtualFileSystem,
                                                                  _libHacHorizonManager,
                                                                  _contentManager,
                                                                  _accountManager,
                                                                  _userChannelPersistence,
                                                                  renderer,
                                                                  new SDL2HardwareDeviceDriver(),
                                                                  (bool)options.ExpandRam ? MemoryConfiguration.MemoryConfiguration6GB : MemoryConfiguration.MemoryConfiguration4GB,
                                                                  window,
                                                                  options.SystemLanguage,
                                                                  options.SystemRegion,
                                                                  (bool)options.EnableVsync,
                                                                  (bool)options.EnableDockedMode,
                                                                  (bool)options.EnablePtc,
                                                                  (bool)options.EnableInternetAccess,
                                                                  (bool)options.EnableFsIntegrityChecks ? IntegrityCheckLevel.ErrorOnInvalid : IntegrityCheckLevel.None,
                                                                  options.FsGlobalAccessLogMode,
                                                                  options.SystemTimeOffset,
                                                                  options.SystemTimeZone,
                                                                  options.MemoryManagerMode,
                                                                  (bool)options.IgnoreMissingServices,
                                                                  options.AspectRatio,
                                                                  options.AudioVolume);

            return new Switch(configuration);
        }

        private static void ExecutionEntrypoint()
        {
            if (OperatingSystem.IsWindows())
            {
                _windowsMultimediaTimerResolution = new WindowsMultimediaTimerResolution(1);
            }

            DisplaySleep.Prevent();

            _window.Initialize(_emulationContext, _inputConfiguration, _enableKeyboard, _enableMouse);

            _window.Execute();

            Ptc.Close();
            PtcProfiler.Stop();

            _emulationContext.Dispose();
            _window.Dispose();

            if (OperatingSystem.IsWindows())
            {
                _windowsMultimediaTimerResolution?.Dispose();
                _windowsMultimediaTimerResolution = null;
            }
        }

        private static bool LoadApplication(Options options)
        {
            string path = options.InputPath;

            Logger.RestartTime();

            _window = new OpenGLWindow(_inputManager, options.LoggingGraphicsDebugLevel, options.AspectRatio, (bool)options.EnableMouse);
            _emulationContext = InitializeEmulationContext(_window, options);

            SetupProgressHandler();

            SystemVersion firmwareVersion = _contentManager.GetCurrentFirmwareVersion();

            Logger.Notice.Print(LogClass.Application, $"Using Firmware Version: {firmwareVersion?.VersionString}");

            if (Directory.Exists(path))
            {
                string[] romFsFiles = Directory.GetFiles(path, "*.istorage");

                if (romFsFiles.Length == 0)
                {
                    romFsFiles = Directory.GetFiles(path, "*.romfs");
                }

                if (romFsFiles.Length > 0)
                {
                    Logger.Info?.Print(LogClass.Application, "Loading as cart with RomFS.");
                    _emulationContext.LoadCart(path, romFsFiles[0]);
                }
                else
                {
                    Logger.Info?.Print(LogClass.Application, "Loading as cart WITHOUT RomFS.");
                    _emulationContext.LoadCart(path);
                }
            }
            else if (File.Exists(path))
            {
                switch (Path.GetExtension(path).ToLowerInvariant())
                {
                    case ".xci":
                        Logger.Info?.Print(LogClass.Application, "Loading as XCI.");
                        _emulationContext.LoadXci(path);
                        break;
                    case ".nca":
                        Logger.Info?.Print(LogClass.Application, "Loading as NCA.");
                        _emulationContext.LoadNca(path);
                        break;
                    case ".nsp":
                    case ".pfs0":
                        Logger.Info?.Print(LogClass.Application, "Loading as NSP.");
                        _emulationContext.LoadNsp(path);
                        break;
                    default:
                        Logger.Info?.Print(LogClass.Application, "Loading as Homebrew.");
                        try
                        {
                            _emulationContext.LoadProgram(path);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            Logger.Error?.Print(LogClass.Application, "The specified file is not supported by Ryujinx.");

                            return false;
                        }
                        break;
                }
            }
            else
            {
                Logger.Warning?.Print(LogClass.Application, "Please specify a valid XCI/NCA/NSP/PFS0/NRO file.");

                _emulationContext.Dispose();

                return false;
            }

            Translator.IsReadyForTranslation.Reset();

            Thread windowThread = new Thread(() =>
            {
                ExecutionEntrypoint();
            })
            {
                Name = "GUI.WindowThread"
            };

            windowThread.Start();
            windowThread.Join();

            return true;
        }
    }
}
