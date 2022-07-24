using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Models;
using Ryujinx.Ava.Ui.Windows;
using System.Threading.Tasks;

namespace Ryujinx.Ava.Ui.Controls
{
    public partial class InputDialog : UserControl
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
        }

        public InputDialog()
        {
            InitializeComponent();
        }

        public static async Task<(UserResult Result, string Input)> ShowInputDialog(string title, string message,
            string input = "", string subMessage = "", uint maxLength = int.MaxValue)
        {
            UserResult result = UserResult.Cancel;

            InputDialog content = new InputDialog(message, input, subMessage, maxLength);
            ContentDialog contentDialog = new ContentDialog
            {
                Title = title,
                PrimaryButtonText = LocaleManager.Instance["InputDialogOk"],
                SecondaryButtonText = "",
                CloseButtonText = LocaleManager.Instance["InputDialogCancel"],
                Content = content,
                PrimaryButtonCommand = MiniCommand.Create(() =>
                {
                    result = UserResult.Ok;
                    input = content.Input;
                })
            };
            await contentDialog.ShowAsync();

            return (result, input);
        }
    }
}