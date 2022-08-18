using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Input;
using Ryujinx.Ava.Ui.Applet;
using Ryujinx.Ava.Ui.Controls;
using Ryujinx.Ava.Ui.Models;
using Ryujinx.Ava.Ui.ViewModels;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Gpu;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.Input.SDL2;
using Ryujinx.Modules;
using Ryujinx.Ui.App.Common;
using Ryujinx.Ui.Common;
using Ryujinx.Ui.Common.Configuration;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using InputManager = Ryujinx.Input.HLE.InputManager;

namespace Ryujinx.Ava.Ui.Windows
{
    public partial class MainWindow : StyleableWindow
    {
        private bool _canUpdate;
        private bool _isClosing;
        private bool _isLoading;

        private Control _mainViewContent;

        private UserChannelPersistence _userChannelPersistence;
        private static bool _deferLoad;
        private static string _launchPath;
        private static bool _startFullscreen;
        private string _currentEmulatedGamePath;
        internal readonly AvaHostUiHandler UiHandler;
        private AutoResetEvent _rendererWaitEvent;

        public VirtualFileSystem VirtualFileSystem { get; private set; }
        public ContentManager ContentManager { get; private set; }
        public AccountManager AccountManager { get; private set; }

        public LibHacHorizonManager LibHacHorizonManager { get; private set; }

        internal AppHost AppHost { get; private set; }
        public InputManager InputManager { get; private set; }

        internal RendererControl RendererControl { get; private set; }
        internal MainWindowViewModel ViewModel { get; private set; }
        public SettingsWindow SettingsWindow { get; set; }

        public bool CanUpdate
        {
            get => _canUpdate;
            set
            {
                _canUpdate = value;

                Dispatcher.UIThread.InvokeAsync(() => UpdateMenuItem.IsEnabled = _canUpdate);
            }
        }

        public static bool ShowKeyErrorOnLoad { get; set; }
        public ApplicationLibrary ApplicationLibrary { get; set; }

        public MainWindow()
        {
            ViewModel = new MainWindowViewModel(this);

            DataContext = ViewModel;

            InitializeComponent();
            Load();

            UiHandler = new AvaHostUiHandler(this);

            Title = $"Ryujinx {Program.Version}";

            Height = Height / Program.WindowScaleFactor;
            Width = Width / Program.WindowScaleFactor;

            if (Program.PreviewerDetached)
            {
                Initialize();

                ViewModel.Initialize();

                InputManager = new InputManager(new AvaloniaKeyboardDriver(this), new SDL2GamepadDriver());

                LoadGameList();
            }

            _rendererWaitEvent = new AutoResetEvent(false);
        }

        public void LoadGameList()
        {
            if (_isLoading)
            {
                return;
            }

            _isLoading = true;

            ViewModel.LoadApplications();

            _isLoading = false;
        }

        private void Update_StatusBar(object sender, StatusUpdatedEventArgs args)
        {
            if (ViewModel.ShowMenuAndStatusBar && !ViewModel.ShowLoadProgress)
            {
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (args.VSyncEnabled)
                    {
                        ViewModel.VsyncColor = new SolidColorBrush(Color.Parse("#ff2eeac9"));
                    }
                    else
                    {
                        ViewModel.VsyncColor = new SolidColorBrush(Color.Parse("#ffff4554"));
                    }

                    ViewModel.DockedStatusText = args.DockedMode;
                    ViewModel.AspectRatioStatusText = args.AspectRatio;
                    ViewModel.GameStatusText = args.GameStatus;
                    ViewModel.FifoStatusText = args.FifoStatus;
                    ViewModel.GpuNameText = args.GpuName;
                    ViewModel.BackendText = args.GpuBackend;

                    ViewModel.ShowStatusSeparator = true;
                });
            }
        }

        public void Application_Opened(object sender, ApplicationOpenedEventArgs args)
        {
            if (args.Application != null)
            {
                ViewModel.SelectedIcon = args.Application.Icon;

                string path = new FileInfo(args.Application.Path).FullName;

                LoadApplication(path);
            }

            args.Handled = true;
        }

        public async Task PerformanceCheck()
        {
            if (ConfigurationState.Instance.Logger.EnableTrace.Value)
            {
                string mainMessage = LocaleManager.Instance["DialogPerformanceCheckLoggingEnabledMessage"];
                string secondaryMessage = LocaleManager.Instance["DialogPerformanceCheckLoggingEnabledConfirmMessage"];

                UserResult result = await ContentDialogHelper.CreateConfirmationDialog(mainMessage, secondaryMessage,
                    LocaleManager.Instance["InputDialogYes"], LocaleManager.Instance["InputDialogNo"],
                    LocaleManager.Instance["RyujinxConfirm"]);

                if (result != UserResult.Yes)
                {
                    ConfigurationState.Instance.Logger.EnableTrace.Value = false;

                    SaveConfig();
                }
            }

            if (!string.IsNullOrWhiteSpace(ConfigurationState.Instance.Graphics.ShadersDumpPath.Value))
            {
                string mainMessage = LocaleManager.Instance["DialogPerformanceCheckShaderDumpEnabledMessage"];
                string secondaryMessage =
                    LocaleManager.Instance["DialogPerformanceCheckShaderDumpEnabledConfirmMessage"];

                UserResult result = await ContentDialogHelper.CreateConfirmationDialog(mainMessage, secondaryMessage,
                    LocaleManager.Instance["InputDialogYes"], LocaleManager.Instance["InputDialogNo"],
                    LocaleManager.Instance["RyujinxConfirm"]);

                if (result != UserResult.Yes)
                {
                    ConfigurationState.Instance.Graphics.ShadersDumpPath.Value = "";

                    SaveConfig();
                }
            }
        }

        internal static void DeferLoadApplication(string launchPathArg, bool startFullscreenArg)
        {
            _deferLoad = true;
            _launchPath = launchPathArg;
            _startFullscreen = startFullscreenArg;
        }

#pragma warning disable CS1998
        public async void LoadApplication(string path, bool startFullscreen = false, string titleName = "")
#pragma warning restore CS1998
        {
            if (AppHost != null)
            {
                await ContentDialogHelper.CreateInfoDialog(
                    LocaleManager.Instance["DialogLoadAppGameAlreadyLoadedMessage"],
                    LocaleManager.Instance["DialogLoadAppGameAlreadyLoadedSubMessage"],
                    LocaleManager.Instance["InputDialogOk"],
                    "",
                    LocaleManager.Instance["RyujinxInfo"]);

                return;
            }

#if RELEASE
            await PerformanceCheck();
#endif

            Logger.RestartTime();

            if (ViewModel.SelectedIcon == null)
            {
                ViewModel.SelectedIcon = ApplicationLibrary.GetApplicationIcon(path);
            }

            PrepareLoadScreen();

            _mainViewContent = MainContent.Content as Control;

            RendererControl = Program.UseVulkan ? new VulkanRendererControl(ConfigurationState.Instance.Logger.GraphicsDebugLevel) : new OpenGLRendererControl(3, 3, ConfigurationState.Instance.Logger.GraphicsDebugLevel);
            AppHost = new AppHost(RendererControl, InputManager, path, VirtualFileSystem, ContentManager, AccountManager, _userChannelPersistence, this);

            if (!AppHost.LoadGuestApplication().Result)
            {
                AppHost.DisposeContext();

                return;
            }

            ViewModel.LoadHeading = string.IsNullOrWhiteSpace(titleName) ? string.Format(LocaleManager.Instance["LoadingHeading"], AppHost.Device.Application.TitleName) : titleName;
            ViewModel.TitleName = string.IsNullOrWhiteSpace(titleName) ? AppHost.Device.Application.TitleName : titleName;

            SwitchToGameControl(startFullscreen);

            _currentEmulatedGamePath = path;
            Thread gameThread = new Thread(InitializeGame)
            {
                Name = "GUI.WindowThread"
            };
            gameThread.Start();
        }

        private void InitializeGame()
        {
            RendererControl.RendererInitialized += GlRenderer_Created;

            AppHost.StatusUpdatedEvent += Update_StatusBar;
            AppHost.AppExit += AppHost_AppExit;

            _rendererWaitEvent.WaitOne();

            AppHost?.Start();

            AppHost.DisposeContext();
        }


        private void HandleRelaunch()
        {
            if (_userChannelPersistence.PreviousIndex != -1 && _userChannelPersistence.ShouldRestart)
            {
                _userChannelPersistence.ShouldRestart = false;

                Dispatcher.UIThread.Post(() =>
                {
                    LoadApplication(_currentEmulatedGamePath);
                });
            }
            else
            {
                // otherwise, clear state.
                _userChannelPersistence = new UserChannelPersistence();
                _currentEmulatedGamePath = null;
            }
        }

        public void SwitchToGameControl(bool startFullscreen = false)
        {
            ViewModel.ShowContent = true;
            ViewModel.ShowLoadProgress = false;
            ViewModel.IsLoadingIndeterminate = false;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                MainContent.Content = RendererControl;

                if (startFullscreen && WindowState != WindowState.FullScreen)
                {
                    ViewModel.ToggleFullscreen();
                }

                RendererControl.Focus();
            });
        }

        public void ShowLoading(bool startFullscreen = false)
        {
            ViewModel.ShowContent = false;
            ViewModel.ShowLoadProgress = true;
            ViewModel.IsLoadingIndeterminate = true;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (startFullscreen && WindowState != WindowState.FullScreen)
                {
                    ViewModel.ToggleFullscreen();
                }
            });
        }

        private void GlRenderer_Created(object sender, EventArgs e)
        {
            ShowLoading();

            _rendererWaitEvent.Set();
        }

        private void AppHost_AppExit(object sender, EventArgs e)
        {
            if (_isClosing)
            {
                return;
            }

            ViewModel.IsGameRunning = false;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (MainContent.Content != _mainViewContent)
                {
                    MainContent.Content = _mainViewContent;
                }

                ViewModel.ShowMenuAndStatusBar = true;
                ViewModel.ShowContent = true;
                ViewModel.ShowLoadProgress = false;
                ViewModel.IsLoadingIndeterminate = false;
                Cursor = Cursor.Default;

                AppHost = null;

                HandleRelaunch();
            });

            RendererControl.RendererInitialized -= GlRenderer_Created;
            RendererControl = null;

            ViewModel.SelectedIcon = null;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Title = $"Ryujinx {Program.Version}";
            });
        }

        public void Sort_Checked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioButton button)
            {
                var sort = Enum.Parse<ApplicationSort>(button.Tag.ToString());
                ViewModel.Sort(sort);
            }
        }

        protected override void HandleWindowStateChanged(WindowState state)
        {
            WindowState = state;

            if (state != WindowState.Minimized)
            {
                Renderer.Start();
            }
        }

        public void Order_Checked(object sender, RoutedEventArgs args)
        {
            if (sender is RadioButton button)
            {
                var tag = button.Tag.ToString();
                ViewModel.Sort(tag != "Descending");
            }
        }

        private void Initialize()
        {
            _userChannelPersistence = new UserChannelPersistence();
            VirtualFileSystem = VirtualFileSystem.CreateInstance();
            LibHacHorizonManager = new LibHacHorizonManager();
            ContentManager = new ContentManager(VirtualFileSystem);

            LibHacHorizonManager.InitializeFsServer(VirtualFileSystem);
            LibHacHorizonManager.InitializeArpServer();
            LibHacHorizonManager.InitializeBcatServer();
            LibHacHorizonManager.InitializeSystemClients();

            ApplicationLibrary = new ApplicationLibrary(VirtualFileSystem);

            // Save data created before we supported extra data in directory save data will not work properly if
            // given empty extra data. Luckily some of that extra data can be created using the data from the
            // save data indexer, which should be enough to check access permissions for user saves.
            // Every single save data's extra data will be checked and fixed if needed each time the emulator is opened.
            // Consider removing this at some point in the future when we don't need to worry about old saves.
            VirtualFileSystem.FixExtraData(LibHacHorizonManager.RyujinxClient);

            AccountManager = new AccountManager(LibHacHorizonManager.RyujinxClient, Program.CommandLineProfile);

            VirtualFileSystem.ReloadKeySet();

            ApplicationHelper.Initialize(VirtualFileSystem, AccountManager, LibHacHorizonManager.RyujinxClient, this);

            RefreshFirmwareStatus();
        }

        protected void CheckLaunchState()
        {
            if (ShowKeyErrorOnLoad)
            {
                ShowKeyErrorOnLoad = false;

                Dispatcher.UIThread.Post(async () => await
                    UserErrorDialog.ShowUserErrorDialog(UserError.NoKeys, this));
            }

            if (_deferLoad)
            {
                _deferLoad = false;

                LoadApplication(_launchPath, _startFullscreen);
            }

            if (ConfigurationState.Instance.CheckUpdatesOnStart.Value && Updater.CanUpdate(false, this))
            {
                Updater.BeginParse(this, false).ContinueWith(task =>
                {
                    Logger.Error?.Print(LogClass.Application, $"Updater Error: {task.Exception}");
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        public void RefreshFirmwareStatus()
        {
            SystemVersion version = null;
            try
            {
                version = ContentManager.GetCurrentFirmwareVersion();
            }
            catch (Exception) { }

            bool hasApplet = false;

            if (version != null)
            {
                LocaleManager.Instance.UpdateDynamicValue("StatusBarSystemVersion",
                    version.VersionString);

                hasApplet = version.Major > 3;
            }
            else
            {
                LocaleManager.Instance.UpdateDynamicValue("StatusBarSystemVersion", "0.0");
            }

            ViewModel.IsAppletMenuActive = hasApplet;
        }

        private void Load()
        {
            VolumeStatus.Click += VolumeStatus_CheckedChanged;

            GameGrid.ApplicationOpened += Application_Opened;

            GameGrid.DataContext = ViewModel;

            GameList.ApplicationOpened += Application_Opened;

            GameList.DataContext = ViewModel;

            LoadHotKeys();
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            CheckLaunchState();
        }

        public static void UpdateGraphicsConfig()
        {
            int resScale = ConfigurationState.Instance.Graphics.ResScale;
            float resScaleCustom = ConfigurationState.Instance.Graphics.ResScaleCustom;

            GraphicsConfig.ResScale = resScale == -1 ? resScaleCustom : resScale;
            GraphicsConfig.MaxAnisotropy = ConfigurationState.Instance.Graphics.MaxAnisotropy;
            GraphicsConfig.ShadersDumpPath = ConfigurationState.Instance.Graphics.ShadersDumpPath;
            GraphicsConfig.EnableShaderCache = ConfigurationState.Instance.Graphics.EnableShaderCache;
            GraphicsConfig.EnableTextureRecompression = ConfigurationState.Instance.Graphics.EnableTextureRecompression;
        }

        public void LoadHotKeys()
        {
            HotKeyManager.SetHotKey(FullscreenHotKey, new KeyGesture(Key.Enter, KeyModifiers.Alt));
            HotKeyManager.SetHotKey(FullscreenHotKey2, new KeyGesture(Key.F11));
            HotKeyManager.SetHotKey(DockToggleHotKey, new KeyGesture(Key.F9));
            HotKeyManager.SetHotKey(ExitHotKey, new KeyGesture(Key.Escape));
        }

        public static void SaveConfig()
        {
            ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);
        }

        public void UpdateGameMetadata(string titleId)
        {
            ApplicationLibrary.LoadAndSaveMetaData(titleId, appMetadata =>
            {
                DateTime lastPlayedDateTime = DateTime.Parse(appMetadata.LastPlayed);
                double sessionTimePlayed = DateTime.UtcNow.Subtract(lastPlayedDateTime).TotalSeconds;

                appMetadata.TimePlayed += Math.Round(sessionTimePlayed, MidpointRounding.AwayFromZero);
            });
        }

        private void PrepareLoadScreen()
        {
            using MemoryStream stream = new MemoryStream(ViewModel.SelectedIcon);
            using var gameIconBmp = SixLabors.ImageSharp.Image.Load<Bgra32>(stream);

            var dominantColor = IconColorPicker.GetFilteredColor(gameIconBmp).ToPixel<Bgra32>();

            const int ColorDivisor = 4;

            Color progressFgColor = Color.FromRgb(dominantColor.R, dominantColor.G, dominantColor.B);
            Color progressBgColor = Color.FromRgb(
                (byte)(dominantColor.R / ColorDivisor),
                (byte)(dominantColor.G / ColorDivisor),
                (byte)(dominantColor.B / ColorDivisor));

            ViewModel.ProgressBarForegroundColor = new SolidColorBrush(progressFgColor);
            ViewModel.ProgressBarBackgroundColor = new SolidColorBrush(progressBgColor);
        }

        private void SearchBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            ViewModel.SearchText = SearchBox.Text;
        }

        private async void StopEmulation_Click(object sender, RoutedEventArgs e)
        {
            if (AppHost != null)
            {
                await AppHost.ShowExitPrompt();
            }
        }

        private async void PauseEmulation_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                AppHost?.Pause();
            });
        }

        private async void ResumeEmulation_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() =>
            {
                AppHost?.Resume();
            });
        }

        private void ScanAmiiboMenuItem_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is MenuItem)
            {
                ViewModel.IsAmiiboRequested = AppHost.Device.System.SearchingForAmiibo(out _);
            }
        }

        private void VsyncStatus_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            AppHost.Device.EnableDeviceVsync = !AppHost.Device.EnableDeviceVsync;

            Logger.Info?.Print(LogClass.Application, $"VSync toggled to: {AppHost.Device.EnableDeviceVsync}");
        }

        private void DockedStatus_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            ConfigurationState.Instance.System.EnableDockedMode.Value = !ConfigurationState.Instance.System.EnableDockedMode.Value;
        }

        private void AspectRatioStatus_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            AspectRatio aspectRatio = ConfigurationState.Instance.Graphics.AspectRatio.Value;

            ConfigurationState.Instance.Graphics.AspectRatio.Value = (int)aspectRatio + 1 > Enum.GetNames(typeof(AspectRatio)).Length - 1 ? AspectRatio.Fixed4x3 : aspectRatio + 1;
        }

        private void VolumeStatus_CheckedChanged(object sender, SplitButtonClickEventArgs e)
        {
            var volumeSplitButton = sender as ToggleSplitButton;
            if (ViewModel.IsGameRunning)
            {
                if (!volumeSplitButton.IsChecked)
                {
                    AppHost.Device.SetVolume(ConfigurationState.Instance.System.AudioVolume);
                }
                else
                {
                    AppHost.Device.SetVolume(0);
                }

                ViewModel.Volume = AppHost.Device.GetVolume();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_isClosing && AppHost != null && ConfigurationState.Instance.ShowConfirmExit)
            {
                e.Cancel = true;

                ConfirmExit();

                return;
            }

            _isClosing = true;

            if (AppHost != null)
            {
                AppHost.AppExit -= AppHost_AppExit;
                AppHost.AppExit += (sender, e) =>
                {
                    AppHost = null;

                    Dispatcher.UIThread.Post(() =>
                    {
                        MainContent = null;

                        Close();
                    });
                };
                AppHost?.Stop();

                e.Cancel = true;

                return;
            }

            ApplicationLibrary.CancelLoading();
            InputManager.Dispose();
            Program.Exit();

            base.OnClosing(e);
        }

        private void ConfirmExit()
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
           {
               _isClosing = await ContentDialogHelper.CreateExitDialog();

               if (_isClosing)
               {
                   Close();
               }
           });
        }
    }
}