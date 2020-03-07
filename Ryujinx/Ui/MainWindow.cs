using Gtk;
using JsonPrettyPrinterPlus;
using Ryujinx.Audio;
using Ryujinx.Common.Logging;
using Ryujinx.Configuration;
using Ryujinx.Debugger.Profiler;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.OpenGL;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.FileSystem.Content;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utf8Json;
using Utf8Json.Resolvers;

using GUI = Gtk.Builder.ObjectAttribute;

namespace Ryujinx.Ui
{
    public class MainWindow : Window
    {
        private static VirtualFileSystem _virtualFileSystem;
        private static ContentManager    _contentManager;

        private static HLE.Switch _emulationContext;

        private static GLRenderer _gLWidget;

        private static AutoResetEvent _deviceExitStatus = new AutoResetEvent(false);

        private static ListStore _tableStore;

        private static bool _updatingGameTable;
        private static bool _gameLoaded;
        private static bool _ending;
        private static bool _debuggerOpened;

        private static TreeView _treeView;

        private static Ryujinx.Debugger.Debugger _debugger;

#pragma warning disable CS0649
#pragma warning disable IDE0044

        [GUI] Window         _mainWin;
        [GUI] MenuBar        _menuBar;
        [GUI] Box            _footerBox;
        [GUI] MenuItem       _fullScreen;
        [GUI] Box            _statusBar;
        [GUI] MenuItem       _stopEmulation;
        [GUI] CheckMenuItem  _favToggle;
        [GUI] MenuItem       _firmwareInstallFile;
        [GUI] MenuItem       _firmwareInstallDirectory;
        [GUI] Label          _hostStatus;
        [GUI] MenuItem       _openDebugger;
        [GUI] CheckMenuItem  _iconToggle;
        [GUI] CheckMenuItem  _appToggle;
        [GUI] CheckMenuItem  _developerToggle;
        [GUI] CheckMenuItem  _versionToggle;
        [GUI] CheckMenuItem  _timePlayedToggle;
        [GUI] CheckMenuItem  _lastPlayedToggle;
        [GUI] CheckMenuItem  _fileExtToggle;
        [GUI] CheckMenuItem  _fileSizeToggle;
        [GUI] CheckMenuItem  _pathToggle;
        [GUI] Label          _gameStatus;
        [GUI] TreeView       _gameTable;
        [GUI] ScrolledWindow _gameTableWindow;
        [GUI] TreeSelection  _gameTableSelection;
        [GUI] Label          _progressLabel;
        [GUI] Label          _firmwareVersionLabel;
        [GUI] LevelBar       _progressBar;
        [GUI] Box            _viewBox;
        [GUI] Label          _vSyncStatus;
        [GUI] Box            _listStatusBox;

#pragma warning restore CS0649
#pragma warning restore IDE0044

        public MainWindow() : this(new Builder("Ryujinx.Ui.MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetObject("_mainWin").Handle)
        {
            builder.Autoconnect(this);

            this.DeleteEvent      += Window_Close;
            _fullScreen.Activated += FullScreen_Toggled;

            ApplicationLibrary.ApplicationAdded        += Application_Added;
            ApplicationLibrary.ApplicationCountUpdated += ApplicationCount_Updated;
            GLRenderer.StatusUpdatedEvent              += Update_StatusBar;

            _gameTable.ButtonReleaseEvent += Row_Clicked;

            // First we check that a migration isn't needed. (because VirtualFileSystem will create the new directory otherwise)
            bool continueWithStartup = Migration.PromptIfMigrationNeededForStartup(this, out bool migrationNeeded);
            if (!continueWithStartup)
            {
                End(null);
            }

            _virtualFileSystem = VirtualFileSystem.CreateInstance();
            _contentManager    = new ContentManager(_virtualFileSystem);

            if (migrationNeeded)
            {
                bool migrationSuccessful = Migration.DoMigrationForStartup(this, _virtualFileSystem);

                if (!migrationSuccessful)
                {
                    End(null);
                }
            }

            // Make sure that everything is loaded.
            _virtualFileSystem.Reload();

            _treeView = _gameTable;

            ApplyTheme();

            _mainWin.Icon            = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png");
            _mainWin.Title           = $"Ryujinx {Program.Version}";
            _stopEmulation.Sensitive = false;

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

#if USE_DEBUGGING
            _debugger = new Debugger.Debugger();
            _openDebugger.Activated += _openDebugger_Opened;
#else
            _openDebugger.Hide();
#endif

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
                typeof(string));

            _tableStore.SetSortFunc(5, TimePlayedSort);
            _tableStore.SetSortFunc(6, LastPlayedSort);
            _tableStore.SetSortFunc(8, FileSizeSort);
            _tableStore.SetSortColumnId(0, SortType.Descending);

            UpdateColumns();
            UpdateGameTable();

            Task.Run(RefreshFirmwareLabel);

            _statusBar.Hide();
        }

#if USE_DEBUGGING
        private void _openDebugger_Opened(object sender, EventArgs e)
        {
            if (_debuggerOpened)
            {
                return;
            }

            Window debugWindow = new Window("Debugger");
            
            debugWindow.SetSizeRequest(1280, 640);
            debugWindow.Child = _debugger.Widget;
            debugWindow.DeleteEvent += DebugWindow_DeleteEvent;
            debugWindow.ShowAll();

            _debugger.Enable();

            _debuggerOpened = true;
        }

        private void DebugWindow_DeleteEvent(object o, DeleteEventArgs args)
        {
            _debuggerOpened = false;

            _debugger.Disable();

            (_debugger.Widget.Parent as Window)?.Remove(_debugger.Widget);
        }
#endif

        internal static void ApplyTheme()
        {
            if (!ConfigurationState.Instance.Ui.EnableCustomTheme)
            {
                return;
            }

            if (File.Exists(ConfigurationState.Instance.Ui.CustomThemePath) && (System.IO.Path.GetExtension(ConfigurationState.Instance.Ui.CustomThemePath) == ".css"))
            {
                CssProvider cssProvider = new CssProvider();

                cssProvider.LoadFromPath(ConfigurationState.Instance.Ui.CustomThemePath);

                StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, 800);
            }
            else
            {
                Logger.PrintWarning(LogClass.Application, $"The \"custom_theme_path\" section in \"Config.json\" contains an invalid path: \"{ConfigurationState.Instance.Ui.CustomThemePath}\".");
            }
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
                if      (column.Title == "Fav"         && ConfigurationState.Instance.Ui.GuiColumns.FavColumn)        column.SortColumnId = 0;
                else if (column.Title == "Application" && ConfigurationState.Instance.Ui.GuiColumns.AppColumn)        column.SortColumnId = 2;
                else if (column.Title == "Developer"   && ConfigurationState.Instance.Ui.GuiColumns.DevColumn)        column.SortColumnId = 3;
                else if (column.Title == "Version"     && ConfigurationState.Instance.Ui.GuiColumns.VersionColumn)    column.SortColumnId = 4;
                else if (column.Title == "Time Played" && ConfigurationState.Instance.Ui.GuiColumns.TimePlayedColumn) column.SortColumnId = 5;
                else if (column.Title == "Last Played" && ConfigurationState.Instance.Ui.GuiColumns.LastPlayedColumn) column.SortColumnId = 6;
                else if (column.Title == "File Ext"    && ConfigurationState.Instance.Ui.GuiColumns.FileExtColumn)    column.SortColumnId = 7;
                else if (column.Title == "File Size"   && ConfigurationState.Instance.Ui.GuiColumns.FileSizeColumn)   column.SortColumnId = 8;
                else if (column.Title == "Path"        && ConfigurationState.Instance.Ui.GuiColumns.PathColumn)       column.SortColumnId = 9;
            }
        }

        private HLE.Switch InitializeSwitchInstance()
        {
            _virtualFileSystem.Reload();

            HLE.Switch instance = new HLE.Switch(_virtualFileSystem, _contentManager, InitializeRenderer(), InitializeAudioEngine());

            instance.Initialize();

            return instance;
        }

        internal static void UpdateGameTable()
        {
            if (_updatingGameTable)
            {
                return;
            }

            _updatingGameTable = true;

            _tableStore.Clear();

            Thread applicationLibraryThread = new Thread(() =>
            {
                ApplicationLibrary.LoadApplications(ConfigurationState.Instance.Ui.GameDirs,
                    _virtualFileSystem, ConfigurationState.Instance.System.Language);

                _updatingGameTable = false;
            });
            applicationLibraryThread.Name = "GUI.ApplicationLibraryThread";
            applicationLibraryThread.IsBackground = true;
            applicationLibraryThread.Start();
        }

        internal void LoadApplication(string path)
        {
            if (_gameLoaded)
            {
                GtkDialog.CreateDialog("Ryujinx", "A game has already been loaded", "Please close it first and try again.");
            }
            else
            {
                Logger.RestartTime();

                HLE.Switch device = InitializeSwitchInstance();

                // TODO: Move this somewhere else + reloadable?
                Graphics.Gpu.GraphicsConfig.ShadersDumpPath = ConfigurationState.Instance.Graphics.ShadersDumpPath;

                if (Directory.Exists(path))
                {
                    string[] romFsFiles = Directory.GetFiles(path, "*.istorage");

                    if (romFsFiles.Length == 0)
                    {
                        romFsFiles = Directory.GetFiles(path, "*.romfs");
                    }

                    if (romFsFiles.Length > 0)
                    {
                        Logger.PrintInfo(LogClass.Application, "Loading as cart with RomFS.");
                        device.LoadCart(path, romFsFiles[0]);
                    }
                    else
                    {
                        Logger.PrintInfo(LogClass.Application, "Loading as cart WITHOUT RomFS.");
                        device.LoadCart(path);
                    }
                }
                else if (File.Exists(path))
                {
                    switch (System.IO.Path.GetExtension(path).ToLowerInvariant())
                    {
                        case ".xci":
                            Logger.PrintInfo(LogClass.Application, "Loading as XCI.");
                            device.LoadXci(path);
                            break;
                        case ".nca":
                            Logger.PrintInfo(LogClass.Application, "Loading as NCA.");
                            device.LoadNca(path);
                            break;
                        case ".nsp":
                        case ".pfs0":
                            Logger.PrintInfo(LogClass.Application, "Loading as NSP.");
                            device.LoadNsp(path);
                            break;
                        default:
                            Logger.PrintInfo(LogClass.Application, "Loading as homebrew.");
                            try
                            {
                                device.LoadProgram(path);
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                Logger.PrintError(LogClass.Application, "The file which you have specified is unsupported by Ryujinx.");
                            }
                            break;
                    }
                }
                else
                {
                    Logger.PrintWarning(LogClass.Application, "Please specify a valid XCI/NCA/NSP/PFS0/NRO file.");
                    End(device);
                }

                _emulationContext = device;

                _deviceExitStatus.Reset();

#if MACOS_BUILD
                CreateGameWindow(device);
#else
                var windowThread = new Thread(() =>
                {
                    CreateGameWindow(device);
                })
                {
                    Name = "GUI.WindowThread"
                };

                windowThread.Start();
#endif

                _gameLoaded              = true;
                _stopEmulation.Sensitive = true;

                _firmwareInstallFile.Sensitive      = false;
                _firmwareInstallDirectory.Sensitive = false;

                DiscordIntegrationModule.SwitchToPlayingState(device.System.TitleIdText, device.System.TitleName);

                ApplicationLibrary.LoadAndSaveMetaData(device.System.TitleIdText, appMetadata =>
                {
                    appMetadata.LastPlayed = DateTime.UtcNow.ToString();
                });
            }
        }

        private void CreateGameWindow(HLE.Switch device)
        {
            device.Hid.InitializePrimaryController(ConfigurationState.Instance.Hid.ControllerType);

            _gLWidget = new GLRenderer(_emulationContext);

            Application.Invoke(delegate
            {
                _viewBox.Remove(_gameTableWindow);
                _gLWidget.Expand = true;
                _viewBox.Child = _gLWidget;

                _gLWidget.ShowAll();
                EditFooterForGameRender();
            });

            _gLWidget.WaitEvent.WaitOne();

            _gLWidget.Start();

            device.Dispose();
            _deviceExitStatus.Set();

            // NOTE: Everything that is here will not be executed when you close the UI.
            Application.Invoke(delegate
            {
                _viewBox.Remove(_gLWidget);
                _gLWidget.Exit();

                if(_gLWidget.Window != this.Window && _gLWidget.Window != null)
                {
                    _gLWidget.Window.Dispose();
                }

                _gLWidget.Dispose();

                _viewBox.Add(_gameTableWindow);

                _gameTableWindow.Expand = true;

                this.Window.Title = $"Ryujinx {Program.Version}";

                _emulationContext = null;
                _gameLoaded       = false;
                _gLWidget         = null;

                DiscordIntegrationModule.SwitchToMainMenu();

                RecreateFooterForMenu();

                UpdateColumns();
                UpdateGameTable();

                Task.Run(RefreshFirmwareLabel);

                _stopEmulation.Sensitive            = false;
                _firmwareInstallFile.Sensitive      = true;
                _firmwareInstallDirectory.Sensitive = true;
            });
        }

        private void RecreateFooterForMenu()
        {
            _listStatusBox.Show();
            _statusBar.Hide();
        }

        private void EditFooterForGameRender()
        {
            _listStatusBox.Hide();
            _statusBar.Show();
        }

        public void ToggleExtraWidgets(bool show)
        {
            if (_gLWidget != null)
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

            bool fullScreenToggled = this.Window.State.HasFlag(Gdk.WindowState.Fullscreen);

            _fullScreen.Label = fullScreenToggled ? "Exit Fullscreen" : "Enter Fullscreen";
        }

        private static void UpdateGameMetadata(string titleId)
        {
            if (_gameLoaded)
            {
                ApplicationLibrary.LoadAndSaveMetaData(titleId, appMetadata =>
                {
                    DateTime lastPlayedDateTime = DateTime.Parse(appMetadata.LastPlayed);
                    double   sessionTimePlayed  = DateTime.UtcNow.Subtract(lastPlayedDateTime).TotalSeconds;

                    appMetadata.TimePlayed += Math.Round(sessionTimePlayed, MidpointRounding.AwayFromZero);
                });
            }
        }

        private void End(HLE.Switch device)
        {

#if USE_DEBUGGING
            _debugger.Dispose();
#endif

            if (_ending)
            {
                return;
            }

            _ending = true;

            if (device != null)
            {
                UpdateGameMetadata(device.System.TitleIdText);

                if (_gLWidget != null)
                {
                    // We tell the widget that we are exiting
                    _gLWidget.Exit();

                    // Wait for the other thread to dispose the HLE context before exiting.
                    _deviceExitStatus.WaitOne();
                }
            }

            Dispose();

            Profile.FinishProfiling();
            DiscordIntegrationModule.Exit();
            Logger.Shutdown();
            Application.Quit();
        }

        private static IRenderer InitializeRenderer()
        {
            return new Renderer();
        }

        /// <summary>
        /// Picks an <see cref="IAalOutput"/> audio output renderer supported on this machine
        /// </summary>
        /// <returns>An <see cref="IAalOutput"/> supported by this machine</returns>
        private static IAalOutput InitializeAudioEngine()
        {
            if (OpenALAudioOut.IsSupported)
            {
                return new OpenALAudioOut();
            }
            else if (SoundIoAudioOut.IsSupported)
            {
                return new SoundIoAudioOut();
            }
            else
            {
                return new DummyAudioOut();
            }
        }

        //Events
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
                    args.AppData.Path);
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

                _progressBar.Value = barValue;
            });
        }

        private void Update_StatusBar(object sender, StatusUpdatedEventArgs args)
        {
            Application.Invoke(delegate
            {
                _hostStatus.Text = args.HostStatus;
                _gameStatus.Text = args.GameStatus;

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

            string titleId = _tableStore.GetValue(treeIter, 2).ToString().Split("\n")[1].ToLower();

            bool newToggleValue = !(bool)_tableStore.GetValue(treeIter, 0);

            _tableStore.SetValue(treeIter, 0, newToggleValue);

            ApplicationLibrary.LoadAndSaveMetaData(titleId, appMetadata =>
            {
                appMetadata.Favorite = newToggleValue;
            });
        }

        private void Row_Activated(object sender, RowActivatedArgs args)
        {
            _gameTableSelection.GetSelected(out TreeIter treeIter);
            string path = (string)_tableStore.GetValue(treeIter, 9);

            LoadApplication(path);
        }

        private void Row_Clicked(object sender, ButtonReleaseEventArgs args)
        {
            if (args.Event.Button != 3) return;

            _gameTableSelection.GetSelected(out TreeIter treeIter);

            if (treeIter.UserData == IntPtr.Zero) return;

            GameTableContextMenu contextMenu = new GameTableContextMenu(_tableStore, treeIter, _virtualFileSystem);
            contextMenu.ShowAll();
            contextMenu.PopupAtPointer(null);
        }

        private void Load_Application_File(object sender, EventArgs args)
        {
            FileChooserDialog fileChooser = new FileChooserDialog("Choose the file to open", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);

            fileChooser.Filter = new FileFilter();
            fileChooser.Filter.AddPattern("*.nsp" );
            fileChooser.Filter.AddPattern("*.pfs0");
            fileChooser.Filter.AddPattern("*.xci" );
            fileChooser.Filter.AddPattern("*.nca" );
            fileChooser.Filter.AddPattern("*.nro" );
            fileChooser.Filter.AddPattern("*.nso" );

            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                LoadApplication(fileChooser.Filename);
            }

            fileChooser.Dispose();
        }

        private void Load_Application_Folder(object sender, EventArgs args)
        {
            FileChooserDialog fileChooser = new FileChooserDialog("Choose the folder to open", this, FileChooserAction.SelectFolder, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);

            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                LoadApplication(fileChooser.Filename);
            }

            fileChooser.Dispose();
        }

        private void Open_Ryu_Folder(object sender, EventArgs args)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName        = _virtualFileSystem.GetBasePath(),
                UseShellExecute = true,
                Verb            = "open"
            });
        }

        private void Exit_Pressed(object sender, EventArgs args)
        {
            End(_emulationContext);
        }

        private void Window_Close(object sender, DeleteEventArgs args)
        {
            End(_emulationContext);
        }

        private void StopEmulation_Pressed(object sender, EventArgs args)
        {
            _gLWidget?.Exit();
        }

        private void Installer_File_Pressed(object o, EventArgs args)
        {
            FileChooserDialog fileChooser = new FileChooserDialog("Choose the firmware file to open",
                                                                  this,
                                                                  FileChooserAction.Open,
                                                                  "Cancel",
                                                                  ResponseType.Cancel,
                                                                  "Open",
                                                                  ResponseType.Accept);

            fileChooser.Filter = new FileFilter();
            fileChooser.Filter.AddPattern("*.zip");
            fileChooser.Filter.AddPattern("*.xci");

            HandleInstallerDialog(fileChooser);
        }

        private void Installer_Directory_Pressed(object o, EventArgs args)
        {
            FileChooserDialog directoryChooser = new FileChooserDialog("Choose the firmware directory to open",
                                                                       this,
                                                                       FileChooserAction.SelectFolder,
                                                                       "Cancel",
                                                                       ResponseType.Cancel,
                                                                       "Open",
                                                                       ResponseType.Accept);

            HandleInstallerDialog(directoryChooser);
        }

        private void RefreshFirmwareLabel()
        {
            var currentFirmware = _contentManager.GetCurrentFirmwareVersion();

            GLib.Idle.Add(new GLib.IdleHandler(() =>
            {
                _firmwareVersionLabel.Text = currentFirmware != null ? currentFirmware.VersionString : "0.0.0";

                return false;
            }));
        }

        private void HandleInstallerDialog(FileChooserDialog fileChooser)
        {
            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                MessageDialog dialog = null;

                try
                {
                    string filename = fileChooser.Filename;

                    fileChooser.Dispose();

                    var firmwareVersion = _contentManager.VerifyFirmwarePackage(filename);

                    if (firmwareVersion == null)
                    {
                        dialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, false, "");

                        dialog.Text = "Firmware not found.";

                        dialog.SecondaryText = $"A valid system firmware was not found in {filename}.";

                        Logger.PrintError(LogClass.Application, $"A valid system firmware was not found in {filename}.");

                        dialog.Run();
                        dialog.Hide();
                        dialog.Dispose();

                        return;
                    }

                    var currentVersion = _contentManager.GetCurrentFirmwareVersion();

                    string dialogMessage = $"System version {firmwareVersion.VersionString} will be installed.";

                    if (currentVersion != null)
                    {
                        dialogMessage += $"This will replace the current system version {currentVersion.VersionString}. ";
                    }

                    dialogMessage += "Do you want to continue?";

                    dialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo, false, "");

                    dialog.Text = $"Install Firmware {firmwareVersion.VersionString}";
                    dialog.SecondaryText = dialogMessage;

                    int response = dialog.Run();

                    dialog.Dispose();

                    dialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.None, false, "");

                    dialog.Text = $"Install Firmware {firmwareVersion.VersionString}";

                    dialog.SecondaryText = "Installing firmware...";

                    if (response == (int)ResponseType.Yes)
                    {
                        Logger.PrintInfo(LogClass.Application, $"Installing firmware {firmwareVersion.VersionString}");
                        
                        Thread thread = new Thread(() =>
                        {
                            GLib.Idle.Add(new GLib.IdleHandler(() =>
                            {
                                dialog.Run();
                                return false;
                            }));

                            try
                            {
                                _contentManager.InstallFirmware(filename);

                                GLib.Idle.Add(new GLib.IdleHandler(() =>
                                {
                                    dialog.Dispose();

                                    dialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, false, "");

                                    dialog.Text = $"Install Firmware {firmwareVersion.VersionString}";

                                    dialog.SecondaryText = $"System version {firmwareVersion.VersionString} successfully installed.";

                                    Logger.PrintInfo(LogClass.Application, $"System version {firmwareVersion.VersionString} successfully installed.");

                                    dialog.Run();
                                    dialog.Dispose();

                                    return false;
                                }));
                            }
                            catch (Exception ex)
                            {
                                GLib.Idle.Add(new GLib.IdleHandler(() =>
                                {
                                    dialog.Dispose();

                                    dialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, false, "");

                                    dialog.Text = $"Install Firmware {firmwareVersion.VersionString} Failed.";

                                    dialog.SecondaryText = $"An error occured while installing system version {firmwareVersion.VersionString}." +
                                     " Please check logs for more info.";

                                    Logger.PrintError(LogClass.Application, ex.Message);

                                    dialog.Run();
                                    dialog.Dispose();

                                    return false;
                                }));
                            }
                            finally
                            {
                                RefreshFirmwareLabel();
                            }
                        });

                        thread.Name = "GUI.FirmwareInstallerThread";
                        thread.Start();
                    }
                    else
                    {
                        dialog.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    if (dialog != null)
                    {
                        dialog.Dispose();
                    }

                    dialog = new MessageDialog(this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, false, "");

                    dialog.Text = "Parsing Firmware Failed.";

                    dialog.SecondaryText = "An error occured while parsing firmware. Please check the logs for more info.";

                    Logger.PrintError(LogClass.Application, ex.Message);

                    dialog.Run();
                    dialog.Dispose();
                }
            }
            else
            {
                fileChooser.Dispose();
            }
        }

        private void FullScreen_Toggled(object o, EventArgs args)
        {
            bool fullScreenToggled = this.Window.State.HasFlag(Gdk.WindowState.Fullscreen);

            if (!fullScreenToggled)
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

        private void Settings_Pressed(object sender, EventArgs args)
        {
            SwitchSettings settingsWin = new SwitchSettings();
            settingsWin.Show();
        }

        private void Update_Pressed(object sender, EventArgs args)
        {
            string ryuUpdater = System.IO.Path.Combine(_virtualFileSystem.GetBasePath(), "RyuUpdater.exe");

            try
            {
                Process.Start(new ProcessStartInfo(ryuUpdater, "/U") { UseShellExecute = true });
            }
            catch(System.ComponentModel.Win32Exception)
            {
                GtkDialog.CreateErrorDialog("Update canceled by user or updater was not found");
            }
        }

        private void About_Pressed(object sender, EventArgs args)
        {
            AboutWindow aboutWin = new AboutWindow();
            aboutWin.Show();
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

        private void Title_Toggled(object sender, EventArgs args)
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

        private static int TimePlayedSort(ITreeModel model, TreeIter a, TreeIter b)
        {
            string aValue = model.GetValue(a, 5).ToString();
            string bValue = model.GetValue(b, 5).ToString();

            if (aValue.Length > 4 && aValue.Substring(aValue.Length - 4) == "mins")
            {
                aValue = (float.Parse(aValue.Substring(0, aValue.Length - 5)) * 60).ToString();
            }
            else if (aValue.Length > 3 && aValue.Substring(aValue.Length - 3) == "hrs")
            {
                aValue = (float.Parse(aValue.Substring(0, aValue.Length - 4)) * 3600).ToString();
            }
            else if (aValue.Length > 4 && aValue.Substring(aValue.Length - 4) == "days")
            {
                aValue = (float.Parse(aValue.Substring(0, aValue.Length - 5)) * 86400).ToString();
            }
            else
            {
                aValue = aValue.Substring(0, aValue.Length - 1);
            }

            if (bValue.Length > 4 && bValue.Substring(bValue.Length - 4) == "mins")
            {
                bValue = (float.Parse(bValue.Substring(0, bValue.Length - 5)) * 60).ToString();
            }
            else if (bValue.Length > 3 && bValue.Substring(bValue.Length - 3) == "hrs")
            {
                bValue = (float.Parse(bValue.Substring(0, bValue.Length - 4)) * 3600).ToString();
            }
            else if (bValue.Length > 4 && bValue.Substring(bValue.Length - 4) == "days")
            {
                bValue = (float.Parse(bValue.Substring(0, bValue.Length - 5)) * 86400).ToString();
            }
            else
            {
                bValue = bValue.Substring(0, bValue.Length - 1);
            }

            if (float.Parse(aValue) > float.Parse(bValue))
            {
                return -1;
            }
            else if (float.Parse(bValue) > float.Parse(aValue))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private static int LastPlayedSort(ITreeModel model, TreeIter a, TreeIter b)
        {
            string aValue = model.GetValue(a, 6).ToString();
            string bValue = model.GetValue(b, 6).ToString();

            if (aValue == "Never")
            {
                aValue = DateTime.UnixEpoch.ToString();
            }

            if (bValue == "Never")
            {
                bValue = DateTime.UnixEpoch.ToString();
            }

            return DateTime.Compare(DateTime.Parse(bValue), DateTime.Parse(aValue));
        }

        private static int FileSizeSort(ITreeModel model, TreeIter a, TreeIter b)
        {
            string aValue = model.GetValue(a, 8).ToString();
            string bValue = model.GetValue(b, 8).ToString();

            if (aValue.Substring(aValue.Length - 2) == "GB")
            {
                aValue = (float.Parse(aValue[0..^2]) * 1024).ToString();
            }
            else
            {
                aValue = aValue[0..^2];
            }

            if (bValue.Substring(bValue.Length - 2) == "GB")
            {
                bValue = (float.Parse(bValue[0..^2]) * 1024).ToString();
            }
            else
            {
                bValue = bValue[0..^2];
            }

            if (float.Parse(aValue) > float.Parse(bValue))
            {
                return -1;
            }
            else if (float.Parse(bValue) > float.Parse(aValue))
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public static void SaveConfig()
        {
            ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);
        }
    }
}
