using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Ryujinx.Ava.Input;
using Ryujinx.Ava.UI.Helpers;
using Ryujinx.Input;
using Ryujinx.Input.Assigner;

namespace Ryujinx.Ava.UI.Views.Settings
{
    public partial class SettingsHotkeysView : UserControl
    {
        private ButtonKeyAssigner _currentAssigner;
        private IGamepadDriver AvaloniaKeyboardDriver;
    
        public SettingsHotkeysView()
        {
            InitializeComponent();
            AvaloniaKeyboardDriver = new AvaloniaKeyboardDriver(this);
        }

        private void MouseClick(object sender, PointerPressedEventArgs e)
        {
            bool shouldUnbind = e.GetCurrentPoint(this).Properties.IsMiddleButtonPressed;

            _currentAssigner?.Cancel(shouldUnbind);

            PointerPressed -= MouseClick;
        }
    
        private void Button_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton button)
            {
                if (_currentAssigner != null && button == _currentAssigner.ToggledButton)
                {
                    return;
                }

                if (_currentAssigner == null && button.IsChecked != null && (bool)button.IsChecked)
                {
                    _currentAssigner = new ButtonKeyAssigner(button);

                    FocusManager.Instance?.Focus(this, NavigationMethod.Pointer);

                    PointerPressed += MouseClick;

                    var keyboard = (IKeyboard)AvaloniaKeyboardDriver.GetGamepad(AvaloniaKeyboardDriver.GamepadsIds[0]);
                    IButtonAssigner assigner = new KeyboardKeyAssigner(keyboard);

                    _currentAssigner.GetInputAndAssign(assigner);
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

        private void Button_Unchecked(object sender, RoutedEventArgs e)
        {
            _currentAssigner?.Cancel();
            _currentAssigner = null;
        }

        public void Dispose()
        {
            _currentAssigner?.Cancel();
            _currentAssigner = null;
        }
    }
}