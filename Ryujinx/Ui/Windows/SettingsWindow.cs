using Gtk;
using Ryujinx.Audio;
using Ryujinx.Audio.Backends.OpenAL;
using Ryujinx.Audio.Backends.SoundIo;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Configuration.Hid;
using Ryujinx.Configuration;
using Ryujinx.Configuration.System;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Services.Time.TimeZone;
using Ryujinx.Ui.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using GUI = Gtk.Builder.ObjectAttribute;

namespace Ryujinx.Ui.Windows
{
    public class SettingsWindow : Window
    {
        private readonly MainWindow             _parent;
        private readonly ListStore              _gameDirsBoxStore;
        private readonly ListStore              _audioBackendStore;
        private readonly TimeZoneContentManager _timeZoneContentManager;
        private readonly HashSet<string>        _validTzRegions;

        private long _systemTimeOffset;

#pragma warning disable CS0649, IDE0044
        [GUI] CheckButton     _errorLogToggle;
        [GUI] CheckButton     _warningLogToggle;
        [GUI] CheckButton     _infoLogToggle;
        [GUI] CheckButton     _stubLogToggle;
        [GUI] CheckButton     _debugLogToggle;
        [GUI] CheckButton     _fileLogToggle;
        [GUI] CheckButton     _guestLogToggle;
        [GUI] CheckButton     _fsAccessLogToggle;
        [GUI] Adjustment      _fsLogSpinAdjustment;
        [GUI] ComboBoxText    _graphicsDebugLevel;
        [GUI] CheckButton     _dockedModeToggle;
        [GUI] CheckButton     _discordToggle;
        [GUI] CheckButton     _checkUpdatesToggle;
        [GUI] CheckButton     _showConfirmExitToggle;
        [GUI] CheckButton     _hideCursorOnIdleToggle;
        [GUI] CheckButton     _vSyncToggle;
        [GUI] CheckButton     _shaderCacheToggle;
        [GUI] CheckButton     _ptcToggle;
        [GUI] CheckButton     _fsicToggle;
        [GUI] CheckButton     _ignoreToggle;
        [GUI] CheckButton     _directKeyboardAccess;
        [GUI] ComboBoxText    _systemLanguageSelect;
        [GUI] ComboBoxText    _systemRegionSelect;
        [GUI] Entry           _systemTimeZoneEntry;
        [GUI] EntryCompletion _systemTimeZoneCompletion;
        [GUI] Box             _audioBackendBox;
        [GUI] ComboBox        _audioBackendSelect;
        [GUI] SpinButton      _systemTimeYearSpin;
        [GUI] SpinButton      _systemTimeMonthSpin;
        [GUI] SpinButton      _systemTimeDaySpin;
        [GUI] SpinButton      _systemTimeHourSpin;
        [GUI] SpinButton      _systemTimeMinuteSpin;
        [GUI] Adjustment      _systemTimeYearSpinAdjustment;
        [GUI] Adjustment      _systemTimeMonthSpinAdjustment;
        [GUI] Adjustment      _systemTimeDaySpinAdjustment;
        [GUI] Adjustment      _systemTimeHourSpinAdjustment;
        [GUI] Adjustment      _systemTimeMinuteSpinAdjustment;
        [GUI] CheckButton     _custThemeToggle;
        [GUI] Entry           _custThemePath;
        [GUI] ToggleButton    _browseThemePath;
        [GUI] Label           _custThemePathLabel;
        [GUI] TreeView        _gameDirsBox;
        [GUI] Entry           _addGameDirBox;
        [GUI] Entry           _graphicsShadersDumpPath;
        [GUI] ComboBoxText    _anisotropy;
        [GUI] ComboBoxText    _aspectRatio;
        [GUI] ComboBoxText    _resScaleCombo;
        [GUI] Entry           _resScaleText;
        [GUI] ToggleButton    _configureController1;
        [GUI] ToggleButton    _configureController2;
        [GUI] ToggleButton    _configureController3;
        [GUI] ToggleButton    _configureController4;
        [GUI] ToggleButton    _configureController5;
        [GUI] ToggleButton    _configureController6;
        [GUI] ToggleButton    _configureController7;
        [GUI] ToggleButton    _configureController8;
        [GUI] ToggleButton    _configureControllerH;

#pragma warning restore CS0649, IDE0044

        public SettingsWindow(MainWindow parent, VirtualFileSystem virtualFileSystem, HLE.FileSystem.Content.ContentManager contentManager) : this(parent, new Builder("Ryujinx.Ui.Windows.SettingsWindow.glade"), virtualFileSystem, contentManager) { }

        private SettingsWindow(MainWindow parent, Builder builder, VirtualFileSystem virtualFileSystem, HLE.FileSystem.Content.ContentManager contentManager) : base(builder.GetObject("_settingsWin").Handle)
        {
            Icon = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.Resources.Logo_Ryujinx.png");

            _parent = parent;

            builder.Autoconnect(this);

            _timeZoneContentManager = new TimeZoneContentManager();
            _timeZoneContentManager.InitializeInstance(virtualFileSystem, contentManager, LibHac.FsSystem.IntegrityCheckLevel.None);

            _validTzRegions = new HashSet<string>(_timeZoneContentManager.LocationNameCache.Length, StringComparer.Ordinal); // Zone regions are identifiers. Must match exactly.

            // Bind Events.
            _configureController1.Pressed += (sender, args) => ConfigureController_Pressed(sender, PlayerIndex.Player1);
            _configureController2.Pressed += (sender, args) => ConfigureController_Pressed(sender, PlayerIndex.Player2);
            _configureController3.Pressed += (sender, args) => ConfigureController_Pressed(sender, PlayerIndex.Player3);
            _configureController4.Pressed += (sender, args) => ConfigureController_Pressed(sender, PlayerIndex.Player4);
            _configureController5.Pressed += (sender, args) => ConfigureController_Pressed(sender, PlayerIndex.Player5);
            _configureController6.Pressed += (sender, args) => ConfigureController_Pressed(sender, PlayerIndex.Player6);
            _configureController7.Pressed += (sender, args) => ConfigureController_Pressed(sender, PlayerIndex.Player7);
            _configureController8.Pressed += (sender, args) => ConfigureController_Pressed(sender, PlayerIndex.Player8);
            _configureControllerH.Pressed += (sender, args) => ConfigureController_Pressed(sender, PlayerIndex.Handheld);
            _systemTimeZoneEntry.FocusOutEvent += TimeZoneEntry_FocusOut;

            _resScaleCombo.Changed += (sender, args) => _resScaleText.Visible = _resScaleCombo.ActiveId == "-1";

            // Setup Currents.
            if (ConfigurationState.Instance.Logger.EnableFileLog)
            {
                _fileLogToggle.Click();
            }

            if (ConfigurationState.Instance.Logger.EnableError)
            {
                _errorLogToggle.Click();
            }

            if (ConfigurationState.Instance.Logger.EnableWarn)
            {
                _warningLogToggle.Click();
            }

            if (ConfigurationState.Instance.Logger.EnableInfo)
            {
                _infoLogToggle.Click();
            }

            if (ConfigurationState.Instance.Logger.EnableStub)
            {
                _stubLogToggle.Click();
            }

            if (ConfigurationState.Instance.Logger.EnableDebug)
            {
                _debugLogToggle.Click();
            }

            if (ConfigurationState.Instance.Logger.EnableGuest)
            {
                _guestLogToggle.Click();
            }

            if (ConfigurationState.Instance.Logger.EnableFsAccessLog)
            {
                _fsAccessLogToggle.Click();
            }

            foreach (GraphicsDebugLevel level in Enum.GetValues(typeof(GraphicsDebugLevel)))
            {
                _graphicsDebugLevel.Append(level.ToString(), level.ToString());
            }

            _graphicsDebugLevel.SetActiveId(ConfigurationState.Instance.Logger.GraphicsDebugLevel.Value.ToString());

            if (ConfigurationState.Instance.System.EnableDockedMode)
            {
                _dockedModeToggle.Click();
            }

            if (ConfigurationState.Instance.EnableDiscordIntegration)
            {
                _discordToggle.Click();
            }

            if (ConfigurationState.Instance.CheckUpdatesOnStart)
            {
                _checkUpdatesToggle.Click();
            }

            if (ConfigurationState.Instance.ShowConfirmExit)
            {
                _showConfirmExitToggle.Click();
            }

            if (ConfigurationState.Instance.HideCursorOnIdle)
            {
                _hideCursorOnIdleToggle.Click();
            }

            if (ConfigurationState.Instance.Graphics.EnableVsync)
            {
                _vSyncToggle.Click();
            }

            if (ConfigurationState.Instance.Graphics.EnableShaderCache)
            {
                _shaderCacheToggle.Click();
            }

            if (ConfigurationState.Instance.System.EnablePtc)
            {
                _ptcToggle.Click();
            }

            if (ConfigurationState.Instance.System.EnableFsIntegrityChecks)
            {
                _fsicToggle.Click();
            }

            if (ConfigurationState.Instance.System.IgnoreMissingServices)
            {
                _ignoreToggle.Click();
            }

            if (ConfigurationState.Instance.Hid.EnableKeyboard)
            {
                _directKeyboardAccess.Click();
            }

            if (ConfigurationState.Instance.Ui.EnableCustomTheme)
            {
                _custThemeToggle.Click();
            }

            // Custom EntryCompletion Columns. If added to glade, need to override more signals
            ListStore tzList = new ListStore(typeof(string), typeof(string), typeof(string));
            _systemTimeZoneCompletion.Model = tzList;

            CellRendererText offsetCol = new CellRendererText();
            CellRendererText abbrevCol = new CellRendererText();

            _systemTimeZoneCompletion.PackStart(offsetCol, false);
            _systemTimeZoneCompletion.AddAttribute(offsetCol, "text", 0);
            _systemTimeZoneCompletion.TextColumn = 1; // Regions Column
            _systemTimeZoneCompletion.PackStart(abbrevCol, false);
            _systemTimeZoneCompletion.AddAttribute(abbrevCol, "text", 2);

            int maxLocationLength = 0;

            foreach (var (offset, location, abbr) in _timeZoneContentManager.ParseTzOffsets())
            {
                var hours = Math.DivRem(offset, 3600, out int seconds);
                var minutes = Math.Abs(seconds) / 60;

                var abbr2 = (abbr.StartsWith('+') || abbr.StartsWith('-')) ? string.Empty : abbr;

                tzList.AppendValues($"UTC{hours:+0#;-0#;+00}:{minutes:D2} ", location, abbr2);
                _validTzRegions.Add(location);

                maxLocationLength = Math.Max(maxLocationLength, location.Length);
            }

            _systemTimeZoneEntry.WidthChars = Math.Max(20, maxLocationLength + 1); // Ensure minimum Entry width
            _systemTimeZoneEntry.Text = _timeZoneContentManager.SanityCheckDeviceLocationName();

            _systemTimeZoneCompletion.MatchFunc = TimeZoneMatchFunc;

            _systemLanguageSelect.SetActiveId(ConfigurationState.Instance.System.Language.Value.ToString());
            _systemRegionSelect.SetActiveId(ConfigurationState.Instance.System.Region.Value.ToString());
            _resScaleCombo.SetActiveId(ConfigurationState.Instance.Graphics.ResScale.Value.ToString());
            _anisotropy.SetActiveId(ConfigurationState.Instance.Graphics.MaxAnisotropy.Value.ToString());
            _aspectRatio.SetActiveId(((int)ConfigurationState.Instance.Graphics.AspectRatio.Value).ToString());

            _custThemePath.Buffer.Text           = ConfigurationState.Instance.Ui.CustomThemePath;
            _resScaleText.Buffer.Text            = ConfigurationState.Instance.Graphics.ResScaleCustom.Value.ToString();
            _resScaleText.Visible                = _resScaleCombo.ActiveId == "-1";
            _graphicsShadersDumpPath.Buffer.Text = ConfigurationState.Instance.Graphics.ShadersDumpPath;
            _fsLogSpinAdjustment.Value           = ConfigurationState.Instance.System.FsGlobalAccessLogMode;
            _systemTimeOffset                    = ConfigurationState.Instance.System.SystemTimeOffset;

            _gameDirsBox.AppendColumn("", new CellRendererText(), "text", 0);
            _gameDirsBoxStore  = new ListStore(typeof(string));
            _gameDirsBox.Model = _gameDirsBoxStore;

            foreach (string gameDir in ConfigurationState.Instance.Ui.GameDirs.Value)
            {
                _gameDirsBoxStore.AppendValues(gameDir);
            }

            if (_custThemeToggle.Active == false)
            {
                _custThemePath.Sensitive      = false;
                _custThemePathLabel.Sensitive = false;
                _browseThemePath.Sensitive    = false;
            }

            //Setup system time spinners
            UpdateSystemTimeSpinners();

            _audioBackendStore = new ListStore(typeof(string), typeof(AudioBackend));

            TreeIter openAlIter  = _audioBackendStore.AppendValues("OpenAL", AudioBackend.OpenAl);
            TreeIter soundIoIter = _audioBackendStore.AppendValues("SoundIO", AudioBackend.SoundIo);
            TreeIter dummyIter   = _audioBackendStore.AppendValues("Dummy", AudioBackend.Dummy);

            _audioBackendSelect = ComboBox.NewWithModelAndEntry(_audioBackendStore);
            _audioBackendSelect.EntryTextColumn = 0;
            _audioBackendSelect.Entry.IsEditable = false;

            switch (ConfigurationState.Instance.System.AudioBackend.Value)
            {
                case AudioBackend.OpenAl:
                    _audioBackendSelect.SetActiveIter(openAlIter);
                    break;
                case AudioBackend.SoundIo:
                    _audioBackendSelect.SetActiveIter(soundIoIter);
                    break;
                case AudioBackend.Dummy:
                    _audioBackendSelect.SetActiveIter(dummyIter);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _audioBackendBox.Add(_audioBackendSelect);
            _audioBackendSelect.Show();

            bool openAlIsSupported  = false;
            bool soundIoIsSupported = false;

            Task.Run(() =>
            {
                openAlIsSupported  = OpenALHardwareDeviceDriver.IsSupported;
                soundIoIsSupported = SoundIoHardwareDeviceDriver.IsSupported;
            });

            // This function runs whenever the dropdown is opened
            _audioBackendSelect.SetCellDataFunc(_audioBackendSelect.Cells[0], (layout, cell, model, iter) =>
            {
                cell.Sensitive = ((AudioBackend)_audioBackendStore.GetValue(iter, 1)) switch
                {
                    AudioBackend.OpenAl  => openAlIsSupported,
                    AudioBackend.SoundIo => soundIoIsSupported,
                    AudioBackend.Dummy   => true,
                    _ => throw new ArgumentOutOfRangeException()
                };
            });
        }

        private void UpdateSystemTimeSpinners()
        {
            //Bind system time events
            _systemTimeYearSpin.ValueChanged   -= SystemTimeSpin_ValueChanged;
            _systemTimeMonthSpin.ValueChanged  -= SystemTimeSpin_ValueChanged;
            _systemTimeDaySpin.ValueChanged    -= SystemTimeSpin_ValueChanged;
            _systemTimeHourSpin.ValueChanged   -= SystemTimeSpin_ValueChanged;
            _systemTimeMinuteSpin.ValueChanged -= SystemTimeSpin_ValueChanged;

            //Apply actual system time + SystemTimeOffset to system time spin buttons
            DateTime systemTime = DateTime.Now.AddSeconds(_systemTimeOffset);

            _systemTimeYearSpinAdjustment.Value   = systemTime.Year;
            _systemTimeMonthSpinAdjustment.Value  = systemTime.Month;
            _systemTimeDaySpinAdjustment.Value    = systemTime.Day;
            _systemTimeHourSpinAdjustment.Value   = systemTime.Hour;
            _systemTimeMinuteSpinAdjustment.Value = systemTime.Minute;

            //Format spin buttons text to include leading zeros
            _systemTimeYearSpin.Text   = systemTime.Year.ToString("0000");
            _systemTimeMonthSpin.Text  = systemTime.Month.ToString("00");
            _systemTimeDaySpin.Text    = systemTime.Day.ToString("00");
            _systemTimeHourSpin.Text   = systemTime.Hour.ToString("00");
            _systemTimeMinuteSpin.Text = systemTime.Minute.ToString("00");

            //Bind system time events
            _systemTimeYearSpin.ValueChanged   += SystemTimeSpin_ValueChanged;
            _systemTimeMonthSpin.ValueChanged  += SystemTimeSpin_ValueChanged;
            _systemTimeDaySpin.ValueChanged    += SystemTimeSpin_ValueChanged;
            _systemTimeHourSpin.ValueChanged   += SystemTimeSpin_ValueChanged;
            _systemTimeMinuteSpin.ValueChanged += SystemTimeSpin_ValueChanged;
        }

        private void SaveSettings()
        {
            List<string> gameDirs = new List<string>();

            _gameDirsBoxStore.GetIterFirst(out TreeIter treeIter);
            for (int i = 0; i < _gameDirsBoxStore.IterNChildren(); i++)
            {
                gameDirs.Add((string)_gameDirsBoxStore.GetValue(treeIter, 0));

                _gameDirsBoxStore.IterNext(ref treeIter);
            }

            if (!float.TryParse(_resScaleText.Buffer.Text, out float resScaleCustom) || resScaleCustom <= 0.0f)
            {
                resScaleCustom = 1.0f;
            }

            if (_validTzRegions.Contains(_systemTimeZoneEntry.Text))
            {
                ConfigurationState.Instance.System.TimeZone.Value = _systemTimeZoneEntry.Text;
            }

            ConfigurationState.Instance.Logger.EnableError.Value               = _errorLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableWarn.Value                = _warningLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableInfo.Value                = _infoLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableStub.Value                = _stubLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableDebug.Value               = _debugLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableGuest.Value               = _guestLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableFsAccessLog.Value         = _fsAccessLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableFileLog.Value             = _fileLogToggle.Active;
            ConfigurationState.Instance.Logger.GraphicsDebugLevel.Value        = Enum.Parse<GraphicsDebugLevel>(_graphicsDebugLevel.ActiveId);
            ConfigurationState.Instance.System.EnableDockedMode.Value          = _dockedModeToggle.Active;
            ConfigurationState.Instance.EnableDiscordIntegration.Value         = _discordToggle.Active;
            ConfigurationState.Instance.CheckUpdatesOnStart.Value              = _checkUpdatesToggle.Active;
            ConfigurationState.Instance.ShowConfirmExit.Value                  = _showConfirmExitToggle.Active;
            ConfigurationState.Instance.HideCursorOnIdle.Value                 = _hideCursorOnIdleToggle.Active;
            ConfigurationState.Instance.Graphics.EnableVsync.Value             = _vSyncToggle.Active;
            ConfigurationState.Instance.Graphics.EnableShaderCache.Value       = _shaderCacheToggle.Active;
            ConfigurationState.Instance.System.EnablePtc.Value                 = _ptcToggle.Active;
            ConfigurationState.Instance.System.EnableFsIntegrityChecks.Value   = _fsicToggle.Active;
            ConfigurationState.Instance.System.IgnoreMissingServices.Value     = _ignoreToggle.Active;
            ConfigurationState.Instance.Hid.EnableKeyboard.Value               = _directKeyboardAccess.Active;
            ConfigurationState.Instance.Ui.EnableCustomTheme.Value             = _custThemeToggle.Active;
            ConfigurationState.Instance.System.Language.Value                  = Enum.Parse<Language>(_systemLanguageSelect.ActiveId);
            ConfigurationState.Instance.System.Region.Value                    = Enum.Parse<Configuration.System.Region>(_systemRegionSelect.ActiveId);
            ConfigurationState.Instance.System.SystemTimeOffset.Value          = _systemTimeOffset;
            ConfigurationState.Instance.Ui.CustomThemePath.Value               = _custThemePath.Buffer.Text;
            ConfigurationState.Instance.Graphics.ShadersDumpPath.Value         = _graphicsShadersDumpPath.Buffer.Text;
            ConfigurationState.Instance.Ui.GameDirs.Value                      = gameDirs;
            ConfigurationState.Instance.System.FsGlobalAccessLogMode.Value     = (int)_fsLogSpinAdjustment.Value;
            ConfigurationState.Instance.Graphics.MaxAnisotropy.Value           = float.Parse(_anisotropy.ActiveId, CultureInfo.InvariantCulture);
            ConfigurationState.Instance.Graphics.AspectRatio.Value             = Enum.Parse<AspectRatio>(_aspectRatio.ActiveId);
            ConfigurationState.Instance.Graphics.ResScale.Value                = int.Parse(_resScaleCombo.ActiveId);
            ConfigurationState.Instance.Graphics.ResScaleCustom.Value          = resScaleCustom;

            if (_audioBackendSelect.GetActiveIter(out TreeIter activeIter))
            {
                ConfigurationState.Instance.System.AudioBackend.Value = (AudioBackend)_audioBackendStore.GetValue(activeIter, 1);
            }

            ConfigurationState.Instance.ToFileFormat().SaveConfig(Program.ConfigurationPath);
            _parent.UpdateGraphicsConfig();
            ThemeHelper.ApplyTheme();
        }

        //
        // Events
        //
        private void TimeZoneEntry_FocusOut(object sender, FocusOutEventArgs e)
        {
            if (!_validTzRegions.Contains(_systemTimeZoneEntry.Text))
            {
                _systemTimeZoneEntry.Text = _timeZoneContentManager.SanityCheckDeviceLocationName();
            }
        }

        private bool TimeZoneMatchFunc(EntryCompletion compl, string key, TreeIter iter)
        {
            key = key.Trim().Replace(' ', '_');

            return ((string)compl.Model.GetValue(iter, 1)).Contains(key, StringComparison.OrdinalIgnoreCase) || // region
                   ((string)compl.Model.GetValue(iter, 2)).StartsWith(key, StringComparison.OrdinalIgnoreCase) || // abbr
                   ((string)compl.Model.GetValue(iter, 0))[3..].StartsWith(key); // offset
        }

        private void SystemTimeSpin_ValueChanged(object sender, EventArgs e)
        {
            int year   = _systemTimeYearSpin.ValueAsInt;
            int month  = _systemTimeMonthSpin.ValueAsInt;
            int day    = _systemTimeDaySpin.ValueAsInt;
            int hour   = _systemTimeHourSpin.ValueAsInt;
            int minute = _systemTimeMinuteSpin.ValueAsInt;

            if (!DateTime.TryParse(year + "-" + month + "-" + day + " " + hour + ":" + minute, out DateTime newTime))
            {
                UpdateSystemTimeSpinners();

                return;
            }

            newTime = newTime.AddSeconds(DateTime.Now.Second).AddMilliseconds(DateTime.Now.Millisecond);

            long systemTimeOffset = (long)Math.Ceiling((newTime - DateTime.Now).TotalMinutes) * 60L;

            if (_systemTimeOffset != systemTimeOffset)
            {
                _systemTimeOffset = systemTimeOffset;
                UpdateSystemTimeSpinners();
            }
        }

        private void AddDir_Pressed(object sender, EventArgs args)
        {
            if (Directory.Exists(_addGameDirBox.Buffer.Text))
            {
                _gameDirsBoxStore.AppendValues(_addGameDirBox.Buffer.Text);
            }
            else
            {
                FileChooserDialog fileChooser = new FileChooserDialog("Choose the game directory to add to the list", this, FileChooserAction.SelectFolder, "Cancel", ResponseType.Cancel, "Add", ResponseType.Accept)
                {
                    SelectMultiple = true
                };

                if (fileChooser.Run() == (int)ResponseType.Accept)
                {
                    foreach (string directory in fileChooser.Filenames)
                    {
                        bool directoryAdded = false;

                        if (_gameDirsBoxStore.GetIterFirst(out TreeIter treeIter))
                        {
                            do
                            {
                                if (directory.Equals((string)_gameDirsBoxStore.GetValue(treeIter, 0)))
                                {
                                    directoryAdded = true;
                                    break;
                                }
                            } while(_gameDirsBoxStore.IterNext(ref treeIter));
                        }

                        if (!directoryAdded)
                        {
                            _gameDirsBoxStore.AppendValues(directory);
                        }
                    }
                }

                fileChooser.Dispose();
            }

            _addGameDirBox.Buffer.Text = "";

            ((ToggleButton)sender).SetStateFlags(StateFlags.Normal, true);
        }

        private void RemoveDir_Pressed(object sender, EventArgs args)
        {
            TreeSelection selection = _gameDirsBox.Selection;

            if (selection.GetSelected(out TreeIter treeIter))
            {
                _gameDirsBoxStore.Remove(ref treeIter);
            }

            ((ToggleButton)sender).SetStateFlags(StateFlags.Normal, true);
        }

        private void CustThemeToggle_Activated(object sender, EventArgs args)
        {
            _custThemePath.Sensitive      = _custThemeToggle.Active;
            _custThemePathLabel.Sensitive = _custThemeToggle.Active;
            _browseThemePath.Sensitive    = _custThemeToggle.Active;
        }

        private void BrowseThemeDir_Pressed(object sender, EventArgs args)
        {
            using (FileChooserDialog fileChooser = new FileChooserDialog("Choose the theme to load", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Select", ResponseType.Accept))
            {
                fileChooser.Filter = new FileFilter();
                fileChooser.Filter.AddPattern("*.css");

                if (fileChooser.Run() == (int)ResponseType.Accept)
                {
                    _custThemePath.Buffer.Text = fileChooser.Filename;
                }
            }

            _browseThemePath.SetStateFlags(StateFlags.Normal, true);
        }

        private void ConfigureController_Pressed(object sender, PlayerIndex playerIndex)
        {
            ((ToggleButton)sender).SetStateFlags(StateFlags.Normal, true);

            ControllerWindow controllerWindow = new ControllerWindow(playerIndex);

            controllerWindow.SetSizeRequest((int)(controllerWindow.DefaultWidth * Program.WindowScaleFactor), (int)(controllerWindow.DefaultHeight * Program.WindowScaleFactor));
            controllerWindow.Show();
        }

        private void SaveToggle_Activated(object sender, EventArgs args)
        {
            SaveSettings();
            Dispose();
        }

        private void ApplyToggle_Activated(object sender, EventArgs args)
        {
            SaveSettings();
        }

        private void CloseToggle_Activated(object sender, EventArgs args)
        {
            Dispose();
        }
    }
}
