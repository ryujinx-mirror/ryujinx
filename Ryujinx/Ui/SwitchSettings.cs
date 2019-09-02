using Gtk;
using GUI = Gtk.Builder.ObjectAttribute;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.HLE.Input;
using Ryujinx.UI.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Ryujinx.UI
{
    public class SwitchSettings : Window
    {
        internal static Configuration SwitchConfig { get; set; }

        internal HLE.Switch Device { get; set; }

        private static ListStore _gameDirsBoxStore;

        private static bool _listeningForKeypress;

#pragma warning disable 649
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
        [GUI] CheckButton  _legacyJitToggle;
        [GUI] CheckButton  _ignoreToggle;
        [GUI] CheckButton  _directKeyboardAccess;
        [GUI] ComboBoxText _systemLanguageSelect;
        [GUI] CheckButton  _custThemeToggle;
        [GUI] Entry        _custThemePath;
        [GUI] ToggleButton _browseThemePath;
        [GUI] Label        _custThemePathLabel;
        [GUI] TreeView     _gameDirsBox;
        [GUI] Entry        _addGameDirBox;
        [GUI] ToggleButton _addDir;
        [GUI] ToggleButton _browseDir;
        [GUI] ToggleButton _removeDir;
        [GUI] Entry        _logPath;
        [GUI] Entry        _graphicsShadersDumpPath;
        [GUI] Image        _controllerImage;

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
#pragma warning restore 649

        public static void ConfigureSettings(Configuration Instance) { SwitchConfig = Instance; }

        public SwitchSettings(HLE.Switch device) : this(new Builder("Ryujinx.Ui.SwitchSettings.glade"), device) { }

        private SwitchSettings(Builder builder, HLE.Switch device) : base(builder.GetObject("_settingsWin").Handle)
        {
            Device = device;

            builder.Autoconnect(this);

            _settingsWin.Icon       = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.RyujinxIcon.png");
            _controllerImage.Pixbuf = new Gdk.Pixbuf(Assembly.GetExecutingAssembly(), "Ryujinx.Ui.assets.JoyCon.png", 500, 500);

            //Bind Events
            _lStickUp1.Clicked     += (o, args) => Button_Pressed(o, args, _lStickUp1);
            _lStickDown1.Clicked   += (o, args) => Button_Pressed(o, args, _lStickDown1);
            _lStickLeft1.Clicked   += (o, args) => Button_Pressed(o, args, _lStickLeft1);
            _lStickRight1.Clicked  += (o, args) => Button_Pressed(o, args, _lStickRight1);
            _lStickButton1.Clicked += (o, args) => Button_Pressed(o, args, _lStickButton1);
            _dpadUp1.Clicked       += (o, args) => Button_Pressed(o, args, _dpadUp1);
            _dpadDown1.Clicked     += (o, args) => Button_Pressed(o, args, _dpadDown1);
            _dpadLeft1.Clicked     += (o, args) => Button_Pressed(o, args, _dpadLeft1);
            _dpadRight1.Clicked    += (o, args) => Button_Pressed(o, args, _dpadRight1);
            _minus1.Clicked        += (o, args) => Button_Pressed(o, args, _minus1);
            _l1.Clicked            += (o, args) => Button_Pressed(o, args, _l1);
            _zL1.Clicked           += (o, args) => Button_Pressed(o, args, _zL1);
            _rStickUp1.Clicked     += (o, args) => Button_Pressed(o, args, _rStickUp1);
            _rStickDown1.Clicked   += (o, args) => Button_Pressed(o, args, _rStickDown1);
            _rStickLeft1.Clicked   += (o, args) => Button_Pressed(o, args, _rStickLeft1);
            _rStickRight1.Clicked  += (o, args) => Button_Pressed(o, args, _rStickRight1);
            _rStickButton1.Clicked += (o, args) => Button_Pressed(o, args, _rStickButton1);
            _a1.Clicked            += (o, args) => Button_Pressed(o, args, _a1);
            _b1.Clicked            += (o, args) => Button_Pressed(o, args, _b1);
            _x1.Clicked            += (o, args) => Button_Pressed(o, args, _x1);
            _y1.Clicked            += (o, args) => Button_Pressed(o, args, _y1);
            _plus1.Clicked         += (o, args) => Button_Pressed(o, args, _plus1);
            _r1.Clicked            += (o, args) => Button_Pressed(o, args, _r1);
            _zR1.Clicked           += (o, args) => Button_Pressed(o, args, _zR1);

            //Setup Currents
            if (SwitchConfig.EnableFileLog)             { _fileLogToggle.Click();        }
            if (SwitchConfig.LoggingEnableError)        { _errorLogToggle.Click();       }
            if (SwitchConfig.LoggingEnableWarn)         { _warningLogToggle.Click();     }
            if (SwitchConfig.LoggingEnableInfo)         { _infoLogToggle.Click();        }
            if (SwitchConfig.LoggingEnableStub)         { _stubLogToggle.Click();        }
            if (SwitchConfig.LoggingEnableDebug)        { _debugLogToggle.Click();       }
            if (SwitchConfig.LoggingEnableGuest)        { _guestLogToggle.Click();       }
            if (SwitchConfig.LoggingEnableFsAccessLog)  { _fsAccessLogToggle.Click();    }
            if (SwitchConfig.DockedMode)                { _dockedModeToggle.Click();     }
            if (SwitchConfig.EnableDiscordIntegration)  { _discordToggle.Click();        }
            if (SwitchConfig.EnableVsync)               { _vSyncToggle.Click();          }
            if (SwitchConfig.EnableMulticoreScheduling) { _multiSchedToggle.Click();     }
            if (SwitchConfig.EnableFsIntegrityChecks)   { _fsicToggle.Click();           }
            if (SwitchConfig.EnableLegacyJit)           { _legacyJitToggle.Click();      }
            if (SwitchConfig.IgnoreMissingServices)     { _ignoreToggle.Click();         }
            if (SwitchConfig.EnableKeyboard)            { _directKeyboardAccess.Click(); }
            if (SwitchConfig.EnableCustomTheme)         { _custThemeToggle.Click();      }

            _systemLanguageSelect.SetActiveId(SwitchConfig.SystemLanguage.ToString());
            _controller1Type     .SetActiveId(SwitchConfig.ControllerType.ToString());

            _lStickUp1.Label     = SwitchConfig.KeyboardControls.LeftJoycon.StickUp.ToString();
            _lStickDown1.Label   = SwitchConfig.KeyboardControls.LeftJoycon.StickDown.ToString();
            _lStickLeft1.Label   = SwitchConfig.KeyboardControls.LeftJoycon.StickLeft.ToString();
            _lStickRight1.Label  = SwitchConfig.KeyboardControls.LeftJoycon.StickRight.ToString();
            _lStickButton1.Label = SwitchConfig.KeyboardControls.LeftJoycon.StickButton.ToString();
            _dpadUp1.Label       = SwitchConfig.KeyboardControls.LeftJoycon.DPadUp.ToString();
            _dpadDown1.Label     = SwitchConfig.KeyboardControls.LeftJoycon.DPadDown.ToString();
            _dpadLeft1.Label     = SwitchConfig.KeyboardControls.LeftJoycon.DPadLeft.ToString();
            _dpadRight1.Label    = SwitchConfig.KeyboardControls.LeftJoycon.DPadRight.ToString();
            _minus1.Label        = SwitchConfig.KeyboardControls.LeftJoycon.ButtonMinus.ToString();
            _l1.Label            = SwitchConfig.KeyboardControls.LeftJoycon.ButtonL.ToString();
            _zL1.Label           = SwitchConfig.KeyboardControls.LeftJoycon.ButtonZl.ToString();
            _rStickUp1.Label     = SwitchConfig.KeyboardControls.RightJoycon.StickUp.ToString();
            _rStickDown1.Label   = SwitchConfig.KeyboardControls.RightJoycon.StickDown.ToString();
            _rStickLeft1.Label   = SwitchConfig.KeyboardControls.RightJoycon.StickLeft.ToString();
            _rStickRight1.Label  = SwitchConfig.KeyboardControls.RightJoycon.StickRight.ToString();
            _rStickButton1.Label = SwitchConfig.KeyboardControls.RightJoycon.StickButton.ToString();
            _a1.Label            = SwitchConfig.KeyboardControls.RightJoycon.ButtonA.ToString();
            _b1.Label            = SwitchConfig.KeyboardControls.RightJoycon.ButtonB.ToString();
            _x1.Label            = SwitchConfig.KeyboardControls.RightJoycon.ButtonX.ToString();
            _y1.Label            = SwitchConfig.KeyboardControls.RightJoycon.ButtonY.ToString();
            _plus1.Label         = SwitchConfig.KeyboardControls.RightJoycon.ButtonPlus.ToString();
            _r1.Label            = SwitchConfig.KeyboardControls.RightJoycon.ButtonR.ToString();
            _zR1.Label           = SwitchConfig.KeyboardControls.RightJoycon.ButtonZr.ToString();

            _custThemePath.Buffer.Text           = SwitchConfig.CustomThemePath;
            _graphicsShadersDumpPath.Buffer.Text = SwitchConfig.GraphicsShadersDumpPath;
            _fsLogSpinAdjustment.Value           = SwitchConfig.FsGlobalAccessLogMode;

            _gameDirsBox.AppendColumn("", new CellRendererText(), "text", 0);
            _gameDirsBoxStore  = new ListStore(typeof(string));
            _gameDirsBox.Model = _gameDirsBoxStore;
            foreach (string gameDir in SwitchConfig.GameDirs)
            {
                _gameDirsBoxStore.AppendValues(gameDir);
            }

            if (_custThemeToggle.Active == false)
            {
                _custThemePath.Sensitive      = false;
                _custThemePathLabel.Sensitive = false;
                _browseThemePath.Sensitive    = false;
            }

            _logPath.Buffer.Text = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Ryujinx.log");

            _listeningForKeypress = false;
        }

        //Events
        private void Button_Pressed(object obj, EventArgs args, ToggleButton Button)
        {
            if (_listeningForKeypress == false)
            {
                KeyPressEvent += On_KeyPress;

                _listeningForKeypress = true;

                void On_KeyPress(object Obj, KeyPressEventArgs KeyPressed)
                {
                    string key    = KeyPressed.Event.Key.ToString();
                    string capKey = key.First().ToString().ToUpper() + key.Substring(1);

                    if (Enum.IsDefined(typeof(OpenTK.Input.Key), capKey))
                    {
                        Button.Label = capKey;
                    }
                    else if (GdkToOpenTKInput.ContainsKey(key))
                    {
                        Button.Label = GdkToOpenTKInput[key];
                    }
                    else
                    {
                        Button.Label = "Space";
                    }

                    Button.SetStateFlags(0, true);

                    KeyPressEvent -= On_KeyPress;

                    _listeningForKeypress = false;
                }
            }
            else
            {
                Button.SetStateFlags(0, true);
            }
        }

        private void AddDir_Pressed(object obj, EventArgs args)
        {
            if (Directory.Exists(_addGameDirBox.Buffer.Text))
            {
                _gameDirsBoxStore.AppendValues(_addGameDirBox.Buffer.Text);
            }

            _addDir.SetStateFlags(0, true);
        }

        private void BrowseDir_Pressed(object obj, EventArgs args)
        {
            FileChooserDialog fileChooser = new FileChooserDialog("Choose the game directory to add to the list", this, FileChooserAction.SelectFolder, "Cancel", ResponseType.Cancel, "Add", ResponseType.Accept);

            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                _gameDirsBoxStore.AppendValues(fileChooser.Filename);
            }

            fileChooser.Destroy();

            _browseDir.SetStateFlags(0, true);
        }

        private void RemoveDir_Pressed(object obj, EventArgs args)
        {
            TreeSelection selection = _gameDirsBox.Selection;

            selection.GetSelected(out TreeIter treeIter);
            _gameDirsBoxStore.Remove(ref treeIter);

            _removeDir.SetStateFlags(0, true);
        }

        private void CustThemeToggle_Activated(object obj, EventArgs args)
        {
            _custThemePath.Sensitive      = _custThemeToggle.Active;
            _custThemePathLabel.Sensitive = _custThemeToggle.Active;
            _browseThemePath.Sensitive    = _custThemeToggle.Active;
        }

        private void BrowseThemeDir_Pressed(object obj, EventArgs args)
        {
            FileChooserDialog fileChooser = new FileChooserDialog("Choose the theme to load", this, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Select", ResponseType.Accept);

            fileChooser.Filter = new FileFilter();
            fileChooser.Filter.AddPattern("*.css");

            if (fileChooser.Run() == (int)ResponseType.Accept)
            {
                _custThemePath.Buffer.Text = fileChooser.Filename;
            }

            fileChooser.Destroy();

            _browseThemePath.SetStateFlags(0, true);
        }

        private void SaveToggle_Activated(object obj, EventArgs args)
        {
            List<string> gameDirs = new List<string>();

            _gameDirsBoxStore.GetIterFirst(out TreeIter treeIter);
            for (int i = 0; i < _gameDirsBoxStore.IterNChildren(); i++)
            {
                _gameDirsBoxStore.GetValue(treeIter, i);

                gameDirs.Add((string)_gameDirsBoxStore.GetValue(treeIter, 0));

                _gameDirsBoxStore.IterNext(ref treeIter);
            }

            SwitchConfig.LoggingEnableError        = _errorLogToggle.Active;
            SwitchConfig.LoggingEnableWarn         = _warningLogToggle.Active;
            SwitchConfig.LoggingEnableInfo         = _infoLogToggle.Active;
            SwitchConfig.LoggingEnableStub         = _stubLogToggle.Active;
            SwitchConfig.LoggingEnableDebug        = _debugLogToggle.Active;
            SwitchConfig.LoggingEnableGuest        = _guestLogToggle.Active;
            SwitchConfig.LoggingEnableFsAccessLog  = _fsAccessLogToggle.Active;
            SwitchConfig.EnableFileLog             = _fileLogToggle.Active;
            SwitchConfig.DockedMode                = _dockedModeToggle.Active;
            SwitchConfig.EnableDiscordIntegration  = _discordToggle.Active;
            SwitchConfig.EnableVsync               = _vSyncToggle.Active;
            SwitchConfig.EnableMulticoreScheduling = _multiSchedToggle.Active;
            SwitchConfig.EnableFsIntegrityChecks   = _fsicToggle.Active;
            SwitchConfig.EnableLegacyJit           = _legacyJitToggle.Active;
            SwitchConfig.IgnoreMissingServices     = _ignoreToggle.Active;
            SwitchConfig.EnableKeyboard            = _directKeyboardAccess.Active;
            SwitchConfig.EnableCustomTheme         = _custThemeToggle.Active;

            SwitchConfig.KeyboardControls.LeftJoycon = new NpadKeyboardLeft()
            {
                StickUp     = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _lStickUp1.Label),
                StickDown   = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _lStickDown1.Label),
                StickLeft   = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _lStickLeft1.Label),
                StickRight  = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _lStickRight1.Label),
                StickButton = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _lStickButton1.Label),
                DPadUp      = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _dpadUp1.Label),
                DPadDown    = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _dpadDown1.Label),
                DPadLeft    = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _dpadLeft1.Label),
                DPadRight   = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _dpadRight1.Label),
                ButtonMinus = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _minus1.Label),
                ButtonL     = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _l1.Label),
                ButtonZl    = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _zL1.Label),
            };

            SwitchConfig.KeyboardControls.RightJoycon = new NpadKeyboardRight()
            {
                StickUp     = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _rStickUp1.Label),
                StickDown   = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _rStickDown1.Label),
                StickLeft   = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _rStickLeft1.Label),
                StickRight  = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _rStickRight1.Label),
                StickButton = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _rStickButton1.Label),
                ButtonA     = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _a1.Label),
                ButtonB     = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _b1.Label),
                ButtonX     = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _x1.Label),
                ButtonY     = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _y1.Label),
                ButtonPlus  = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _plus1.Label),
                ButtonR     = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _r1.Label),
                ButtonZr    = (OpenTK.Input.Key)Enum.Parse(typeof(OpenTK.Input.Key), _zR1.Label),
            };

            SwitchConfig.SystemLanguage          = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), _systemLanguageSelect.ActiveId);
            SwitchConfig.ControllerType          = (ControllerStatus)Enum.Parse(typeof(ControllerStatus), _controller1Type.ActiveId);
            SwitchConfig.CustomThemePath         = _custThemePath.Buffer.Text;
            SwitchConfig.GraphicsShadersDumpPath = _graphicsShadersDumpPath.Buffer.Text;
            SwitchConfig.GameDirs                = gameDirs;
            SwitchConfig.FsGlobalAccessLogMode   = (int)_fsLogSpinAdjustment.Value;

            Configuration.SaveConfig(SwitchConfig, System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config.json"));
            Configuration.Configure(Device, SwitchConfig);

            MainWindow.ApplyTheme();
            MainWindow.UpdateGameTable();

            Destroy();
        }

        private void CloseToggle_Activated(object obj, EventArgs args)
        {
            Destroy();
        }

        public readonly Dictionary<string, string> GdkToOpenTKInput = new Dictionary<string, string>()
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
