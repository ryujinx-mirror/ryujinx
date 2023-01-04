using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.Ncm;
using Ryujinx.Ava.Common;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Input;
using Ryujinx.Ava.UI.Controls;
using Ryujinx.Ava.UI.Helpers;
using Ryujinx.Ava.UI.Windows;
using Ryujinx.Common;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Cpu;
using Ryujinx.HLE;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using Ryujinx.Modules;
using Ryujinx.Ui.App.Common;
using Ryujinx.Ui.Common;
using Ryujinx.Ui.Common.Configuration;
using Ryujinx.Ui.Common.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Path = System.IO.Path;
using ShaderCacheLoadingState = Ryujinx.Graphics.Gpu.Shader.ShaderCacheState;

namespace Ryujinx.Ava.UI.ViewModels
{
    internal class MainWindowViewModel : BaseModel
    {
        private const int HotKeyPressDelayMs = 500;

        private readonly MainWindow _owner;
        private ObservableCollection<ApplicationData> _applications;
        private string _aspectStatusText;

        private string _loadHeading;
        private string _cacheLoadStatus;
        private string _searchText;
        private Timer _searchTimer;
        private string _dockedStatusText;
        private string _fifoStatusText;
        private string _gameStatusText;
        private string _volumeStatusText;
        private string _gpuStatusText;
        private bool _isAmiiboRequested;
        private bool _isGameRunning;
        private bool _isLoading;
        private int _progressMaximum;
        private int _progressValue;
        private long _lastFullscreenToggle = Environment.TickCount64;
        private bool _showLoadProgress;
        private bool _showMenuAndStatusBar = true;
        private bool _showStatusSeparator;
        private Brush _progressBarForegroundColor;
        private Brush _progressBarBackgroundColor;
        private Brush _vsyncColor;
        private byte[] _selectedIcon;
        private bool _isAppletMenuActive;
        private int _statusBarProgressMaximum;
        private int _statusBarProgressValue;
        private bool _isPaused;
        private bool _showContent = true;
        private bool _isLoadingIndeterminate = true;
        private bool _showAll;
        private string _lastScannedAmiiboId;
        private ReadOnlyObservableCollection<ApplicationData> _appsObservableList;
        public ApplicationLibrary ApplicationLibrary => _owner.ApplicationLibrary;

        public string TitleName { get; internal set; }

        public MainWindowViewModel(MainWindow owner) : this()
        {
            _owner = owner;
        }

        public MainWindowViewModel()
        {
            Applications = new ObservableCollection<ApplicationData>();

            Applications.ToObservableChangeSet()
                .Filter(Filter)
                .Sort(GetComparer())
                .Bind(out _appsObservableList).AsObservableList();

            if (Program.PreviewerDetached)
            {
                LoadConfigurableHotKeys();

                Volume = ConfigurationState.Instance.System.AudioVolume;
            }
        }

        public void Initialize()
        {
            ApplicationLibrary.ApplicationCountUpdated += ApplicationLibrary_ApplicationCountUpdated;
            ApplicationLibrary.ApplicationAdded += ApplicationLibrary_ApplicationAdded;
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;

                _searchTimer?.Dispose();

                _searchTimer = new Timer(TimerCallback, null, 1000, 0);
            }
        }

        private void TimerCallback(object obj)
        {
            RefreshView();

            _searchTimer.Dispose();
            _searchTimer = null;
        }

        public ReadOnlyObservableCollection<ApplicationData> AppsObservableList
        {
            get => _appsObservableList;
            set
            {
                _appsObservableList = value;

                OnPropertyChanged();
            }
        }

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;

                OnPropertyChanged();
            }
        }

        public bool EnableNonGameRunningControls => !IsGameRunning;

        public bool ShowFirmwareStatus => !ShowLoadProgress;

        public bool IsGameRunning
        {
            get => _isGameRunning;
            set
            {
                _isGameRunning = value;

                if (!value)
                {
                    ShowMenuAndStatusBar = false;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(EnableNonGameRunningControls));
                OnPropertyChanged(nameof(ShowFirmwareStatus));
            }
        }

        public bool IsAmiiboRequested
        {
            get => _isAmiiboRequested && _isGameRunning;
            set
            {
                _isAmiiboRequested = value;

                OnPropertyChanged();
            }
        }

        public bool ShowLoadProgress
        {
            get => _showLoadProgress;
            set
            {
                _showLoadProgress = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowFirmwareStatus));
            }
        }

        public string GameStatusText
        {
            get => _gameStatusText;
            set
            {
                _gameStatusText = value;

                OnPropertyChanged();
            }
        }

        private string _showUikey = "F4";
        private string _pauseKey = "F5";
        private string _screenshotkey = "F8";
        private float  _volume;
        private string _backendText;

        public ApplicationData SelectedApplication
        {
            get
            {
                return Glyph switch
                {
                    Glyph.List => _owner.GameList.SelectedApplication,
                    Glyph.Grid => _owner.GameGrid.SelectedApplication,
                    _          => null,
                };
            }
        }

        public string LoadHeading
        {
            get => _loadHeading;
            set
            {
                _loadHeading = value;

                OnPropertyChanged();
            }
        }

        public string CacheLoadStatus
        {
            get => _cacheLoadStatus;
            set
            {
                _cacheLoadStatus = value;

                OnPropertyChanged();
            }
        }

        public Brush ProgressBarBackgroundColor
        {
            get => _progressBarBackgroundColor;
            set
            {
                _progressBarBackgroundColor = value;

                OnPropertyChanged();
            }
        }

        public Brush ProgressBarForegroundColor
        {
            get => _progressBarForegroundColor;
            set
            {
                _progressBarForegroundColor = value;

                OnPropertyChanged();
            }
        }

        public Brush VsyncColor
        {
            get => _vsyncColor;
            set
            {
                _vsyncColor = value;

                OnPropertyChanged();
            }
        }

        public byte[] SelectedIcon
        {
            get => _selectedIcon;
            set
            {
                _selectedIcon = value;

                OnPropertyChanged();
            }
        }

        public int ProgressMaximum
        {
            get => _progressMaximum;
            set
            {
                _progressMaximum = value;

                OnPropertyChanged();
            }
        }

        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;

                OnPropertyChanged();
            }
        }

        public int StatusBarProgressMaximum
        {
            get => _statusBarProgressMaximum;
            set
            {
                _statusBarProgressMaximum = value;

                OnPropertyChanged();
            }
        }

        public int StatusBarProgressValue
        {
            get => _statusBarProgressValue;
            set
            {
                _statusBarProgressValue = value;

                OnPropertyChanged();
            }
        }

        public string FifoStatusText
        {
            get => _fifoStatusText;
            set
            {
                _fifoStatusText = value;

                OnPropertyChanged();
            }
        }

        public string GpuNameText
        {
            get => _gpuStatusText;
            set
            {
                _gpuStatusText = value;

                OnPropertyChanged();
            }
        }

        public string BackendText
        {
            get => _backendText;
            set
            {
                _backendText = value;

                OnPropertyChanged();
            }
        }

        public string DockedStatusText
        {
            get => _dockedStatusText;
            set
            {
                _dockedStatusText = value;

                OnPropertyChanged();
            }
        }

        public string AspectRatioStatusText
        {
            get => _aspectStatusText;
            set
            {
                _aspectStatusText = value;

                OnPropertyChanged();
            }
        }

        public string VolumeStatusText
        {
            get => _volumeStatusText;
            set
            {
                _volumeStatusText = value;

                OnPropertyChanged();
            }
        }

        public bool VolumeMuted => _volume == 0;

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;

                if (_isGameRunning)
                {
                    _owner.AppHost.Device.SetVolume(_volume);
                }

                OnPropertyChanged(nameof(VolumeStatusText));
                OnPropertyChanged(nameof(VolumeMuted));
                OnPropertyChanged();
            }
        }

        public bool ShowStatusSeparator
        {
            get => _showStatusSeparator;
            set
            {
                _showStatusSeparator = value;

                OnPropertyChanged();
            }
        }

        public bool ShowMenuAndStatusBar
        {
            get => _showMenuAndStatusBar;
            set
            {
                _showMenuAndStatusBar = value;

                OnPropertyChanged();
            }
        }

        public bool IsLoadingIndeterminate
        {
            get => _isLoadingIndeterminate;
            set
            {
                _isLoadingIndeterminate = value;

                OnPropertyChanged();
            }
        }

        public bool ShowContent
        {
            get => _showContent;
            set
            {
                _showContent = value;

                OnPropertyChanged();
            }
        }

        public bool IsAppletMenuActive
        {
            get => _isAppletMenuActive && EnableNonGameRunningControls;
            set
            {
                _isAppletMenuActive = value;

                OnPropertyChanged();
            }
        }

        public bool IsGrid => Glyph == Glyph.Grid;
        public bool IsList => Glyph == Glyph.List;

        internal void Sort(bool isAscending)
        {
            IsAscending = isAscending;

            RefreshView();
        }

        internal void Sort(ApplicationSort sort)
        {
            SortMode = sort;

            RefreshView();
        }

        private IComparer<ApplicationData> GetComparer()
        {
            return SortMode switch
            {
                ApplicationSort.LastPlayed      => new Models.Generic.LastPlayedSortComparer(IsAscending),
                ApplicationSort.FileSize        => IsAscending  ? SortExpressionComparer<ApplicationData>.Ascending(app  => app.FileSizeBytes)
                                                                : SortExpressionComparer<ApplicationData>.Descending(app => app.FileSizeBytes),
                ApplicationSort.TotalTimePlayed => IsAscending  ? SortExpressionComparer<ApplicationData>.Ascending(app  => app.TimePlayedNum)
                                                                : SortExpressionComparer<ApplicationData>.Descending(app => app.TimePlayedNum),
                ApplicationSort.Title           => IsAscending  ? SortExpressionComparer<ApplicationData>.Ascending(app  => app.TitleName)
                                                                : SortExpressionComparer<ApplicationData>.Descending(app => app.TitleName),
                ApplicationSort.Favorite        => !IsAscending ? SortExpressionComparer<ApplicationData>.Ascending(app  => app.Favorite)
                                                                : SortExpressionComparer<ApplicationData>.Descending(app => app.Favorite),
                ApplicationSort.Developer       => IsAscending  ? SortExpressionComparer<ApplicationData>.Ascending(app  => app.Developer)
                                                                : SortExpressionComparer<ApplicationData>.Descending(app => app.Developer),
                ApplicationSort.FileType        => IsAscending  ? SortExpressionComparer<ApplicationData>.Ascending(app  => app.FileExtension)
                                                                : SortExpressionComparer<ApplicationData>.Descending(app => app.FileExtension),
                ApplicationSort.Path            => IsAscending  ? SortExpressionComparer<ApplicationData>.Ascending(app  => app.Path)
                                                                : SortExpressionComparer<ApplicationData>.Descending(app => app.Path),
                _                               => null,
            };
        }

        private void RefreshView()
        {
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            Applications.ToObservableChangeSet()
                .Filter(Filter)
                .Sort(GetComparer())
                .Bind(out _appsObservableList).AsObservableList();

            OnPropertyChanged(nameof(AppsObservableList));
        }

        public bool StartGamesInFullscreen
        {
            get => ConfigurationState.Instance.Ui.StartFullscreen;
            set
            {
                ConfigurationState.Instance.Ui.StartFullscreen.Value = value;

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);

                OnPropertyChanged();
            }
        }

        public bool ShowConsole
        {
            get => ConfigurationState.Instance.Ui.ShowConsole;
            set
            {
                ConfigurationState.Instance.Ui.ShowConsole.Value = value;

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);

                OnPropertyChanged();
            }
        }

        public bool ShowConsoleVisible
        {
            get => ConsoleHelper.SetConsoleWindowStateSupported;
        }

        public ObservableCollection<ApplicationData> Applications
        {
            get => _applications;
            set
            {
                _applications = value;

                OnPropertyChanged();
            }
        }

        public Glyph Glyph
        {
            get => (Glyph)ConfigurationState.Instance.Ui.GameListViewMode.Value;
            set
            {
                ConfigurationState.Instance.Ui.GameListViewMode.Value = (int)value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGrid));
                OnPropertyChanged(nameof(IsList));

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);
            }
        }

        public bool ShowNames
        {
            get => ConfigurationState.Instance.Ui.ShowNames && ConfigurationState.Instance.Ui.GridSize > 1; set
            {
                ConfigurationState.Instance.Ui.ShowNames.Value = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(GridSizeScale));

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);
            }
        }

        internal ApplicationSort SortMode
        {
            get => (ApplicationSort)ConfigurationState.Instance.Ui.ApplicationSort.Value;
            private set
            {
                ConfigurationState.Instance.Ui.ApplicationSort.Value = (int)value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(SortName));

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);
            }
        }

        public bool IsSortedByFavorite   => SortMode == ApplicationSort.Favorite;
        public bool IsSortedByTitle      => SortMode == ApplicationSort.Title;
        public bool IsSortedByDeveloper  => SortMode == ApplicationSort.Developer;
        public bool IsSortedByLastPlayed => SortMode == ApplicationSort.LastPlayed;
        public bool IsSortedByTimePlayed => SortMode == ApplicationSort.TotalTimePlayed;
        public bool IsSortedByType       => SortMode == ApplicationSort.FileType;
        public bool IsSortedBySize       => SortMode == ApplicationSort.FileSize;
        public bool IsSortedByPath       => SortMode == ApplicationSort.Path;

        public string SortName
        {
            get
            {
                return SortMode switch
                {
                    ApplicationSort.Title           => LocaleManager.Instance[LocaleKeys.GameListHeaderApplication],
                    ApplicationSort.Developer       => LocaleManager.Instance[LocaleKeys.GameListHeaderDeveloper],
                    ApplicationSort.LastPlayed      => LocaleManager.Instance[LocaleKeys.GameListHeaderLastPlayed],
                    ApplicationSort.TotalTimePlayed => LocaleManager.Instance[LocaleKeys.GameListHeaderTimePlayed],
                    ApplicationSort.FileType        => LocaleManager.Instance[LocaleKeys.GameListHeaderFileExtension],
                    ApplicationSort.FileSize        => LocaleManager.Instance[LocaleKeys.GameListHeaderFileSize],
                    ApplicationSort.Path            => LocaleManager.Instance[LocaleKeys.GameListHeaderPath],
                    ApplicationSort.Favorite        => LocaleManager.Instance[LocaleKeys.CommonFavorite],
                    _                               => string.Empty,
                };
            }
        }

        public bool IsAscending
        {
            get => ConfigurationState.Instance.Ui.IsAscendingOrder;
            private set
            {
                ConfigurationState.Instance.Ui.IsAscendingOrder.Value = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(SortMode));
                OnPropertyChanged(nameof(SortName));

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);
            }
        }

        public KeyGesture ShowUiKey
        {
            get => KeyGesture.Parse(_showUikey); set
            {
                _showUikey = value.ToString();

                OnPropertyChanged();
            }
        }

        public KeyGesture ScreenshotKey
        {
            get => KeyGesture.Parse(_screenshotkey); set
            {
                _screenshotkey = value.ToString();

                OnPropertyChanged();
            }
        }

        public KeyGesture PauseKey
        {
            get => KeyGesture.Parse(_pauseKey); set
            {
                _pauseKey = value.ToString();

                OnPropertyChanged();
            }
        }

        public bool IsGridSmall  => ConfigurationState.Instance.Ui.GridSize == 1;
        public bool IsGridMedium => ConfigurationState.Instance.Ui.GridSize == 2;
        public bool IsGridLarge  => ConfigurationState.Instance.Ui.GridSize == 3;
        public bool IsGridHuge   => ConfigurationState.Instance.Ui.GridSize == 4;

        public int GridSizeScale
        {
            get => ConfigurationState.Instance.Ui.GridSize;
            set
            {
                ConfigurationState.Instance.Ui.GridSize.Value = value;

                if (value < 2)
                {
                    ShowNames = false;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsGridSmall));
                OnPropertyChanged(nameof(IsGridMedium));
                OnPropertyChanged(nameof(IsGridLarge));
                OnPropertyChanged(nameof(IsGridHuge));
                OnPropertyChanged(nameof(ShowNames));

                ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);
            }
        }

        public async void OpenAmiiboWindow()
        {
            if (!_isAmiiboRequested)
            {
                return;
            }

            if (_owner.AppHost.Device.System.SearchingForAmiibo(out int deviceId))
            {
                string      titleId = _owner.AppHost.Device.Application.TitleIdText.ToUpper();
                AmiiboWindow window = new(_showAll, _lastScannedAmiiboId, titleId);

                await window.ShowDialog(_owner);

                if (window.IsScanned)
                {
                    _showAll             = window.ViewModel.ShowAllAmiibo;
                    _lastScannedAmiiboId = window.ScannedAmiibo.GetId();

                    _owner.AppHost.Device.System.ScanAmiibo(deviceId, _lastScannedAmiiboId, window.ViewModel.UseRandomUuid);
                }
            }
        }

        public void SetUiProgressHandlers(Switch emulationContext)
        {
            if (emulationContext.Application.DiskCacheLoadState != null)
            {
                emulationContext.Application.DiskCacheLoadState.StateChanged -= ProgressHandler;
                emulationContext.Application.DiskCacheLoadState.StateChanged += ProgressHandler;
            }

            emulationContext.Gpu.ShaderCacheStateChanged -= ProgressHandler;
            emulationContext.Gpu.ShaderCacheStateChanged += ProgressHandler;
        }

        private bool Filter(object arg)
        {
            if (arg is ApplicationData app)
            {
                return string.IsNullOrWhiteSpace(_searchText) || app.TitleName.ToLower().Contains(_searchText.ToLower());
            }

            return false;
        }

        private void ApplicationLibrary_ApplicationAdded(object sender, ApplicationAddedEventArgs e)
        {
            AddApplication(e.AppData);
        }

        private void ApplicationLibrary_ApplicationCountUpdated(object sender, ApplicationCountUpdatedEventArgs e)
        {
            StatusBarProgressValue   = e.NumAppsLoaded;
            StatusBarProgressMaximum = e.NumAppsFound;

            LocaleManager.Instance.UpdateDynamicValue(LocaleKeys.StatusBarGamesLoaded, StatusBarProgressValue, StatusBarProgressMaximum);

            Dispatcher.UIThread.Post(() =>
            {
                if (e.NumAppsFound == 0)
                {
                    _owner.LoadProgressBar.IsVisible = false;
                }

                if (e.NumAppsLoaded == e.NumAppsFound)
                {
                    _owner.LoadProgressBar.IsVisible = false;
                }
            });
        }

        public void AddApplication(ApplicationData applicationData)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Applications.Add(applicationData);
            });
        }

        public async void LoadApplications()
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Applications.Clear();

                _owner.LoadProgressBar.IsVisible = true;
                StatusBarProgressMaximum         = 0;
                StatusBarProgressValue           = 0;

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

            Thread thread = new(() =>
            {
                ApplicationLibrary.LoadApplications(ConfigurationState.Instance.Ui.GameDirs.Value, ConfigurationState.Instance.System.Language);

                _isLoading = false;
            })
            { Name = "GUI.AppListLoadThread", Priority = ThreadPriority.AboveNormal };

            thread.Start();
        }

        public async void OpenFile()
        {
            OpenFileDialog dialog = new()
            {
                Title = LocaleManager.Instance[LocaleKeys.OpenFileDialogTitle]
            };

            dialog.Filters.Add(new FileDialogFilter
            {
                Name = LocaleManager.Instance[LocaleKeys.AllSupportedFormats],
                Extensions =
                {
                    "nsp",
                    "pfs0",
                    "xci",
                    "nca",
                    "nro",
                    "nso"
                }
            });

            dialog.Filters.Add(new FileDialogFilter { Name = "NSP",  Extensions = { "nsp" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "PFS0", Extensions = { "pfs0" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "XCI",  Extensions = { "xci" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "NCA",  Extensions = { "nca" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "NRO",  Extensions = { "nro" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "NSO",  Extensions = { "nso" } });

            string[] files = await dialog.ShowAsync(_owner);

            if (files != null && files.Length > 0)
            {
                _owner.LoadApplication(files[0]);
            }
        }

        public async void OpenFolder()
        {
            OpenFolderDialog dialog = new()
            {
                Title = LocaleManager.Instance[LocaleKeys.OpenFolderDialogTitle]
            };

            string folder = await dialog.ShowAsync(_owner);

            if (!string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder))
            {
                _owner.LoadApplication(folder);
            }
        }

        public void LoadConfigurableHotKeys()
        {
            if (AvaloniaKeyboardMappingHelper.TryGetAvaKey((Ryujinx.Input.Key)ConfigurationState.Instance.Hid.Hotkeys.Value.ShowUi, out var showUiKey))
            {
                ShowUiKey = new KeyGesture(showUiKey, KeyModifiers.None);
            }

            if (AvaloniaKeyboardMappingHelper.TryGetAvaKey((Ryujinx.Input.Key)ConfigurationState.Instance.Hid.Hotkeys.Value.Screenshot, out var screenshotKey))
            {
                ScreenshotKey = new KeyGesture(screenshotKey, KeyModifiers.None);
            }

            if (AvaloniaKeyboardMappingHelper.TryGetAvaKey((Ryujinx.Input.Key)ConfigurationState.Instance.Hid.Hotkeys.Value.Pause, out var pauseKey))
            {
                PauseKey = new KeyGesture(pauseKey, KeyModifiers.None);
            }
        }

        public void TakeScreenshot()
        {
            _owner.AppHost.ScreenshotRequested = true;
        }

        public void HideUi()
        {
            ShowMenuAndStatusBar = false;
        }

        public void SetListMode()
        {
            Glyph = Glyph.List;
        }

        public void SetGridMode()
        {
            Glyph = Glyph.Grid;
        }

        public void OpenMiiApplet()
        {
            string contentPath = _owner.ContentManager.GetInstalledContentPath(0x0100000000001009, StorageId.BuiltInSystem, NcaContentType.Program);

            if (!string.IsNullOrWhiteSpace(contentPath))
            {
                _owner.LoadApplication(contentPath, false, "Mii Applet");
            }
        }

        public static void OpenRyujinxFolder()
        {
            OpenHelper.OpenFolder(AppDataManager.BaseDirPath);
        }

        public static void OpenLogsFolder()
        {
            string logPath = Path.Combine(ReleaseInformations.GetBaseApplicationDirectory(), "Logs");

            new DirectoryInfo(logPath).Create();

            OpenHelper.OpenFolder(logPath);
        }

        public void ToggleFullscreen()
        {
            if (Environment.TickCount64 - _lastFullscreenToggle < HotKeyPressDelayMs)
            {
                return;
            }

            _lastFullscreenToggle = Environment.TickCount64;

            if (_owner.WindowState == WindowState.FullScreen)
            {
                _owner.WindowState = WindowState.Normal;

                if (IsGameRunning)
                {
                    ShowMenuAndStatusBar = true;
                }
            }
            else
            {
                _owner.WindowState = WindowState.FullScreen;

                if (IsGameRunning)
                {
                    ShowMenuAndStatusBar = false;
                }
            }

            OnPropertyChanged(nameof(IsFullScreen));
        }

        public bool IsFullScreen => _owner.WindowState == WindowState.FullScreen;

        public void ToggleDockMode()
        {
            if (IsGameRunning)
            {
                ConfigurationState.Instance.System.EnableDockedMode.Value = !ConfigurationState.Instance.System.EnableDockedMode.Value;
            }
        }

        public async void ExitCurrentState()
        {
            if (_owner.WindowState == WindowState.FullScreen)
            {
                ToggleFullscreen();
            }
            else if (IsGameRunning)
            {
                await Task.Delay(100);

                _owner.AppHost?.ShowExitPrompt();
            }
        }

        public async void OpenSettings()
        {
            _owner.SettingsWindow = new(_owner.VirtualFileSystem, _owner.ContentManager);

            await _owner.SettingsWindow.ShowDialog(_owner);

            LoadConfigurableHotKeys();
        }

        public async void ManageProfiles()
        {
            await NavigationDialogHost.Show(_owner.AccountManager, _owner.ContentManager, _owner.VirtualFileSystem, _owner.LibHacHorizonManager.RyujinxClient);
        }

        public async void OpenAboutWindow()
        {
            await new AboutWindow().ShowDialog(_owner);
        }

        public void ChangeLanguage(object obj)
        {
            LocaleManager.Instance.LoadDefaultLanguage();
            LocaleManager.Instance.LoadLanguage((string)obj);
        }

        private void ProgressHandler<T>(T state, int current, int total) where T : Enum
        {
            try
            {
                ProgressMaximum = total;
                ProgressValue   = current;

                switch (state)
                {
                    case LoadState ptcState:
                        CacheLoadStatus = $"{current} / {total}";
                        switch (ptcState)
                        {
                            case LoadState.Unloaded:
                            case LoadState.Loading:
                                LoadHeading            = LocaleManager.Instance[LocaleKeys.CompilingPPTC];
                                IsLoadingIndeterminate = false;
                                break;
                            case LoadState.Loaded:
                                LoadHeading            = string.Format(LocaleManager.Instance[LocaleKeys.LoadingHeading], TitleName);
                                IsLoadingIndeterminate = true;
                                CacheLoadStatus        = "";
                                break;
                        }
                        break;
                    case ShaderCacheLoadingState shaderCacheState:
                        CacheLoadStatus = $"{current} / {total}";
                        switch (shaderCacheState)
                        {
                            case ShaderCacheLoadingState.Start:
                            case ShaderCacheLoadingState.Loading:
                                LoadHeading            = LocaleManager.Instance[LocaleKeys.CompilingShaders];
                                IsLoadingIndeterminate = false;
                                break;
                            case ShaderCacheLoadingState.Loaded:
                                LoadHeading            = string.Format(LocaleManager.Instance[LocaleKeys.LoadingHeading], TitleName);
                                IsLoadingIndeterminate = true;
                                CacheLoadStatus        = "";
                                break;
                        }
                        break;
                    default:
                        throw new ArgumentException($"Unknown Progress Handler type {typeof(T)}");
                }
            }
            catch (Exception) { }
        }

        public void OpenUserSaveDirectory()
        {
            ApplicationData selection = SelectedApplication;
            if (selection != null)
            {
                Task.Run(() =>
                {
                    if (!ulong.TryParse(selection.TitleId, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong titleIdNumber))
                    {
                        Dispatcher.UIThread.Post(async () =>
                        {
                            await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance[LocaleKeys.DialogRyujinxErrorMessage], LocaleManager.Instance[LocaleKeys.DialogInvalidTitleIdErrorMessage]);
                        });

                        return;
                    }

                    UserId         userId         = new((ulong)_owner.AccountManager.LastOpenedUser.UserId.High, (ulong)_owner.AccountManager.LastOpenedUser.UserId.Low);
                    SaveDataFilter saveDataFilter = SaveDataFilter.Make(titleIdNumber, saveType: default, userId, saveDataId: default, index: default);
                    OpenSaveDirectory(in saveDataFilter, selection, titleIdNumber);
                });
            }
        }

        public void ToggleFavorite()
        {
            ApplicationData selection = SelectedApplication;
            if (selection != null)
            {
                selection.Favorite = !selection.Favorite;

                ApplicationLibrary.LoadAndSaveMetaData(selection.TitleId, appMetadata =>
                {
                    appMetadata.Favorite = selection.Favorite;
                });

                RefreshView();
            }
        }

        public void OpenModsDirectory()
        {
            ApplicationData selection = SelectedApplication;
            if (selection != null)
            {
                string modsBasePath  = _owner.VirtualFileSystem.ModLoader.GetModsBasePath();
                string titleModsPath = _owner.VirtualFileSystem.ModLoader.GetTitleDir(modsBasePath, selection.TitleId);

                OpenHelper.OpenFolder(titleModsPath);
            }
        }

        public void OpenSdModsDirectory()
        {
            ApplicationData selection = SelectedApplication;

            if (selection != null)
            {
                string sdModsBasePath = _owner.VirtualFileSystem.ModLoader.GetSdModsBasePath();
                string titleModsPath  = _owner.VirtualFileSystem.ModLoader.GetTitleDir(sdModsBasePath, selection.TitleId);

                OpenHelper.OpenFolder(titleModsPath);
            }
        }

        public void OpenPtcDirectory()
        {
            ApplicationData selection = SelectedApplication;
            if (selection != null)
            {
                string ptcDir     = Path.Combine(AppDataManager.GamesDirPath, selection.TitleId, "cache", "cpu");
                string mainPath   = Path.Combine(ptcDir, "0");
                string backupPath = Path.Combine(ptcDir, "1");

                if (!Directory.Exists(ptcDir))
                {
                    Directory.CreateDirectory(ptcDir);
                    Directory.CreateDirectory(mainPath);
                    Directory.CreateDirectory(backupPath);
                }

                OpenHelper.OpenFolder(ptcDir);
            }
        }

        public async void PurgePtcCache()
        {
            ApplicationData selection = SelectedApplication;
            if (selection != null)
            {
                DirectoryInfo mainDir   = new(Path.Combine(AppDataManager.GamesDirPath, selection.TitleId, "cache", "cpu", "0"));
                DirectoryInfo backupDir = new(Path.Combine(AppDataManager.GamesDirPath, selection.TitleId, "cache", "cpu", "1"));

                // FIXME: Found a way to reproduce the bold effect on the title name (fork?).
                UserResult result = await ContentDialogHelper.CreateConfirmationDialog(LocaleManager.Instance[LocaleKeys.DialogWarning],
                                                                                       string.Format(LocaleManager.Instance[LocaleKeys.DialogPPTCDeletionMessage], selection.TitleName),
                                                                                       LocaleManager.Instance[LocaleKeys.InputDialogYes],
                                                                                       LocaleManager.Instance[LocaleKeys.InputDialogNo],
                                                                                       LocaleManager.Instance[LocaleKeys.RyujinxConfirm]);

                List<FileInfo> cacheFiles = new();

                if (mainDir.Exists)
                {
                    cacheFiles.AddRange(mainDir.EnumerateFiles("*.cache"));
                }

                if (backupDir.Exists)
                {
                    cacheFiles.AddRange(backupDir.EnumerateFiles("*.cache"));
                }

                if (cacheFiles.Count > 0 && result == UserResult.Yes)
                {
                    foreach (FileInfo file in cacheFiles)
                    {
                        try
                        {
                            file.Delete();
                        }
                        catch (Exception e)
                        {
                            await ContentDialogHelper.CreateErrorDialog(string.Format(LocaleManager.Instance[LocaleKeys.DialogPPTCDeletionErrorMessage], file.Name, e));
                        }
                    }
                }
            }
        }

        public void OpenShaderCacheDirectory()
        {
            ApplicationData selection = SelectedApplication;
            if (selection != null)
            {
                string shaderCacheDir = Path.Combine(AppDataManager.GamesDirPath, selection.TitleId, "cache", "shader");

                if (!Directory.Exists(shaderCacheDir))
                {
                    Directory.CreateDirectory(shaderCacheDir);
                }

                OpenHelper.OpenFolder(shaderCacheDir);
            }
        }

        public void SimulateWakeUpMessage()
        {
            _owner.AppHost.Device.System.SimulateWakeUpMessage();
        }

        public async void PurgeShaderCache()
        {
            ApplicationData selection = SelectedApplication;
            if (selection != null)
            {
                DirectoryInfo shaderCacheDir = new(Path.Combine(AppDataManager.GamesDirPath, selection.TitleId, "cache", "shader"));

                // FIXME: Found a way to reproduce the bold effect on the title name (fork?).
                UserResult result = await ContentDialogHelper.CreateConfirmationDialog(LocaleManager.Instance[LocaleKeys.DialogWarning],
                                                                                       string.Format(LocaleManager.Instance[LocaleKeys.DialogShaderDeletionMessage], selection.TitleName),
                                                                                       LocaleManager.Instance[LocaleKeys.InputDialogYes],
                                                                                       LocaleManager.Instance[LocaleKeys.InputDialogNo],
                                                                                       LocaleManager.Instance[LocaleKeys.RyujinxConfirm]);

                List<DirectoryInfo> oldCacheDirectories = new();
                List<FileInfo>      newCacheFiles       = new();

                if (shaderCacheDir.Exists)
                {
                    oldCacheDirectories.AddRange(shaderCacheDir.EnumerateDirectories("*"));
                    newCacheFiles.AddRange(shaderCacheDir.GetFiles("*.toc"));
                    newCacheFiles.AddRange(shaderCacheDir.GetFiles("*.data"));
                }

                if ((oldCacheDirectories.Count > 0 || newCacheFiles.Count > 0) && result == UserResult.Yes)
                {
                    foreach (DirectoryInfo directory in oldCacheDirectories)
                    {
                        try
                        {
                            directory.Delete(true);
                        }
                        catch (Exception e)
                        {
                            await ContentDialogHelper.CreateErrorDialog(string.Format(LocaleManager.Instance[LocaleKeys.DialogPPTCDeletionErrorMessage], directory.Name, e));
                        }
                    }
                }

                foreach (FileInfo file in newCacheFiles)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception e)
                    {
                        await ContentDialogHelper.CreateErrorDialog(string.Format(LocaleManager.Instance[LocaleKeys.ShaderCachePurgeError], file.Name, e));
                    }
                }
            }
        }

        public async void CheckForUpdates()
        {
            if (Updater.CanUpdate(true, _owner))
            {
                await Updater.BeginParse(_owner, true);
            }
        }

        public async void OpenTitleUpdateManager()
        {
            ApplicationData selection = SelectedApplication;
            if (selection != null)
            {
                await new TitleUpdateWindow(_owner.VirtualFileSystem, ulong.Parse(selection.TitleId, NumberStyles.HexNumber), selection.TitleName).ShowDialog(_owner);
            }
        }

        public async void OpenDownloadableContentManager()
        {
            ApplicationData selection = SelectedApplication;
            if (selection != null)
            {
                await new DownloadableContentManagerWindow(_owner.VirtualFileSystem, ulong.Parse(selection.TitleId, NumberStyles.HexNumber), selection.TitleName).ShowDialog(_owner);
            }
        }

        public async void OpenCheatManager()
        {
            ApplicationData selection = SelectedApplication;
            if (selection != null)
            {
                await new CheatWindow(_owner.VirtualFileSystem, selection.TitleId, selection.TitleName).ShowDialog(_owner);
            }
        }

        public async void OpenCheatManagerForCurrentApp()
        {
            if (!IsGameRunning)
            {
                return;
            }

            ApplicationLoader application = _owner.AppHost.Device.Application;
            if (application != null)
            {
                await new CheatWindow(_owner.VirtualFileSystem, application.TitleIdText, application.TitleName).ShowDialog(_owner);

                _owner.AppHost.Device.EnableCheats();
            }
        }

        public void OpenDeviceSaveDirectory()
        {
            ApplicationData selection = SelectedApplication;
            if (selection != null)
            {
                Task.Run(() =>
                {
                    if (!ulong.TryParse(selection.TitleId, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong titleIdNumber))
                    {
                        Dispatcher.UIThread.Post(async () =>
                        {
                            await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance[LocaleKeys.DialogRyujinxErrorMessage], LocaleManager.Instance[LocaleKeys.DialogInvalidTitleIdErrorMessage]);
                        });

                        return;
                    }

                    var saveDataFilter = SaveDataFilter.Make(titleIdNumber, SaveDataType.Device, userId: default, saveDataId: default, index: default);
                    OpenSaveDirectory(in saveDataFilter, selection, titleIdNumber);
                });
            }
        }

        public void OpenBcatSaveDirectory()
        {
            ApplicationData selection = SelectedApplication;
            if (selection != null)
            {
                Task.Run(() =>
                {
                    if (!ulong.TryParse(selection.TitleId, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ulong titleIdNumber))
                    {
                        Dispatcher.UIThread.Post(async () =>
                        {
                            await ContentDialogHelper.CreateErrorDialog(LocaleManager.Instance[LocaleKeys.DialogRyujinxErrorMessage], LocaleManager.Instance[LocaleKeys.DialogInvalidTitleIdErrorMessage]);
                        });

                        return;
                    }

                    var saveDataFilter = SaveDataFilter.Make(titleIdNumber, SaveDataType.Bcat, userId: default, saveDataId: default, index: default);
                    OpenSaveDirectory(in saveDataFilter, selection, titleIdNumber);
                });
            }
        }

        private void OpenSaveDirectory(in SaveDataFilter filter, ApplicationData data, ulong titleId)
        {
            ApplicationHelper.OpenSaveDir(in filter, titleId, data.ControlHolder, data.TitleName);
        }

        private async void ExtractLogo()
        {
            var selection = SelectedApplication;
            if (selection != null)
            {
                await ApplicationHelper.ExtractSection(NcaSectionType.Logo, selection.Path);
            }
        }

        private async void ExtractRomFs()
        {
            var selection = SelectedApplication;
            if (selection != null)
            {
                await ApplicationHelper.ExtractSection(NcaSectionType.Data, selection.Path);
            }
        }

        private async void ExtractExeFs()
        {
            var selection = SelectedApplication;
            if (selection != null)
            {
                await ApplicationHelper.ExtractSection(NcaSectionType.Code, selection.Path);
            }
        }

        public void CloseWindow()
        {
            _owner.Close();
        }

        private async Task HandleFirmwareInstallation(string filename)
        {
            try
            {
                SystemVersion firmwareVersion = _owner.ContentManager.VerifyFirmwarePackage(filename);

                if (firmwareVersion == null)
                {
                    await ContentDialogHelper.CreateErrorDialog(string.Format(LocaleManager.Instance[LocaleKeys.DialogFirmwareInstallerFirmwareNotFoundErrorMessage], filename));

                    return;
                }

                string dialogTitle = string.Format(LocaleManager.Instance[LocaleKeys.DialogFirmwareInstallerFirmwareInstallTitle], firmwareVersion.VersionString);

                SystemVersion currentVersion = _owner.ContentManager.GetCurrentFirmwareVersion();

                string dialogMessage = string.Format(LocaleManager.Instance[LocaleKeys.DialogFirmwareInstallerFirmwareInstallMessage], firmwareVersion.VersionString);

                if (currentVersion != null)
                {
                    dialogMessage += string.Format(LocaleManager.Instance[LocaleKeys.DialogFirmwareInstallerFirmwareInstallSubMessage], currentVersion.VersionString);
                }

                dialogMessage += LocaleManager.Instance[LocaleKeys.DialogFirmwareInstallerFirmwareInstallConfirmMessage];

                UserResult result = await ContentDialogHelper.CreateConfirmationDialog(
                    dialogTitle,
                    dialogMessage,
                    LocaleManager.Instance[LocaleKeys.InputDialogYes],
                    LocaleManager.Instance[LocaleKeys.InputDialogNo],
                    LocaleManager.Instance[LocaleKeys.RyujinxConfirm]);

                UpdateWaitWindow waitingDialog = ContentDialogHelper.CreateWaitingDialog(dialogTitle, LocaleManager.Instance[LocaleKeys.DialogFirmwareInstallerFirmwareInstallWaitMessage]);

                if (result == UserResult.Yes)
                {
                    Logger.Info?.Print(LogClass.Application, $"Installing firmware {firmwareVersion.VersionString}");

                    Thread thread = new(() =>
                    {
                        Dispatcher.UIThread.InvokeAsync(delegate
                        {
                            waitingDialog.Show();
                        });

                        try
                        {
                            _owner.ContentManager.InstallFirmware(filename);

                            Dispatcher.UIThread.InvokeAsync(async delegate
                            {
                                waitingDialog.Close();

                                string message = string.Format(LocaleManager.Instance[LocaleKeys.DialogFirmwareInstallerFirmwareInstallSuccessMessage], firmwareVersion.VersionString);

                                await ContentDialogHelper.CreateInfoDialog(dialogTitle, message, LocaleManager.Instance[LocaleKeys.InputDialogOk], "", LocaleManager.Instance[LocaleKeys.RyujinxInfo]);

                                Logger.Info?.Print(LogClass.Application, message);

                                // Purge Applet Cache.

                                DirectoryInfo miiEditorCacheFolder = new DirectoryInfo(Path.Combine(AppDataManager.GamesDirPath, "0100000000001009", "cache"));

                                if (miiEditorCacheFolder.Exists)
                                {
                                    miiEditorCacheFolder.Delete(true);
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                waitingDialog.Close();

                                await ContentDialogHelper.CreateErrorDialog(ex.Message);
                            });
                        }
                        finally
                        {
                            _owner.RefreshFirmwareStatus();
                        }
                    });

                    thread.Name = "GUI.FirmwareInstallerThread";
                    thread.Start();
                }
            }
            catch (LibHac.Common.Keys.MissingKeyException ex)
            {
                Logger.Error?.Print(LogClass.Application, ex.ToString());

                Dispatcher.UIThread.Post(async () => await UserErrorDialog.ShowUserErrorDialog(UserError.NoKeys, _owner));
            }
            catch (Exception ex)
            {
                await ContentDialogHelper.CreateErrorDialog(ex.Message);
            }
        }

        public async void InstallFirmwareFromFile()
        {
            OpenFileDialog dialog = new() { AllowMultiple = false };
            dialog.Filters.Add(new FileDialogFilter { Name = LocaleManager.Instance[LocaleKeys.FileDialogAllTypes], Extensions = { "xci", "zip" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "XCI",                                        Extensions = { "xci" } });
            dialog.Filters.Add(new FileDialogFilter { Name = "ZIP",                                        Extensions = { "zip" } });

            string[] file = await dialog.ShowAsync(_owner);

            if (file != null && file.Length > 0)
            {
                await HandleFirmwareInstallation(file[0]);
            }
        }

        public async void InstallFirmwareFromFolder()
        {
            OpenFolderDialog dialog = new();

            string folder = await dialog.ShowAsync(_owner);

            if (!string.IsNullOrWhiteSpace(folder))
            {
                await HandleFirmwareInstallation(folder);
            }
        }
    }
}