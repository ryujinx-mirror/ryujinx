using ARMeilleure.Translation;
using ARMeilleure.Translation.PTC;
using Gtk;
using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.FsSystem;
using LibHac.Ncm;
using LibHac.Ns;
using LibHac.Tools.FsSystem;
using Ryujinx.Audio.Backends.Dummy;
using Ryujinx.Audio.Backends.OpenAL;
using Ryujinx.Audio.Backends.SDL2;
using Ryujinx.Audio.Backends.SoundIo;
using Ryujinx.Audio.Integration;
using Ryujinx.Common;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Common.System;
using Ryujinx.Configuration;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.GAL.Multithreading;
using Ryujinx.Graphics.OpenGL;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.Input.GTK3;
using Ryujinx.Input.HLE;
using Ryujinx.Input.SDL2;
using Ryujinx.Modules;
using Ryujinx.Ui.App;
using Ryujinx.Ui.Applet;
using Ryujinx.Ui.Helper;
using Ryujinx.Ui.Widgets;
using Ryujinx.Ui.Windows;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using GUI = Gtk.Builder.ObjectAttribute;
using PtcLoadingState = ARMeilleure.Translation.PTC.PtcLoadingState;
using ShaderCacheLoadingState = Ryujinx.Graphics.Gpu.Shader.ShaderCacheState;

namespace Ryujinx.Ui
{
    public class MainWindow : Window
    {
        private readonly VirtualFileSystem    _virtualFileSystem;
        private readonly ContentManager       _contentManager;
        private readonly AccountManager       _accountManager;
        private readonly LibHacHorizonManager _libHacHorizonManager;

        private UserChannelPersistence _userChannelPersistence;

        private HLE.Switch _emulationContext;

        private WindowsMultimediaTimerResolution _windowsMultimediaTimerResolution;

        private readonly ApplicationLibrary _applicationLibrary;
        private readonly GtkHostUiHandler   _uiHandler;
        private readonly AutoResetEvent     _deviceExitStatus;
        private readonly ListStore          _tableStore;

        private bool _updatingGameTable;
        private bool _gameLoaded;
        private bool _ending;

        private string _currentEmulatedGamePath = null;

        private string _lastScannedAmiiboId = "";
        private bool   _lastScannedAmiiboShowAll = false;

        public RendererWidgetBase RendererWidget;
        public InputManager InputManager;

        public bool IsFocused;

        private static bool UseVulkan = false;

#pragma warning disable CS0169, CS0649, IDE0044

        [GUI] public MenuItem ExitMenuItem;
        [GUI] public MenuItem UpdateMenuItem;
        [GUI] MenuBar         _menuBar;
        [GUI] Box             _footerBox;
        [GUI] Box             _statusBar;
        [GUI] MenuItem        _optionMenu;
        [GUI] MenuItem        _manageUserProfiles;
        [GUI] MenuItem        _fileMenu;
        [GUI] MenuItem        _loadApplicationFile;
        [GUI] MenuItem        _loadApplicationFolder;
        [GUI] MenuItem        _appletMenu;
        [GUI] MenuItem        _actionMenu;
        [GUI] MenuItem        _pauseEmulation;
        [GUI] MenuItem        _resumeEmulation;
        [GUI] MenuItem        _stopEmulation;
        [GUI] MenuItem        _simulateWakeUpMessage;
        [GUI] MenuItem        _scanAmiibo;
        [GUI] MenuItem        _takeScreenshot;
        [GUI] MenuItem        _hideUi;
        [GUI] MenuItem        _fullScreen;
        [GUI] CheckMenuItem   _startFullScreen;
        [GUI] CheckMenuItem   _showConsole;
        [GUI] CheckMenuItem   _favToggle;
        [GUI] MenuItem        _firmwareInstallDirectory;
        [GUI] MenuItem        _firmwareInstallFile;
        [GUI] Label           _fifoStatus;
        [GUI] CheckMenuItem   _iconToggle;
        [GUI] CheckMenuItem   _developerToggle;
        [GUI] CheckMenuItem   _appToggle;
        [GUI] CheckMenuItem   _timePlayedToggle;
        [GUI] CheckMenuItem   _versionToggle;
        [GUI] CheckMenuItem   _lastPlayedToggle;
        [GUI] CheckMenuItem   _fileExtToggle;
        [GUI] CheckMenuItem   _pathToggle;
        [GUI] CheckMenuItem   _fileSizeToggle;
        [GUI] Label           _dockedMode;
        [GUI] Label           _aspectRatio;
        [GUI] Label           _gameStatus;
        [GUI] TreeView        _gameTable;
        [GUI] TreeSelection   _gameTableSelection;
        [GUI] ScrolledWindow  _gameTableWindow;
        [GUI] Label           _gpuName;
        [GUI] Label           _progressLabel;
        [GUI] Label           _firmwareVersionLabel;
        [GUI] Gtk.ProgressBar _progressBar;
        [GUI] Box             _viewBox;
        [GUI] Label           _vSyncStatus;
        [GUI] Label           _volumeStatus;
        [GUI] Box             _listStatusBox;
        [GUI] Label           _loadingStatusLabel;
        [GUI] Gtk.ProgressBar _loadingStatusBar;

#pragma warning restore CS0649, IDE0044, CS0169

        public MainWindow() : this(new Builder("Ryujinx.Ui.MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetObject("_mainWin").Handle)
        {
            builder.Autoconnect(this);

            // Apply custom theme if needed.
            ThemeHelper.ApplyTheme();

            // Sets overridden fields.
            int monitorWidth  = Display.PrimaryMonitor.Geometry.Width  * Display.PrimaryMonitor.ScaleFactor;
            int monitorHeight = Display.PrimaryMonitor.Geometry.Height * Display.PrimaryMonitor.ScaleFactor;

            DefaultWidth  = monitorWidth  < 1280 ? monitorWidth  : 1280;
            DefaultHeight = monitorHeight < 760  ? monitorHeight : 760;

            Icon  = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.Resources.Logo_Ryujinx.png");
            Title = $"Ryujinx {Program.Version}";

            // Hide emulation context status bar.
            _statusBar.Hide();

            // Instantiate HLE objects.
            _virtualFileSystem    = VirtualFileSystem.CreateInstance();
            _libHacHorizonManager = new LibHacHorizonManager();

            _libHacHorizonManager.InitializeFsServer(_virtualFileSystem);
            _libHacHorizonManager.InitializeArpServer();
            _libHacHorizonManager.InitializeBcatServer();
            _libHacHorizonManager.InitializeSystemClients();

            // Save data created before we supported extra data in directory save data will not work properly if
            // given empty extra data. Luckily some of that extra data can be created using the data from the
            // save data indexer, which should be enough to check access permissions for user saves.
            // Every single save data's extra data will be checked and fixed if needed each time the emulator is opened.
            // Consider removing this at some point in the future when we don't need to worry about old saves.
            VirtualFileSystem.FixExtraData(_libHacHorizonManager.RyujinxClient);

            _contentManager         = new ContentManager(_virtualFileSystem);
            _accountManager         = new AccountManager(_libHacHorizonManager.RyujinxClient, Program.CommandLineProfile);
            _userChannelPersistence = new UserChannelPersistence();

            // Instantiate GUI objects.
            _applicationLibrary = new ApplicationLibrary(_virtualFileSystem);
            _uiHandler          = new GtkHostUiHandler(this);
            _deviceExitStatus   = new AutoResetEvent(false);

            WindowStateEvent += WindowStateEvent_Changed;
            DeleteEvent      += Window_Close;
            FocusInEvent     += MainWindow_FocusInEvent;
            FocusOutEvent    += MainWindow_FocusOutEvent;

            _applicationLibrary.ApplicationAdded        += Application_Added;
            _applicationLibrary.ApplicationCountUpdated += ApplicationCount_Updated;

            _fileMenu.StateChanged   += FileMenu_StateChanged;
            _actionMenu.StateChanged += ActionMenu_StateChanged;
            _optionMenu.StateChanged += OptionMenu_StateChanged;

            _gameTable.ButtonReleaseEvent += Row_Clicked;
            _fullScreen.Activated         += FullScreen_Toggled;

            RendererWidgetBase.StatusUpdatedEvent += Update_StatusBar;

            ConfigurationState.Instance.System.IgnoreMissingServices.Event += UpdateIgnoreMissingServicesState;
            ConfigurationState.Instance.Graphics.AspectRatio.Event         += UpdateAspectRatioState;
            ConfigurationState.Instance.System.EnableDockedMode.Event      += UpdateDockedModeState;
            ConfigurationState.Instance.System.AudioVolume.Event           += UpdateAudioVolumeState;

            if (ConfigurationState.Instance.Ui.StartFullscreen)
            {
                _startFullScreen.Active = true;
            }

            _showConsole.Active = ConfigurationState.Instance.Ui.ShowConsole.Value;
            _showConsole.Visible = ConsoleHelper.SetConsoleWindowStateSupported;

            _actionMenu.Sensitive = false;
            _pauseEmulation.Sensitive = false;
            _resumeEmulation.Sensitive = false;

            if (ConfigurationState.Instance.Ui.GuiColumns.FavColumn)        _favToggle.Active        = true;
            if (ConfigurationState.Instance.Ui.GuiColumns.IconColumn)       _iconToggle.Active       = true;
            if (ConfigurationState.Instance.Ui.GuiColumns.AppColumn)        _appToggle.Active        = true;
            if (ConfigurationState.Instance.Ui.GuiColumns.DevColumn)        _developerToggle.Active  = true;
            if (ConfigurationState.Instance.Ui.GuiColumns.VersionColumn)    _versionToggle.Active    = true;
            if (ConfigurationState.Instance.Ui.GuiColumns.TimePlayedColumn) _timePlayedToggle.Active = true;
            if (ConfigurationState.Instance.Ui.GuiColumns.LastPlayedColumn) _lastPlayedToggle.Active = true;
            if (ConfigurationState.Instance.Ui.GuiColumns.FileExtColumn)    _fileExtToggle.Active    = true;
            if (ConfigurationState.Instance.Ui.GuiColumns.FileSizeColumn)   _fileSizeToggle.Active   = true;
            if (ConfigurationState.Instance.Ui.GuiColumns.PathColumn)       _pathToggle.Active       = true;

            _favToggle.Toggled        += Fav_Toggled;
            _iconToggle.Toggled       += Icon_Toggled;
            _appToggle.Toggled        += App_Toggled;
            _developerToggle.Toggled  += Developer_Toggled;
            _versionToggle.Toggled    += Version_Toggled;
            _timePlayedToggle.Toggled += TimePlayed_Toggled;
            _lastPlayedToggle.Toggled += LastPlayed_Toggled;
            _fileExtToggle.Toggled    += FileExt_Toggled;
            _fileSizeToggle.Toggled   += FileSize_Toggled;
            _pathToggle.Toggled       += Path_Toggled;

            _gameTable.Model = _tableStore = new ListStore(
                typeof(bool),
                typeof(Gdk.Pixbuf),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(BlitStruct<ApplicationControlProperty>));

            _tableStore.SetSortFunc(5, SortHelper.TimePlayedSort);
            _tableStore.SetSortFunc(6, SortHelper.LastPlayedSort);
            _tableStore.SetSortFunc(8, SortHelper.FileSizeSort);

            int  columnId  = ConfigurationState.Instance.Ui.ColumnSort.SortColumnId;
            bool ascending = ConfigurationState.Instance.Ui.ColumnSort.SortAscending;

            _tableStore.SetSortColumnId(columnId, ascending ? SortType.Ascending : SortType.Descending);

            _gameTable.EnableSearch = true;
            _gameTable.SearchColumn = 2;
            _gameTable.SearchEqualFunc = (model, col, key, iter) => !((string)model.GetValue(iter, col)).Contains(key, StringComparison.InvariantCultureIgnoreCase);

            _hideUi.Label = _hideUi.Label.Replace("SHOWUIKEY", ConfigurationState.Instance.Hid.Hotkeys.Value.ShowUi.ToString());

            UpdateColumns();
            UpdateGameTable();

            ConfigurationState.Instance.Ui.GameDirs.Event += (sender, args) =>
            {
                if (args.OldValue != args.NewValue)
                {
                    UpdateGameTable();
                }
            };

            Task.Run(RefreshFirmwareLabel);

            InputManager = new InputManager(new GTK3KeyboardDriver(this), new SDL2GamepadDriver());
        }

        private void UpdateIgnoreMissingServicesState(object sender, ReactiveEventArgs<bool> args)
        {
            if (_emulationContext != null)
            {
                _emulationContext.Configuration.IgnoreMissingServices = args.NewValue;
            }
        }

        private void UpdateAspectRatioState(object sender, ReactiveEventArgs<AspectRatio> args)
        {
            if (_emulationContext != null)
            {
                _emulationContext.Configuration.AspectRatio = args.NewValue;
            }
        }

        private void UpdateDockedModeState(object sender, ReactiveEventArgs<bool> e)
        {
            if (_emulationContext != null)
            {
                _emulationContext.System.ChangeDockedModeState(e.NewValue);
            }
        }

        private void UpdateAudioVolumeState(object sender, ReactiveEventArgs<float> e)
        {
            _emulationContext?.SetVolume(e.NewValue);
        }

        private void WindowStateEvent_Changed(object o, WindowStateEventArgs args)
        {
            _fullScreen.Label = args.Event.NewWindowState.HasFlag(Gdk.WindowState.Fullscreen) ? "Exit Fullscreen" : "Enter Fullscreen";
        }

        private void MainWindow_FocusOutEvent(object o, FocusOutEventArgs args)
        {
            IsFocused = false;
        }

        private void MainWindow_FocusInEvent(object o, FocusInEventArgs args)
        {
            IsFocused = true;
        }

        private void UpdateColumns()
        {
            foreach (TreeViewColumn column in _gameTable.Columns)
            {
                _gameTable.RemoveColumn(column);
            }

            CellRendererToggle favToggle = new CellRendererToggle();
            favToggle.Toggled += FavToggle_Toggled;

            if (ConfigurationState.Instance.Ui.GuiColumns.FavColumn)        _gameTable.AppendColumn("Fav",         favToggle,                "active", 0);
            if (ConfigurationState.Instance.Ui.GuiColumns.IconColumn)       _gameTable.AppendColumn("Icon",        new CellRendererPixbuf(), "pixbuf", 1);
            if (ConfigurationState.Instance.Ui.GuiColumns.AppColumn)        _gameTable.AppendColumn("Application", new CellRendererText(),   "text",   2);
            if (ConfigurationState.Instance.Ui.GuiColumns.DevColumn)        _gameTable.AppendColumn("Developer",   new CellRendererText(),   "text",   3);
            if (ConfigurationState.Instance.Ui.GuiColumns.VersionColumn)    _gameTable.AppendColumn("Version",     new CellRendererText(),   "text",   4);
            if (ConfigurationState.Instance.Ui.GuiColumns.TimePlayedColumn) _gameTable.AppendColumn("Time Played", new CellRendererText(),   "text",   5);
            if (ConfigurationState.Instance.Ui.GuiColumns.LastPlayedColumn) _gameTable.AppendColumn("Last Played", new CellRendererText(),   "text",   6);
            if (ConfigurationState.Instance.Ui.GuiColumns.FileExtColumn)    _gameTable.AppendColumn("File Ext",    new CellRendererText(),   "text",   7);
            if (ConfigurationState.Instance.Ui.GuiColumns.FileSizeColumn)   _gameTable.AppendColumn("File Size",   new CellRendererText(),   "text",   8);
            if (ConfigurationState.Instance.Ui.GuiColumns.PathColumn)       _gameTable.AppendColumn("Path",        new CellRendererText(),   "text",   9);

            foreach (TreeViewColumn column in _gameTable.Columns)
            {
                switch (column.Title)
                {
                    case "Fav":
                        column.SortColumnId = 0;
                        column.Clicked += Column_Clicked;
                        break;
                    case "Application":
                        column.SortColumnId = 2;
                        column.Clicked += Column_Clicked;
                        break;
                    case "Developer":
                        column.SortColumnId = 3;
                        column.Clicked += Column_Clicked;
                        break;
                    case "Version":
                        column.SortColumnId = 4;
                        column.Clicked += Column_Clicked;
                        break;
                    case "Time Played":
                        column.SortColumnId = 5;
                        column.Clicked += Column_Clicked;
                        break;
                    case "Last Played":
                        column.SortColumnId = 6;
                        column.Clicked += Column_Clicked;
                        break;
                    case "File Ext":
                        column.SortColumnId = 7;
                        column.Clicked += Column_Clicked;
                        break;
                    case "File Size":
                        column.SortColumnId = 8;
                        column.Clicked += Column_Clicked;
                        break;
                    case "Path":
                        column.SortColumnId = 9;
                        column.Clicked += Column_Clicked;
                        break;
                }
            }
        }

        protected override void OnDestroyed()
        {
            InputManager.Dispose();
        }

        private void InitializeSwitchInstance()
        {
            _virtualFileSystem.ReloadKeySet();

            IRenderer renderer;

            if (UseVulkan)
            {
                throw new NotImplementedException();
            }
            else
            {
                renderer = new Renderer();
            }

            BackendThreading threadingMode = ConfigurationState.Instance.Graphics.BackendThreading;

            bool threadedGAL = threadingMode == BackendThreading.On || (threadingMode == BackendThreading.Auto && renderer.PreferThreading);

            if (threadedGAL)
            {
                renderer = new ThreadedRenderer(renderer);
            }

            Logger.Info?.PrintMsg(LogClass.Gpu, $"Backend Threading ({threadingMode}): {threadedGAL}");

            IHardwareDeviceDriver deviceDriver = new DummyHardwareDeviceDriver();

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
                        SaveConfig();

                        deviceDriver = new OpenALHardwareDeviceDriver();
                    }
                    else
                    {
                        Logger.Warning?.Print(LogClass.Audio, "OpenAL is not supported, trying to fall back to SoundIO.");

                        if (SoundIoHardwareDeviceDriver.IsSupported)
                        {
                            Logger.Warning?.Print(LogClass.Audio, "Found SoundIO, changing configuration.");

                            ConfigurationState.Instance.System.AudioBackend.Value = AudioBackend.SoundIo;
                            SaveConfig();

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
                        SaveConfig();

                        deviceDriver = new SDL2HardwareDeviceDriver();
                    }
                    else
                    {
                        Logger.Warning?.Print(LogClass.Audio, "SDL2 is not supported, trying to fall back to OpenAL.");

                        if (OpenALHardwareDeviceDriver.IsSupported)
                        {
                            Logger.Warning?.Print(LogClass.Audio, "Found OpenAL, changing configuration.");

                            ConfigurationState.Instance.System.AudioBackend.Value = AudioBackend.OpenAl;
                            SaveConfig();

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
                        SaveConfig();

                        deviceDriver = new SDL2HardwareDeviceDriver();
                    }
                    else
                    {
                        Logger.Warning?.Print(LogClass.Audio, "SDL2 is not supported, trying to fall back to SoundIO.");

                        if (SoundIoHardwareDeviceDriver.IsSupported)
                        {
                            Logger.Warning?.Print(LogClass.Audio, "Found SoundIO, changing configuration.");

                            ConfigurationState.Instance.System.AudioBackend.Value = AudioBackend.SoundIo;
                            SaveConfig();

                            deviceDriver = new SoundIoHardwareDeviceDriver();
                        }
                        else
                        {
                            Logger.Warning?.Print(LogClass.Audio, "SoundIO is not supported, falling back to dummy audio out.");
                        }
                    }
                }
            }

            var memoryConfiguration = ConfigurationState.Instance.System.ExpandRam.Value
                ? HLE.MemoryConfiguration.MemoryConfiguration6GB
                : HLE.MemoryConfiguration.MemoryConfiguration4GB;

            IntegrityCheckLevel fsIntegrityCheckLevel = ConfigurationState.Instance.System.EnableFsIntegrityChecks ? IntegrityCheckLevel.ErrorOnInvalid : IntegrityCheckLevel.None;

            HLE.HLEConfiguration configuration = new HLE.HLEConfiguration(_virtualFileSystem,
                                                                          _libHacHorizonManager,
                                                                          _contentManager,
                                                                          _accountManager,
                                                                          _userChannelPersistence,
                                                                          renderer,
                                                                          deviceDriver,
                                                                          memoryConfiguration,
                                                                          _uiHandler,
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

            _emulationContext = new HLE.Switch(configuration);
        }

        private void SetupProgressUiHandlers()
        {
            Ptc.PtcStateChanged -= ProgressHandler;
            Ptc.PtcStateChanged += ProgressHandler;

            _emulationContext.Gpu.ShaderCacheStateChanged -= ProgressHandler;
            _emulationContext.Gpu.ShaderCacheStateChanged += ProgressHandler;
        }

        private void ProgressHandler<T>(T state, int current, int total) where T : Enum
        {
            bool visible;
            string label;

            switch (state)
            {
                case PtcLoadingState ptcState:
                    visible = ptcState != PtcLoadingState.Loaded;
                    label = $"PTC : {current}/{total}";
                    break;
                case ShaderCacheLoadingState shaderCacheState:
                    visible = shaderCacheState != ShaderCacheLoadingState.Loaded;
                    label = $"Shaders : {current}/{total}";
                    break;
                default:
                    throw new ArgumentException($"Unknown Progress Handler type {typeof(T)}");
            }

            Application.Invoke(delegate
            {
                _loadingStatusLabel.Text = label;
                _loadingStatusBar.Fraction = total > 0 ? (double)current / total : 0;
                _loadingStatusBar.Visible = visible;
                _loadingStatusLabel.Visible = visible;
            });
        }

        public void UpdateGameTable()
        {
            if (_updatingGameTable || _gameLoaded)
            {
                return;
            }

            _updatingGameTable = true;

            _tableStore.Clear();

            Thread applicationLibraryThread = new Thread(() =>
            {
                _applicationLibrary.LoadApplications(ConfigurationState.Instance.Ui.GameDirs, ConfigurationState.Instance.System.Language);

                _updatingGameTable = false;
            });
            applicationLibraryThread.Name         = "GUI.ApplicationLibraryThread";
            applicationLibraryThread.IsBackground = true;
            applicationLibraryThread.Start();
        }

        [Conditional("RELEASE")]
        public void PerformanceCheck()
        {
            if (ConfigurationState.Instance.Logger.EnableTrace.Value)
            {
                MessageDialog debugWarningDialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo, null)
                {
                    Title         = "Ryujinx - Warning",
                    Text          = "You have trace logging enabled, which is designed to be used by developers only.",
                    SecondaryText = "For optimal performance, it's recommended to disable trace logging. Would you like to disable trace logging now?"
                };

                if (debugWarningDialog.Run() == (int)ResponseType.Yes)
                {
                    ConfigurationState.Instance.Logger.EnableTrace.Value = false;
                    SaveConfig();
                }

                debugWarningDialog.Dispose();
            }

            if (!string.IsNullOrWhiteSpace(ConfigurationState.Instance.Graphics.ShadersDumpPath.Value))
            {
                MessageDialog shadersDumpWarningDialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Warning, ButtonsType.YesNo, null)
                {
                    Title         = "Ryujinx - Warning",
                    Text          = "You have shader dumping enabled, which is designed to be used by developers only.",
                    SecondaryText = "For optimal performance, it's recommended to disable shader dumping. Would you like to disable shader dumping now?"
                };

                if (shadersDumpWarningDialog.Run() == (int)ResponseType.Yes)
                {
                    ConfigurationState.Instance.Graphics.ShadersDumpPath.Value = "";
                    SaveConfig();
                }

                shadersDumpWarningDialog.Dispose();
            }
        }

        public void LoadApplication(string path, bool startFullscreen = false)
        {
            if (_gameLoaded)
            {
                GtkDialog.CreateInfoDialog("A game has already been loaded", "Please stop emulation or close the emulator before launching another game.");
            }
            else
            {
                PerformanceCheck();

                Logger.RestartTime();

                RendererWidget = CreateRendererWidget();

                SwitchToRenderWidget(startFullscreen);

                InitializeSwitchInstance();

                UpdateGraphicsConfig();

                SetupProgressUiHandlers();

                SystemVersion firmwareVersion = _contentManager.GetCurrentFirmwareVersion();

                bool isDirectory     = Directory.Exists(path);
                bool isFirmwareTitle = false;

                if (path.StartsWith("@SystemContent"))
                {
                    path = _virtualFileSystem.SwitchPathToSystemPath(path);

                    isFirmwareTitle = true;
                }

                if (!SetupValidator.CanStartApplication(_contentManager, path, out UserError userError))
                {
                    if (SetupValidator.CanFixStartApplication(_contentManager, path, userError, out firmwareVersion))
                    {
                        if (userError == UserError.NoFirmware)
                        {
                            string message = $"Would you like to install the firmware embedded in this game? (Firmware {firmwareVersion.VersionString})";

                            ResponseType responseDialog = (ResponseType)GtkDialog.CreateConfirmationDialog("No Firmware Installed", message).Run();

                            if (responseDialog != ResponseType.Yes)
                            {
                                UserErrorDialog.CreateUserErrorDialog(userError);

                                _emulationContext.Dispose();
                                SwitchToGameTable();
                                RendererWidget.Dispose();

                                return;
                            }
                        }

                        if (!SetupValidator.TryFixStartApplication(_contentManager, path, userError, out _))
                        {
                            UserErrorDialog.CreateUserErrorDialog(userError);

                            _emulationContext.Dispose();
                            SwitchToGameTable();
                            RendererWidget.Dispose();

                            return;
                        }

                        // Tell the user that we installed a firmware for them.
                        if (userError == UserError.NoFirmware)
                        {
                            firmwareVersion = _contentManager.GetCurrentFirmwareVersion();

                            RefreshFirmwareLabel();

                            string message = $"No installed firmware was found but Ryujinx was able to install firmware {firmwareVersion.VersionString} from the provided game.\nThe emulator will now start.";

                            GtkDialog.CreateInfoDialog($"Firmware {firmwareVersion.VersionString} was installed", message);
                        }
                    }
                    else
                    {
                        UserErrorDialog.CreateUserErrorDialog(userError);

                        _emulationContext.Dispose();
                        SwitchToGameTable();
                        RendererWidget.Dispose();

                        return;
                    }
                }

                Logger.Notice.Print(LogClass.Application, $"Using Firmware Version: {firmwareVersion?.VersionString}");

                if (isFirmwareTitle)
                {
                    Logger.Info?.Print(LogClass.Application, "Loading as Firmware Title (NCA).");

                    _emulationContext.LoadNca(path);
                }
                else if (Directory.Exists(path))
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
                    switch (System.IO.Path.GetExtension(path).ToLowerInvariant())
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
                            }
                            break;
                    }
                }
                else
                {
                    Logger.Warning?.Print(LogClass.Application, "Please specify a valid XCI/NCA/NSP/PFS0/NRO file.");

                    _emulationContext.Dispose();
                    RendererWidget.Dispose();

                    return;
                }

                _currentEmulatedGamePath = path;

                _deviceExitStatus.Reset();

                Translator.IsReadyForTranslation.Reset();
#if MACOS_BUILD
                CreateGameWindow();
#else
                Thread windowThread = new Thread(() =>
                {
                    CreateGameWindow();
                })
                {
                    Name = "GUI.WindowThread"
                };

                windowThread.Start();
#endif

                _gameLoaded           = true;
                _actionMenu.Sensitive = true;

                _lastScannedAmiiboId = "";

                _firmwareInstallFile.Sensitive      = false;
                _firmwareInstallDirectory.Sensitive = false;

                DiscordIntegrationModule.SwitchToPlayingState(_emulationContext.Application.TitleIdText, _emulationContext.Application.TitleName);

                _applicationLibrary.LoadAndSaveMetaData(_emulationContext.Application.TitleIdText, appMetadata =>
                {
                    appMetadata.LastPlayed = DateTime.UtcNow.ToString();
                });
            }
        }

        private RendererWidgetBase CreateRendererWidget()
        {
            if (UseVulkan)
            {
                return new VKRenderer(InputManager, ConfigurationState.Instance.Logger.GraphicsDebugLevel);
            }
            else
            {
                return new GlRenderer(InputManager, ConfigurationState.Instance.Logger.GraphicsDebugLevel);
            }
        }

        private void SwitchToRenderWidget(bool startFullscreen = false)
        {
            _viewBox.Remove(_gameTableWindow);
            RendererWidget.Expand = true;
            _viewBox.Child = RendererWidget;

            RendererWidget.ShowAll();
            EditFooterForGameRenderer();

            if (Window.State.HasFlag(Gdk.WindowState.Fullscreen))
            {
                ToggleExtraWidgets(false);
            }
            else if (startFullscreen || ConfigurationState.Instance.Ui.StartFullscreen.Value)
            {
                FullScreen_Toggled(null, null);
            }
        }

        private void SwitchToGameTable()
        {
            if (Window.State.HasFlag(Gdk.WindowState.Fullscreen))
            {
                ToggleExtraWidgets(true);
            }

            RendererWidget.Exit();

            if (RendererWidget.Window != Window && RendererWidget.Window != null)
            {
                RendererWidget.Window.Dispose();
            }

            RendererWidget.Dispose();

            if (OperatingSystem.IsWindows())
            {
                _windowsMultimediaTimerResolution?.Dispose();
                _windowsMultimediaTimerResolution = null;
            }

            DisplaySleep.Restore();

            _viewBox.Remove(RendererWidget);
            _viewBox.Add(_gameTableWindow);

            _gameTableWindow.Expand = true;

            Window.Title = $"Ryujinx {Program.Version}";

            _emulationContext = null;
            _gameLoaded = false;
            RendererWidget = null;

            DiscordIntegrationModule.SwitchToMainMenu();

            RecreateFooterForMenu();

            UpdateColumns();
            UpdateGameTable();

            Task.Run(RefreshFirmwareLabel);
            Task.Run(HandleRelaunch);

            _actionMenu.Sensitive = false;
            _firmwareInstallFile.Sensitive = true;
            _firmwareInstallDirectory.Sensitive = true;
        }

        private void CreateGameWindow()
        {
            if (OperatingSystem.IsWindows())
            {
                _windowsMultimediaTimerResolution = new WindowsMultimediaTimerResolution(1);
            }

            DisplaySleep.Prevent();

            RendererWidget.Initialize(_emulationContext);

            RendererWidget.WaitEvent.WaitOne();

            RendererWidget.Start();

            Ptc.Close();
            PtcProfiler.Stop();

            _emulationContext.Dispose();
            _deviceExitStatus.Set();

            // NOTE: Everything that is here will not be executed when you close the UI.
            Application.Invoke(delegate
            {
                SwitchToGameTable();
            });
        }

        private void RecreateFooterForMenu()
        {
            _listStatusBox.Show();
            _statusBar.Hide();
        }

        private void EditFooterForGameRenderer()
        {
            _listStatusBox.Hide();
            _statusBar.Show();
        }

        public void ToggleExtraWidgets(bool show)
        {
            if (RendererWidget != null)
            {
                if (show)
                {
                    _menuBar.ShowAll();
                    _footerBox.Show();
                    _statusBar.Show();
                }
                else
                {
                    _menuBar.Hide();
                    _footerBox.Hide();
                }
            }
        }

        private void UpdateGameMetadata(string titleId)
        {
            if (_gameLoaded)
            {
                _applicationLibrary.LoadAndSaveMetaData(titleId, appMetadata =>
                {
                    DateTime lastPlayedDateTime = DateTime.Parse(appMetadata.LastPlayed);
                    double   sessionTimePlayed  = DateTime.UtcNow.Subtract(lastPlayedDateTime).TotalSeconds;

                    appMetadata.TimePlayed += Math.Round(sessionTimePlayed, MidpointRounding.AwayFromZero);
                });
            }
        }

        public void UpdateGraphicsConfig()
        {
            int   resScale       = ConfigurationState.Instance.Graphics.ResScale;
            float resScaleCustom = ConfigurationState.Instance.Graphics.ResScaleCustom;

            Graphics.Gpu.GraphicsConfig.ResScale          = (resScale == -1) ? resScaleCustom : resScale;
            Graphics.Gpu.GraphicsConfig.MaxAnisotropy     = ConfigurationState.Instance.Graphics.MaxAnisotropy;
            Graphics.Gpu.GraphicsConfig.ShadersDumpPath   = ConfigurationState.Instance.Graphics.ShadersDumpPath;
            Graphics.Gpu.GraphicsConfig.EnableShaderCache = ConfigurationState.Instance.Graphics.EnableShaderCache;
        }

        public void SaveConfig()
        {
            ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);
        }

        private void End()
        {
            if (_ending)
            {
                return;
            }

            _ending = true;

            if (_emulationContext != null)
            {
                UpdateGameMetadata(_emulationContext.Application.TitleIdText);

                if (RendererWidget != null)
                {
                    // We tell the widget that we are exiting.
                    RendererWidget.Exit();

                    // Wait for the other thread to dispose the HLE context before exiting.
                    _deviceExitStatus.WaitOne();
                    RendererWidget.Dispose();
                }
            }

            Dispose();

            Program.Exit();
            Application.Quit();
        }

        //
        // Events
        //
        private void Application_Added(object sender, ApplicationAddedEventArgs args)
        {
            Application.Invoke(delegate
            {
                _tableStore.AppendValues(
                    args.AppData.Favorite,
                    new Gdk.Pixbuf(args.AppData.Icon, 75, 75),
                    $"{args.AppData.TitleName}\n{args.AppData.TitleId.ToUpper()}",
                    args.AppData.Developer,
                    args.AppData.Version,
                    args.AppData.TimePlayed,
                    args.AppData.LastPlayed,
                    args.AppData.FileExtension,
                    args.AppData.FileSize,
                    args.AppData.Path,
                    args.AppData.ControlHolder);
            });
        }

        private void ApplicationCount_Updated(object sender, ApplicationCountUpdatedEventArgs args)
        {
            Application.Invoke(delegate
            {
                _progressLabel.Text = $"{args.NumAppsLoaded}/{args.NumAppsFound} Games Loaded";
                float barValue      = 0;

                if (args.NumAppsFound != 0)
                {
                    barValue = (float)args.NumAppsLoaded / args.NumAppsFound;
                }

                _progressBar.Fraction = barValue;

                // Reset the vertical scrollbar to the top when titles finish loading
                if (args.NumAppsLoaded == args.NumAppsFound)
                {
                    _gameTableWindow.Vadjustment.Value = 0;
                }
            });
        }

        private void Update_StatusBar(object sender, StatusUpdatedEventArgs args)
        {
            Application.Invoke(delegate
            {
                _gameStatus.Text   = args.GameStatus;
                _fifoStatus.Text   = args.FifoStatus;
                _gpuName.Text      = args.GpuName;
                _dockedMode.Text   = args.DockedMode;
                _aspectRatio.Text  = args.AspectRatio;
                _volumeStatus.Text = GetVolumeLabelText(args.Volume);

                if (args.VSyncEnabled)
                {
                    _vSyncStatus.Attributes = new Pango.AttrList();
                    _vSyncStatus.Attributes.Insert(new Pango.AttrForeground(11822, 60138, 51657));
                }
                else
                {
                    _vSyncStatus.Attributes = new Pango.AttrList();
                    _vSyncStatus.Attributes.Insert(new Pango.AttrForeground(ushort.MaxValue, 17733, 21588));
                }
            });
        }

        private void FavToggle_Toggled(object sender, ToggledArgs args)
        {
            _tableStore.GetIter(out TreeIter treeIter, new TreePath(args.Path));

            string titleId        = _tableStore.GetValue(treeIter, 2).ToString().Split("\n")[1].ToLower();
            bool   newToggleValue = !(bool)_tableStore.GetValue(treeIter, 0);

            _tableStore.SetValue(treeIter, 0, newToggleValue);

            _applicationLibrary.LoadAndSaveMetaData(titleId, appMetadata =>
            {
                appMetadata.Favorite = newToggleValue;
            });
        }

        private void Column_Clicked(object sender, EventArgs args)
        {
            TreeViewColumn column = (TreeViewColumn)sender;

            ConfigurationState.Instance.Ui.ColumnSort.SortColumnId.Value  = column.SortColumnId;
            ConfigurationState.Instance.Ui.ColumnSort.SortAscending.Value = column.SortOrder == SortType.Ascending;

            SaveConfig();
        }

        private void Row_Activated(object sender, RowActivatedArgs args)
        {
            _gameTableSelection.GetSelected(out TreeIter treeIter);

            string path = (string)_tableStore.GetValue(treeIter, 9);

            LoadApplication(path);
        }

        private void VSyncStatus_Clicked(object sender, ButtonReleaseEventArgs args)
        {
            _emulationContext.EnableDeviceVsync = !_emulationContext.EnableDeviceVsync;

            Logger.Info?.Print(LogClass.Application, $"VSync toggled to: {_emulationContext.EnableDeviceVsync}");
        }

        private void DockedMode_Clicked(object sender, ButtonReleaseEventArgs args)
        {
            ConfigurationState.Instance.System.EnableDockedMode.Value = !ConfigurationState.Instance.System.EnableDockedMode.Value;
        }

        private string GetVolumeLabelText(float volume)
        {
            string icon = volume == 0 ? "🔇" : "🔊";

            return $"{icon} {(int)(volume * 100)}%";
        }

        private void VolumeStatus_Clicked(object sender, ButtonReleaseEventArgs args)
        {
            if (_emulationContext != null)
            {
                if (_emulationContext.IsAudioMuted())
                {
                    _emulationContext.SetVolume(ConfigurationState.Instance.System.AudioVolume);
                }
                else
                {
                    _emulationContext.SetVolume(0);
                }
            }
        }

        private void AspectRatio_Clicked(object sender, ButtonReleaseEventArgs args)
        {
            AspectRatio aspectRatio = ConfigurationState.Instance.Graphics.AspectRatio.Value;

            ConfigurationState.Instance.Graphics.AspectRatio.Value = ((int)aspectRatio + 1) > Enum.GetNames<AspectRatio>().Length - 1 ? AspectRatio.Fixed4x3 : aspectRatio + 1;
        }

        private void Row_Clicked(object sender, ButtonReleaseEventArgs args)
        {
            if (args.Event.Button != 3 /* Right Click */)
            {
                return;
            }

            _gameTableSelection.GetSelected(out TreeIter treeIter);

            if (treeIter.UserData == IntPtr.Zero)
            {
                return;
            }

            string titleFilePath = _tableStore.GetValue(treeIter, 9).ToString();
            string titleName     = _tableStore.GetValue(treeIter, 2).ToString().Split("\n")[0];
            string titleId       = _tableStore.GetValue(treeIter, 2).ToString().Split("\n")[1].ToLower();

            BlitStruct<ApplicationControlProperty> controlData = (BlitStruct<ApplicationControlProperty>)_tableStore.GetValue(treeIter, 10);

            _ = new GameTableContextMenu(this, _virtualFileSystem, _accountManager, _libHacHorizonManager.RyujinxClient, titleFilePath, titleName, titleId, controlData);
        }

        private void Load_Application_File(object sender, EventArgs args)
        {
            using (FileChooserNative fileChooser = new FileChooserNative("Choose the file to open", this, FileChooserAction.Open, "Open", "Cancel"))
            {
                FileFilter filter = new FileFilter()
                {
                    Name = "Switch Executables"
                };
                filter.AddPattern("*.xci");
                filter.AddPattern("*.nsp");
                filter.AddPattern("*.pfs0");
                filter.AddPattern("*.nca");
                filter.AddPattern("*.nro");
                filter.AddPattern("*.nso");

                fileChooser.AddFilter(filter);

                if (fileChooser.Run() == (int)ResponseType.Accept)
                {
                    LoadApplication(fileChooser.Filename);
                }
            }
        }

        private void Load_Application_Folder(object sender, EventArgs args)
        {
            using (FileChooserNative fileChooser = new FileChooserNative("Choose the folder to open", this, FileChooserAction.SelectFolder, "Open", "Cancel"))
            {
                if (fileChooser.Run() == (int)ResponseType.Accept)
                {
                    LoadApplication(fileChooser.Filename);
                }
            }
        }

        private void FileMenu_StateChanged(object o, StateChangedArgs args)
        {
            _appletMenu.Sensitive            = _emulationContext == null && _contentManager.GetCurrentFirmwareVersion() != null && _contentManager.GetCurrentFirmwareVersion().Major > 3;
            _loadApplicationFile.Sensitive   = _emulationContext == null;
            _loadApplicationFolder.Sensitive = _emulationContext == null;
        }

        private void Load_Mii_Edit_Applet(object sender, EventArgs args)
        {
            string contentPath = _contentManager.GetInstalledContentPath(0x0100000000001009, StorageId.BuiltInSystem, NcaContentType.Program);

            LoadApplication(contentPath);
        }

        private void Open_Ryu_Folder(object sender, EventArgs args)
        {
            OpenHelper.OpenFolder(AppDataManager.BaseDirPath);
        }

        private void OpenLogsFolder_Pressed(object sender, EventArgs args)
        {
            string logPath = System.IO.Path.Combine(ReleaseInformations.GetBaseApplicationDirectory(), "Logs");

            new DirectoryInfo(logPath).Create();

            OpenHelper.OpenFolder(logPath);
        }

        private void Exit_Pressed(object sender, EventArgs args)
        {
            if (!_gameLoaded || !ConfigurationState.Instance.ShowConfirmExit || GtkDialog.CreateExitDialog())
            {
                End();
            }
        }

        private void Window_Close(object sender, DeleteEventArgs args)
        {
            if (!_gameLoaded || !ConfigurationState.Instance.ShowConfirmExit || GtkDialog.CreateExitDialog())
            {
                End();
            }
            else
            {
                args.RetVal = true;
            }
        }

        private void StopEmulation_Pressed(object sender, EventArgs args)
        {
            if (_emulationContext != null)
            {
                UpdateGameMetadata(_emulationContext.Application.TitleIdText);
            }

            _pauseEmulation.Sensitive = false;
            _resumeEmulation.Sensitive = false;
            RendererWidget?.Exit();
        }

        private void PauseEmulation_Pressed(object sender, EventArgs args)
        {
            _pauseEmulation.Sensitive = false;
            _resumeEmulation.Sensitive = true;
            _emulationContext.System.TogglePauseEmulation(true);
        }

        private void ResumeEmulation_Pressed(object sender, EventArgs args)
        {
            _pauseEmulation.Sensitive = true;
            _resumeEmulation.Sensitive = false;
            _emulationContext.System.TogglePauseEmulation(false);
        }

        public void ActivatePauseMenu()
        {
            _pauseEmulation.Sensitive = true;
            _resumeEmulation.Sensitive = false;
        }

        public void TogglePause()
        {
            _pauseEmulation.Sensitive ^= true;
            _resumeEmulation.Sensitive ^= true;
            _emulationContext.System.TogglePauseEmulation(_resumeEmulation.Sensitive);
        }

        private void Installer_File_Pressed(object o, EventArgs args)
        {
            FileChooserNative fileChooser = new FileChooserNative("Choose the firmware file to open", this, FileChooserAction.Open, "Open", "Cancel");

            FileFilter filter = new FileFilter
            {
                Name = "Switch Firmware Files"
            };
            filter.AddPattern("*.zip");
            filter.AddPattern("*.xci");

            fileChooser.AddFilter(filter);

            HandleInstallerDialog(fileChooser);
        }

        private void Installer_Directory_Pressed(object o, EventArgs args)
        {
            FileChooserNative directoryChooser = new FileChooserNative("Choose the firmware directory to open", this, FileChooserAction.SelectFolder, "Open", "Cancel");

            HandleInstallerDialog(directoryChooser);
        }

        private void HandleInstallerDialog(FileChooserNative fileChooser)
        {
            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                try
                {
                    string filename = fileChooser.Filename;

                    fileChooser.Dispose();

                    SystemVersion firmwareVersion = _contentManager.VerifyFirmwarePackage(filename);

                    if (firmwareVersion is null)
                    {
                        GtkDialog.CreateErrorDialog($"A valid system firmware was not found in {filename}.");

                        return;
                    }

                    string dialogTitle = $"Install Firmware {firmwareVersion.VersionString}";

                    SystemVersion currentVersion = _contentManager.GetCurrentFirmwareVersion();

                    string dialogMessage = $"System version {firmwareVersion.VersionString} will be installed.";

                    if (currentVersion != null)
                    {
                        dialogMessage += $"\n\nThis will replace the current system version {currentVersion.VersionString}. ";
                    }

                    dialogMessage += "\n\nDo you want to continue?";

                    ResponseType responseInstallDialog = (ResponseType)GtkDialog.CreateConfirmationDialog(dialogTitle, dialogMessage).Run();

                    MessageDialog waitingDialog = GtkDialog.CreateWaitingDialog(dialogTitle, "Installing firmware...");

                    if (responseInstallDialog == ResponseType.Yes)
                    {
                        Logger.Info?.Print(LogClass.Application, $"Installing firmware {firmwareVersion.VersionString}");

                        Thread thread = new Thread(() =>
                        {
                            Application.Invoke(delegate
                            {
                                waitingDialog.Run();

                            });

                            try
                            {
                                _contentManager.InstallFirmware(filename);

                                Application.Invoke(delegate
                                {
                                    waitingDialog.Dispose();

                                    string message = $"System version {firmwareVersion.VersionString} successfully installed.";

                                    GtkDialog.CreateInfoDialog(dialogTitle, message);
                                    Logger.Info?.Print(LogClass.Application, message);

                                    // Purge Applet Cache.

                                    DirectoryInfo miiEditorCacheFolder = new DirectoryInfo(System.IO.Path.Combine(AppDataManager.GamesDirPath, "0100000000001009", "cache"));

                                    if (miiEditorCacheFolder.Exists)
                                    {
                                        miiEditorCacheFolder.Delete(true);
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                Application.Invoke(delegate
                                {
                                    waitingDialog.Dispose();

                                    GtkDialog.CreateErrorDialog(ex.Message);
                                });
                            }
                            finally
                            {
                                RefreshFirmwareLabel();
                            }
                        });

                        thread.Name = "GUI.FirmwareInstallerThread";
                        thread.Start();
                    }
                }
                catch (MissingKeyException ex)
                {
                    Logger.Error?.Print(LogClass.Application, ex.ToString());
                    UserErrorDialog.CreateUserErrorDialog(UserError.FirmwareParsingFailed);
                }
                catch (Exception ex)
                {
                    GtkDialog.CreateErrorDialog(ex.Message);
                }
            }
            else
            {
                fileChooser.Dispose();
            }
        }

        private void RefreshFirmwareLabel()
        {
            SystemVersion currentFirmware = _contentManager.GetCurrentFirmwareVersion();

            Application.Invoke(delegate
            {
                _firmwareVersionLabel.Text = currentFirmware != null ? currentFirmware.VersionString : "0.0.0";
            });
        }

        private void HandleRelaunch()
        {
            if (_userChannelPersistence.PreviousIndex != -1 && _userChannelPersistence.ShouldRestart)
            {
                _userChannelPersistence.ShouldRestart = false;

                LoadApplication(_currentEmulatedGamePath);
            }
            else
            {
                // otherwise, clear state.
                _userChannelPersistence  = new UserChannelPersistence();
                _currentEmulatedGamePath = null;
            }
        }

        private void FullScreen_Toggled(object sender, EventArgs args)
        {
            if (!Window.State.HasFlag(Gdk.WindowState.Fullscreen))
            {
                Fullscreen();

                ToggleExtraWidgets(false);
            }
            else
            {
                Unfullscreen();

                ToggleExtraWidgets(true);
            }
        }

        private void StartFullScreen_Toggled(object sender, EventArgs args)
        {
            ConfigurationState.Instance.Ui.StartFullscreen.Value = _startFullScreen.Active;

            SaveConfig();
        }

        private void ShowConsole_Toggled(object sender, EventArgs args)
        {
            ConfigurationState.Instance.Ui.ShowConsole.Value = _showConsole.Active;

            SaveConfig();
        }

        private void OptionMenu_StateChanged(object o, StateChangedArgs args)
        {
            _manageUserProfiles.Sensitive = _emulationContext == null;
        }

        private void Settings_Pressed(object sender, EventArgs args)
        {
            SettingsWindow settingsWindow = new SettingsWindow(this, _virtualFileSystem, _contentManager);

            settingsWindow.SetSizeRequest((int)(settingsWindow.DefaultWidth * Program.WindowScaleFactor), (int)(settingsWindow.DefaultHeight * Program.WindowScaleFactor));
            settingsWindow.Show();
        }

        private void HideUi_Pressed(object sender, EventArgs args)
        {
            ToggleExtraWidgets(false);
        }

        private void ManageCheats_Pressed(object sender, EventArgs args)
        {
           var window = new CheatWindow(_virtualFileSystem, _emulationContext.Application.TitleId, _emulationContext.Application.TitleName);

            window.Destroyed += CheatWindow_Destroyed;
            window.Show();
        }

        private void CheatWindow_Destroyed(object sender, EventArgs e)
        {
            _emulationContext.EnableCheats();
            (sender as CheatWindow).Destroyed -= CheatWindow_Destroyed;
        }

        private void ManageUserProfiles_Pressed(object sender, EventArgs args)
        {
            UserProfilesManagerWindow userProfilesManagerWindow = new UserProfilesManagerWindow(_accountManager, _contentManager, _virtualFileSystem);

            userProfilesManagerWindow.SetSizeRequest((int)(userProfilesManagerWindow.DefaultWidth * Program.WindowScaleFactor), (int)(userProfilesManagerWindow.DefaultHeight * Program.WindowScaleFactor));
            userProfilesManagerWindow.Show();
        }

        private void Simulate_WakeUp_Message_Pressed(object sender, EventArgs args)
        {
            if (_emulationContext != null)
            {
                _emulationContext.System.SimulateWakeUpMessage();
            }
        }

        private void ActionMenu_StateChanged(object o, StateChangedArgs args)
        {
            _scanAmiibo.Sensitive     = _emulationContext != null && _emulationContext.System.SearchingForAmiibo(out int _);
            _takeScreenshot.Sensitive = _emulationContext != null;
        }

        private void Scan_Amiibo(object sender, EventArgs args)
        {
            if (_emulationContext.System.SearchingForAmiibo(out int deviceId))
            {
                AmiiboWindow amiiboWindow = new AmiiboWindow
                {
                    LastScannedAmiiboShowAll = _lastScannedAmiiboShowAll,
                    LastScannedAmiiboId      = _lastScannedAmiiboId,
                    DeviceId                 = deviceId,
                    TitleId                  = _emulationContext.Application.TitleIdText.ToUpper()
                };

                amiiboWindow.DeleteEvent += AmiiboWindow_DeleteEvent;

                amiiboWindow.Show();
            }
            else
            {
                GtkDialog.CreateInfoDialog($"Amiibo", "The game is currently not ready to receive Amiibo scan data. Ensure that you have an Amiibo-compatible game open and ready to receive Amiibo scan data.");
            }
        }

        private void Take_Screenshot(object sender, EventArgs args)
        {
            if (_emulationContext != null && RendererWidget != null)
            {
                RendererWidget.ScreenshotRequested = true;
            }
        }

        private void AmiiboWindow_DeleteEvent(object sender, DeleteEventArgs args)
        {
            if (((AmiiboWindow)sender).AmiiboId != "" && ((AmiiboWindow)sender).Response == ResponseType.Ok)
            {
                _lastScannedAmiiboId      = ((AmiiboWindow)sender).AmiiboId;
                _lastScannedAmiiboShowAll = ((AmiiboWindow)sender).LastScannedAmiiboShowAll;

                _emulationContext.System.ScanAmiibo(((AmiiboWindow)sender).DeviceId, ((AmiiboWindow)sender).AmiiboId, ((AmiiboWindow)sender).UseRandomUuid);
            }
        }

        private void Update_Pressed(object sender, EventArgs args)
        {
            if (Updater.CanUpdate(true))
            {
                Updater.BeginParse(this, true).ContinueWith(task =>
                {
                    Logger.Error?.Print(LogClass.Application, $"Updater error: {task.Exception}");
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
        }

        private void About_Pressed(object sender, EventArgs args)
        {
            AboutWindow aboutWindow = new AboutWindow();

            aboutWindow.SetSizeRequest((int)(aboutWindow.DefaultWidth * Program.WindowScaleFactor), (int)(aboutWindow.DefaultHeight * Program.WindowScaleFactor));
            aboutWindow.Show();
        }

        private void Fav_Toggled(object sender, EventArgs args)
        {
            ConfigurationState.Instance.Ui.GuiColumns.FavColumn.Value = _favToggle.Active;

            SaveConfig();
            UpdateColumns();
        }

        private void Icon_Toggled(object sender, EventArgs args)
        {
            ConfigurationState.Instance.Ui.GuiColumns.IconColumn.Value = _iconToggle.Active;

            SaveConfig();
            UpdateColumns();
        }

        private void App_Toggled(object sender, EventArgs args)
        {
            ConfigurationState.Instance.Ui.GuiColumns.AppColumn.Value = _appToggle.Active;

            SaveConfig();
            UpdateColumns();
        }

        private void Developer_Toggled(object sender, EventArgs args)
        {
            ConfigurationState.Instance.Ui.GuiColumns.DevColumn.Value = _developerToggle.Active;

            SaveConfig();
            UpdateColumns();
        }

        private void Version_Toggled(object sender, EventArgs args)
        {
            ConfigurationState.Instance.Ui.GuiColumns.VersionColumn.Value = _versionToggle.Active;

            SaveConfig();
            UpdateColumns();
        }

        private void TimePlayed_Toggled(object sender, EventArgs args)
        {
            ConfigurationState.Instance.Ui.GuiColumns.TimePlayedColumn.Value = _timePlayedToggle.Active;

            SaveConfig();
            UpdateColumns();
        }

        private void LastPlayed_Toggled(object sender, EventArgs args)
        {
            ConfigurationState.Instance.Ui.GuiColumns.LastPlayedColumn.Value = _lastPlayedToggle.Active;

            SaveConfig();
            UpdateColumns();
        }

        private void FileExt_Toggled(object sender, EventArgs args)
        {
            ConfigurationState.Instance.Ui.GuiColumns.FileExtColumn.Value = _fileExtToggle.Active;

            SaveConfig();
            UpdateColumns();
        }

        private void FileSize_Toggled(object sender, EventArgs args)
        {
            ConfigurationState.Instance.Ui.GuiColumns.FileSizeColumn.Value = _fileSizeToggle.Active;

            SaveConfig();
            UpdateColumns();
        }

        private void Path_Toggled(object sender, EventArgs args)
        {
            ConfigurationState.Instance.Ui.GuiColumns.PathColumn.Value = _pathToggle.Active;

            SaveConfig();
            UpdateColumns();
        }

        private void RefreshList_Pressed(object sender, ButtonReleaseEventArgs args)
        {
            UpdateGameTable();
        }
    }
}
