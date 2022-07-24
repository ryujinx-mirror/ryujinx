using Avalonia.Controls;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Models;
using Ryujinx.Ava.Ui.Windows;
using Ryujinx.Common.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ryujinx.Ava.Ui.Controls
{
    public static class ContentDialogHelper
    {
        private static bool _isChoiceDialogOpen;

        private async static Task<UserResult> ShowContentDialog(
            string title,
            string primaryText,
            string secondaryText,
            string primaryButton,
            string secondaryButton,
            string closeButton,
            int iconSymbol,
            UserResult primaryButtonResult = UserResult.Ok)
        {
            UserResult result = UserResult.None;

            ContentDialog contentDialog = new ContentDialog();

            await ShowDialog();

            async Task ShowDialog()
            {
                contentDialog.Title = title;
                contentDialog.PrimaryButtonText = primaryButton;
                contentDialog.SecondaryButtonText = secondaryButton;
                contentDialog.CloseButtonText = closeButton;
                contentDialog.Content = CreateDialogTextContent(primaryText, secondaryText, iconSymbol);

                contentDialog.PrimaryButtonCommand = MiniCommand.Create(() =>
                {
                    result = primaryButtonResult;
                });
                contentDialog.SecondaryButtonCommand = MiniCommand.Create(() =>
                {
                    result = UserResult.No;
                });
                contentDialog.CloseButtonCommand = MiniCommand.Create(() =>
                {
                    result = UserResult.Cancel;
                });

                await contentDialog.ShowAsync(ContentDialogPlacement.Popup);
            }

            return result;
        }

        public async static Task<UserResult> ShowDeferredContentDialog(
            StyleableWindow window,
            string title,
            string primaryText,
            string secondaryText,
            string primaryButton,
            string secondaryButton,
            string closeButton,
            int iconSymbol,
            ManualResetEvent deferResetEvent,
            Func<Window, Task> doWhileDeferred = null)
        {
            bool startedDeferring = false;

            UserResult result = UserResult.None;

            ContentDialog contentDialog = new ContentDialog
            {
                Title = title,
                PrimaryButtonText = primaryButton,
                SecondaryButtonText = secondaryButton,
                CloseButtonText = closeButton,
                Content = CreateDialogTextContent(primaryText, secondaryText, iconSymbol),
                PrimaryButtonCommand = MiniCommand.Create(() =>
                {
                    result = primaryButton == LocaleManager.Instance["InputDialogYes"] ? UserResult.Yes : UserResult.Ok;
                }),
            };
            contentDialog.SecondaryButtonCommand = MiniCommand.Create(() =>
            {
                contentDialog.PrimaryButtonClick -= DeferClose;
                result = UserResult.No;
            });
            contentDialog.CloseButtonCommand = MiniCommand.Create(() =>
            {
                contentDialog.PrimaryButtonClick -= DeferClose;
                result = UserResult.Cancel;
            });
            contentDialog.PrimaryButtonClick += DeferClose;
            await contentDialog.ShowAsync(ContentDialogPlacement.Popup);

            return result;

            async void DeferClose(ContentDialog sender, ContentDialogButtonClickEventArgs args)
            {
                if (startedDeferring)
                {
                    return;
                }

                contentDialog.PrimaryButtonClick -= DeferClose;

                startedDeferring = true;

                var deferral = args.GetDeferral();

                result = primaryButton == LocaleManager.Instance["InputDialogYes"] ? UserResult.Yes : UserResult.Ok;

                contentDialog.PrimaryButtonClick -= DeferClose;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(() =>
                {
                    deferResetEvent.WaitOne();

                    Dispatcher.UIThread.Post(() =>
                    {
                        deferral.Complete();
                    });
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                if (doWhileDeferred != null)
                {
                    await doWhileDeferred(window);

                    deferResetEvent.Set();
                }
            }
        }

        private static Grid CreateDialogTextContent(string primaryText, string secondaryText, int symbol)
        {
            Grid content = new Grid();
            content.RowDefinitions = new RowDefinitions() { new RowDefinition(), new RowDefinition() };
            content.ColumnDefinitions = new ColumnDefinitions() { new ColumnDefinition(GridLength.Auto), new ColumnDefinition() };

            content.MinHeight = 80;

            SymbolIcon icon = new SymbolIcon { Symbol = (Symbol)symbol, Margin = new Avalonia.Thickness(10) };
            icon.FontSize = 40;
            icon.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            Grid.SetColumn(icon, 0);
            Grid.SetRowSpan(icon, 2);
            Grid.SetRow(icon, 0);

            TextBlock primaryLabel = new TextBlock()
            {
                Text = primaryText,
                Margin = new Avalonia.Thickness(5),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                MaxWidth = 450
            };
            TextBlock secondaryLabel = new TextBlock()
            {
                Text = secondaryText,
                Margin = new Avalonia.Thickness(5),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                MaxWidth = 450
            };

            Grid.SetColumn(primaryLabel, 1);
            Grid.SetColumn(secondaryLabel, 1);
            Grid.SetRow(primaryLabel, 0);
            Grid.SetRow(secondaryLabel, 1);

            content.Children.Add(icon);
            content.Children.Add(primaryLabel);
            content.Children.Add(secondaryLabel);

            return content;
        }

        public static async Task<UserResult> CreateInfoDialog(
            string primary,
            string secondaryText,
            string acceptButton,
            string closeButton,
            string title)
        {
            return await ShowContentDialog(
                title,
                primary,
                secondaryText,
                acceptButton,
                "",
                closeButton,
                (int)Symbol.Important);
        }

        internal static async Task<UserResult> CreateConfirmationDialog(
            string primaryText,
            string secondaryText,
            string acceptButtonText,
            string cancelButtonText,
            string title,
            UserResult primaryButtonResult = UserResult.Yes)
        {
            return await ShowContentDialog(
                string.IsNullOrWhiteSpace(title) ? LocaleManager.Instance["DialogConfirmationTitle"] : title,
                primaryText,
                secondaryText,
                acceptButtonText,
                "",
                cancelButtonText,
                (int)Symbol.Help,
                primaryButtonResult);
        }

        internal static UpdateWaitWindow CreateWaitingDialog(string mainText, string secondaryText)
        {
            return new(mainText, secondaryText);
        }

        internal static async Task CreateUpdaterInfoDialog(string primary, string secondaryText)
        {
            await ShowContentDialog(
                LocaleManager.Instance["DialogUpdaterTitle"],
                primary,
                secondaryText,
                "",
                "",
                LocaleManager.Instance["InputDialogOk"],
                (int)Symbol.Important);
        }

        internal static async Task CreateWarningDialog(string primary, string secondaryText)
        {
            await ShowContentDialog(
                LocaleManager.Instance["DialogWarningTitle"],
                primary,
                secondaryText,
                "",
                "",
                LocaleManager.Instance["InputDialogOk"],
                (int)Symbol.Important);
        }

        internal static async Task CreateErrorDialog(string errorMessage, string secondaryErrorMessage = "")
        {
            Logger.Error?.Print(LogClass.Application, errorMessage);

            await ShowContentDialog(
                LocaleManager.Instance["DialogErrorTitle"],
                LocaleManager.Instance["DialogErrorMessage"],
                errorMessage,
                secondaryErrorMessage,
                "",
                LocaleManager.Instance["InputDialogOk"],
                (int)Symbol.Dismiss);
        }

        internal static async Task<bool> CreateChoiceDialog(string title, string primary, string secondaryText)
        {
            if (_isChoiceDialogOpen)
            {
                return false;
            }

            _isChoiceDialogOpen = true;

            UserResult response =
                await ShowContentDialog(
                    title,
                    primary,
                    secondaryText,
                    LocaleManager.Instance["InputDialogYes"],
                    "",
                    LocaleManager.Instance["InputDialogNo"],
                    (int)Symbol.Help,
                    UserResult.Yes);

            _isChoiceDialogOpen = false;

            return response == UserResult.Yes;
        }

        internal static async Task<bool> CreateExitDialog()
        {
            return await CreateChoiceDialog(
                LocaleManager.Instance["DialogExitTitle"],
                LocaleManager.Instance["DialogExitMessage"],
                LocaleManager.Instance["DialogExitSubMessage"]);
        }

        internal static async Task<bool> CreateStopEmulationDialog()
        {
            return await CreateChoiceDialog(
                LocaleManager.Instance["DialogStopEmulationTitle"],
                LocaleManager.Instance["DialogStopEmulationMessage"],
                LocaleManager.Instance["DialogExitSubMessage"]);
        }

        internal static async Task<string> CreateInputDialog(
            string title,
            string mainText,
            string subText,
            uint maxLength = int.MaxValue,
            string input = "")
        {
            var result = await InputDialog.ShowInputDialog(
                title,
                mainText,
                input,
                subText,
                maxLength);

            if (result.Result == UserResult.Ok)
            {
                return result.Input;
            }

            return string.Empty;
        }
    }
}