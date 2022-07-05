using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Windows;
using Ryujinx.HLE.HOS.Applets;
using System;
using System.Threading.Tasks;

namespace Ryujinx.Ava.Ui.Controls
{
    internal class SwkbdAppletDialog : UserControl
    {
        private Predicate<int> _checkLength;
        private int _inputMax;
        private int _inputMin;
        private string _placeholder;

        private ContentDialog _host;

        public SwkbdAppletDialog(string mainText, string secondaryText, string placeholder)
        {
            MainText = mainText;
            SecondaryText = secondaryText;
            DataContext = this;
            _placeholder = placeholder;
            InitializeComponent();

            SetInputLengthValidation(0, int.MaxValue); // Disable by default.
        }

        public SwkbdAppletDialog()
        {
            DataContext = this;
            InitializeComponent();
        }

        public string Message { get; set; } = "";
        public string MainText { get; set; } = "";
        public string SecondaryText { get; set; } = "";

        public TextBlock Error { get; private set; }
        public TextBox Input { get; set; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Error = this.FindControl<TextBlock>("Error");
            Input = this.FindControl<TextBox>("Input");

            Input.Watermark = _placeholder;

            Input.AddHandler(TextInputEvent, Message_TextInput, RoutingStrategies.Tunnel, true);
        }

        public static async Task<(UserResult Result, string Input)> ShowInputDialog(StyleableWindow window, string title, SoftwareKeyboardUiArgs args)
        {
            ContentDialog contentDialog = window.ContentDialog;

            UserResult result = UserResult.Cancel;

            SwkbdAppletDialog content = new SwkbdAppletDialog(args.HeaderText, args.SubtitleText, args.GuideText)
            {
                Message = args.InitialText ?? ""
            };

            string input = string.Empty;

            content.SetInputLengthValidation(args.StringLengthMin, args.StringLengthMax);

            if (contentDialog != null)
            {
                content._host = contentDialog;
                contentDialog.Title = title;
                contentDialog.PrimaryButtonText = args.SubmitText;
                contentDialog.IsPrimaryButtonEnabled = content._checkLength(content.Message.Length);
                contentDialog.SecondaryButtonText = "";
                contentDialog.CloseButtonText = LocaleManager.Instance["InputDialogCancel"];
                contentDialog.Content = content;
                TypedEventHandler<ContentDialog, ContentDialogClosedEventArgs> handler = (sender, eventArgs) =>
                {
                    if (eventArgs.Result == ContentDialogResult.Primary)
                    {
                        result = UserResult.Ok;
                        input = content.Input.Text;
                    }
                };
                contentDialog.Closed += handler;
                await contentDialog.ShowAsync();
                contentDialog.Closed -= handler;
            }

            return (result, input);
        }

        public void SetInputLengthValidation(int min, int max)
        {
            _inputMin = Math.Min(min, max);
            _inputMax = Math.Max(min, max);

            Error.IsVisible = false;
            Error.FontStyle = FontStyle.Italic;

            if (_inputMin <= 0 && _inputMax == int.MaxValue) // Disable.
            {
                Error.IsVisible = false;

                _checkLength = length => true;
            }
            else if (_inputMin > 0 && _inputMax == int.MaxValue)
            {
                Error.IsVisible = true;
                Error.Text = string.Format(LocaleManager.Instance["SwkbdMinCharacters"], _inputMin);

                _checkLength = length => _inputMin <= length;
            }
            else
            {
                Error.IsVisible = true;
                Error.Text = string.Format(LocaleManager.Instance["SwkbdMinRangeCharacters"], _inputMin, _inputMax);

                _checkLength = length => _inputMin <= length && length <= _inputMax;
            }

            Message_TextInput(this, new TextInputEventArgs());
        }

        private void Message_TextInput(object sender, TextInputEventArgs e)
        {
            if (_host != null)
            {
                _host.IsPrimaryButtonEnabled = _checkLength(Message.Length);
            }
        }

        private void Message_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _host.IsPrimaryButtonEnabled)
            {
                _host.Hide(ContentDialogResult.Primary);
            }
            else
            {
                _host.IsPrimaryButtonEnabled = _checkLength(Message.Length);
            }
        }
    }
}