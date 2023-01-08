using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Input;
using Ryujinx.Ava.UI.Applet;
using Ryujinx.Ava.UI.Helpers;
using Ryujinx.Ava.UI.ViewModels;
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
using Ryujinx.Ui.Common.Helper;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using InputManager = Ryujinx.Input.HLE.InputManager;

namespace Ryujinx.Ava.UI.Windows
{
    public partial class MainWindow : StyleableWindow
    {
        internal static MainWindowViewModel MainWindowViewModel { get; private set; }

        private bool _isLoading;

        private UserChannelPersistence _userChannelPersistence;
        private static bool _deferLoad;
        private static string _launchPath;
        private static bool _startFullscreen;
        internal readonly AvaHostUiHandler UiHandler;

        public VirtualFileSystem VirtualFileSystem { get; private set; }
        public ContentManager ContentManager { get; private set; }
        public AccountManager AccountManager { get; private set; }

        public LibHacHorizonManager LibHacHorizonManager { get; private set; }

        public InputManager InputManager { get; private set; }

        internal MainWindowViewModel ViewModel { get; private set; }
        public SettingsWindow SettingsWindow { get; set; }

        public static bool ShowKeyErrorOnLoad { get; set; }
        public ApplicationLibrary ApplicationLibrary { get; set; }

        public MainWindow()
        {
            ViewModel = new MainWindowViewModel();

            MainWindowViewModel = ViewModel;

            DataContext = ViewModel;

            InitializeComponent();
            Load();

            UiHandler = new AvaHostUiHandler(this);

            ViewModel.Title = $"Ryujinx {Program.Version}";

            // NOTE: Height of MenuBar and StatusBar is not usable here, since it would still be 0 at this point.
            double barHeight = MenuBar.MinHeight + StatusBarView.StatusBar.MinHeight;
            Height = ((Height - barHeight) / Program.WindowScaleFactor) + barHeight;
            Width /= Program.WindowScaleFactor;

            if (Program.PreviewerDetached)
            {
                Initialize();

                InputManager = new InputManager(new AvaloniaKeyboardDriver(this), new SDL2GamepadDriver());

                ViewModel.Initialize(
                    ContentManager,
                    ApplicationLibrary,
                    VirtualFileSystem,
                    AccountManager,
                    InputManager,
                    _userChannelPersistence,
                    LibHacHorizonManager,
                    UiHandler,
                    ShowLoading,
                    SwitchToGameControl,
                    SetMainContent,
                    this);

                ViewModel.RefreshFirmwareStatus();

                LoadGameList();

                this.GetObservable(IsActiveProperty).Subscribe(IsActiveChanged);
            }

            ApplicationLibrary.ApplicationCountUpdated += ApplicationLibrary_ApplicationCountUpdated;
            ApplicationLibrary.ApplicationAdded += ApplicationLibrary_ApplicationAdded;
            ViewModel.ReloadGameList += ReloadGameList;
        }

        private void IsActiveChanged(bool obj)
        {
            ViewModel.IsActive = obj;
        }

        public void LoadGameList()
        {
            if (_isLoading)
            {
                return;
            }

            _isLoading = true;

            LoadApplications();

            _isLoading = false;
        }

        protected override void HandleScalingChanged(double scale)
        {
            Program.DesktopScaleFactor = scale;
            base.HandleScalingChanged(scale);
        }

        public void AddApplication(ApplicationData applicationData)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ViewModel.Applications.Add(applicationData);
            });
        }

        private void ApplicationLibrary_ApplicationAdded(object sender, ApplicationAddedEventArgs e)
        {
            AddApplication(e.AppData);
        }

        private void ApplicationLibrary_ApplicationCountUpdated(object sender, ApplicationCountUpdatedEventArgs e)
        {
            LocaleManager.Instance.UpdateDynamicValue(LocaleKeys.StatusBarGamesLoaded, e.NumAppsLoaded, e.NumAppsFound);

            Dispatcher.UIThread.Post(() =>
            {
                ViewModel.StatusBarProgressValue   = e.NumAppsLoaded;
                ViewModel.StatusBarProgressMaximum = e.NumAppsFound;

                if (e.NumAppsFound == 0)
                {
                    StatusBarView.LoadProgressBar.IsVisible = false;
                }

                if (e.NumAppsLoaded == e.NumAppsFound)
                {
                    StatusBarView.LoadProgressBar.IsVisible = false;
                }
            });
        }

        public void Application_Opened(object sender, ApplicationOpenedEventArgs args)
        {
            if (args.Application != null)
            {
                ViewModel.SelectedIcon = args.Application.Icon;

                string path = new FileInfo(args.Application.Path).FullName;

                ViewModel.LoadApplication(path);
            }

            args.Handled = true;
        }

        internal static void DeferLoadApplication(string launchPathArg, bool startFullscreenArg)
        {
            _deferLoad = true;
            _launchPath = launchPathArg;
            _startFullscreen = startFullscreenArg;
        }

        public void SwitchToGameControl(bool startFullscreen = false)
        {
            ViewModel.ShowLoadProgress = false;
            ViewModel.ShowContent = true;
            ViewModel.IsLoadingIndeterminate = false;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (startFullscreen && ViewModel.WindowState != WindowState.FullScreen)
                {
                    ViewModel.ToggleFullscreen();
                }
            });
        }

        public void ShowLoading(bool startFullscreen = false)
        {
            ViewModel.ShowContent = false;
            ViewModel.ShowLoadProgress = true;
            ViewModel.IsLoadingIndeterminate = true;

            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (startFullscreen && ViewModel.WindowState != WindowState.FullScreen)
                {
                    ViewModel.ToggleFullscreen();
                }
            });
        }

        protected override void HandleWindowStateChanged(WindowState state)
        {
            ViewModel.WindowState = state;

            if (state != WindowState.Minimized)
            {
                Renderer.Start();
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

            AccountManager = new AccountManager(LibHacHorizonManager.RyujinxClient, CommandLineState.Profile);

            VirtualFileSystem.ReloadKeySet();

            ApplicationHelper.Initialize(VirtualFileSystem, AccountManager, LibHacHorizonManager.RyujinxClient, this);
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

                ViewModel.LoadApplication(_launchPath, _startFullscreen);
            }

            if (ConfigurationState.Instance.CheckUpdatesOnStart.Value && Updater.CanUpdate(false, this))
            {
                Updater.BeginParse(this, false).ContinueWith(task =>
                {
                    Logger.Error?.Print(LogClass.Application, $"Updater Error: {task.Exception}");
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        private void Load()
        {
            StatusBarView.VolumeStatus.Click += VolumeStatus_CheckedChanged;

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

        private void SetMainContent(Control content = null)
        {
            if (content == null)
            {
                content = GameLibrary;
            }

            if (MainContent.Content != content)
            {
                MainContent.Content = content;
            }
        }

        public static void UpdateGraphicsConfig()
        {
            GraphicsConfig.ResScale                   = ConfigurationState.Instance.Graphics.ResScale == -1 ? ConfigurationState.Instance.Graphics.ResScaleCustom : ConfigurationState.Instance.Graphics.ResScale;
            GraphicsConfig.MaxAnisotropy              = ConfigurationState.Instance.Graphics.MaxAnisotropy;
            GraphicsConfig.ShadersDumpPath            = ConfigurationState.Instance.Graphics.ShadersDumpPath;
            GraphicsConfig.EnableShaderCache          = ConfigurationState.Instance.Graphics.EnableShaderCache;
            GraphicsConfig.EnableTextureRecompression = ConfigurationState.Instance.Graphics.EnableTextureRecompression;
            GraphicsConfig.EnableMacroHLE             = ConfigurationState.Instance.Graphics.EnableMacroHLE;
        }

        public void LoadHotKeys()
        {
            HotKeyManager.SetHotKey(FullscreenHotKey,  new KeyGesture(Key.Enter, KeyModifiers.Alt));
            HotKeyManager.SetHotKey(FullscreenHotKey2, new KeyGesture(Key.F11));
            HotKeyManager.SetHotKey(DockToggleHotKey,  new KeyGesture(Key.F9));
            HotKeyManager.SetHotKey(ExitHotKey,        new KeyGesture(Key.Escape));
        }

        private void VolumeStatus_CheckedChanged(object sender, SplitButtonClickEventArgs e)
        {
            var volumeSplitButton = sender as ToggleSplitButton;
            if (ViewModel.IsGameRunning)
            {
                if (!volumeSplitButton.IsChecked)
                {
                    ViewModel.AppHost.Device.SetVolume(ConfigurationState.Instance.System.AudioVolume);
                }
                else
                {
                    ViewModel.AppHost.Device.SetVolume(0);
                }

                ViewModel.Volume = ViewModel.AppHost.Device.GetVolume();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!ViewModel.IsClosing && ViewModel.AppHost != null && ConfigurationState.Instance.ShowConfirmExit)
            {
                e.Cancel = true;

                ConfirmExit();

                return;
            }

            ViewModel.IsClosing = true;

            if (ViewModel.AppHost != null)
            {
                ViewModel.AppHost.AppExit -= ViewModel.AppHost_AppExit;
                ViewModel.AppHost.AppExit += (sender, e) =>
                {
                    ViewModel.AppHost = null;

                    Dispatcher.UIThread.Post(() =>
                    {
                        MainContent = null;

                        Close();
                    });
                };
                ViewModel.AppHost?.Stop();

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
               ViewModel.IsClosing = await ContentDialogHelper.CreateExitDialog();

               if (ViewModel.IsClosing)
               {
                   Close();
               }
           });
        }

        public async void LoadApplications()
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ViewModel.Applications.Clear();

                StatusBarView.LoadProgressBar.IsVisible = true;
                ViewModel.StatusBarProgressMaximum      = 0;
                ViewModel.StatusBarProgressValue        = 0;

                LocaleManager.Instance.UpdateDynamicValue(LocaleKeys.StatusBarGamesLoaded, 0, 0);
            });

            ReloadGameList();
        }

        private void ReloadGameList()
        {
            if (_isLoading)
            {
                return;
            }

            _isLoading = true;

            ApplicationLibrary.LoadApplications(ConfigurationState.Instance.Ui.GameDirs.Value, ConfigurationState.Instance.System.Language);

            _isLoading = false;
        }
    }
}