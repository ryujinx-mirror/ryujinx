using DiscordRPC;
using Gtk;
using GUI = Gtk.Builder.ObjectAttribute;
using Ryujinx.Audio;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Gal;
using Ryujinx.Graphics.Gal.OpenGL;
using Ryujinx.Profiler;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Ryujinx.UI
{
    public class MainWindow : Window
    {
        internal static HLE.Switch _device;

        private static IGalRenderer _renderer;

        private static IAalOutput _audioOut;

        private static Application _gtkApplication;

        private static ListStore _tableStore;

        private static bool _gameLoaded = false;

        private static string _userId = "00000000000000000000000000000001";

        public static bool DiscordIntegrationEnabled { get; set; }

        public static DiscordRpcClient DiscordClient;

        public static RichPresence DiscordPresence;

#pragma warning disable 649
        [GUI] Window        _mainWin;
        [GUI] CheckMenuItem _fullScreen;
        [GUI] MenuItem      _stopEmulation;
        [GUI] CheckMenuItem _iconToggle;
        [GUI] CheckMenuItem _titleToggle;
        [GUI] CheckMenuItem _developerToggle;
        [GUI] CheckMenuItem _versionToggle;
        [GUI] CheckMenuItem _timePlayedToggle;
        [GUI] CheckMenuItem _lastPlayedToggle;
        [GUI] CheckMenuItem _fileExtToggle;
        [GUI] CheckMenuItem _fileSizeToggle;
        [GUI] CheckMenuItem _pathToggle;
        [GUI] Box           _box;
        [GUI] TreeView      _gameTable;
        [GUI] GLArea        _glScreen;
#pragma warning restore 649

        public MainWindow(string[] args, Application gtkApplication) : this(new Builder("Ryujinx.Ui.MainWindow.glade"), args, gtkApplication) { }

        private MainWindow(Builder builder, string[] args, Application gtkApplication) : base(builder.GetObject("_mainWin").Handle)
        {
            _renderer = new OglRenderer();

            _audioOut = InitializeAudioEngine();

            _device = new HLE.Switch(_renderer, _audioOut);

            Configuration.Load(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));
            Configuration.InitialConfigure(_device);

            ApplicationLibrary.Init(SwitchSettings.SwitchConfig.GameDirs, _device.System.KeySet, _device.System.State.DesiredTitleLanguage);

            _gtkApplication = gtkApplication;

            ApplyTheme();

            if (DiscordIntegrationEnabled)
            {
                DiscordClient   = new DiscordRpcClient("568815339807309834");
                DiscordPresence = new RichPresence
                {
                    Assets = new Assets
                    {
                        LargeImageKey  = "ryujinx",
                        LargeImageText = "Ryujinx is an emulator for the Nintendo Switch"
                    },
                    Details    = "Main Menu",
                    State      = "Idling",
                    Timestamps = new Timestamps(DateTime.UtcNow)
                };

                DiscordClient.Initialize();
                DiscordClient.SetPresence(DiscordPresence);
            }

            builder.Autoconnect(this);

            DeleteEvent += Window_Close;

            _mainWin.Icon            = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.ryujinxIcon.png");
            _stopEmulation.Sensitive = false;

            if (SwitchSettings.SwitchConfig.GuiColumns[0]) { _iconToggle.Active       = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns[1]) { _titleToggle.Active      = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns[2]) { _developerToggle.Active  = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns[3]) { _versionToggle.Active    = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns[4]) { _timePlayedToggle.Active = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns[5]) { _lastPlayedToggle.Active = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns[6]) { _fileExtToggle.Active    = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns[7]) { _fileSizeToggle.Active   = true; }
            if (SwitchSettings.SwitchConfig.GuiColumns[8]) { _pathToggle.Active       = true; }

            if (args.Length == 1)
            {
                // Temporary code section start, remove this section when game is rendered to the GLArea in the GUI
                _box.Remove(_glScreen);

                if (SwitchSettings.SwitchConfig.GuiColumns[0]) { _gameTable.AppendColumn("Icon",        new CellRendererPixbuf(), "pixbuf", 0); }
                if (SwitchSettings.SwitchConfig.GuiColumns[1]) { _gameTable.AppendColumn("Application", new CellRendererText(),   "text",   1); }
                if (SwitchSettings.SwitchConfig.GuiColumns[2]) { _gameTable.AppendColumn("Developer",   new CellRendererText(),   "text",   2); }
                if (SwitchSettings.SwitchConfig.GuiColumns[3]) { _gameTable.AppendColumn("Version",     new CellRendererText(),   "text",   3); }
                if (SwitchSettings.SwitchConfig.GuiColumns[4]) { _gameTable.AppendColumn("Time Played", new CellRendererText(),   "text",   4); }
                if (SwitchSettings.SwitchConfig.GuiColumns[5]) { _gameTable.AppendColumn("Last Played", new CellRendererText(),   "text",   5); }
                if (SwitchSettings.SwitchConfig.GuiColumns[6]) { _gameTable.AppendColumn("File Ext",    new CellRendererText(),   "text",   6); }
                if (SwitchSettings.SwitchConfig.GuiColumns[7]) { _gameTable.AppendColumn("File Size",   new CellRendererText(),   "text",   7); }
                if (SwitchSettings.SwitchConfig.GuiColumns[8]) { _gameTable.AppendColumn("Path",        new CellRendererText(),   "text",   8); }

                _tableStore      = new ListStore(typeof(Gdk.Pixbuf), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));
                _gameTable.Model = _tableStore;

                UpdateGameTable();
                // Temporary code section end
            }
            else
            {
                _box.Remove(_glScreen);

                if (SwitchSettings.SwitchConfig.GuiColumns[0]) { _gameTable.AppendColumn("Icon",        new CellRendererPixbuf(), "pixbuf", 0); }
                if (SwitchSettings.SwitchConfig.GuiColumns[1]) { _gameTable.AppendColumn("Application", new CellRendererText(),   "text",   1); }
                if (SwitchSettings.SwitchConfig.GuiColumns[2]) { _gameTable.AppendColumn("Developer",   new CellRendererText(),   "text",   2); }
                if (SwitchSettings.SwitchConfig.GuiColumns[3]) { _gameTable.AppendColumn("Version",     new CellRendererText(),   "text",   3); }
                if (SwitchSettings.SwitchConfig.GuiColumns[4]) { _gameTable.AppendColumn("Time Played", new CellRendererText(),   "text",   4); }
                if (SwitchSettings.SwitchConfig.GuiColumns[5]) { _gameTable.AppendColumn("Last Played", new CellRendererText(),   "text",   5); }
                if (SwitchSettings.SwitchConfig.GuiColumns[6]) { _gameTable.AppendColumn("File Ext",    new CellRendererText(),   "text",   6); }
                if (SwitchSettings.SwitchConfig.GuiColumns[7]) { _gameTable.AppendColumn("File Size",   new CellRendererText(),   "text",   7); }
                if (SwitchSettings.SwitchConfig.GuiColumns[8]) { _gameTable.AppendColumn("Path",        new CellRendererText(),   "text",   8); }

                _tableStore      = new ListStore(typeof(Gdk.Pixbuf), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string));
                _gameTable.Model = _tableStore;

                UpdateGameTable();
            }
        }

        public static void CreateErrorDialog(string errorMessage)
        {
            MessageDialog errorDialog = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, errorMessage)
            {
                Title          = "Ryujinx - Error",
                Icon           = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.ryujinxIcon.png"),
                WindowPosition = WindowPosition.Center
            };
            errorDialog.SetSizeRequest(100, 20);
            errorDialog.Run();
            errorDialog.Destroy();
        }

        public static void UpdateGameTable()
        {
            _tableStore.Clear();
            ApplicationLibrary.Init(SwitchSettings.SwitchConfig.GameDirs, _device.System.KeySet, _device.System.State.DesiredTitleLanguage);

            foreach (ApplicationLibrary.ApplicationData AppData in ApplicationLibrary.ApplicationLibraryData)
            {
                _tableStore.AppendValues(new Gdk.Pixbuf(AppData.Icon, 75, 75), $"{AppData.TitleName}\n{AppData.TitleId.ToUpper()}", AppData.Developer, AppData.Version, AppData.TimePlayed, AppData.LastPlayed, AppData.FileExt, AppData.FileSize, AppData.Path);
            }
        }

        public static void ApplyTheme()
        {
            CssProvider cssProvider = new CssProvider();

            if (SwitchSettings.SwitchConfig.EnableCustomTheme)
            {
                if (File.Exists(SwitchSettings.SwitchConfig.CustomThemePath) && (System.IO.Path.GetExtension(SwitchSettings.SwitchConfig.CustomThemePath) == ".css"))
                {
                    cssProvider.LoadFromPath(SwitchSettings.SwitchConfig.CustomThemePath);
                }
                else
                {
                    Logger.PrintWarning(LogClass.Application, $"The \"custom_theme_path\" section in \"Config.json\" contains an invalid path: \"{SwitchSettings.SwitchConfig.CustomThemePath}\"");
                }
            }
            else
            {
                cssProvider.LoadFromPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Theme.css"));
            }

            StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, 800);
        }

        internal void LoadApplication(string path)
        {
            if (_gameLoaded)
            {
                CreateErrorDialog("A game has already been loaded. Please close the emulator and try again");
            }
            else
            {
                Logger.RestartTime();

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
                        _device.LoadCart(path, romFsFiles[0]);
                    }
                    else
                    {
                        Logger.PrintInfo(LogClass.Application, "Loading as cart WITHOUT RomFS.");
                        _device.LoadCart(path);
                    }
                }
                else if (File.Exists(path))
                {
                    switch (System.IO.Path.GetExtension(path).ToLowerInvariant())
                    {
                        case ".xci":
                            Logger.PrintInfo(LogClass.Application, "Loading as XCI.");
                            _device.LoadXci(path);
                            break;
                        case ".nca":
                            Logger.PrintInfo(LogClass.Application, "Loading as NCA.");
                            _device.LoadNca(path);
                            break;
                        case ".nsp":
                        case ".pfs0":
                            Logger.PrintInfo(LogClass.Application, "Loading as NSP.");
                            _device.LoadNsp(path);
                            break;
                        default:
                            Logger.PrintInfo(LogClass.Application, "Loading as homebrew.");
                            try
                            {
                                _device.LoadProgram(path);
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
                    End();
                }

                new Thread(new ThreadStart(CreateGameWindow)).Start();

                _gameLoaded              = true;
                _stopEmulation.Sensitive = true;

                if (DiscordIntegrationEnabled)
                {
                    if (File.ReadAllLines(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RPsupported.dat")).Contains(_device.System.TitleID))
                    {
                        DiscordPresence.Assets.LargeImageKey = _device.System.TitleID;
                    }

                    string state = _device.System.TitleID;

                    if (state == null)
                    {
                        state = "Ryujinx";
                    }
                    else
                    {
                        state = state.ToUpper();
                    }

                    string details = "Idling";

                    if (_device.System.TitleName != null)
                    {
                        details = $"Playing {_device.System.TitleName}";
                    }

                    DiscordPresence.Details               = details;
                    DiscordPresence.State                 = state;
                    DiscordPresence.Assets.LargeImageText = _device.System.TitleName;
                    DiscordPresence.Assets.SmallImageKey  = "ryujinx";
                    DiscordPresence.Assets.SmallImageText = "Ryujinx is an emulator for the Nintendo Switch";
                    DiscordPresence.Timestamps            = new Timestamps(DateTime.UtcNow);

                    DiscordClient.SetPresence(DiscordPresence);
                }

                try
                {
                    string savePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RyuFS", "nand", "user", "save", "0000000000000000", _userId, _device.System.TitleID);

                    if (File.Exists(System.IO.Path.Combine(savePath, "TimePlayed.dat")) == false)
                    {
                        Directory.CreateDirectory(savePath);
                        using (FileStream stream = File.OpenWrite(System.IO.Path.Combine(savePath, "TimePlayed.dat")))
                        {
                            stream.Write(Encoding.ASCII.GetBytes("0"));
                        }
                    }

                    if (File.Exists(System.IO.Path.Combine(savePath, "LastPlayed.dat")) == false)
                    {
                        Directory.CreateDirectory(savePath);
                        using (FileStream stream = File.OpenWrite(System.IO.Path.Combine(savePath, "LastPlayed.dat")))
                        {
                            stream.Write(Encoding.ASCII.GetBytes("Never"));
                        }
                    }

                    using (FileStream stream = File.OpenWrite(System.IO.Path.Combine(savePath, "LastPlayed.dat")))
                    {
                        using (StreamWriter writer = new StreamWriter(stream))
                        {
                            writer.WriteLine(DateTime.UtcNow);
                        }
                    }
                }
                catch (ArgumentNullException)
                {
                    Logger.PrintWarning(LogClass.Application, $"Could not access save path to retrieve time/last played data using: UserID: {_userId}, TitleID: {_device.System.TitleID}");
                }
            }
        }

        private static void CreateGameWindow()
        {
            Configuration.ConfigureHid(_device, SwitchSettings.SwitchConfig);
            
            using (GlScreen screen = new GlScreen(_device, _renderer))
            {
                screen.MainLoop();

                End();
            }
        }

        private static void End()
        {
            if (_gameLoaded)
            {
                try
                {
                    string savePath        = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RyuFS", "nand", "user", "save", "0000000000000000", _userId, _device.System.TitleID);
                    double currentPlayTime = 0;

                    using (FileStream stream = File.OpenRead(System.IO.Path.Combine(savePath, "LastPlayed.dat")))
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            DateTime startTime = DateTime.Parse(reader.ReadLine());

                            using (FileStream lastPlayedStream = File.OpenRead(System.IO.Path.Combine(savePath, "TimePlayed.dat")))
                            {
                                using (StreamReader lastPlayedReader = new StreamReader(lastPlayedStream))
                                {
                                    currentPlayTime = double.Parse(lastPlayedReader.ReadLine());
                                }
                            }

                            using (FileStream timePlayedStream = File.OpenWrite(System.IO.Path.Combine(savePath, "TimePlayed.dat")))
                            {
                                using (StreamWriter timePlayedWriter = new StreamWriter(timePlayedStream))
                                {
                                    timePlayedWriter.WriteLine(currentPlayTime + Math.Round(DateTime.UtcNow.Subtract(startTime).TotalSeconds, MidpointRounding.AwayFromZero));
                                }
                            }
                        }
                    }
                }
                catch (ArgumentNullException)
                {
                    Logger.PrintWarning(LogClass.Application, $"Could not access save path to retrieve time/last played data using: UserID: {_userId}, TitleID: {_device.System.TitleID}");
                }
            }

            Profile.FinishProfiling();
            _device.Dispose();
            _audioOut.Dispose();
            DiscordClient?.Dispose();
            Logger.Shutdown();
            Environment.Exit(0);
        }

        /// <summary>
        /// Picks an <see cref="IAalOutput"/> audio output renderer supported on this machine
        /// </summary>
        /// <returns>An <see cref="IAalOutput"/> supported by this machine</returns>
        private static IAalOutput InitializeAudioEngine()
        {
            if (SoundIoAudioOut.IsSupported)
            {
                return new SoundIoAudioOut();
            }
            else if (OpenALAudioOut.IsSupported)
            {
                return new OpenALAudioOut();
            }
            else
            {
                return new DummyAudioOut();
            }
        }

        //Events
        private void Row_Activated(object o, RowActivatedArgs args)
        {
            _tableStore.GetIter(out TreeIter treeIter, new TreePath(args.Path.ToString()));
            string path = (string)_tableStore.GetValue(treeIter, 8);

            LoadApplication(path);
        }

        private void Load_Application_File(object o, EventArgs args)
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

            fileChooser.Destroy();
        }

        private void Load_Application_Folder(object o, EventArgs args)
        {
            FileChooserDialog fileChooser = new FileChooserDialog("Choose the folder to open", this, FileChooserAction.SelectFolder, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);

            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                LoadApplication(fileChooser.Filename);
            }

            fileChooser.Destroy();
        }

        private void Open_Ryu_Folder(object o, EventArgs args)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName        = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RyuFs"),
                UseShellExecute = true,
                Verb            = "open"
            });
        }

        private void Exit_Pressed(object o, EventArgs args)
        {
            End();
        }

        private void Window_Close(object o, DeleteEventArgs args)
        {
            End();
        }

        private void StopEmulation_Pressed(object o, EventArgs args)
        {
            // TODO: Write logic to kill running game
        }

        private void FullScreen_Toggled(object o, EventArgs args)
        {
            if (_fullScreen.Active)
            {
                Fullscreen();
            }
            else
            {
                Unfullscreen();
            }
        }

        private void Settings_Pressed(object o, EventArgs args)
        {
            SwitchSettings SettingsWin = new SwitchSettings(_device);

            _gtkApplication.Register(GLib.Cancellable.Current);
            _gtkApplication.AddWindow(SettingsWin);

            SettingsWin.Show();
        }

        private void Update_Pressed(object o, EventArgs args)
        {
            string ryuUpdater = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RyuFS", "RyuUpdater.exe");

            try
            {
                Process.Start(new ProcessStartInfo(ryuUpdater, "/U") { UseShellExecute = true });
            }
            catch(System.ComponentModel.Win32Exception)
            {
                CreateErrorDialog("Update canceled by user or updater was not found");
            }
        }

        private void About_Pressed(object o, EventArgs args)
        {
            AboutWindow AboutWin = new AboutWindow();

            _gtkApplication.Register(GLib.Cancellable.Current);
            _gtkApplication.AddWindow(AboutWin);

            AboutWin.Show();
        }

        private void Icon_Toggled(object o, EventArgs args)
        {
            SwitchSettings.SwitchConfig.GuiColumns[0] = _iconToggle.Active;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));
        }

        private void Title_Toggled(object o, EventArgs args)
        {
            SwitchSettings.SwitchConfig.GuiColumns[1] = _titleToggle.Active;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));
        }

        private void Developer_Toggled(object o, EventArgs args)
        {
            SwitchSettings.SwitchConfig.GuiColumns[2] = _developerToggle.Active;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));
        }

        private void Version_Toggled(object o, EventArgs args)
        {
            SwitchSettings.SwitchConfig.GuiColumns[3] = _versionToggle.Active;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));
        }

        private void TimePlayed_Toggled(object o, EventArgs args)
        {
            SwitchSettings.SwitchConfig.GuiColumns[4] = _timePlayedToggle.Active;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));
        }

        private void LastPlayed_Toggled(object o, EventArgs args)
        {
            SwitchSettings.SwitchConfig.GuiColumns[5] = _lastPlayedToggle.Active;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));
        }

        private void FileExt_Toggled(object o, EventArgs args)
        {
            SwitchSettings.SwitchConfig.GuiColumns[6] = _fileExtToggle.Active;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));
        }

        private void FileSize_Toggled(object o, EventArgs args)
        {
            SwitchSettings.SwitchConfig.GuiColumns[7] = _fileSizeToggle.Active;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));
        }

        private void Path_Toggled(object o, EventArgs args)
        {
            SwitchSettings.SwitchConfig.GuiColumns[8] = _pathToggle.Active;

            Configuration.SaveConfig(SwitchSettings.SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));
        }
    }
}
