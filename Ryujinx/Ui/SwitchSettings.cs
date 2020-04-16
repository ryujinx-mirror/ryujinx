using Gtk;
using Ryujinx.Configuration;
using Ryujinx.Configuration.Hid;
using Ryujinx.Configuration.System;
using Ryujinx.HLE.HOS.Services.Time.TimeZone;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

using GUI = Gtk.Builder.ObjectAttribute;

namespace Ryujinx.Ui
{
    public class SwitchSettings : Window
    {
        private static ListStore _gameDirsBoxStore;

        private static bool _listeningForKeypress;

        private long _systemTimeOffset;

#pragma warning disable CS0649
#pragma warning disable IDE0044
        [GUI] Window       _settingsWin;
        [GUI] CheckButton  _errorLogToggle;
        [GUI] CheckButton  _warningLogToggle;
        [GUI] CheckButton  _infoLogToggle;
        [GUI] CheckButton  _stubLogToggle;
        [GUI] CheckButton  _debugLogToggle;
        [GUI] CheckButton  _fileLogToggle;
        [GUI] CheckButton  _guestLogToggle;
        [GUI] CheckButton  _fsAccessLogToggle;
        [GUI] Adjustment   _fsLogSpinAdjustment;
        [GUI] CheckButton  _dockedModeToggle;
        [GUI] CheckButton  _discordToggle;
        [GUI] CheckButton  _vSyncToggle;
        [GUI] CheckButton  _multiSchedToggle;
        [GUI] CheckButton  _fsicToggle;
        [GUI] CheckButton  _ignoreToggle;
        [GUI] CheckButton  _directKeyboardAccess;
        [GUI] ComboBoxText _systemLanguageSelect;
        [GUI] ComboBoxText _systemRegionSelect;
        [GUI] ComboBoxText _systemTimeZoneSelect;
        [GUI] SpinButton   _systemTimeYearSpin;
        [GUI] SpinButton   _systemTimeMonthSpin;
        [GUI] SpinButton   _systemTimeDaySpin;
        [GUI] SpinButton   _systemTimeHourSpin;
        [GUI] SpinButton   _systemTimeMinuteSpin;
        [GUI] Adjustment   _systemTimeYearSpinAdjustment;
        [GUI] Adjustment   _systemTimeMonthSpinAdjustment;
        [GUI] Adjustment   _systemTimeDaySpinAdjustment;
        [GUI] Adjustment   _systemTimeHourSpinAdjustment;
        [GUI] Adjustment   _systemTimeMinuteSpinAdjustment;
        [GUI] CheckButton  _custThemeToggle;
        [GUI] Entry        _custThemePath;
        [GUI] ToggleButton _browseThemePath;
        [GUI] Label        _custThemePathLabel;
        [GUI] TreeView     _gameDirsBox;
        [GUI] Entry        _addGameDirBox;
        [GUI] ToggleButton _addDir;
        [GUI] ToggleButton _browseDir;
        [GUI] ToggleButton _removeDir;
        [GUI] Entry        _graphicsShadersDumpPath;
        [GUI] ComboBoxText _anisotropy;
        [GUI] Image        _controller1Image;

        [GUI] ComboBoxText _controller1Type;
        [GUI] ToggleButton _lStickUp1;
        [GUI] ToggleButton _lStickDown1;
        [GUI] ToggleButton _lStickLeft1;
        [GUI] ToggleButton _lStickRight1;
        [GUI] ToggleButton _lStickButton1;
        [GUI] ToggleButton _dpadUp1;
        [GUI] ToggleButton _dpadDown1;
        [GUI] ToggleButton _dpadLeft1;
        [GUI] ToggleButton _dpadRight1;
        [GUI] ToggleButton _minus1;
        [GUI] ToggleButton _l1;
        [GUI] ToggleButton _zL1;
        [GUI] ToggleButton _rStickUp1;
        [GUI] ToggleButton _rStickDown1;
        [GUI] ToggleButton _rStickLeft1;
        [GUI] ToggleButton _rStickRight1;
        [GUI] ToggleButton _rStickButton1;
        [GUI] ToggleButton _a1;
        [GUI] ToggleButton _b1;
        [GUI] ToggleButton _x1;
        [GUI] ToggleButton _y1;
        [GUI] ToggleButton _plus1;
        [GUI] ToggleButton _r1;
        [GUI] ToggleButton _zR1;
#pragma warning restore CS0649
#pragma warning restore IDE0044

        public SwitchSettings(HLE.FileSystem.VirtualFileSystem virtualFileSystem, HLE.FileSystem.Content.ContentManager contentManager) : this(new Builder("Ryujinx.Ui.SwitchSettings.glade"), virtualFileSystem, contentManager) { }

        private SwitchSettings(Builder builder, HLE.FileSystem.VirtualFileSystem virtualFileSystem, HLE.FileSystem.Content.ContentManager contentManager) : base(builder.GetObject("_settingsWin").Handle)
        {
            builder.Autoconnect(this);

            _settingsWin.Icon        = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.Icon.png");
            _controller1Image.Pixbuf = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.JoyCon.png", 500, 500);

            //Bind Events
            _lStickUp1.Clicked       += (sender, args) => Button_Pressed(sender, args, _lStickUp1);
            _lStickDown1.Clicked     += (sender, args) => Button_Pressed(sender, args, _lStickDown1);
            _lStickLeft1.Clicked     += (sender, args) => Button_Pressed(sender, args, _lStickLeft1);
            _lStickRight1.Clicked    += (sender, args) => Button_Pressed(sender, args, _lStickRight1);
            _lStickButton1.Clicked   += (sender, args) => Button_Pressed(sender, args, _lStickButton1);
            _dpadUp1.Clicked         += (sender, args) => Button_Pressed(sender, args, _dpadUp1);
            _dpadDown1.Clicked       += (sender, args) => Button_Pressed(sender, args, _dpadDown1);
            _dpadLeft1.Clicked       += (sender, args) => Button_Pressed(sender, args, _dpadLeft1);
            _dpadRight1.Clicked      += (sender, args) => Button_Pressed(sender, args, _dpadRight1);
            _minus1.Clicked          += (sender, args) => Button_Pressed(sender, args, _minus1);
            _l1.Clicked              += (sender, args) => Button_Pressed(sender, args, _l1);
            _zL1.Clicked             += (sender, args) => Button_Pressed(sender, args, _zL1);
            _rStickUp1.Clicked       += (sender, args) => Button_Pressed(sender, args, _rStickUp1);
            _rStickDown1.Clicked     += (sender, args) => Button_Pressed(sender, args, _rStickDown1);
            _rStickLeft1.Clicked     += (sender, args) => Button_Pressed(sender, args, _rStickLeft1);
            _rStickRight1.Clicked    += (sender, args) => Button_Pressed(sender, args, _rStickRight1);
            _rStickButton1.Clicked   += (sender, args) => Button_Pressed(sender, args, _rStickButton1);
            _a1.Clicked              += (sender, args) => Button_Pressed(sender, args, _a1);
            _b1.Clicked              += (sender, args) => Button_Pressed(sender, args, _b1);
            _x1.Clicked              += (sender, args) => Button_Pressed(sender, args, _x1);
            _y1.Clicked              += (sender, args) => Button_Pressed(sender, args, _y1);
            _plus1.Clicked           += (sender, args) => Button_Pressed(sender, args, _plus1);
            _r1.Clicked              += (sender, args) => Button_Pressed(sender, args, _r1);
            _zR1.Clicked             += (sender, args) => Button_Pressed(sender, args, _zR1);
            _controller1Type.Changed += (sender, args) => Controller_Changed(sender, args, _controller1Type.ActiveId, _controller1Image);

            //Setup Currents
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

            if (ConfigurationState.Instance.System.EnableDockedMode)
            {
                _dockedModeToggle.Click();
            }

            if (ConfigurationState.Instance.EnableDiscordIntegration)
            {
                _discordToggle.Click();
            }

            if (ConfigurationState.Instance.Graphics.EnableVsync)
            {
                _vSyncToggle.Click();
            }

            if (ConfigurationState.Instance.System.EnableMulticoreScheduling)
            {
                _multiSchedToggle.Click();
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

            TimeZoneContentManager timeZoneContentManager = new TimeZoneContentManager();

            timeZoneContentManager.InitializeInstance(virtualFileSystem, contentManager, LibHac.FsSystem.IntegrityCheckLevel.None);

            List<string> locationNames = timeZoneContentManager.LocationNameCache.ToList();

            locationNames.Sort();

            foreach (string locationName in locationNames)
            {
                _systemTimeZoneSelect.Append(locationName, locationName);
            }

            _systemLanguageSelect.SetActiveId(ConfigurationState.Instance.System.Language.Value.ToString());
            _systemRegionSelect  .SetActiveId(ConfigurationState.Instance.System.Region.Value.ToString());
            _systemTimeZoneSelect.SetActiveId(timeZoneContentManager.SanityCheckDeviceLocationName());
            _anisotropy          .SetActiveId(ConfigurationState.Instance.Graphics.MaxAnisotropy.Value.ToString());
            _controller1Type     .SetActiveId(ConfigurationState.Instance.Hid.ControllerType.Value.ToString());
            Controller_Changed(null, null, _controller1Type.ActiveId, _controller1Image);

            _lStickUp1.Label     = ConfigurationState.Instance.Hid.KeyboardControls.Value.LeftJoycon.StickUp.ToString();
            _lStickDown1.Label   = ConfigurationState.Instance.Hid.KeyboardControls.Value.LeftJoycon.StickDown.ToString();
            _lStickLeft1.Label   = ConfigurationState.Instance.Hid.KeyboardControls.Value.LeftJoycon.StickLeft.ToString();
            _lStickRight1.Label  = ConfigurationState.Instance.Hid.KeyboardControls.Value.LeftJoycon.StickRight.ToString();
            _lStickButton1.Label = ConfigurationState.Instance.Hid.KeyboardControls.Value.LeftJoycon.StickButton.ToString();
            _dpadUp1.Label       = ConfigurationState.Instance.Hid.KeyboardControls.Value.LeftJoycon.DPadUp.ToString();
            _dpadDown1.Label     = ConfigurationState.Instance.Hid.KeyboardControls.Value.LeftJoycon.DPadDown.ToString();
            _dpadLeft1.Label     = ConfigurationState.Instance.Hid.KeyboardControls.Value.LeftJoycon.DPadLeft.ToString();
            _dpadRight1.Label    = ConfigurationState.Instance.Hid.KeyboardControls.Value.LeftJoycon.DPadRight.ToString();
            _minus1.Label        = ConfigurationState.Instance.Hid.KeyboardControls.Value.LeftJoycon.ButtonMinus.ToString();
            _l1.Label            = ConfigurationState.Instance.Hid.KeyboardControls.Value.LeftJoycon.ButtonL.ToString();
            _zL1.Label           = ConfigurationState.Instance.Hid.KeyboardControls.Value.LeftJoycon.ButtonZl.ToString();
            _rStickUp1.Label     = ConfigurationState.Instance.Hid.KeyboardControls.Value.RightJoycon.StickUp.ToString();
            _rStickDown1.Label   = ConfigurationState.Instance.Hid.KeyboardControls.Value.RightJoycon.StickDown.ToString();
            _rStickLeft1.Label   = ConfigurationState.Instance.Hid.KeyboardControls.Value.RightJoycon.StickLeft.ToString();
            _rStickRight1.Label  = ConfigurationState.Instance.Hid.KeyboardControls.Value.RightJoycon.StickRight.ToString();
            _rStickButton1.Label = ConfigurationState.Instance.Hid.KeyboardControls.Value.RightJoycon.StickButton.ToString();
            _a1.Label            = ConfigurationState.Instance.Hid.KeyboardControls.Value.RightJoycon.ButtonA.ToString();
            _b1.Label            = ConfigurationState.Instance.Hid.KeyboardControls.Value.RightJoycon.ButtonB.ToString();
            _x1.Label            = ConfigurationState.Instance.Hid.KeyboardControls.Value.RightJoycon.ButtonX.ToString();
            _y1.Label            = ConfigurationState.Instance.Hid.KeyboardControls.Value.RightJoycon.ButtonY.ToString();
            _plus1.Label         = ConfigurationState.Instance.Hid.KeyboardControls.Value.RightJoycon.ButtonPlus.ToString();
            _r1.Label            = ConfigurationState.Instance.Hid.KeyboardControls.Value.RightJoycon.ButtonR.ToString();
            _zR1.Label           = ConfigurationState.Instance.Hid.KeyboardControls.Value.RightJoycon.ButtonZr.ToString();

            _custThemePath.Buffer.Text           = ConfigurationState.Instance.Ui.CustomThemePath;
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

            _listeningForKeypress = false;

            //Setup system time spinners
            UpdateSystemTimeSpinners();
        }

        private void UpdateSystemTimeSpinners()
        {
            //Unbind system time spin events
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

            //Bind system time spin button events
            _systemTimeYearSpin.ValueChanged   += SystemTimeSpin_ValueChanged;
            _systemTimeMonthSpin.ValueChanged  += SystemTimeSpin_ValueChanged;
            _systemTimeDaySpin.ValueChanged    += SystemTimeSpin_ValueChanged;
            _systemTimeHourSpin.ValueChanged   += SystemTimeSpin_ValueChanged;
            _systemTimeMinuteSpin.ValueChanged += SystemTimeSpin_ValueChanged;
        }

        //Events
        private void SystemTimeSpin_ValueChanged(Object sender, EventArgs e)
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

        private void Button_Pressed(object sender, EventArgs args, ToggleButton button)
        {
            if (_listeningForKeypress == false)
            {
                KeyPressEvent += On_KeyPress;

                _listeningForKeypress = true;

                void On_KeyPress(object o, KeyPressEventArgs keyPressed)
                {
                    string key    = keyPressed.Event.Key.ToString();
                    string capKey = key.First().ToString().ToUpper() + key.Substring(1);

                    if (Enum.IsDefined(typeof(Configuration.Hid.Key), capKey))
                    {
                        button.Label = capKey;
                    }
                    else if (GdkToOpenTkInput.ContainsKey(key))
                    {
                        button.Label = GdkToOpenTkInput[key];
                    }
                    else
                    {
                        button.Label = "Space";
                    }

                    button.SetStateFlags(0, true);

                    KeyPressEvent -= On_KeyPress;

                    _listeningForKeypress = false;
                }
            }
            else
            {
                button.SetStateFlags(0, true);
            }
        }

        private void Controller_Changed(object sender, EventArgs args, string controllerType, Image controllerImage)
        {
            switch (controllerType)
            {
                case "ProController":
                    controllerImage.Pixbuf = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.ProCon.png", 500, 500);
                    break;
                case "NpadLeft":
                    controllerImage.Pixbuf = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.BlueCon.png", 500, 500);
                    break;
                case "NpadRight":
                    controllerImage.Pixbuf = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.RedCon.png", 500, 500);
                    break;
                default:
                    controllerImage.Pixbuf = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.JoyCon.png", 500, 500);
                    break;
            }
        }

        private void AddDir_Pressed(object sender, EventArgs args)
        {
            if (Directory.Exists(_addGameDirBox.Buffer.Text))
            {
                _gameDirsBoxStore.AppendValues(_addGameDirBox.Buffer.Text);
            }

            _addDir.SetStateFlags(0, true);
        }

        private void BrowseDir_Pressed(object sender, EventArgs args)
        {
            FileChooserDialog fileChooser = new FileChooserDialog("Choose the game directory to add to the list", this, FileChooserAction.SelectFolder, "Cancel", ResponseType.Cancel, "Add", ResponseType.Accept);

            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                _gameDirsBoxStore.AppendValues(fileChooser.Filename);
            }

            fileChooser.Dispose();

            _browseDir.SetStateFlags(0, true);
        }

        private void RemoveDir_Pressed(object sender, EventArgs args)
        {
            TreeSelection selection = _gameDirsBox.Selection;

            selection.GetSelected(out TreeIter treeIter);
            _gameDirsBoxStore.Remove(ref treeIter);

            _removeDir.SetStateFlags(0, true);
        }

        private void CustThemeToggle_Activated(object sender, EventArgs args)
        {
            _custThemePath.Sensitive      = _custThemeToggle.Active;
            _custThemePathLabel.Sensitive = _custThemeToggle.Active;
            _browseThemePath.Sensitive    = _custThemeToggle.Active;
        }

        private void BrowseThemeDir_Pressed(object sender, EventArgs args)
        {
            FileChooserDialog fileChooser = new FileChooserDialog("Choose the theme to load", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Select", ResponseType.Accept);

            fileChooser.Filter = new FileFilter();
            fileChooser.Filter.AddPattern("*.css");

            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                _custThemePath.Buffer.Text = fileChooser.Filename;
            }

            fileChooser.Dispose();

            _browseThemePath.SetStateFlags(0, true);
        }

        private void OpenLogsFolder_Pressed(object sender, EventArgs args)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName        = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"),
                UseShellExecute = true,
                Verb            = "open"
            });
        }

        private void SaveToggle_Activated(object sender, EventArgs args)
        {
            List<string> gameDirs = new List<string>();

            _gameDirsBoxStore.GetIterFirst(out TreeIter treeIter);
            for (int i = 0; i < _gameDirsBoxStore.IterNChildren(); i++)
            {
                _gameDirsBoxStore.GetValue(treeIter, i);

                gameDirs.Add((string)_gameDirsBoxStore.GetValue(treeIter, 0));

                _gameDirsBoxStore.IterNext(ref treeIter);
            }

            ConfigurationState.Instance.Logger.EnableError.Value               = _errorLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableWarn.Value                = _warningLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableInfo.Value                = _infoLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableStub.Value                = _stubLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableDebug.Value               = _debugLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableGuest.Value               = _guestLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableFsAccessLog.Value         = _fsAccessLogToggle.Active;
            ConfigurationState.Instance.Logger.EnableFileLog.Value             = _fileLogToggle.Active;
            ConfigurationState.Instance.System.EnableDockedMode.Value          = _dockedModeToggle.Active;
            ConfigurationState.Instance.EnableDiscordIntegration.Value         = _discordToggle.Active;
            ConfigurationState.Instance.Graphics.EnableVsync.Value             = _vSyncToggle.Active;
            ConfigurationState.Instance.System.EnableMulticoreScheduling.Value = _multiSchedToggle.Active;
            ConfigurationState.Instance.System.EnableFsIntegrityChecks.Value   = _fsicToggle.Active;
            ConfigurationState.Instance.System.IgnoreMissingServices.Value     = _ignoreToggle.Active;
            ConfigurationState.Instance.Hid.EnableKeyboard.Value               = _directKeyboardAccess.Active;
            ConfigurationState.Instance.Ui.EnableCustomTheme.Value             = _custThemeToggle.Active;

            ConfigurationState.Instance.Hid.KeyboardControls.Value.LeftJoycon = new NpadKeyboardLeft()
            {
                StickUp     = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _lStickUp1.Label),
                StickDown   = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _lStickDown1.Label),
                StickLeft   = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _lStickLeft1.Label),
                StickRight  = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _lStickRight1.Label),
                StickButton = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _lStickButton1.Label),
                DPadUp      = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _dpadUp1.Label),
                DPadDown    = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _dpadDown1.Label),
                DPadLeft    = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _dpadLeft1.Label),
                DPadRight   = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _dpadRight1.Label),
                ButtonMinus = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _minus1.Label),
                ButtonL     = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _l1.Label),
                ButtonZl    = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _zL1.Label),
            };

            ConfigurationState.Instance.Hid.KeyboardControls.Value.RightJoycon = new NpadKeyboardRight()
            {
                StickUp     = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _rStickUp1.Label),
                StickDown   = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _rStickDown1.Label),
                StickLeft   = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _rStickLeft1.Label),
                StickRight  = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _rStickRight1.Label),
                StickButton = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _rStickButton1.Label),
                ButtonA     = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _a1.Label),
                ButtonB     = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _b1.Label),
                ButtonX     = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _x1.Label),
                ButtonY     = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _y1.Label),
                ButtonPlus  = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _plus1.Label),
                ButtonR     = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _r1.Label),
                ButtonZr    = (Configuration.Hid.Key)Enum.Parse(typeof(Configuration.Hid.Key), _zR1.Label),
            };

            ConfigurationState.Instance.System.Language.Value              = (Language)Enum.Parse(typeof(Language), _systemLanguageSelect.ActiveId);
            ConfigurationState.Instance.System.Region.Value                = (Configuration.System.Region)Enum.Parse(typeof(Configuration.System.Region), _systemRegionSelect.ActiveId);
            ConfigurationState.Instance.Graphics.MaxAnisotropy.Value       = float.Parse(_anisotropy.ActiveId);
            ConfigurationState.Instance.Hid.ControllerType.Value           = (ControllerType)Enum.Parse(typeof(ControllerType), _controller1Type.ActiveId);
            ConfigurationState.Instance.Ui.CustomThemePath.Value           = _custThemePath.Buffer.Text;
            ConfigurationState.Instance.Graphics.ShadersDumpPath.Value     = _graphicsShadersDumpPath.Buffer.Text;
            ConfigurationState.Instance.Ui.GameDirs.Value                  = gameDirs;
            ConfigurationState.Instance.System.FsGlobalAccessLogMode.Value = (int)_fsLogSpinAdjustment.Value;

            ConfigurationState.Instance.System.TimeZone.Value              = _systemTimeZoneSelect.ActiveId;
            ConfigurationState.Instance.System.SystemTimeOffset.Value      = _systemTimeOffset;

            MainWindow.SaveConfig();
            MainWindow.ApplyTheme();
            MainWindow.UpdateGameTable();
            Dispose();
        }

        private void CloseToggle_Activated(object sender, EventArgs args)
        {
            Dispose();
        }

        public readonly Dictionary<string, string> GdkToOpenTkInput = new Dictionary<string, string>()
        {
            { "Key_0",       "Number0"        },
            { "Key_1",       "Number1"        },
            { "Key_2",       "Number2"        },
            { "Key_3",       "Number3"        },
            { "Key_4",       "Number4"        },
            { "Key_5",       "Number5"        },
            { "Key_6",       "Number6"        },
            { "Key_7",       "Number7"        },
            { "Key_8",       "Number8"        },
            { "Key_9",       "Number9"        },
            { "equal",       "Plus"           },
            { "uparrow",     "Up"             },
            { "downarrow",   "Down"           },
            { "leftarrow",   "Left"           },
            { "rightarrow",  "Right"          },
            { "Control_L",   "ControlLeft"    },
            { "Control_R",   "ControlRight"   },
            { "Shift_L",     "ShiftLeft"      },
            { "Shift_R",     "ShiftRight"     },
            { "Alt_L",       "AltLeft"        },
            { "Alt_R",       "AltRight"       },
            { "Page_Up",     "PageUp"         },
            { "Page_Down",   "PageDown"       },
            { "KP_Enter",    "KeypadEnter"    },
            { "KP_Up",       "Up"             },
            { "KP_Down",     "Down"           },
            { "KP_Left",     "Left"           },
            { "KP_Right",    "Right"          },
            { "KP_Divide",   "KeypadDivide"   },
            { "KP_Multiply", "KeypadMultiply" },
            { "KP_Subtract", "KeypadSubtract" },
            { "KP_Add",      "KeypadAdd"      },
            { "KP_Decimal",  "KeypadDecimal"  },
            { "KP_0",        "Keypad0"        },
            { "KP_1",        "Keypad1"        },
            { "KP_2",        "Keypad2"        },
            { "KP_3",        "Keypad3"        },
            { "KP_4",        "Keypad4"        },
            { "KP_5",        "Keypad5"        },
            { "KP_6",        "Keypad6"        },
            { "KP_7",        "Keypad7"        },
            { "KP_8",        "Keypad8"        },
            { "KP_9",        "Keypad9"        },
        };
    }
}
