using ARMeilleure.Translation;
using ARMeilleure.Translation.PTC;
using Avalonia.Input;
using Avalonia.Threading;
using LibHac.Tools.FsSystem;
using Ryujinx.Audio.Backends.Dummy;
using Ryujinx.Audio.Backends.OpenAL;
using Ryujinx.Audio.Backends.SDL2;
using Ryujinx.Audio.Backends.SoundIo;
using Ryujinx.Audio.Integration;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Input;
using Ryujinx.Ava.Ui.Controls;
using Ryujinx.Ava.Ui.Models;
using Ryujinx.Ava.Ui.Windows;
using Ryujinx.Common;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Common.System;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.GAL.Multithreading;
using Ryujinx.Graphics.Gpu;
using Ryujinx.Graphics.OpenGL;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.Input;
using Ryujinx.Input.HLE;
using Ryujinx.Ui.Common;
using Ryujinx.Ui.Common.Configuration;
using Ryujinx.Ui.Common.Helper;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using InputManager = Ryujinx.Input.HLE.InputManager;
using Key = Ryujinx.Input.Key;
using MouseButton = Ryujinx.Input.MouseButton;
using Size = Avalonia.Size;
using Switch = Ryujinx.HLE.Switch;
using WindowState = Avalonia.Controls.WindowState;

namespace Ryujinx.Ava
{
    internal class AppHost
    {
        private const int CursorHideIdleTime = 8; // Hide Cursor seconds

        private static readonly Cursor InvisibleCursor = new Cursor(StandardCursorType.None);

        private readonly AccountManager _accountManager;
        private readonly UserChannelPersistence _userChannelPersistence;

        private readonly InputManager _inputManager;

        private readonly IKeyboard _keyboardInterface;

        private readonly MainWindow _parent;

        private readonly GraphicsDebugLevel _glLogLevel;

        private bool _hideCursorOnIdle;
        private bool _isStopped;
        private bool _isActive;
        private long _lastCursorMoveTime;

        private KeyboardHotkeyState _prevHotkeyState;

        private IRenderer _renderer;
        private readonly Thread _renderingThread;

        private bool _isMouseInRenderer;
        private bool _renderingStarted;
        private bool _dialogShown;

        private WindowsMultimediaTimerResolution _windowsMultimediaTimerResolution;

        private readonly CancellationTokenSource _gpuCancellationTokenSource;

        public event EventHandler AppExit;
        public event EventHandler<StatusUpdatedEventArgs> StatusUpdatedEvent;

        public RendererControl Renderer { get; }
        public VirtualFileSystem VirtualFileSystem { get; }
        public ContentManager ContentManager { get; }
        public Switch Device { get; set; }
        public NpadManager NpadManager { get; }
        public TouchScreenManager TouchScreenManager { get; }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public string ApplicationPath { get; private set; }

        private bool _isFirmwareTitle;

        public bool ScreenshotRequested { get; set; }

        private object _lockObject = new();

        public AppHost(
            RendererControl renderer,
            InputManager inputManager,
            string applicationPath,
            VirtualFileSystem virtualFileSystem,
            ContentManager contentManager,
            AccountManager accountManager,
            UserChannelPersistence userChannelPersistence,
            MainWindow parent)
        {
            _parent = parent;
            _inputManager = inputManager;
            _accountManager = accountManager;
            _userChannelPersistence = userChannelPersistence;
            _renderingThread = new Thread(RenderLoop) { Name = "GUI.RenderThread" };
            _hideCursorOnIdle = ConfigurationState.Instance.HideCursorOnIdle;
            _lastCursorMoveTime = Stopwatch.GetTimestamp();
            _glLogLevel = ConfigurationState.Instance.Logger.GraphicsDebugLevel;
            _inputManager.SetMouseDriver(new AvaloniaMouseDriver(renderer));
            _keyboardInterface = (IKeyboard)_inputManager.KeyboardDriver.GetGamepad("0");

            NpadManager = _inputManager.CreateNpadManager();
            TouchScreenManager = _inputManager.CreateTouchScreenManager();
            Renderer = renderer;
            ApplicationPath = applicationPath;
            VirtualFileSystem = virtualFileSystem;
            ContentManager = contentManager;

            if (ApplicationPath.StartsWith("@SystemContent"))
            {
                ApplicationPath = _parent.VirtualFileSystem.SwitchPathToSystemPath(ApplicationPath);

                _isFirmwareTitle = true;
            }

            ConfigurationState.Instance.HideCursorOnIdle.Event += HideCursorState_Changed;

            _parent.PointerLeave += Parent_PointerLeft;
            _parent.PointerMoved += Parent_PointerMoved;

            ConfigurationState.Instance.System.IgnoreMissingServices.Event += UpdateIgnoreMissingServicesState;
            ConfigurationState.Instance.Graphics.AspectRatio.Event += UpdateAspectRatioState;
            ConfigurationState.Instance.System.EnableDockedMode.Event += UpdateDockedModeState;
            ConfigurationState.Instance.System.AudioVolume.Event += UpdateAudioVolumeState;

            _gpuCancellationTokenSource = new CancellationTokenSource();
        }

        private void Parent_PointerMoved(object sender, PointerEventArgs e)
        {
            _lastCursorMoveTime = Stopwatch.GetTimestamp();
            var p = e.GetCurrentPoint(_parent).Position;
            var r = _parent.InputHitTest(p);
            _isMouseInRenderer = r == Renderer;
        }

        private void Parent_PointerLeft(object sender, PointerEventArgs e)
        {
            _isMouseInRenderer = false;
            _parent.Cursor = Cursor.Default;
        }

        private void SetRendererWindowSize(Size size)
        {
            if (_renderer != null)
            {
                double scale = _parent.PlatformImpl.RenderScaling;
                _renderer.Window.SetSize((int)(size.Width * scale), (int)(size.Height * scale));
            }
        }

        private unsafe void Renderer_ScreenCaptured(object sender, ScreenCaptureImageInfo e)
        {
            if (e.Data.Length > 0 && e.Height > 0 && e.Width > 0)
            {
                Task.Run(() =>
                {
                    lock (_lockObject)
                    {
                        var currentTime = DateTime.Now;
                        string filename = $"ryujinx_capture_{currentTime.Year}-{currentTime.Month:D2}-{currentTime.Day:D2}_{currentTime.Hour:D2}-{currentTime.Minute:D2}-{currentTime.Second:D2}.png";
                        string directory = AppDataManager.Mode switch
                        {
                            AppDataManager.LaunchMode.Portable => Path.Combine(AppDataManager.BaseDirPath, "screenshots"),
                            _ => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Ryujinx")
                        };

                        string path = Path.Combine(directory, filename);

                        try
                        {
                            Directory.CreateDirectory(directory);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error?.Print(LogClass.Application, $"Failed to create directory at path {directory}. Error : {ex.GetType().Name}", "Screenshot");

                            return;
                        }

                        Image image = e.IsBgra ? Image.LoadPixelData<Bgra32>(e.Data, e.Width, e.Height)
                                               : Image.LoadPixelData<Rgba32>(e.Data, e.Width, e.Height);

                        if (e.FlipX)
                        {
                            image.Mutate(x => x.Flip(FlipMode.Horizontal));
                        }

                        if (e.FlipY)
                        {
                            image.Mutate(x => x.Flip(FlipMode.Vertical));
                        }

                        image.SaveAsPng(path, new PngEncoder()
                        {
                            ColorType = PngColorType.Rgb
                        });

                        image.Dispose();

                        Logger.Notice.Print(LogClass.Application, $"Screenshot saved to {path}", "Screenshot");
                    }
                });
            }
            else
            {
                Logger.Error?.Print(LogClass.Application, $"Screenshot is empty. Size : {e.Data.Length} bytes. Resolution : {e.Width}x{e.Height}", "Screenshot");
            }
        }

        public void Start()
        {
            if (OperatingSystem.IsWindows())
            {
                _windowsMultimediaTimerResolution = new WindowsMultimediaTimerResolution(1);
            }

            DisplaySleep.Prevent();

            NpadManager.Initialize(Device, ConfigurationState.Instance.Hid.InputConfig, ConfigurationState.Instance.Hid.EnableKeyboard, ConfigurationState.Instance.Hid.EnableMouse);
            TouchScreenManager.Initialize(Device);

            _parent.ViewModel.IsGameRunning = true;

            string titleNameSection = string.IsNullOrWhiteSpace(Device.Application.TitleName)
                ? string.Empty
                : $" - {Device.Application.TitleName}";

            string titleVersionSection = string.IsNullOrWhiteSpace(Device.Application.DisplayVersion)
                ? string.Empty
                : $" v{Device.Application.DisplayVersion}";

            string titleIdSection = string.IsNullOrWhiteSpace(Device.Application.TitleIdText)
                ? string.Empty
                : $" ({Device.Application.TitleIdText.ToUpper()})";

            string titleArchSection = Device.Application.TitleIs64Bit
                ? " (64-bit)"
                : " (32-bit)";

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                _parent.Title = $"Ryujinx {Program.Version}{titleNameSection}{titleVersionSection}{titleIdSection}{titleArchSection}";
            });

            _parent.ViewModel.HandleShaderProgress(Device);

            Renderer.SizeChanged += Window_SizeChanged;

            _isActive = true;

            _renderingThread.Start();

            _parent.ViewModel.Volume = ConfigurationState.Instance.System.AudioVolume.Value;

            MainLoop();

            Exit();
        }

        private void UpdateIgnoreMissingServicesState(object sender, ReactiveEventArgs<bool> args)
        {
            if (Device != null)
            {
                Device.Configuration.IgnoreMissingServices = args.NewValue;
            }
        }

        private void UpdateAspectRatioState(object sender, ReactiveEventArgs<AspectRatio> args)
        {
            if (Device != null)
            {
                Device.Configuration.AspectRatio = args.NewValue;
            }
        }

        private void UpdateDockedModeState(object sender, ReactiveEventArgs<bool> e)
        {
            Device?.System.ChangeDockedModeState(e.NewValue);
        }

        private void UpdateAudioVolumeState(object sender, ReactiveEventArgs<float> e)
        {
            Device?.SetVolume(e.NewValue);
            Dispatcher.UIThread.Post(() =>
            {
                var value = e.NewValue;
                _parent.ViewModel.Volume = e.NewValue;
            });
        }

        public void Stop()
        {
            _isActive = false;
        }

        private void Exit()
        {
            (_keyboardInterface as AvaloniaKeyboard)?.Clear();

            if (_isStopped)
            {
                return;
            }

            _isStopped = true;
            _isActive = false;
        }

        public void DisposeContext()
        {
            Dispose();

            _isActive = false;

            _renderingThread.Join();

            DisplaySleep.Restore();

            Ptc.Close();
            PtcProfiler.Stop();
            NpadManager.Dispose();
            TouchScreenManager.Dispose();
            Device.Dispose();

            DisposeGpu();

            AppExit?.Invoke(this, EventArgs.Empty);
        }

        private void Dispose()
        {
            if (Device.Application != null)
            {
                _parent.UpdateGameMetadata(Device.Application.TitleIdText);
            }

            ConfigurationState.Instance.System.IgnoreMissingServices.Event -= UpdateIgnoreMissingServicesState;
            ConfigurationState.Instance.Graphics.AspectRatio.Event -= UpdateAspectRatioState;
            ConfigurationState.Instance.System.EnableDockedMode.Event -= UpdateDockedModeState;

            _gpuCancellationTokenSource.Cancel();
            _gpuCancellationTokenSource.Dispose();
        }

        public void DisposeGpu()
        {
            if (OperatingSystem.IsWindows())
            {
                _windowsMultimediaTimerResolution?.Dispose();
                _windowsMultimediaTimerResolution = null;
            }

            Renderer?.MakeCurrent();

            Device.DisposeGpu();

            Renderer?.DestroyBackgroundContext();
            Renderer?.MakeCurrent(null);
        }

        private void HideCursorState_Changed(object sender, ReactiveEventArgs<bool> state)
        {
            Dispatcher.UIThread.InvokeAsync(delegate
            {
                _hideCursorOnIdle = state.NewValue;

                if (_hideCursorOnIdle)
                {
                    _lastCursorMoveTime = Stopwatch.GetTimestamp();
                }
                else
                {
                    _parent.Cursor = Cursor.Default;
                }
            });
        }

        public async Task<bool> LoadGuestApplication()
        {
            InitializeSwitchInstance();

            MainWindow.UpdateGraphicsConfig();

            SystemVersion firmwareVersion = ContentManager.GetCurrentFirmwareVersion();

            if (!SetupValidator.CanStartApplication(ContentManager, ApplicationPath, out UserError userError))
            {
                if (SetupValidator.CanFixStartApplication(ContentManager, ApplicationPath, userError, out firmwareVersion))
                {
                    if (userError == UserError.NoFirmware)
                    {
                        string message = string.Format(LocaleManager.Instance["DialogFirmwareInstallEmbeddedMessage"],
                            firmwareVersion.VersionString);

                        UserResult result = await ContentDialogHelper.CreateConfirmationDialog(
                            LocaleManager.Instance["DialogFirmwareNoFirmwareInstalledMessage"], message,
                            LocaleManager.Instance["InputDialogYes"], LocaleManager.Instance["InputDialogNo"], "");

                        if (result != UserResult.Yes)
                        {
                            Dispatcher.UIThread.Post(async () => await
                                UserErrorDialog.ShowUserErrorDialog(userError, _parent));
                            Device.Dispose();

                            return false;
                        }
                    }

                    if (!SetupValidator.TryFixStartApplication(ContentManager, ApplicationPath, userError, out _))
                    {
                        Dispatcher.UIThread.Post(async () => await
                            UserErrorDialog.ShowUserErrorDialog(userError, _parent));
                        Device.Dispose();

                        return false;
                    }

                    // Tell the user that we installed a firmware for them.
                    if (userError == UserError.NoFirmware)
                    {
                        firmwareVersion = ContentManager.GetCurrentFirmwareVersion();

                        _parent.RefreshFirmwareStatus();

                        string message = string.Format(LocaleManager.Instance["DialogFirmwareInstallEmbeddedSuccessMessage"], firmwareVersion.VersionString);

                        await ContentDialogHelper.CreateInfoDialog(
                            string.Format(LocaleManager.Instance["DialogFirmwareInstalledMessage"], firmwareVersion.VersionString),
                            message,
                            LocaleManager.Instance["InputDialogOk"],
                            "",
                            LocaleManager.Instance["RyujinxInfo"]);
                    }
                }
                else
                {
                    Dispatcher.UIThread.Post(async () => await
                        UserErrorDialog.ShowUserErrorDialog(userError, _parent));

                    Device.Dispose();

                    return false;
                }
            }

            Logger.Notice.Print(LogClass.Application, $"Using Firmware Version: {firmwareVersion?.VersionString}");

            if (_isFirmwareTitle)
            {
                Logger.Info?.Print(LogClass.Application, "Loading as Firmware Title (NCA).");

                Device.LoadNca(ApplicationPath);
            }
            else if (Directory.Exists(ApplicationPath))
            {
                string[] romFsFiles = Directory.GetFiles(ApplicationPath, "*.istorage");

                if (romFsFiles.Length == 0)
                {
                    romFsFiles = Directory.GetFiles(ApplicationPath, "*.romfs");
                }

                if (romFsFiles.Length > 0)
                {
                    Logger.Info?.Print(LogClass.Application, "Loading as cart with RomFS.");

                    Device.LoadCart(ApplicationPath, romFsFiles[0]);
                }
                else
                {
                    Logger.Info?.Print(LogClass.Application, "Loading as cart WITHOUT RomFS.");

                    Device.LoadCart(ApplicationPath);
                }
            }
            else if (File.Exists(ApplicationPath))
            {
                switch (System.IO.Path.GetExtension(ApplicationPath).ToLowerInvariant())
                {
                    case ".xci":
                        {
                            Logger.Info?.Print(LogClass.Application, "Loading as XCI.");

                            Device.LoadXci(ApplicationPath);

                            break;
                        }
                    case ".nca":
                        {
                            Logger.Info?.Print(LogClass.Application, "Loading as NCA.");

                            Device.LoadNca(ApplicationPath);

                            break;
                        }
                    case ".nsp":
                    case ".pfs0":
                        {
                            Logger.Info?.Print(LogClass.Application, "Loading as NSP.");

                            Device.LoadNsp(ApplicationPath);

                            break;
                        }
                    default:
                        {
                            Logger.Info?.Print(LogClass.Application, "Loading as homebrew.");

                            try
                            {
                                Device.LoadProgram(ApplicationPath);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                Logger.Error?.Print(LogClass.Application, "The specified file is not supported by Ryujinx.");

                                Dispose();

                                return false;
                            }

                            break;
                        }
                }
            }
            else
            {
                Logger.Warning?.Print(LogClass.Application, "Please specify a valid XCI/NCA/NSP/PFS0/NRO file.");

                Dispose();

                return false;
            }

            DiscordIntegrationModule.SwitchToPlayingState(Device.Application.TitleIdText, Device.Application.TitleName);

            _parent.ApplicationLibrary.LoadAndSaveMetaData(Device.Application.TitleIdText, appMetadata =>
            {
                appMetadata.LastPlayed = DateTime.UtcNow.ToString();
            });

            return true;
        }

        internal void Resume()
        {
            Device?.System.TogglePauseEmulation(false);
            _parent.ViewModel.IsPaused = false;
        }

        internal void Pause()
        {
            Device?.System.TogglePauseEmulation(true);
            _parent.ViewModel.IsPaused = true;
        }

        private void InitializeSwitchInstance()
        {
            VirtualFileSystem.ReloadKeySet();

            IRenderer renderer = new Renderer();
            IHardwareDeviceDriver deviceDriver = new DummyHardwareDeviceDriver();

            BackendThreading threadingMode = ConfigurationState.Instance.Graphics.BackendThreading;

            var isGALthreaded = threadingMode == BackendThreading.On || (threadingMode == BackendThreading.Auto && renderer.PreferThreading);

            if (isGALthreaded)
            {
                renderer = new ThreadedRenderer(renderer);
            }

            Logger.Info?.PrintMsg(LogClass.Gpu, $"Backend Threading ({threadingMode}): {isGALthreaded}");

            if (ConfigurationState.Instance.System.AudioBackend.Value == AudioBackend.SDL2)
            {
                if (SDL2HardwareDeviceDriver.IsSupported)
                {
                    deviceDriver = new SDL2HardwareDeviceDriver();
                }
                else
                {
                    Logger.Warning?.Print(LogClass.Audio, "SDL2 is not supported, trying to fall back to OpenAL.");

                    if (OpenALHardwareDeviceDriver.IsSupported)
                    {
                        Logger.Warning?.Print(LogClass.Audio, "Found OpenAL, changing configuration.");

                        ConfigurationState.Instance.System.AudioBackend.Value = AudioBackend.OpenAl;
                        MainWindow.SaveConfig();

                        deviceDriver = new OpenALHardwareDeviceDriver();
                    }
                    else
                    {
                        Logger.Warning?.Print(LogClass.Audio, "OpenAL is not supported, trying to fall back to SoundIO.");

                        if (SoundIoHardwareDeviceDriver.IsSupported)
                        {
                            Logger.Warning?.Print(LogClass.Audio, "Found SoundIO, changing configuration.");

                            ConfigurationState.Instance.System.AudioBackend.Value = AudioBackend.SoundIo;
                            MainWindow.SaveConfig();

                            deviceDriver = new SoundIoHardwareDeviceDriver();
                        }
                        else
                        {
                            Logger.Warning?.Print(LogClass.Audio, "SoundIO is not supported, falling back to dummy audio out.");
                        }
                    }
                }
            }
            else if (ConfigurationState.Instance.System.AudioBackend.Value == AudioBackend.SoundIo)
            {
                if (SoundIoHardwareDeviceDriver.IsSupported)
                {
                    deviceDriver = new SoundIoHardwareDeviceDriver();
                }
                else
                {
                    Logger.Warning?.Print(LogClass.Audio, "SoundIO is not supported, trying to fall back to SDL2.");

                    if (SDL2HardwareDeviceDriver.IsSupported)
                    {
                        Logger.Warning?.Print(LogClass.Audio, "Found SDL2, changing configuration.");

                        ConfigurationState.Instance.System.AudioBackend.Value = AudioBackend.SDL2;
                        MainWindow.SaveConfig();

                        deviceDriver = new SDL2HardwareDeviceDriver();
                    }
                    else
                    {
                        Logger.Warning?.Print(LogClass.Audio, "SDL2 is not supported, trying to fall back to OpenAL.");

                        if (OpenALHardwareDeviceDriver.IsSupported)
                        {
                            Logger.Warning?.Print(LogClass.Audio, "Found OpenAL, changing configuration.");

                            ConfigurationState.Instance.System.AudioBackend.Value = AudioBackend.OpenAl;
                            MainWindow.SaveConfig();

                            deviceDriver = new OpenALHardwareDeviceDriver();
                        }
                        else
                        {
                            Logger.Warning?.Print(LogClass.Audio, "OpenAL is not supported, falling back to dummy audio out.");
                        }
                    }
                }
            }
            else if (ConfigurationState.Instance.System.AudioBackend.Value == AudioBackend.OpenAl)
            {
                if (OpenALHardwareDeviceDriver.IsSupported)
                {
                    deviceDriver = new OpenALHardwareDeviceDriver();
                }
                else
                {
                    Logger.Warning?.Print(LogClass.Audio, "OpenAL is not supported, trying to fall back to SDL2.");

                    if (SDL2HardwareDeviceDriver.IsSupported)
                    {
                        Logger.Warning?.Print(LogClass.Audio, "Found SDL2, changing configuration.");

                        ConfigurationState.Instance.System.AudioBackend.Value = AudioBackend.SDL2;
                        MainWindow.SaveConfig();

                        deviceDriver = new SDL2HardwareDeviceDriver();
                    }
                    else
                    {
                        Logger.Warning?.Print(LogClass.Audio, "SDL2 is not supported, trying to fall back to SoundIO.");

                        if (SoundIoHardwareDeviceDriver.IsSupported)
                        {
                            Logger.Warning?.Print(LogClass.Audio, "Found SoundIO, changing configuration.");

                            ConfigurationState.Instance.System.AudioBackend.Value = AudioBackend.SoundIo;
                            MainWindow.SaveConfig();

                            deviceDriver = new SoundIoHardwareDeviceDriver();
                        }
                        else
                        {
                            Logger.Warning?.Print(LogClass.Audio, "SoundIO is not supported, falling back to dummy audio out.");
                        }
                    }
                }
            }

            var memoryConfiguration = ConfigurationState.Instance.System.ExpandRam.Value ? HLE.MemoryConfiguration.MemoryConfiguration6GB : HLE.MemoryConfiguration.MemoryConfiguration4GB;

            IntegrityCheckLevel fsIntegrityCheckLevel = ConfigurationState.Instance.System.EnableFsIntegrityChecks ? IntegrityCheckLevel.ErrorOnInvalid : IntegrityCheckLevel.None;

            HLE.HLEConfiguration configuration = new HLE.HLEConfiguration(VirtualFileSystem,
                                                                          _parent.LibHacHorizonManager,
                                                                          ContentManager,
                                                                          _accountManager,
                                                                          _userChannelPersistence,
                                                                          renderer,
                                                                          deviceDriver,
                                                                          memoryConfiguration,
                                                                          _parent.UiHandler,
                                                                          (SystemLanguage)ConfigurationState.Instance.System.Language.Value,
                                                                          (RegionCode)ConfigurationState.Instance.System.Region.Value,
                                                                          ConfigurationState.Instance.Graphics.EnableVsync,
                                                                          ConfigurationState.Instance.System.EnableDockedMode,
                                                                          ConfigurationState.Instance.System.EnablePtc,
                                                                          ConfigurationState.Instance.System.EnableInternetAccess,
                                                                          fsIntegrityCheckLevel,
                                                                          ConfigurationState.Instance.System.FsGlobalAccessLogMode,
                                                                          ConfigurationState.Instance.System.SystemTimeOffset,
                                                                          ConfigurationState.Instance.System.TimeZone,
                                                                          ConfigurationState.Instance.System.MemoryManagerMode,
                                                                          ConfigurationState.Instance.System.IgnoreMissingServices,
                                                                          ConfigurationState.Instance.Graphics.AspectRatio,
                                                                          ConfigurationState.Instance.System.AudioVolume);

            Device = new Switch(configuration);
        }

        private void Window_SizeChanged(object sender, Size e)
        {
            Width = (int)e.Width;
            Height = (int)e.Height;

            SetRendererWindowSize(e);
        }

        private void MainLoop()
        {
            while (_isActive)
            {
                UpdateFrame();

                // Polling becomes expensive if it's not slept
                Thread.Sleep(1);
            }
        }

        private unsafe void RenderLoop()
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_parent.ViewModel.StartGamesInFullscreen)
                {
                    _parent.WindowState = WindowState.FullScreen;
                }

                if (_parent.WindowState == WindowState.FullScreen)
                {
                    _parent.ViewModel.ShowMenuAndStatusBar = false;
                }
            });

            IRenderer renderer = Device.Gpu.Renderer;

            if (renderer is ThreadedRenderer tr)
            {
                renderer = tr.BaseRenderer;
            }

            _renderer = renderer;

            _renderer.ScreenCaptured += Renderer_ScreenCaptured;

            (_renderer as Renderer).InitializeBackgroundContext(SPBOpenGLContext.CreateBackgroundContext(Renderer.GameContext));

            Renderer.MakeCurrent();

            Device.Gpu.Renderer.Initialize(_glLogLevel);

            Width = (int)Renderer.Bounds.Width;
            Height = (int)Renderer.Bounds.Height;

            _renderer.Window.SetSize((int)(Width * _parent.PlatformImpl.RenderScaling), (int)(Height * _parent.PlatformImpl.RenderScaling));

            Device.Gpu.Renderer.RunLoop(() =>
            {
                Device.Gpu.SetGpuThread();
                Device.Gpu.InitializeShaderCache(_gpuCancellationTokenSource.Token);
                Translator.IsReadyForTranslation.Set();

                Renderer.Start();

                Renderer.QueueRender();

                while (_isActive)
                {
                    if (Device.WaitFifo())
                    {
                        Device.Statistics.RecordFifoStart();
                        Device.ProcessFrame();
                        Device.Statistics.RecordFifoEnd();
                    }

                    while (Device.ConsumeFrameAvailable())
                    {
                        if (!_renderingStarted)
                        {
                            _renderingStarted = true;
                            _parent.SwitchToGameControl();
                        }

                        Device.PresentFrame(Present);
                    }
                }

                Renderer.Stop();
            });

            Renderer?.MakeCurrent(null);

            Renderer.SizeChanged -= Window_SizeChanged;
        }

        private void Present(object image)
        {
            // Run a status update only when a frame is to be drawn. This prevents from updating the ui and wasting a render when no frame is queued
            string dockedMode = ConfigurationState.Instance.System.EnableDockedMode ? LocaleManager.Instance["Docked"] : LocaleManager.Instance["Handheld"];
            float scale = GraphicsConfig.ResScale;

            if (scale != 1)
            {
                dockedMode += $" ({scale}x)";
            }

            string vendor = _renderer is Renderer renderer ? renderer.GpuVendor : "";

            StatusUpdatedEvent?.Invoke(this, new StatusUpdatedEventArgs(
                Device.EnableDeviceVsync,
                Device.GetVolume(),
                dockedMode,
                ConfigurationState.Instance.Graphics.AspectRatio.Value.ToText(),
                LocaleManager.Instance["Game"] + $": {Device.Statistics.GetGameFrameRate():00.00} FPS ({Device.Statistics.GetGameFrameTime():00.00} ms)",
                $"FIFO: {Device.Statistics.GetFifoPercent():00.00} %",
                $"GPU: {vendor}"));

            Renderer.Present(image);
        }

        public async Task ShowExitPrompt()
        {
            bool shouldExit = !ConfigurationState.Instance.ShowConfirmExit;

            if (!shouldExit)
            {
                if (_dialogShown)
                {
                    return;
                }

                _dialogShown = true;
                shouldExit = await ContentDialogHelper.CreateStopEmulationDialog();

                _dialogShown = false;
            }

            if (shouldExit)
            {
                Stop();
            }
        }

        private void HandleScreenState()
        {
            if (ConfigurationState.Instance.Hid.EnableMouse)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _parent.Cursor = _isMouseInRenderer ? InvisibleCursor : Cursor.Default;
                });
            }
            else
            {
                if (_hideCursorOnIdle)
                {
                    long cursorMoveDelta = Stopwatch.GetTimestamp() - _lastCursorMoveTime;

                    Dispatcher.UIThread.Post(() =>
                    {
                        _parent.Cursor = cursorMoveDelta >= CursorHideIdleTime * Stopwatch.Frequency ? InvisibleCursor : Cursor.Default;
                    });
                }
            }
        }

        private bool UpdateFrame()
        {
            if (!_isActive)
            {
                return false;
            }

            if (_parent.IsActive)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    HandleScreenState();

                    if (_keyboardInterface.GetKeyboardStateSnapshot().IsPressed(Key.Delete) && _parent.WindowState != WindowState.FullScreen)
                    {
                        Ptc.Continue();
                    }
                });
            }

            NpadManager.Update(ConfigurationState.Instance.Graphics.AspectRatio.Value.ToFloat());

            if (_parent.IsActive)
            {
                KeyboardHotkeyState currentHotkeyState = GetHotkeyState();

                if (currentHotkeyState != _prevHotkeyState)
                {
                    switch (currentHotkeyState)
                    {
                        case KeyboardHotkeyState.ToggleVSync:
                            Device.EnableDeviceVsync = !Device.EnableDeviceVsync;
                            break;
                        case KeyboardHotkeyState.Screenshot:
                            ScreenshotRequested = true;
                            break;
                        case KeyboardHotkeyState.ShowUi:
                            _parent.ViewModel.ShowMenuAndStatusBar = true;
                            break;
                        case KeyboardHotkeyState.Pause:
                            if (_parent.ViewModel.IsPaused)
                            {
                                Resume();
                            }
                            else
                            {
                                Pause();
                            }
                            break;
                        case KeyboardHotkeyState.ToggleMute:
                            if (Device.IsAudioMuted())
                            {
                                Device.SetVolume(ConfigurationState.Instance.System.AudioVolume);
                            }
                            else
                            {
                                Device.SetVolume(0);
                            }

                            _parent.ViewModel.Volume = Device.GetVolume();
                            break;
                        case KeyboardHotkeyState.None:
                            (_keyboardInterface as AvaloniaKeyboard).Clear();
                            break;
                    }
                }

                _prevHotkeyState = currentHotkeyState;

                if (ScreenshotRequested)
                {
                    ScreenshotRequested = false;
                    _renderer.Screenshot();
                }
            }

            // Touchscreen
            bool hasTouch = false;

            if (_parent.IsActive && !ConfigurationState.Instance.Hid.EnableMouse)
            {
                hasTouch = TouchScreenManager.Update(true, (_inputManager.MouseDriver as AvaloniaMouseDriver).IsButtonPressed(MouseButton.Button1), ConfigurationState.Instance.Graphics.AspectRatio.Value.ToFloat());
            }

            if (!hasTouch)
            {
                Device.Hid.Touchscreen.Update();
            }

            Device.Hid.DebugPad.Update();

            return true;
        }

        private KeyboardHotkeyState GetHotkeyState()
        {
            KeyboardHotkeyState state = KeyboardHotkeyState.None;

            if (_keyboardInterface.IsPressed((Key)ConfigurationState.Instance.Hid.Hotkeys.Value.ToggleVsync))
            {
                state = KeyboardHotkeyState.ToggleVSync;
            }
            else if (_keyboardInterface.IsPressed((Key)ConfigurationState.Instance.Hid.Hotkeys.Value.Screenshot))
            {
                state = KeyboardHotkeyState.Screenshot;
            }
            else if (_keyboardInterface.IsPressed((Key)ConfigurationState.Instance.Hid.Hotkeys.Value.ShowUi))
            {
                state = KeyboardHotkeyState.ShowUi;
            }
            else if (_keyboardInterface.IsPressed((Key)ConfigurationState.Instance.Hid.Hotkeys.Value.Pause))
            {
                state = KeyboardHotkeyState.Pause;
            }
            else if (_keyboardInterface.IsPressed((Key)ConfigurationState.Instance.Hid.Hotkeys.Value.ToggleMute))
            {
                state = KeyboardHotkeyState.ToggleMute;
            }

            return state;
        }
    }
}