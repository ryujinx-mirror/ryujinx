using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Models;
using Ryujinx.Ava.Ui.Windows;
using System.Threading.Tasks;

namespace Ryujinx.Ava.Ui.Controls
{
    public class InputDialog : UserControl
    {
        public string Message { get; set; }
        public string Input { get; set; }
        public string SubMessage { get; set; }

        public uint MaxLength { get; }

        public InputDialog(string message, string input = "", string subMessage = "", uint maxLength = int.MaxValue)
        {
            Message = message;
            Input = input;
            SubMessage = subMessage;
            MaxLength = maxLength;

            DataContext = this;

            InitializeComponent();
        }

        public InputDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public static async Task<(UserResult Result, string Input)> ShowInputDialog(StyleableWindow window, string title, string message, string input = "", string subMessage = "", uint maxLength = int.MaxValue)
        {
            ContentDialog contentDialog = window.ContentDialog;

            UserResult result = UserResult.Cancel;

            InputDialog content = new InputDialog(message, input = "", subMessage = "", maxLength);

            if (contentDialog != null)
            {
                contentDialog.Title = title;
                contentDialog.PrimaryButtonText = LocaleManager.Instance["InputDialogOk"];
                contentDialog.SecondaryButtonText = "";
                contentDialog.CloseButtonText = LocaleManager.Instance["InputDialogCancel"];
                contentDialog.Content = content;
                contentDialog.PrimaryButtonCommand = MiniCommand.Create(() =>
                {
                    result = UserResult.Ok;
                    input = content.Input;
                });
                await contentDialog.ShowAsync();
            }

            return (result, input);
        }
    }
}