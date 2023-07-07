using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Windows.Input;

namespace Ryujinx.Ava.UI.Helpers
{
    public class HotKeyControl : ContentControl, ICommandSource
    {
        public static readonly StyledProperty<object> CommandParameterProperty =
            AvaloniaProperty.Register<HotKeyControl, object>(nameof(CommandParameter));

        public static readonly DirectProperty<HotKeyControl, ICommand> CommandProperty =
            AvaloniaProperty.RegisterDirect<HotKeyControl, ICommand>(nameof(Command),
                control => control.Command, (control, command) => control.Command = command, enableDataValidation: true);

        public static readonly StyledProperty<KeyGesture> HotKeyProperty = HotKeyManager.HotKeyProperty.AddOwner<Button>();

        private ICommand _command;
        private bool _commandCanExecute;

        public ICommand Command
        {
            get { return _command; }
            set { SetAndRaise(CommandProperty, ref _command, value); }
        }

        public KeyGesture HotKey
        {
            get { return GetValue(HotKeyProperty); }
            set { SetValue(HotKeyProperty, value); }
        }

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public void CanExecuteChanged(object sender, EventArgs e)
        {
            var canExecute = Command == null || Command.CanExecute(CommandParameter);

            if (canExecute != _commandCanExecute)
            {
                _commandCanExecute = canExecute;
                UpdateIsEffectivelyEnabled();
            }
        }
    }
}
