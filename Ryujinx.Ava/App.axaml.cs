using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using FluentAvalonia.Styling;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Ava.Ui.Controls;
using Ryujinx.Ava.Ui.Windows;
using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Ui.Common.Configuration;
using System;
using System.Diagnostics;
using System.IO;

namespace Ryujinx.Ava
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();

            if (Program.PreviewerDetached)
            {
                ApplyConfiguredTheme();

                ConfigurationState.Instance.Ui.BaseStyle.Event += ThemeChanged_Event;
                ConfigurationState.Instance.Ui.CustomThemePath.Event += ThemeChanged_Event;
                ConfigurationState.Instance.Ui.EnableCustomTheme.Event += CustomThemeChanged_Event;
            }
        }

        private void CustomThemeChanged_Event(object sender, ReactiveEventArgs<bool> e)
        {
            ApplyConfiguredTheme();
        }

        private void ShowRestartDialog()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var result = await ContentDialogHelper.CreateConfirmationDialog(
                        (desktop.MainWindow as MainWindow).SettingsWindow,
                        LocaleManager.Instance["DialogThemeRestartMessage"],
                        LocaleManager.Instance["DialogThemeRestartSubMessage"],
                        LocaleManager.Instance["InputDialogYes"],
                        LocaleManager.Instance["InputDialogNo"],
                        LocaleManager.Instance["DialogRestartRequiredMessage"]);

                    if (result == UserResult.Yes)
                    {
                        var path = Process.GetCurrentProcess().MainModule.FileName;
                        var info = new ProcessStartInfo() { FileName = path, UseShellExecute = false };
                        var proc = Process.Start(info);
                        desktop.Shutdown();
                        Environment.Exit(0);
                    }
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void ThemeChanged_Event(object sender, ReactiveEventArgs<string> e)
        {
            ApplyConfiguredTheme();
        }

        private void ApplyConfiguredTheme()
        {
            try
            {
                string baseStyle = ConfigurationState.Instance.Ui.BaseStyle;
                string themePath = ConfigurationState.Instance.Ui.CustomThemePath;
                bool enableCustomTheme = ConfigurationState.Instance.Ui.EnableCustomTheme;

                const string BaseStyleUrl = "avares://Ryujinx.Ava/Assets/Styles/Base{0}.xaml";

                if (string.IsNullOrWhiteSpace(baseStyle))
                {
                    ConfigurationState.Instance.Ui.BaseStyle.Value = "Dark";

                    baseStyle = ConfigurationState.Instance.Ui.BaseStyle;
                }

                var theme = AvaloniaLocator.Current.GetService<FluentAvaloniaTheme>();

                theme.RequestedTheme = baseStyle;

                var currentStyles = this.Styles;

                // Remove all styles except the base style.
                if (currentStyles.Count > 1)
                {
                    currentStyles.RemoveRange(1, currentStyles.Count - 1);
                }

                IStyle newStyles = null;

                // Load requested style, and fallback to Dark theme if loading failed.
                try
                {
                    newStyles = (Styles)AvaloniaXamlLoader.Load(new Uri(string.Format(BaseStyleUrl, baseStyle), UriKind.Absolute));
                }
                catch (XamlLoadException)
                {
                    newStyles = (Styles)AvaloniaXamlLoader.Load(new Uri(string.Format(BaseStyleUrl, "Dark"), UriKind.Absolute));
                }

                currentStyles.Add(newStyles);

                if (enableCustomTheme)
                {
                    if (!string.IsNullOrWhiteSpace(themePath))
                    {
                        try
                        {
                            var themeContent = File.ReadAllText(themePath);
                            var customStyle = AvaloniaRuntimeXamlLoader.Parse<IStyle>(themeContent);

                            currentStyles.Add(customStyle);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error?.Print(LogClass.Application, $"Failed to Apply Custom Theme. Error: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception)
            {
                Logger.Warning?.Print(LogClass.Application, "Failed to Apply Theme. A restart is needed to apply the selected theme");

                ShowRestartDialog();
            }
        }
    }
}