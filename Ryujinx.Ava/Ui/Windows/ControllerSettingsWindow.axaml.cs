using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Controls;
using Ryujinx.Ava.Ui.ViewModels;
using Ryujinx.Common.Configuration.Hid;
using Ryujinx.Common.Configuration.Hid.Controller;
using Ryujinx.Input;
using Ryujinx.Input.Assigner;
using Ryujinx.Ui.Common.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Key = Ryujinx.Input.Key;

namespace Ryujinx.Ava.Ui.Windows
{
    public class ControllerSettingsWindow : UserControl
    {
        private bool _dialogOpen;

        public Grid SettingButtons { get; set; }
        private ButtonKeyAssigner _currentAssigner;
        internal ControllerSettingsViewModel ViewModel { get; set; }

        public ControllerSettingsWindow()
        {
            DataContext = ViewModel = new ControllerSettingsViewModel(this);

            InitializeComponent();

            foreach (ILogical visual in SettingButtons.GetLogicalDescendants())
            {
                if (visual is ToggleButton button && !(visual is CheckBox))
                {
                    button.Checked += Button_Checked;
                    button.Unchecked += Button_Unchecked;
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            SettingButtons = this.FindControl<Grid>("SettingButtons");
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (_currentAssigner != null && _currentAssigner.ToggledButton != null && !_currentAssigner.ToggledButton.IsPointerOver)
            {
                _currentAssigner.Cancel();
            }
        }

        private void Button_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton button)
            {
                if (_currentAssigner != null && button == _currentAssigner.ToggledButton)
                {
                    return;
                }

                bool isStick = button.Tag != null && button.Tag.ToString() == "stick";

                if (_currentAssigner == null && (bool)button.IsChecked)
                {
                    _currentAssigner = new ButtonKeyAssigner(button);

                    FocusManager.Instance.Focus(this, NavigationMethod.Pointer);

                    PointerPressed += MouseClick;

                    IKeyboard keyboard = (IKeyboard)ViewModel.AvaloniaKeyboardDriver.GetGamepad("0"); // Open Avalonia keyboard for cancel operations.
                    IButtonAssigner assigner = CreateButtonAssigner(isStick);

                    _currentAssigner.ButtonAssigned += (sender, e) =>
                    {
                        if (e.IsAssigned)
                        {
                            ViewModel.IsModified = true;
                        }
                    };

                    _currentAssigner.GetInputAndAssign(assigner, keyboard);
                }
                else
                {
                    if (_currentAssigner != null)
                    {
                        ToggleButton oldButton = _currentAssigner.ToggledButton;

                        _currentAssigner.Cancel();
                        _currentAssigner = null;
                        button.IsChecked = false;
                    }
                }
            }
        }

        public void SaveCurrentProfile()
        {
            ViewModel.Save();
        }

        private IButtonAssigner CreateButtonAssigner(bool forStick)
        {
            IButtonAssigner assigner;

            var device = ViewModel.Devices[ViewModel.Device];

            if (device.Type == Models.DeviceType.Keyboard)
            {
                assigner = new KeyboardKeyAssigner((IKeyboard)ViewModel.SelectedGamepad);
            }
            else if (device.Type == Models.DeviceType.Controller)
            {
                InputConfig config = ConfigurationState.Instance.Hid.InputConfig.Value.Find(inputConfig => inputConfig.Id == ViewModel.SelectedGamepad.Id);

                assigner = new GamepadButtonAssigner(ViewModel.SelectedGamepad, (config as StandardControllerInputConfig).TriggerThreshold, forStick);
            }
            else
            {
                throw new Exception("Controller not supported");
            }

            return assigner;
        }

        private void Button_Unchecked(object sender, RoutedEventArgs e)
        {
            _currentAssigner?.Cancel();
            _currentAssigner = null;
        }

        private void MouseClick(object sender, PointerPressedEventArgs e)
        {
            bool shouldUnbind = false;

            if (e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed)
            {
                shouldUnbind = true;
            }

            _currentAssigner?.Cancel(shouldUnbind);

            PointerPressed -= MouseClick;
        }

        private async void PlayerIndexBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.IsModified && !_dialogOpen)
            {
                _dialogOpen = true;

                var result = await ContentDialogHelper.CreateConfirmationDialog(
                    this.GetVisualRoot() as StyleableWindow,
                    LocaleManager.Instance["DialogControllerSettingsModifiedConfirmMessage"],
                    LocaleManager.Instance["DialogControllerSettingsModifiedConfirmSubMessage"],
                    LocaleManager.Instance["InputDialogYes"],
                    LocaleManager.Instance["InputDialogNo"],
                    LocaleManager.Instance["RyujinxConfirm"]);

                if (result == UserResult.Yes)
                {
                    ViewModel.Save();
                }

                _dialogOpen = false;

                ViewModel.IsModified = false;

                if (e.AddedItems.Count > 0)
                {
                    (PlayerIndex key, _) = (KeyValuePair<PlayerIndex, string>)e.AddedItems[0];
                    ViewModel.PlayerId = key;
                }
            }
        }

        public void Dispose()
        {
            _currentAssigner?.Cancel();
            _currentAssigner = null;
            ViewModel.Dispose();
        }
    }
}