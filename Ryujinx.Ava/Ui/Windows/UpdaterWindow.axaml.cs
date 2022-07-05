using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Ryujinx.Ava.Common.Locale;
using Ryujinx.Modules;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ryujinx.Ava.Ui.Windows
{
    public class UpdaterWindow : StyleableWindow
    {
        private readonly string _buildUrl;
        private readonly MainWindow _mainWindow;
        private readonly Version _newVersion;
        private bool _restartQuery;

        public UpdaterWindow()
        {
            DataContext = this;

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Title = LocaleManager.Instance["RyujinxUpdater"];
        }

        public UpdaterWindow(MainWindow mainWindow, Version newVersion, string buildUrl) : this()
        {
            _mainWindow = mainWindow;
            _newVersion = newVersion;
            _buildUrl = buildUrl;
        }

        public TextBlock MainText { get; set; }
        public TextBlock SecondaryText { get; set; }
        public ProgressBar ProgressBar { get; set; }
        public StackPanel ButtonBox { get; set; }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            MainText = this.FindControl<TextBlock>("MainText");
            SecondaryText = this.FindControl<TextBlock>("SecondaryText");
            ProgressBar = this.FindControl<ProgressBar>("ProgressBar");
            ButtonBox = this.FindControl<StackPanel>("ButtonBox");
        }

        [DllImport("libc", SetLastError = true)]
        private static extern int chmod(string path, uint mode);

        public void YesPressed()
        {
            if (_restartQuery)
            {
                string ryuName = OperatingSystem.IsWindows() ? "Ryujinx.Ava.exe" : "Ryujinx.Ava";
                string ryuExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ryuName);
                string ryuArg = string.Join(" ", Environment.GetCommandLineArgs().AsEnumerable().Skip(1).ToArray());

                if (!OperatingSystem.IsWindows())
                {
                    chmod(ryuExe, 0777);
                }

                Process.Start(ryuExe, ryuArg);

                Environment.Exit(0);
            }
            else
            {
                ButtonBox.IsVisible = false;
                ProgressBar.IsVisible = true;

                SecondaryText.Text = "";
                _restartQuery = true;

                Updater.UpdateRyujinx(this, _buildUrl);
            }
        }

        public void NoPressed()
        {
            _mainWindow.UpdateMenuItem.IsEnabled = true;

            Close();
        }
    }
}